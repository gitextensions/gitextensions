using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace GitlabIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(GitlabAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class GitlabSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string _defaultProjectName;

        public GitlabSettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        public void Initialize(string defaultProjectName, IEnumerable<string> remotes)
        {
            _defaultProjectName = defaultProjectName;
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            if (buildServerConfig != null)
            {
                GitlabServerUrl.Text = buildServerConfig.GetString("BuildServerUrl", string.Empty);
                GitlabProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
                GitlabApiToken.Text = buildServerConfig.GetString("GitlabApiToken", string.Empty);
            }
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            buildServerConfig.SetString("BuildServerUrl", GitlabServerUrl.Text);
            buildServerConfig.SetString("ProjectName", GitlabProjectName.Text);
            buildServerConfig.SetString("GitlabApiToken", GitlabApiToken.Text);
        }
    }
}
