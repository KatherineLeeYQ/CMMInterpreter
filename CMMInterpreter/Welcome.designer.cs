namespace CMMInterpreter
{
    partial class Welcome
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Welcome));
            this.WelcomePic = new System.Windows.Forms.PictureBox();
            this.WelcomeTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.WelcomePic)).BeginInit();
            this.SuspendLayout();
            // 
            // WelcomePic
            // 
            this.WelcomePic.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WelcomePic.BackgroundImage")));
            this.WelcomePic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WelcomePic.Location = new System.Drawing.Point(0, 0);
            this.WelcomePic.Name = "WelcomePic";
            this.WelcomePic.Size = new System.Drawing.Size(458, 496);
            this.WelcomePic.TabIndex = 0;
            this.WelcomePic.TabStop = false;
            // 
            // WelcomeTimer
            // 
            this.WelcomeTimer.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Welcome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 496);
            this.Controls.Add(this.WelcomePic);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Welcome";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Welcome";
            ((System.ComponentModel.ISupportInitialize)(this.WelcomePic)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox WelcomePic;
        private System.Windows.Forms.Timer WelcomeTimer;
    }
}