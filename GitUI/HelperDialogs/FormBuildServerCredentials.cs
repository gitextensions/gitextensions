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

        public bool UseGuestAccess { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            UseGuestAccess = radioButtonGuestAccess.Checked;
            UserName = textBoxUserName.Text;
            Password = textBoxPassword.Text;

            Close();
        }

        private void FormBuildServerCredentials_Load(object sender, EventArgs e)
        {
            radioButtonGuestAccess.Checked = UseGuestAccess;
            radioButtonAuthenticatedUser.Checked = !UseGuestAccess;
            textBoxUserName.Text = UserName;
            textBoxPassword.Text = Password;

            UpdateUI();
        }

        private void authenticationMethodChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            textBoxUserName.Enabled = !radioButtonGuestAccess.Checked;
            textBoxPassword.Enabled = !radioButtonGuestAccess.Checked;
        }
    }
}
