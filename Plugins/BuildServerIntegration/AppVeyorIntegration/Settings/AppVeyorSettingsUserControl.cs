using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitCommands.Settings;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace AppVeyorIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata("AppVeyor")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class AppVeyorSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string _defaultProjectName;

        public AppVeyorSettingsUserControl()
        {
            InitializeComponent();
            Translate();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        public void Initialize(string defaultProjectName)
        {
            _defaultProjectName = defaultProjectName;
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            if (buildServerConfig != null)
            {
                AppVeyorProjectName.Text = buildServerConfig.GetString("AppVeyorProjectName", _defaultProjectName);
                AppVeyorAccountName.Text = buildServerConfig.GetString("AppVeyorAccountName", string.Empty);
                AppVeyorAccountToken.Text = buildServerConfig.GetString("AppVeyorAccountToken", string.Empty);
                cbLoadTestResults.Checked = buildServerConfig.GetBool("AppVeyorLoadTestsResults", false);
                cbGitHubPullRequest.Checked = buildServerConfig.GetBool("AppVeyorDisplayGitHubPullRequests", false);
                txtGitHubToken.Text = buildServerConfig.GetString("AppVeyorGitHubToken", string.Empty);
                txtGitHubToken.Enabled = cbGitHubPullRequest.Checked;
            }
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            buildServerConfig.SetString("AppVeyorProjectName", AppVeyorProjectName.Text);
            buildServerConfig.SetString("AppVeyorAccountName", AppVeyorAccountName.Text);
            buildServerConfig.SetString("AppVeyorAccountToken", AppVeyorAccountToken.Text);
            buildServerConfig.SetBool("AppVeyorLoadTestsResults", cbLoadTestResults.Checked);
            buildServerConfig.SetBool("AppVeyorDisplayGitHubPullRequests", cbGitHubPullRequest.Checked);
            buildServerConfig.SetString("AppVeyorGitHubToken", txtGitHubToken.Text);
        }

        private void cbGitHubPullRequest_CheckedChanged(object sender, System.EventArgs e)
        {
            txtGitHubToken.Enabled = cbGitHubPullRequest.Checked;
        }
    }
}
