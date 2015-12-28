using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FarsiLibrary.Win;
using FastColoredTextBoxNS;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing.Drawing2D;
using CMMInterpreter.Process;

namespace CMMInterpreter
{
    public partial class Main : Form
    {
        /// <summary>
        /// Divides numbers and words: "123AND456" -> "123 AND 456"
        /// Or "i=2" -> "i = 2"
        /// </summary>
        class InsertSpaceSnippet : AutocompleteItem
        {
            string pattern;

            public InsertSpaceSnippet(string pattern)
                : base("")
            {
                this.pattern = pattern;
            }

            public InsertSpaceSnippet()
                : this(@"^(\d+)([a-zA-Z_]+)(\d*)$")
            {
            }

            public override CompareResult Compare(string fragmentText)
            {
                if (Regex.IsMatch(fragmentText, pattern))
                {
                    Text = InsertSpaces(fragmentText);
                    if (Text != fragmentText)
                        return CompareResult.Visible;
                }
                return CompareResult.Hidden;
            }

            public string InsertSpaces(string fragment)
            {
                var m = Regex.Match(fragment, pattern);
                if (m == null)
                    return fragment;
                if (m.Groups[1].Value == "" && m.Groups[3].Value == "")
                    return fragment;
                return (m.Groups[1].Value + " " + m.Groups[2].Value + " " + m.Groups[3].Value).Trim();
            }

            public override string ToolTipTitle
            {
                get
                {
                    return Text;
                }
            }
        }

        /// <summary>
        /// Inerts line break after '}'
        /// </summary>
        class InsertEnterSnippet : AutocompleteItem
        {
            Place enterPlace = Place.Empty;

            public InsertEnterSnippet()
                : base("[Line break]")
            {
            }

            public override CompareResult Compare(string fragmentText)
            {
                var r = Parent.Fragment.Clone();
                while (r.Start.iChar > 0)
                {
                    if (r.CharBeforeStart == '}')
                    {
                        enterPlace = r.Start;
                        return CompareResult.Visible;
                    }

                    r.GoLeftThroughFolded();
                }
                return CompareResult.Hidden;
            }

            public override string GetTextForReplace()
            {
                //extend range
                Range r = Parent.Fragment;
                Place end = r.End;
                r.Start = enterPlace;
                r.End = r.End;
                //insert line break
                return Environment.NewLine + r.Text;
            }

            public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e)
            {
                base.OnSelected(popupMenu, e);
                if (Parent.Fragment.tb.AutoIndent)
                    Parent.Fragment.tb.DoAutoIndent();
            }

            public override string ToolTipTitle
            {
                get
                {
                    return "Insert line break after '}'";
                }
            }
        }

        string[] keywords = { "int", "real", "void", "if", "else", "while", "for", "read", "write", "return"};
        string[] snippets = { "if(^)\n{\n;\n}", "if(^)\n{\n;\n}\nelse\n{\n;\n}", "for(^;;)\n{\n;\n}", "while(^)\n{\n;\n}"};
        
        Color currentLineColor = Color.FromArgb(100, 210, 210, 255);//没有用到
        Color changedLineColor = Color.FromArgb(255, 230, 230, 255);

        //构造函数
        public Main()
        {
            InitializeComponent();

            //init menu images
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            PasteSelection.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripButton.Image")));
        }

        private Style sameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(50, Color.Gray)));

        FastColoredTextBox CurrentTB
        {
            get
            {
                if (CurrentFiles.SelectedItem == null)
                    return null;
                return (CurrentFiles.SelectedItem.Controls[0] as FastColoredTextBox);
            }

            set
            {
                CurrentFiles.SelectedItem = (value.Parent as FATabStripItem);
                value.Focus();
            }
        }

        //处理 撤销/重做 按钮的相应能力
        private void tmUpdateInterface_Tick(object sender, EventArgs e)
        {
            try
            {
                if (CurrentTB != null && CurrentFiles.Items.Count > 0)
                {
                    var tb = CurrentTB;
                    UndoButton.Enabled = UndoSelection.Enabled = tb.UndoEnabled;
                    RedoButton.Enabled = RedoSelection.Enabled = tb.RedoEnabled;
                }
                else
                {
                    UndoButton.Enabled = UndoSelection.Enabled = false;
                    RedoButton.Enabled = RedoSelection.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        bool tbFindChanged = false;

        //ctrl+k弹窗快捷提示
        void popupMenu_Opening(object sender, CancelEventArgs e)
        {
            //---block autocomplete menu for comments
            //get index of green style (used for comments)
            var iGreenStyle = CurrentTB.GetStyleIndex(CurrentTB.SyntaxHighlighter.GreenStyle);
            if (iGreenStyle >= 0)
                if (CurrentTB.Selection.Start.iChar > 0)
                {
                    //current char (before caret)
                    var c = CurrentTB[CurrentTB.Selection.Start.iLine][CurrentTB.Selection.Start.iChar - 1];
                    //green Style
                    var greenStyleIndex = Range.ToStyleIndex(iGreenStyle);
                    //if char contains green style then block popup menu
                    if ((c.style & greenStyleIndex) != 0)
                        e.Cancel = true;
                }
        }

        //添加弹窗快捷提示
        private void BuildAutocompleteMenu(AutocompleteMenu popupMenu)
        {
            List<AutocompleteItem> items = new List<AutocompleteItem>();

            foreach (var item in snippets)//if ifelse while for
                items.Add(new SnippetAutocompleteItem(item) { ImageIndex = 1 });
            foreach (var item in keywords)
                items.Add(new AutocompleteItem(item));

            items.Add(new InsertSpaceSnippet());
            items.Add(new InsertSpaceSnippet(@"^(\w+)([=<>!:]+)(\w+)$"));
            items.Add(new InsertEnterSnippet());

            //set as autocomplete source
            popupMenu.Items.SetAutocompleteItems(items);
            popupMenu.SearchPattern = @"[\w\.:=!<>]";
        }

        //鼠标移动相应
        void tb_MouseMove(object sender, MouseEventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            var place = tb.PointToPlace(e.Location);
            var r = new Range(tb, place, place);

            string text = r.GetFragment("[a-zA-Z]").Text;
            CurrentWordUnderMouse.Text = text;
        }

        //单行注释样式
        Style SCommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Regular);
        Style MCommentStyle = new TextStyle(Brushes.Brown, null, FontStyle.Regular);
        //关键字样式 CadetBlue Blue
        Style KeyWord = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        string key = @"\bint\b|\breal\b|\bvoid\b|\bif\b|\belse\b|\bwhile\b|for\b|\bread\b|\bwrite\b|\breturn\b";
        //刷新输入样式
        void TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            Range r = e.ChangedRange;
            Range m = (sender as FastColoredTextBox).VisibleRange;
            //清除变化部分的折叠标记  
            r.ClearFoldingMarkers();
            //设置折叠标记  
            r.SetFoldingMarkers("{", "}");
            r.SetFoldingMarkers(@"#region\b", @"#endregion\b");

            //清除关键字样式
            r.ClearStyle(KeyWord);
            //高亮关键字
            r.SetStyle(KeyWord, key, RegexOptions.None);

            //清除单行注释样式  
            r.ClearStyle(SCommentStyle);
            //高亮单行注释
            r.SetStyle(SCommentStyle, @"//.*$", RegexOptions.Multiline);

            //清除多行注释样式  
            r.ClearStyle(MCommentStyle);
            m.ClearStyle(MCommentStyle);
            //高亮多行注释
            r.SetStyle(MCommentStyle, @"/\*(\s|\S)*\S*\*\/(\s|\r\n)*", RegexOptions.Multiline);
            m.SetStyle(MCommentStyle, @"(/\*.*?\*\/)|(/\*.*)", RegexOptions.Singleline);
            m.SetStyle(MCommentStyle, @"(/\*.*?\*\/)|(.*\*\/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
        }

        //右键 粘贴 按钮响应函数
        private void PasteSelection_Click(object sender, EventArgs e)
        {
            CurrentTB.Paste();
        }

        //右键 全选 按钮响应函数
        private void SelectAllSelection_Click(object sender, EventArgs e)
        {
            CurrentTB.Selection.SelectAll();
        }

        //右键 替换 按钮响应函数
        private void ReplaceSelection_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowReplaceDialog();
        }

        //窗口关闭时函数
        //private void PowerfulCSharpEditor_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    List<FATabStripItem> list = new List<FATabStripItem>();
        //    foreach (FATabStripItem tab in CurrentFiles.Items)
        //        list.Add(tab);
        //    foreach (var tab in list)
        //    {
        //        TabStripItemClosingEventArgs args = new TabStripItemClosingEventArgs(tab);
        //        if (args.Cancel)
        //        {
        //            e.Cancel = true;
        //            return;
        //        }
        //        CurrentFiles.RemoveTab(tab);
        //    }
        //}
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<FATabStripItem> list = new List<FATabStripItem>();
            foreach (FATabStripItem tab in CurrentFiles.Items)
                list.Add(tab);
            foreach (var tab in list)
            {
                TabStripItemClosingEventArgs args = new TabStripItemClosingEventArgs(tab);
                if (args.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                CurrentFiles.RemoveTab(tab);
            }
            //Application.Exit();//强制关闭所有的窗口
        }

        //切换标签页时会调用的函数，判断是否已经有标签页了
        private void CurrentFiles_TabStripItemSelectionChanged(TabStripItemChangedEventArgs e)
        {
            if (CurrentTB != null)
            {
                CurrentTB.Focus();
                string text = CurrentTB.Text;
            }
        }

        private void autoIndentSelectedTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.DoAutoIndent();
        }

        //按钮方法--------------------------------------------------------------------------------------------
        private void Zoom_click(object sender, EventArgs e)
        {
            if (CurrentTB != null)
                CurrentTB.Zoom = int.Parse((sender as ToolStripItem).Tag.ToString());
        }

        private void NewFile_Click(object sender, EventArgs e)
        {
            CreateTab(null);
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            if (OpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(OpenFileDialog.FileName);
        }

        private void CreateTab(string fileName)
        {
            try
            {
                var tb = new FastColoredTextBox();
                tb.Font = new Font("Consolas", 11f);
                tb.ImeMode = ImeMode.On;//支持中文

                //必须把函数绑定写在初次Text赋值之前，否则第一次赋值无法触发TB_TextChanged
                tb.TextChanged += new EventHandler<TextChangedEventArgs>(TB_TextChanged);
                tb.MouseMove += new MouseEventHandler(tb_MouseMove);

                tb.ChangedLineColor = changedLineColor;
                tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;

                tb.ContextMenuStrip = RightClickMenu;
                tb.Dock = DockStyle.Fill;
                tb.BorderStyle = BorderStyle.Fixed3D;
                //tb.VirtualSpace = true;
                tb.LeftPadding = 12;
                //tb.Language = Language.CSharp;//特！么！就！是！这！句！话！找！了！一！晚！上！
                tb.AddStyle(sameWordsStyle);//same words style
                
                var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "[new]", tb);
                CurrentFiles.AddTab(tab);
                CurrentFiles.SelectedItem = tab;
                tab.Tag = fileName;
                tb.Text = "";
                if (fileName != null)
                    tb.OpenFile(fileName);
                tb.Tag = new TbInfo();
                tb.Focus();

                //create autocomplete popup menu
                AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                popupMenu.Items.ImageList = ilAutocomplete;
                popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                BuildAutocompleteMenu(popupMenu);
                (tb.Tag as TbInfo).popupMenu = popupMenu;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    CreateTab(fileName);
            }
        }

        private void SaveFile_Click(object sender, EventArgs e)
        {
            if (CurrentFiles.SelectedItem != null)
                Save(CurrentFiles.SelectedItem);
        }

        private bool Save(FATabStripItem tab)
        {
            var tb = (tab.Controls[0] as FastColoredTextBox);
            if (tab.Tag == null)
            {
                if (SaveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return false;
                tab.Title = Path.GetFileName(SaveFileDialog.FileName);
                tab.Tag = SaveFileDialog.FileName;
            }

            try
            {
                File.WriteAllText(tab.Tag as string, tb.Text);
                tb.IsChanged = false;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    return Save(tab);
                else
                    return false;
            }

            tb.Invalidate();

            return true;
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            if (CurrentTB.UndoEnabled)
                CurrentTB.Undo();
        }

        private void RedoButton_Click(object sender, EventArgs e)
        {
            if (CurrentTB.RedoEnabled)
                CurrentTB.Redo();
        }

        //监听Enter键
        private void FindBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && CurrentTB != null)
            {
                Range r = tbFindChanged ? CurrentTB.Range.Clone() : CurrentTB.Selection.Clone();
                tbFindChanged = false;
                r.End = new Place(CurrentTB[CurrentTB.LinesCount - 1].Count, CurrentTB.LinesCount - 1);
                var pattern = Regex.Escape(FindBox.Text);
                foreach (var found in r.GetRanges(pattern))
                {
                    found.Inverse();
                    CurrentTB.Selection = found;
                    CurrentTB.DoSelectionVisible();
                    return;
                }
                MessageBox.Show("Not found.");
            }
            else
                tbFindChanged = true;
        }

        private void ChangeLayout_Click(object sender, EventArgs e)
        {
            if (CurrentFiles.Location.X == ControlPage.Location.X)//竖向布局(ControlPage:400 CodeIn)
            {
                CurrentFiles.Width = 380;
                ControlPage.Width = 420;

                //改为横向布局
                ControlPage.Location = new System.Drawing.Point(CurrentFiles.Location.X + CurrentFiles.Width + 5, CurrentFiles.Location.Y);
                ControlPage.Height = CurrentFiles.Height;

                //改变结果显示部分的大小
                manipulateTextBox();

                this.Size = new Size(CurrentFiles.Width + ControlPage.Width + 25, CurrentFiles.Height + 88);//新横向布局大小
                int x = Screen.PrimaryScreen.Bounds.Width - this.Width;
                int y = Screen.PrimaryScreen.Bounds.Height - this.Height;
                this.Location = new System.Drawing.Point(x / 2, y / 2 - 100);
            }
            else if (CurrentFiles.Location.Y == ControlPage.Location.Y)//横向布局
            {
                CurrentFiles.Width = 520;
                CurrentFiles.Height = 361;

                //改为竖向布局
                ControlPage.Location = new Point(CurrentFiles.Location.X, CurrentFiles.Location.Y + CurrentFiles.Height + 3);
                ControlPage.Width = CurrentFiles.Width + 4;
                ControlPage.Height = 140;

                manipulateTextBox();
                this.Size = new Size(CurrentFiles.Width + 20, 590);
            }
        }

        private void manipulateTextBox()
        {
            List<Control> list = new List<Control>();
            list.Add(LexicalOut);
            list.Add(GrammarOut);
            list.Add(MidcodeOut);
            list.Add(ErrorOut);
            //list.Add(ResultOut);

            if (LexicalOut.Controls.Find("LexGrid", false).Count() > 0)
                list.Add(LexicalOut.Controls.Find("LexGrid", false)[0]);
            if (ErrorOut.Controls.Find("ErrGrid", false).Count() > 0)
                list.Add(ErrorOut.Controls.Find("ErrGrid", false)[0]);
            if (MidcodeOut.Controls.Find("MidcodeGrid", false).Count() > 0)
                list.Add(MidcodeOut.Controls.Find("MidcodeGrid", false)[0]);

            for (int i = 0; i < list.Count(); i++)
            {
                list[i].Height = ControlPage.Height - 25;
                list[i].Width = ControlPage.Width -9;
            }

            for (int i = 5; i < list.Count(); i++ )
            {
                DataGridView grid = (DataGridView)list[i];
                int currentWidth = grid.Width;
                int totalWidth = 0;
                for (int j = 0; j < grid.ColumnCount; j++)
                    totalWidth += grid.Columns[j].Width;

                if (totalWidth != grid.Width)
                {
                    if (grid.Name == "LexGrid")
                    {
                        int off = (currentWidth - totalWidth) / 7;
                        grid.Columns[1].Width += 3 * off;
                        grid.Columns[2].Width += (currentWidth - totalWidth) - 3 * off;
                    }
                    else if (grid.Name == "ErrGrid")
                        grid.Columns[1].Width += currentWidth - totalWidth;
                    else if (grid.Name == "MidcodeGrid")
                    {
                        int count = grid.Columns.Count;
                        for (int x = 0; x < count; x++)
                            grid.Columns[x].Width += (currentWidth - totalWidth) / count;
                    }
                }
            }
        }

        //设置Button相应事件
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.N))         //新建文件
                CreateTab(null);
            if (keyData == (Keys.Control | Keys.O))         //打开文件
            {
                if (OpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    CreateTab(OpenFileDialog.FileName);
            }
            if (keyData == (Keys.Control | Keys.S))         //保存文件
            {
                if (CurrentFiles.SelectedItem != null)
                    Save(CurrentFiles.SelectedItem);
            }
            if (keyData == (Keys.Control | Keys.Right))     //增加高度
                AddWidth(); 
            if (keyData == (Keys.Control | Keys.Left))      //减小宽度
                MinusWidth();
            if (keyData == (Keys.Control | Keys.Up))        //减小高度
                MinusHeight();
            if (keyData == (Keys.Control | Keys.Down))      //增加高度
                AddHeight();
            if (keyData == (Keys.Control | Keys.Enter))     //更换布局
                ChangeLayout.PerformClick();
            //展开所选中节点的所有子节点
            if (keyData == (Keys.Alt | Keys.O) && GrammarOut.Contains(grm.gTree) && grm.gTree.Focused == true)//展开选中的节点树
            {
                grm.gTree.SelectedNode.ExpandAll();
            }
            //关闭所选中节点的所有子节点
            if (keyData == (Keys.Alt | Keys.C) && GrammarOut.Contains(grm.gTree) && grm.gTree.Focused == true)//关闭选中的节点树
            {
                grm.gTree.SelectedNode.Collapse();
            }
            //展开根节点的一级节点
            if (keyData == (Keys.Alt | Keys.Shift | Keys.O) && GrammarOut.Contains(grm.gTree) && grm.gTree.Focused == true)//关闭选中的节点树
            {
                grm.gTree.SelectedNode = grm.gTree.Nodes[0];
                //grm.gTree.SelectedNode.Collapse();
                grm.gTree.SelectedNode.ExpandAll();
            }
            //关闭至根节点只展开一级节点
            if (keyData == (Keys.Alt | Keys.Shift | Keys.C) && GrammarOut.Contains(grm.gTree) && grm.gTree.Focused == true)//关闭选中的节点树
            {
                grm.gTree.SelectedNode = grm.gTree.Nodes[0];
                grm.gTree.SelectedNode.Collapse();
                grm.gTree.SelectedNode.Expand();
            }   

            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void AddWidth()
        {
            ResizeWidth(20);
        }

        private void MinusWidth()
        {
            ResizeWidth(-20);
        }
        private void AddHeight()
        {
            ResizeHeight(20);
        }
        private void MinusHeight()
        {
            ResizeHeight(-20);
        }

        private void ResizeWidth(int off)
        {
            bool isTooArror = this.Width + off > 540;
            bool isOverFlow = this.Location.X + this.Width + off < Screen.PrimaryScreen.Bounds.Width;
            if (isTooArror && isOverFlow)
            {
                ControlPage.Width = ControlPage.Width + off;
                if (CurrentFiles.Location.X == ControlPage.Location.X)//竖向布局，给CodeIn和ControlPage同时加宽度
                {
                    CurrentFiles.Width = ControlPage.Width;
                    this.Size = new Size(CurrentFiles.Width + 20, 590);
                }
                else
                    this.Size = new Size(CurrentFiles.Width + ControlPage.Width + 25, CurrentFiles.Height + 88);
                manipulateTextBox();
            }
        }
        private void ResizeHeight(int off)//只对横向布局处理
        {
            bool isTooArror = this.Height + off > 449;
            bool isOverFlow = this.Location.Y + this.Height + off < Screen.PrimaryScreen.Bounds.Height - 20;
            if (isTooArror && isOverFlow)
            {
                if (CurrentFiles.Location.Y == ControlPage.Location.Y)//横向布局，给CodeIn和ControlPage同时加高度
                {
                    ControlPage.Height = ControlPage.Height + off;
                    CurrentFiles.Height = ControlPage.Height;
                    this.Size = new Size(CurrentFiles.Width + ControlPage.Width + 25, CurrentFiles.Height + 88);
                }
                manipulateTextBox();
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //默认布局是竖向布局，即二者location.x相同
            ControlPage.Location = new Point(CurrentFiles.Location.X, CurrentFiles.Location.Y + CurrentFiles.Height+3);
            ControlPage.Width = CurrentFiles.Width+4;
        }
        

        private bool TestContent()
        {
            if (CurrentFiles.SelectedItem == null)
            {
                MessageBox.Show("请打开或新建一个文件");
                return false;
            }
            if (CurrentFiles.SelectedItem.Controls[0].Text.Trim() == "")
            {
                MessageBox.Show("请输入源码");
                return false;
            }
            return true;
        }

        private void FormateErr(Control ctrl,string errStr, string type)
        {
            DataGridView grid = (DataGridView)ctrl;

            grid.Name = "ErrGrid";
            grid.RowHeadersVisible = false;        //去除行头
            grid.ScrollBars = ScrollBars.Vertical; //不显示水平滚动条
            grid.BorderStyle = BorderStyle.None;   //去除边框显示
            grid.ReadOnly = true;

            string[] arr;
            if (type == "lex")
                arr = new string[3]{"Line", "Token", "ErrReason"};
            else
                arr = new string[2] { "Line","ErrReason" };
            for (int x = 0; x < arr.Length; x++)    //新建列
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.Name = arr[x];
                col.HeaderText = arr[x];
                int widthLen = 0;
                switch (x)
                {
                    case 0: widthLen = 60; break;
                    case 1: widthLen = 150; break;
                    case 2: widthLen = 200; break;
                }  
                col.Width = widthLen;
                grid.Columns.Add(col);
            }

            //为每个错误新加一行
            string[] resultArr = Regex.Split(errStr.Trim(), "\r\n", RegexOptions.IgnoreCase);
            for (int i = 0; i < resultArr.Count(); i++)
            {
                string[] sublineArr;
                if(type == "lex")
                    sublineArr = Regex.Split(resultArr[i], "\t\t", RegexOptions.IgnoreCase);
                else
                    sublineArr = Regex.Split(resultArr[i], " : \t", RegexOptions.IgnoreCase);

                DataGridViewRow drSub = new DataGridViewRow();      //新建行
                drSub.CreateCells(grid);                            //初始化行信息
                for (int j = 0; j < sublineArr.Count(); ++j)        //给列赋值
                    drSub.Cells[j].Value = sublineArr[j].Trim();
                drSub.Height = 18;
                grid.Rows.Add(drSub);                               //将行添加到Grid中
            }
            grid.Parent = ErrorOut;
            int width = ErrorOut.Width;
            CorrectWidth(grid);
        }
        private void CorrectWidth(Control ctrl)//要在指定parent之后才能使用
        {
            DataGridView grid = (DataGridView)ctrl;
            grid.Leave += new EventHandler(Grid_Leave);                    //添加失焦事件处理
            grid.Size = grid.Parent.Size;                                  //new System.Drawing.Size(0, 0);
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;  //点击选中该行
            grid.AllowUserToResizeRows = false;                            //不允许改变行高
            grid.AllowUserToAddRows = false;                               //不允许用户添加空白行，也去掉了自动的空白行
            grid.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            grid.ReadOnly = true;

            int totalWidth = 0;
            for (int i = 0; i < grid.ColumnCount; i++)
            {
                totalWidth += grid.Columns[i].Width;
            }
            if (totalWidth < grid.Width)
                grid.Columns[grid.ColumnCount - 1].Width += grid.Width - totalWidth;
        }

        Lexical lex = null;
        bool isLexError = false;
        bool isGrmAnal = false;
        private void LexicalAnal_Click(object sender, EventArgs e)
        {
            if (!TestContent())
                return;

            lex = new Lexical();

            //进行初始化
            Clear();
            isLexError = false;

            lexical();
            ControlPage.SelectedIndex = 0;
            if (isLexError)//有词法错误
            {
                DataGridView grid = new DataGridView();
                FormateErr(grid, lex.errStr, "lex");

                //跳转到ErrorOut页面,实现按键跳转到错误页面
                string commonStr = "发现" + lex.errNum + "个词法错误！是否跳转到ErrorPage？";
                string grmStr = "有词法错误，不能继续进行语法分析！\r\n";
                if (MessageBox.Show(isGrmAnal ? grmStr + commonStr : commonStr, "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    isLexError = true;
                    ControlPage.SelectedIndex = 3;
                    grid.ClearSelection();
                }
            }
            manipulateTextBox();
        }
        private void lexical()
        {
            var currentBox = CurrentFiles.SelectedItem.Controls[0] as FastColoredTextBox;
            lex.lexicalAnalyze(currentBox.Text);
            lex.formateResult(currentBox.Text,lex.lexResult);

            //显示词法结果--------------------------------
            lex.lGrid.Name = "LexGrid";
            lex.lGrid.Parent = LexicalOut;
            CorrectWidth(lex.lGrid);
            lex.lGrid.ClearSelection();
            //-------------------------------------------

            if (lex.errStr.Trim() != "")
                isLexError = true;    //有词法错误
            else
                isLexError = false;   //没有词法错误
        }
        private void Grid_Leave(object sender, EventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            grid.ClearSelection();
        }

        Grammar grm = null;
        bool isGrmErr = false;
        bool isMidcode = false;
        private void GrammarAnal_Click(object sender, EventArgs e)
        {
            if (!TestContent())
                return;

            grm = new Grammar();

            //进行初始化
            Clear();
            isGrmAnal = true;
            isGrmErr = false;

            LexicalAnal.PerformClick();
            if(!isLexError)     //没有词法错误
            {
                grammar();
                ControlPage.SelectedIndex = 1;
                if (isGrmErr)  //有语法错误
                {
                    string commonStr = "发现" + grm.errNum + "个语法错误！是否跳转到ErrorPage？";
                    string grmStr = "有语法错误，不能继续生成中间代码！\r\n";
                    if (MessageBox.Show(isMidcode ? grmStr + commonStr : commonStr, "警告", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        DataGridView grid = new DataGridView();
                        FormateErr(grid, grm.errStr, "grm");
                        ControlPage.SelectedIndex = 3;
                        grid.ClearSelection();
                    }
                }
            }
            isGrmAnal = false;
        }
        private void grammar()
        {
            grm.grammarAnalyze();

            //显示语法结果-------------------------------------------
            grm.gTree.Parent = GrammarOut;
            grm.gTree.SelectedNode = grm.gTree.Nodes[0];
            grm.gTree.SelectedNode.ExpandAll();
            //------------------------------------------------------

            if (grm.errNum != 0)
                isGrmErr = true;    //有语法错误
            else
                isGrmErr = false;   //没有语法错误
        }

        MidcodeGenerate mid = null;
        private void Compile_Click(object sender, EventArgs e)
        {
            if (!TestContent())
                return;

            mid = new MidcodeGenerate();

            //进行初始化
            Clear();
            isMidcode = true;

            GrammarAnal.PerformClick();
            if (!isLexError && !isGrmErr) //没有词法错误和语法错误
            {
                mid.midcodeGenerate();
                mid.formateMidcode();
                ControlPage.SelectedIndex = 2;

                //显示中间代码--------------------------------
                mid.midcodeGrid.Name = "MidcodeGrid";
                mid.midcodeGrid.Parent = MidcodeOut;
                CorrectWidth(mid.midcodeGrid);
                mid.midcodeGrid.ClearSelection();
                //-------------------------------------------

                ControlPage.SelectedIndex = 2;
            }
            manipulateTextBox();
            isMidcode = false;

            //生成本地中间代码
            mid.SaveMidDefault();
        }

        Run run = new Run();
        private void Run_Click(object sender, EventArgs e)
        {
            if (!TestContent())
                return;
            Compile.PerformClick();

            if (!isLexError && !isGrmErr)
                run.StartRun();
        }

        private void Clear()
        {
            LexicalOut.Controls.Clear();
            GrammarOut.Controls.Clear();
            MidcodeOut.Controls.Clear();
            ErrorOut.Controls.Clear();
        }

        //“关于”按钮点击事件响应
        private void About_Click(object sender, EventArgs e)
        {
            string about = "";
            about += "软件：\tCMM-Interpreter\r\n";
            about += "版本：\t3.7\r\n";
            about += "作者：\tKatherine Lee\r\n";
            about += "版权：\t归个人所有\r\n";
            about += "  \t他人设计若有雷同\r\n";
            about += "  \t总之我是原创(⊙v⊙)\r\n";
            MessageBox.Show(about, "关于");
        }

        //“快捷键”按钮点击事件响应
        private void CheckShortCut_Click(object sender, EventArgs e)
        {
            string show = "";
            show += "Ctrl+1\t\t词法分析\r\n";
            show += "Ctrl+2\t\t语法分析\r\n";
            show += "Ctrl+3\t\t中间代码\r\n";
            show += "Alt+1\t\t保存词法分析\r\n";
            show += "Alt+2\t\t保存语法分析\r\n";
            show += "Alt+3\t\t保存中间代码\r\n";
            show += "\t\n";
            show += "Ctrl+Up\t\t增加高度\r\n";
            show += "Ctrl+Down\t减小高度\r\n";
            show += "Ctrl+Left\t\t减小宽度\r\n";
            show += "Ctrl+Right\t增加宽度\r\n";
            show += "Ctrl+Enter\t改变布局\r\n";
            show += "\t\n";
            show += "Ctrl+O\t\t打开文件\r\n";
            show += "Ctrl+N\t\t新建文件\r\n";
            show += "Ctrl+S\t\t保存文件\r\n";

            show += "\t\n";
            show += "Alt+C\t\t关闭选中节点的所有子节点\r\n";
            show += "Alt+O\t\t展开选中节点的所有子节点\r\n";
            show += "Alt+Shift+C\t将语法树展开到第一层子节点\r\n";
            show += "Alt+Shift+O\t将语法树全部展开";

            MessageBox.Show(show, "快捷键查看");
        }

        //取消ControlPage的默认快捷键响应
        private void ControlPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                e.Handled = true;
            }
        }

        private void SaveLexResult_Click(object sender, EventArgs e)
        {
            if (lex == null)
            {
                MessageBox.Show("未进行词法分析！");
                return;
            }

            SaveFileDialog lexSave = new SaveFileDialog();
            lexSave.Filter = "文本文件|.txt";
            if (lexSave.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(lexSave.FileName);

                if (lex.errNum != 0)
                {
                    sw.WriteLine("【1】词法分析有误，出现"+lex.errNum+"个错误！");
                    sw.WriteLine(lex.errStr);
                }else
                    sw.WriteLine("【1】词法分析正确！");

                sw.WriteLine("【2】词法分析Token如下：");
                sw.WriteLine(lex.lexResult);
                sw.Flush();
                sw.Close();
            }
        }

        private void SaveMidcode_Click(object sender, EventArgs e)
        {
            if (mid == null)
            {
                MessageBox.Show("未生成中间代码！");
                return;
            }

            SaveFileDialog midcodeSave = new SaveFileDialog();
            midcodeSave.Filter = "文本文件|.txt";
            if (midcodeSave.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(midcodeSave.FileName);
                string outText = mid.GetOutTxt();
                sw.WriteLine(outText);
                sw.Flush();
                sw.Close();
            }
        }
    }

    public class TbInfo
    {
        public AutocompleteMenu popupMenu;
    }
}
