using System;
using System.Windows.Forms;

namespace GitUI.Script
{
    public partial class SimplePrompt : Form
    {
        public string UserInput { get; private set; } = "";

        public SimplePrompt()
        {
            InitializeComponent();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            UserInput = txt_UserInput.Text;
            Close();
        }

        private void SimplePrompt_Shown(object sender, EventArgs e)
        {
            txt_UserInput.Focus();
        }
    }
}
