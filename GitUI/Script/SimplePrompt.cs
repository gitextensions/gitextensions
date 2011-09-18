using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.Script
{
    public partial class SimplePrompt : Form
    {
        private string _UserInput = "";

        public string UserInput { get{ return _UserInput;} }

        public SimplePrompt()
        {
            InitializeComponent();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            _UserInput = txt_UserInput.Text;
            this.Close();
        }

        private void SimplePrompt_Shown(object sender, EventArgs e)
        {
            txt_UserInput.Focus();
        }
    }
}
