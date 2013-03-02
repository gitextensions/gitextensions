using System;
using System.Windows.Forms;

namespace GitUI.HelperDialogs
{
    public partial class FormBuildServerCredentials : Form
    {
        public FormBuildServerCredentials(string buildServerUniqueKey)
        {
            InitializeComponent();

            labelHeader.Text = string.Format(labelHeader.Text, buildServerUniqueKey);
        }

        public string UserName { get; private set; }
        public string Password { get; private set; }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            UserName = textBoxUserName.Text;
            Password = textBoxPassword.Text;

            Close();
        }
    }
}
