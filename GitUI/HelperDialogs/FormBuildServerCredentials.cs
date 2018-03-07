using System;
using System.Windows.Forms;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitUI.HelperDialogs
{
    public partial class FormBuildServerCredentials : Form
    {
        public FormBuildServerCredentials(string buildServerUniqueKey)
        {
            InitializeComponent();

            labelHeader.Text = string.Format(labelHeader.Text, buildServerUniqueKey);
        }

        public IBuildServerCredentials BuildServerCredentials { get; set; }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (BuildServerCredentials == null)
            {
                BuildServerCredentials = new BuildServerCredentials();
            }

            BuildServerCredentials.UseGuestAccess = radioButtonGuestAccess.Checked;
            BuildServerCredentials.Username = textBoxUserName.Text;
            BuildServerCredentials.Password = textBoxPassword.Text;

            Close();
        }

        private void FormBuildServerCredentials_Load(object sender, EventArgs e)
        {
            if (BuildServerCredentials != null)
            {
                radioButtonGuestAccess.Checked = BuildServerCredentials.UseGuestAccess;
                radioButtonAuthenticatedUser.Checked = !BuildServerCredentials.UseGuestAccess;
                textBoxUserName.Text = BuildServerCredentials.Username;
                textBoxPassword.Text = BuildServerCredentials.Password;
            }

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
