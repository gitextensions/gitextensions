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

        public string UserName { get; set; }

        public string Password { get; set; }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            UserName = textBoxUserName.Text;
            Password = textBoxPassword.Text;

            Close();
        }

        private void FormBuildServerCredentials_Load(object sender, EventArgs e)
        {
            textBoxUserName.Text = UserName;
            textBoxPassword.Text = Password;
        }
    }
}
