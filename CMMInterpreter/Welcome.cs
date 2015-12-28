using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CMMInterpreter
{
    public partial class Welcome : Form
    {
        public Welcome()
        {
            InitializeComponent();
            WelcomeTimer.Enabled = true;//设置timer控件可用 
            WelcomeTimer.Interval = 2000;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            Hide();
            WelcomeTimer.Enabled = false;
            Main mainFrame = new Main();
            mainFrame.ShowDialog();
            Close();
        }
    }
}