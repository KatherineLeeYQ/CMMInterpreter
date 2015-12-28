namespace CMMInterpreter
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.FooterBar = new System.Windows.Forms.StatusStrip();
            this.CurrentWordUnderMouse = new System.Windows.Forms.ToolStripStatusLabel();
            this.ZoomButton = new System.Windows.Forms.ToolStripSplitButton();
            this.Per300 = new System.Windows.Forms.ToolStripMenuItem();
            this.Per200 = new System.Windows.Forms.ToolStripMenuItem();
            this.Per150 = new System.Windows.Forms.ToolStripMenuItem();
            this.Per100 = new System.Windows.Forms.ToolStripMenuItem();
            this.Per50 = new System.Windows.Forms.ToolStripMenuItem();
            this.Per25 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuBar = new System.Windows.Forms.ToolStrip();
            this.NewFile = new System.Windows.Forms.ToolStripButton();
            this.OpenFile = new System.Windows.Forms.ToolStripButton();
            this.SaveFile = new System.Windows.Forms.ToolStripButton();
            this.Separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.FindBox = new System.Windows.Forms.ToolStripTextBox();
            this.FindLabel = new System.Windows.Forms.ToolStripLabel();
            this.Debug = new System.Windows.Forms.ToolStripDropDownButton();
            this.LexicalAnal = new System.Windows.Forms.ToolStripMenuItem();
            this.GrammarAnal = new System.Windows.Forms.ToolStripMenuItem();
            this.Compile = new System.Windows.Forms.ToolStripMenuItem();
            this.Run = new System.Windows.Forms.ToolStripMenuItem();
            this.CopeWithResult = new System.Windows.Forms.ToolStripDropDownButton();
            this.SaveLexResult = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveMidcode = new System.Windows.Forms.ToolStripMenuItem();
            this.Help = new System.Windows.Forms.ToolStripDropDownButton();
            this.CheckShortCut = new System.Windows.Forms.ToolStripMenuItem();
            this.About = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ChangeLayout = new System.Windows.Forms.ToolStripButton();
            this.Separator3 = new System.Windows.Forms.ToolStripSeparator();
            this.UndoButton = new System.Windows.Forms.ToolStripButton();
            this.RedoButton = new System.Windows.Forms.ToolStripButton();
            this.CurrentFiles = new FarsiLibrary.Win.FATabStrip();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.RightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.PasteSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.SelectAllSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.RightClickSeperator1 = new System.Windows.Forms.ToolStripSeparator();
            this.UndoSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.RedoSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.RightClickSeperator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ReplaceSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.tmUpdateInterface = new System.Windows.Forms.Timer(this.components);
            this.ilAutocomplete = new System.Windows.Forms.ImageList(this.components);
            this.ErrorPage = new System.Windows.Forms.TabPage();
            this.ErrorOut = new System.Windows.Forms.Panel();
            this.MidcodePage = new System.Windows.Forms.TabPage();
            this.MidcodeOut = new System.Windows.Forms.Panel();
            this.GrammarPage = new System.Windows.Forms.TabPage();
            this.GrammarOut = new System.Windows.Forms.Panel();
            this.LexicalPage = new System.Windows.Forms.TabPage();
            this.LexicalOut = new System.Windows.Forms.Panel();
            this.ControlPage = new System.Windows.Forms.TabControl();
            this.FooterBar.SuspendLayout();
            this.MenuBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CurrentFiles)).BeginInit();
            this.RightClickMenu.SuspendLayout();
            this.ErrorPage.SuspendLayout();
            this.MidcodePage.SuspendLayout();
            this.GrammarPage.SuspendLayout();
            this.LexicalPage.SuspendLayout();
            this.ControlPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // FooterBar
            // 
            this.FooterBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CurrentWordUnderMouse,
            this.ZoomButton});
            this.FooterBar.Location = new System.Drawing.Point(0, 529);
            this.FooterBar.Name = "FooterBar";
            this.FooterBar.Size = new System.Drawing.Size(524, 23);
            this.FooterBar.TabIndex = 2;
            this.FooterBar.Text = "statusStrip1";
            // 
            // CurrentWordUnderMouse
            // 
            this.CurrentWordUnderMouse.AutoSize = false;
            this.CurrentWordUnderMouse.ForeColor = System.Drawing.Color.Gray;
            this.CurrentWordUnderMouse.Name = "CurrentWordUnderMouse";
            this.CurrentWordUnderMouse.Size = new System.Drawing.Size(451, 18);
            this.CurrentWordUnderMouse.Spring = true;
            this.CurrentWordUnderMouse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ZoomButton
            // 
            this.ZoomButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ZoomButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Per300,
            this.Per200,
            this.Per150,
            this.Per100,
            this.Per50,
            this.Per25});
            this.ZoomButton.Image = ((System.Drawing.Image)(resources.GetObject("ZoomButton.Image")));
            this.ZoomButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ZoomButton.Name = "ZoomButton";
            this.ZoomButton.Size = new System.Drawing.Size(58, 21);
            this.ZoomButton.Text = "Zoom";
            // 
            // Per300
            // 
            this.Per300.Name = "Per300";
            this.Per300.Size = new System.Drawing.Size(108, 22);
            this.Per300.Tag = "300";
            this.Per300.Text = "300%";
            this.Per300.Click += new System.EventHandler(this.Zoom_click);
            // 
            // Per200
            // 
            this.Per200.Name = "Per200";
            this.Per200.Size = new System.Drawing.Size(108, 22);
            this.Per200.Tag = "200";
            this.Per200.Text = "200%";
            this.Per200.Click += new System.EventHandler(this.Zoom_click);
            // 
            // Per150
            // 
            this.Per150.Name = "Per150";
            this.Per150.Size = new System.Drawing.Size(108, 22);
            this.Per150.Tag = "150";
            this.Per150.Text = "150%";
            this.Per150.Click += new System.EventHandler(this.Zoom_click);
            // 
            // Per100
            // 
            this.Per100.Name = "Per100";
            this.Per100.Size = new System.Drawing.Size(108, 22);
            this.Per100.Tag = "100";
            this.Per100.Text = "100%";
            this.Per100.Click += new System.EventHandler(this.Zoom_click);
            // 
            // Per50
            // 
            this.Per50.Name = "Per50";
            this.Per50.Size = new System.Drawing.Size(108, 22);
            this.Per50.Tag = "50";
            this.Per50.Text = "50%";
            this.Per50.Click += new System.EventHandler(this.Zoom_click);
            // 
            // Per25
            // 
            this.Per25.Name = "Per25";
            this.Per25.Size = new System.Drawing.Size(108, 22);
            this.Per25.Tag = "25";
            this.Per25.Text = "25%";
            this.Per25.Click += new System.EventHandler(this.Zoom_click);
            // 
            // MenuBar
            // 
            this.MenuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewFile,
            this.OpenFile,
            this.SaveFile,
            this.Separator1,
            this.FindBox,
            this.FindLabel,
            this.Debug,
            this.CopeWithResult,
            this.Help,
            this.Separator2,
            this.ChangeLayout,
            this.Separator3,
            this.UndoButton,
            this.RedoButton});
            this.MenuBar.Location = new System.Drawing.Point(0, 0);
            this.MenuBar.Name = "MenuBar";
            this.MenuBar.Size = new System.Drawing.Size(524, 25);
            this.MenuBar.TabIndex = 3;
            // 
            // NewFile
            // 
            this.NewFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NewFile.Image = ((System.Drawing.Image)(resources.GetObject("NewFile.Image")));
            this.NewFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewFile.Name = "NewFile";
            this.NewFile.Size = new System.Drawing.Size(23, 22);
            this.NewFile.Text = "新建";
            this.NewFile.Click += new System.EventHandler(this.NewFile_Click);
            // 
            // OpenFile
            // 
            this.OpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.OpenFile.Image = ((System.Drawing.Image)(resources.GetObject("OpenFile.Image")));
            this.OpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenFile.Name = "OpenFile";
            this.OpenFile.Size = new System.Drawing.Size(23, 22);
            this.OpenFile.Text = "打开";
            this.OpenFile.Click += new System.EventHandler(this.OpenFile_Click);
            // 
            // SaveFile
            // 
            this.SaveFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveFile.Image = ((System.Drawing.Image)(resources.GetObject("SaveFile.Image")));
            this.SaveFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveFile.Name = "SaveFile";
            this.SaveFile.Size = new System.Drawing.Size(23, 22);
            this.SaveFile.Text = "保存";
            this.SaveFile.Click += new System.EventHandler(this.SaveFile_Click);
            // 
            // Separator1
            // 
            this.Separator1.Name = "Separator1";
            this.Separator1.Size = new System.Drawing.Size(6, 25);
            // 
            // FindBox
            // 
            this.FindBox.AcceptsReturn = true;
            this.FindBox.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.FindBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.FindBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FindBox.Name = "FindBox";
            this.FindBox.Size = new System.Drawing.Size(100, 25);
            this.FindBox.ToolTipText = "Enter查找文档中字段";
            this.FindBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindBox_KeyPress);
            // 
            // FindLabel
            // 
            this.FindLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.FindLabel.Name = "FindLabel";
            this.FindLabel.Size = new System.Drawing.Size(39, 22);
            this.FindLabel.Text = "Find: ";
            // 
            // Debug
            // 
            this.Debug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LexicalAnal,
            this.GrammarAnal,
            this.Compile,
            this.Run});
            this.Debug.Image = ((System.Drawing.Image)(resources.GetObject("Debug.Image")));
            this.Debug.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Debug.Name = "Debug";
            this.Debug.Size = new System.Drawing.Size(61, 22);
            this.Debug.Text = "执行";
            // 
            // LexicalAnal
            // 
            this.LexicalAnal.Name = "LexicalAnal";
            this.LexicalAnal.ShortcutKeyDisplayString = "Ctrl+1";
            this.LexicalAnal.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D1)));
            this.LexicalAnal.Size = new System.Drawing.Size(168, 22);
            this.LexicalAnal.Text = "词法分析";
            this.LexicalAnal.Click += new System.EventHandler(this.LexicalAnal_Click);
            // 
            // GrammarAnal
            // 
            this.GrammarAnal.Name = "GrammarAnal";
            this.GrammarAnal.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D2)));
            this.GrammarAnal.Size = new System.Drawing.Size(168, 22);
            this.GrammarAnal.Text = "语法分析";
            this.GrammarAnal.Click += new System.EventHandler(this.GrammarAnal_Click);
            // 
            // Compile
            // 
            this.Compile.Name = "Compile";
            this.Compile.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D3)));
            this.Compile.Size = new System.Drawing.Size(168, 22);
            this.Compile.Text = "中间代码";
            this.Compile.Click += new System.EventHandler(this.Compile_Click);
            // 
            // Run
            // 
            this.Run.Name = "Run";
            this.Run.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.Run.Size = new System.Drawing.Size(168, 22);
            this.Run.Text = "编译执行";
            this.Run.Click += new System.EventHandler(this.Run_Click);
            // 
            // CopeWithResult
            // 
            this.CopeWithResult.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SaveLexResult,
            this.SaveMidcode});
            this.CopeWithResult.Image = ((System.Drawing.Image)(resources.GetObject("CopeWithResult.Image")));
            this.CopeWithResult.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CopeWithResult.Name = "CopeWithResult";
            this.CopeWithResult.Size = new System.Drawing.Size(61, 22);
            this.CopeWithResult.Text = "处理";
            // 
            // SaveLexResult
            // 
            this.SaveLexResult.Name = "SaveLexResult";
            this.SaveLexResult.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D1)));
            this.SaveLexResult.Size = new System.Drawing.Size(187, 22);
            this.SaveLexResult.Text = "保存词法分析";
            this.SaveLexResult.Click += new System.EventHandler(this.SaveLexResult_Click);
            // 
            // SaveMidcode
            // 
            this.SaveMidcode.Name = "SaveMidcode";
            this.SaveMidcode.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D2)));
            this.SaveMidcode.Size = new System.Drawing.Size(187, 22);
            this.SaveMidcode.Text = "保存中间代码";
            this.SaveMidcode.Click += new System.EventHandler(this.SaveMidcode_Click);
            // 
            // Help
            // 
            this.Help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CheckShortCut,
            this.About});
            this.Help.Image = ((System.Drawing.Image)(resources.GetObject("Help.Image")));
            this.Help.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Help.Name = "Help";
            this.Help.Size = new System.Drawing.Size(61, 22);
            this.Help.Text = "帮助";
            // 
            // CheckShortCut
            // 
            this.CheckShortCut.Image = ((System.Drawing.Image)(resources.GetObject("CheckShortCut.Image")));
            this.CheckShortCut.Name = "CheckShortCut";
            this.CheckShortCut.Size = new System.Drawing.Size(136, 22);
            this.CheckShortCut.Text = "快捷键查看";
            this.CheckShortCut.Click += new System.EventHandler(this.CheckShortCut_Click);
            // 
            // About
            // 
            this.About.Image = ((System.Drawing.Image)(resources.GetObject("About.Image")));
            this.About.Name = "About";
            this.About.Size = new System.Drawing.Size(136, 22);
            this.About.Text = "相关信息";
            this.About.Click += new System.EventHandler(this.About_Click);
            // 
            // Separator2
            // 
            this.Separator2.Name = "Separator2";
            this.Separator2.Size = new System.Drawing.Size(6, 25);
            // 
            // ChangeLayout
            // 
            this.ChangeLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ChangeLayout.Image = ((System.Drawing.Image)(resources.GetObject("ChangeLayout.Image")));
            this.ChangeLayout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ChangeLayout.Name = "ChangeLayout";
            this.ChangeLayout.Size = new System.Drawing.Size(23, 22);
            this.ChangeLayout.Text = "切换布局";
            this.ChangeLayout.Click += new System.EventHandler(this.ChangeLayout_Click);
            // 
            // Separator3
            // 
            this.Separator3.Name = "Separator3";
            this.Separator3.Size = new System.Drawing.Size(6, 25);
            // 
            // UndoButton
            // 
            this.UndoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.UndoButton.Image = ((System.Drawing.Image)(resources.GetObject("UndoButton.Image")));
            this.UndoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.UndoButton.Name = "UndoButton";
            this.UndoButton.Size = new System.Drawing.Size(23, 22);
            this.UndoButton.Text = "撤销 (Ctrl+Z)";
            this.UndoButton.Click += new System.EventHandler(this.UndoButton_Click);
            // 
            // RedoButton
            // 
            this.RedoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RedoButton.Image = ((System.Drawing.Image)(resources.GetObject("RedoButton.Image")));
            this.RedoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RedoButton.Name = "RedoButton";
            this.RedoButton.Size = new System.Drawing.Size(23, 22);
            this.RedoButton.Text = "重做 (Ctrl+R)";
            this.RedoButton.Click += new System.EventHandler(this.RedoButton_Click);
            // 
            // CurrentFiles
            // 
            this.CurrentFiles.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentFiles.Location = new System.Drawing.Point(3, 25);
            this.CurrentFiles.Name = "CurrentFiles";
            this.CurrentFiles.Size = new System.Drawing.Size(520, 361);
            this.CurrentFiles.TabIndex = 0;
            this.CurrentFiles.Text = "CurrentFiles";
            this.CurrentFiles.TabStripItemSelectionChanged += new FarsiLibrary.Win.TabStripItemChangedHandler(this.CurrentFiles_TabStripItemSelectionChanged);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 25);
            this.splitter1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 504);
            this.splitter1.TabIndex = 5;
            this.splitter1.TabStop = false;
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.DefaultExt = "cs";
            this.SaveFileDialog.Filter = "C file(*.txt)|*.txt";
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.DefaultExt = "cs";
            this.OpenFileDialog.Filter = "C file(*.txt)|*.txt";
            this.OpenFileDialog.RestoreDirectory = true;
            // 
            // RightClickMenu
            // 
            this.RightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PasteSelection,
            this.SelectAllSelection,
            this.RightClickSeperator1,
            this.UndoSelection,
            this.RedoSelection,
            this.RightClickSeperator2,
            this.ReplaceSelection});
            this.RightClickMenu.Name = "cmMain";
            this.RightClickMenu.Size = new System.Drawing.Size(128, 126);
            // 
            // PasteSelection
            // 
            this.PasteSelection.Name = "PasteSelection";
            this.PasteSelection.Size = new System.Drawing.Size(127, 22);
            this.PasteSelection.Text = "Paste";
            this.PasteSelection.Click += new System.EventHandler(this.PasteSelection_Click);
            // 
            // SelectAllSelection
            // 
            this.SelectAllSelection.Name = "SelectAllSelection";
            this.SelectAllSelection.Size = new System.Drawing.Size(127, 22);
            this.SelectAllSelection.Text = "Select all";
            this.SelectAllSelection.Click += new System.EventHandler(this.SelectAllSelection_Click);
            // 
            // RightClickSeperator1
            // 
            this.RightClickSeperator1.Name = "RightClickSeperator1";
            this.RightClickSeperator1.Size = new System.Drawing.Size(124, 6);
            // 
            // UndoSelection
            // 
            this.UndoSelection.Name = "UndoSelection";
            this.UndoSelection.Size = new System.Drawing.Size(127, 22);
            this.UndoSelection.Text = "Undo";
            this.UndoSelection.Click += new System.EventHandler(this.UndoButton_Click);
            // 
            // RedoSelection
            // 
            this.RedoSelection.Name = "RedoSelection";
            this.RedoSelection.Size = new System.Drawing.Size(127, 22);
            this.RedoSelection.Text = "Redo";
            this.RedoSelection.Click += new System.EventHandler(this.RedoButton_Click);
            // 
            // RightClickSeperator2
            // 
            this.RightClickSeperator2.Name = "RightClickSeperator2";
            this.RightClickSeperator2.Size = new System.Drawing.Size(124, 6);
            // 
            // ReplaceSelection
            // 
            this.ReplaceSelection.Name = "ReplaceSelection";
            this.ReplaceSelection.Size = new System.Drawing.Size(127, 22);
            this.ReplaceSelection.Text = "Replace";
            this.ReplaceSelection.Click += new System.EventHandler(this.ReplaceSelection_Click);
            // 
            // tmUpdateInterface
            // 
            this.tmUpdateInterface.Enabled = true;
            this.tmUpdateInterface.Interval = 400;
            this.tmUpdateInterface.Tick += new System.EventHandler(this.tmUpdateInterface_Tick);
            // 
            // ilAutocomplete
            // 
            this.ilAutocomplete.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilAutocomplete.ImageStream")));
            this.ilAutocomplete.TransparentColor = System.Drawing.Color.Transparent;
            this.ilAutocomplete.Images.SetKeyName(0, "script_16x16.png");
            this.ilAutocomplete.Images.SetKeyName(1, "app_16x16.png");
            this.ilAutocomplete.Images.SetKeyName(2, "1302166543_virtualbox.png");
            // 
            // ErrorPage
            // 
            this.ErrorPage.Controls.Add(this.ErrorOut);
            this.ErrorPage.Location = new System.Drawing.Point(4, 22);
            this.ErrorPage.Name = "ErrorPage";
            this.ErrorPage.Padding = new System.Windows.Forms.Padding(3);
            this.ErrorPage.Size = new System.Drawing.Size(516, 114);
            this.ErrorPage.TabIndex = 3;
            this.ErrorPage.Text = "错误";
            this.ErrorPage.UseVisualStyleBackColor = true;
            // 
            // ErrorOut
            // 
            this.ErrorOut.Location = new System.Drawing.Point(0, 2);
            this.ErrorOut.Name = "ErrorOut";
            this.ErrorOut.Size = new System.Drawing.Size(514, 112);
            this.ErrorOut.TabIndex = 303;
            // 
            // MidcodePage
            // 
            this.MidcodePage.Controls.Add(this.MidcodeOut);
            this.MidcodePage.Location = new System.Drawing.Point(4, 22);
            this.MidcodePage.Name = "MidcodePage";
            this.MidcodePage.Padding = new System.Windows.Forms.Padding(3);
            this.MidcodePage.Size = new System.Drawing.Size(516, 114);
            this.MidcodePage.TabIndex = 2;
            this.MidcodePage.Text = "中间代码";
            this.MidcodePage.UseVisualStyleBackColor = true;
            // 
            // MidcodeOut
            // 
            this.MidcodeOut.Location = new System.Drawing.Point(1, 1);
            this.MidcodeOut.Name = "MidcodeOut";
            this.MidcodeOut.Size = new System.Drawing.Size(514, 112);
            this.MidcodeOut.TabIndex = 305;
            // 
            // GrammarPage
            // 
            this.GrammarPage.BackColor = System.Drawing.Color.Transparent;
            this.GrammarPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.GrammarPage.Controls.Add(this.GrammarOut);
            this.GrammarPage.Location = new System.Drawing.Point(4, 22);
            this.GrammarPage.Name = "GrammarPage";
            this.GrammarPage.Padding = new System.Windows.Forms.Padding(3);
            this.GrammarPage.Size = new System.Drawing.Size(516, 114);
            this.GrammarPage.TabIndex = 1;
            this.GrammarPage.Text = "语法分析";
            // 
            // GrammarOut
            // 
            this.GrammarOut.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.GrammarOut.Location = new System.Drawing.Point(0, 0);
            this.GrammarOut.Name = "GrammarOut";
            this.GrammarOut.Size = new System.Drawing.Size(515, 115);
            this.GrammarOut.TabIndex = 301;
            // 
            // LexicalPage
            // 
            this.LexicalPage.BackColor = System.Drawing.Color.Transparent;
            this.LexicalPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.LexicalPage.Controls.Add(this.LexicalOut);
            this.LexicalPage.Location = new System.Drawing.Point(4, 22);
            this.LexicalPage.Name = "LexicalPage";
            this.LexicalPage.Padding = new System.Windows.Forms.Padding(3);
            this.LexicalPage.Size = new System.Drawing.Size(516, 114);
            this.LexicalPage.TabIndex = 0;
            this.LexicalPage.Text = "词法分析";
            // 
            // LexicalOut
            // 
            this.LexicalOut.Location = new System.Drawing.Point(0, 2);
            this.LexicalOut.Name = "LexicalOut";
            this.LexicalOut.Size = new System.Drawing.Size(513, 112);
            this.LexicalOut.TabIndex = 300;
            // 
            // ControlPage
            // 
            this.ControlPage.Controls.Add(this.LexicalPage);
            this.ControlPage.Controls.Add(this.GrammarPage);
            this.ControlPage.Controls.Add(this.MidcodePage);
            this.ControlPage.Controls.Add(this.ErrorPage);
            this.ControlPage.Cursor = System.Windows.Forms.Cursors.Default;
            this.ControlPage.Location = new System.Drawing.Point(3, 389);
            this.ControlPage.Name = "ControlPage";
            this.ControlPage.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ControlPage.SelectedIndex = 0;
            this.ControlPage.Size = new System.Drawing.Size(524, 140);
            this.ControlPage.TabIndex = 301;
            this.ControlPage.TabStop = false;
            this.ControlPage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ControlPage_KeyDown);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 552);
            this.Controls.Add(this.ControlPage);
            this.Controls.Add(this.CurrentFiles);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.MenuBar);
            this.Controls.Add(this.FooterBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CMM-Interpreter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.FooterBar.ResumeLayout(false);
            this.FooterBar.PerformLayout();
            this.MenuBar.ResumeLayout(false);
            this.MenuBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CurrentFiles)).EndInit();
            this.RightClickMenu.ResumeLayout(false);
            this.ErrorPage.ResumeLayout(false);
            this.MidcodePage.ResumeLayout(false);
            this.GrammarPage.ResumeLayout(false);
            this.LexicalPage.ResumeLayout(false);
            this.ControlPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip FooterBar;
        private System.Windows.Forms.ToolStrip MenuBar;
        private FarsiLibrary.Win.FATabStrip CurrentFiles;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.ContextMenuStrip RightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem PasteSelection;
        private System.Windows.Forms.ToolStripMenuItem SelectAllSelection;
        private System.Windows.Forms.ToolStripSeparator RightClickSeperator1;
        private System.Windows.Forms.ToolStripMenuItem UndoSelection;
        private System.Windows.Forms.ToolStripMenuItem RedoSelection;
        private System.Windows.Forms.Timer tmUpdateInterface;
        private System.Windows.Forms.ToolStripButton NewFile;
        private System.Windows.Forms.ToolStripButton OpenFile;
        private System.Windows.Forms.ToolStripButton SaveFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton UndoButton;
        private System.Windows.Forms.ToolStripButton RedoButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripTextBox FindBox;
        private System.Windows.Forms.ToolStripSeparator RightClickSeperator2;
        private System.Windows.Forms.ToolStripMenuItem ReplaceSelection;
        private System.Windows.Forms.ToolStripSeparator Separator1;
        private System.Windows.Forms.ToolStripStatusLabel CurrentWordUnderMouse;
        private System.Windows.Forms.ImageList ilAutocomplete;
        private System.Windows.Forms.ToolStripSplitButton ZoomButton;
        private System.Windows.Forms.ToolStripMenuItem Per300;
        private System.Windows.Forms.ToolStripMenuItem Per200;
        private System.Windows.Forms.ToolStripMenuItem Per150;
        private System.Windows.Forms.ToolStripMenuItem Per100;
        private System.Windows.Forms.ToolStripMenuItem Per50;
        private System.Windows.Forms.ToolStripMenuItem Per25;
        private System.Windows.Forms.ToolStripDropDownButton Debug;
        private System.Windows.Forms.ToolStripDropDownButton Help;
        private System.Windows.Forms.ToolStripMenuItem LexicalAnal;
        private System.Windows.Forms.ToolStripMenuItem GrammarAnal;
        private System.Windows.Forms.ToolStripMenuItem Compile;
        private System.Windows.Forms.ToolStripMenuItem Run;
        private System.Windows.Forms.ToolStripMenuItem CheckShortCut;
        private System.Windows.Forms.ToolStripMenuItem About;
        private System.Windows.Forms.ToolStripDropDownButton CopeWithResult;
        private System.Windows.Forms.ToolStripMenuItem SaveLexResult;
        private System.Windows.Forms.ToolStripMenuItem SaveMidcode;
        private System.Windows.Forms.ToolStripSeparator Separator2;
        private System.Windows.Forms.ToolStripButton ChangeLayout;
        private System.Windows.Forms.ToolStripSeparator Separator3;
        private System.Windows.Forms.ToolStripLabel FindLabel;
        private System.Windows.Forms.TabPage ErrorPage;
        private System.Windows.Forms.Panel ErrorOut;
        private System.Windows.Forms.TabPage MidcodePage;
        private System.Windows.Forms.Panel MidcodeOut;
        private System.Windows.Forms.TabPage GrammarPage;
        private System.Windows.Forms.Panel GrammarOut;
        private System.Windows.Forms.TabPage LexicalPage;
        private System.Windows.Forms.Panel LexicalOut;
        private System.Windows.Forms.TabControl ControlPage;
    }
}