using System;
using System.Windows.Forms;

namespace GitUI.Script
{
    public partial class SimplePrompt : Form
    {
        private string _UserInput = "";

        public string UserInput => _UserInput;

        public SimplePrompt()
        {
            InitializeComponent();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            _UserInput = txt_UserInput.Text;
            Close();
        }

        private void SimplePrompt_Shown(object sender, EventArgs e)
        {
            txt_UserInput.Focus();
        }
    }
}
