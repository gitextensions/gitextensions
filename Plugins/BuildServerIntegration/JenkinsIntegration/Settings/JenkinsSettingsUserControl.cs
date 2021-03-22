using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace JenkinsIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(JenkinsAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class JenkinsSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string? _defaultProjectName;

        public JenkinsSettingsUserControl()
        {
            InitializeComponent();
            InitializeComplete();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        public void Initialize(string defaultProjectName, IEnumerable<string?> remotes)
        {
            _defaultProjectName = defaultProjectName;
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            JenkinsServerUrl.Text = buildServerConfig.GetValue("BuildServerUrl", string.Empty);
            JenkinsProjectName.Text = buildServerConfig.GetValue("ProjectName", _defaultProjectName ?? string.Empty);
            IgnoreBuildBranch.Text = buildServerConfig.GetValue("IgnoreBuildBranch", string.Empty);
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            buildServerConfig.SetValue("BuildServerUrl", JenkinsServerUrl.Text);
            buildServerConfig.SetValue("ProjectName", JenkinsProjectName.Text);
            buildServerConfig.SetValue("IgnoreBuildBranch", IgnoreBuildBranch.Text);
        }
    }
}
