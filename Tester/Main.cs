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
using cmmInterpreter.Process;

namespace Tester
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
        //string[] declarationSnippets = { 
        //       "public class ^\n{\n}", "private class ^\n{\n}", "internal class ^\n{\n}",
        //       "public struct ^\n{\n;\n}", "private struct ^\n{\n;\n}", "internal struct ^\n{\n;\n}",
        //       "public void ^()\n{\n;\n}", "private void ^()\n{\n;\n}", "internal void ^()\n{\n;\n}", "protected void ^()\n{\n;\n}",
        //       "public ^{ get; set; }", "private ^{ get; set; }", "internal ^{ get; set; }", "protected ^{ get; set; }"
        //       };
        
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

        //Ctrl+k监听
        void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.K | Keys.Control))
            {
                //forced show (MinFragmentLength will be ignored)
                (CurrentTB.Tag as TbInfo).popupMenu.Show(true);
                e.Handled = true;
            }
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
        private void PowerfulCSharpEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<FATabStripItem> list = new List<FATabStripItem>();
            foreach (FATabStripItem tab in  CurrentFiles.Items)
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
                tb.KeyDown += new KeyEventHandler(tb_KeyDown);
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
            list.Add(InterCodeOut);
            list.Add(ErrorOut);
            list.Add(ResultOut);
            for (int i = 0; i < list.Count(); i++)
            {
                list[i].Height = ControlPage.Height - 25;
                list[i].Width = ControlPage.Width -9;
            }
        }

        //设置Button相应事件
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.N))
                CreateTab(null);
            if (keyData == (Keys.Control | Keys.O))
            {
                if (OpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    CreateTab(OpenFileDialog.FileName);
            }
            if (keyData == (Keys.Control | Keys.S))
            {
                if (CurrentFiles.SelectedItem != null)
                    Save(CurrentFiles.SelectedItem);
            }
            if (keyData == (Keys.Control | Keys.Right))
                AddWidth();
            if (keyData == (Keys.Control | Keys.Left))
                MinusWidth();
            if (keyData == (Keys.Control | Keys.Up))
                MinusHeight();
            if (keyData == (Keys.Control | Keys.Down))
                AddHeight();
            if (keyData == (Keys.Control | Keys.Enter))
                ChangeLayout.PerformClick();
            if (keyData == (Keys.Alt | Keys.O) && GrammarOut.Contains(grm.gTree) && grm.gTree.Focused == true)//展开选中的节点树
            {
                grm.gTree.SelectedNode.ExpandAll();
            }
            if (keyData == (Keys.Alt | Keys.C) && GrammarOut.Contains(grm.gTree) && grm.gTree.Focused == true)//关闭选中的节点树
            {
                grm.gTree.SelectedNode.Collapse();
            }
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
        Lexical lex = new Lexical();
        private void LexicalAnal_Click(object sender, EventArgs e)
        {
            //进行清理
            LexicalOut.Text = "";

            if (!TestContent())
                return;

            lexical();
            if (lex.errStr.Trim() != "")
            {
                ControlPage.SelectedIndex = 0;
                //跳转到ErrorOut页面,实现按键跳转到错误页面
                if (MessageBox.Show("发现" + lex.errNum + "个词法错误！是否跳转到ErrorPage？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ControlPage.SelectedIndex = 3;
                }
                ErrorOut.Text = lex.errStr;
            }
        }
        private bool lexical()
        {
            var currentBox = CurrentFiles.SelectedItem.Controls[0] as FastColoredTextBox;
            //若输入为空，不进行词法分析
            if (currentBox.Text.Trim() == "")
                return false;

            lex.lexicalAnalyze(currentBox.Text);
            LexicalOut.Text = lex.formateResult(currentBox.Text, lex.lexResult);
            if (lex.errStr.Trim() != "")
                return true;    //有词法错误
            else
                return false;   //没有词法错误
        }

        Grammar grm = new Grammar();
        private void GrammarAnal_Click(object sender, EventArgs e)
        {
            if (!TestContent())
                return;

            bool isLexError = lexical();
            if (isLexError) //有词法错误
            {
                if (MessageBox.Show("有词法错误，不能进行语法分析！\r\n发现" + lex.errNum + "个词法错误！是否跳转到ErrorPage？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ControlPage.SelectedIndex = 3;
                }
                ErrorOut.Text = lex.errStr;
            }
            else            //没有词法错误
            {
                ControlPage.SelectedIndex = 1;
                if (grammar())  //有语法错误
                {
                    if (MessageBox.Show("有语法错误！\r\n发现" + grm.errNum + "个语法错误！是否跳转到ErrorPage？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ControlPage.SelectedIndex = 3;
                    }
                    ErrorOut.Text = grm.formateError();
                }

                GrammarOut.Controls.Clear();
                grm.gTree.Parent = GrammarOut;
                grm.gTree.SelectedNode = grm.gTree.Nodes[0];
                grm.gTree.SelectedNode.ExpandAll();
            }
        }
        private bool grammar()
        {
            //若输入为空，不进行语法分析
            if (CurrentFiles.Text.Trim() == "")
                return false;//没错误

            grm.grammarAnalyze();
            if (grm.errNum != 0)
                return true;    //有语法错误
            else
                return false;   //没有语法错误
        }

        private void Compile_Click(object sender, EventArgs e)
        {
            LexicalOut.Text = "";
            GrammarOut.Text = "";
            ErrorOut.Text = "";
            InterCodeOut.Text = "";
            if (CurrentFiles.Text.Trim() == "")
            {
                MessageBox.Show("请输入代码！");
            }
            else
            {

            }
        }

        private void Run_Click(object sender, EventArgs e)
        {

        }        
        //------------------------------------------------------------------
    }

    

    public class TbInfo
    {
        public AutocompleteMenu popupMenu;
    }
}
