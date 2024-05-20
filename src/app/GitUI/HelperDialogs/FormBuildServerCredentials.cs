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

        public IBuildServerCredentials? BuildServerCredentials { get; set; }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            BuildServerCredentials ??= new BuildServerCredentials();

            if (radioButtonGuestAccess.Checked)
            {
                BuildServerCredentials.BuildServerCredentialsType = BuildServerCredentialsType.Guest;
            }

            if (radioButtonAuthenticatedUser.Checked)
            {
                BuildServerCredentials.BuildServerCredentialsType = BuildServerCredentialsType.UsernameAndPassword;
            }

            if (radioButtonBearerToken.Checked)
            {
                BuildServerCredentials.BuildServerCredentialsType = BuildServerCredentialsType.BearerToken;
            }

            BuildServerCredentials.Username = textBoxUserName.Text;
            BuildServerCredentials.Password = textBoxPassword.Text;
            BuildServerCredentials.BearerToken = textBoxBearerToken.Text;

            Close();
        }

        private void FormBuildServerCredentials_Load(object sender, EventArgs e)
        {
            if (BuildServerCredentials is not null)
            {
                radioButtonGuestAccess.Checked = BuildServerCredentials.BuildServerCredentialsType == BuildServerCredentialsType.Guest;
                radioButtonAuthenticatedUser.Checked = BuildServerCredentials.BuildServerCredentialsType == BuildServerCredentialsType.UsernameAndPassword;
                radioButtonBearerToken.Checked = BuildServerCredentials.BuildServerCredentialsType == BuildServerCredentialsType.BearerToken;
                textBoxUserName.Text = BuildServerCredentials.Username;
                textBoxPassword.Text = BuildServerCredentials.Password;
                textBoxBearerToken.Text = BuildServerCredentials.BearerToken;
            }

            UpdateUI();
        }

        private void authenticationMethodChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            textBoxUserName.Enabled = radioButtonAuthenticatedUser.Checked;
            textBoxPassword.Enabled = radioButtonAuthenticatedUser.Checked;
            textBoxBearerToken.Enabled = radioButtonBearerToken.Checked;
        }
    }
}
