using System.ComponentModel.Composition;
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
            JenkinsServerUrl.Text = buildServerConfig.GetString("BuildServerUrl", null);
            JenkinsProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
            IgnoreBuildBranch.Text = buildServerConfig.GetString("IgnoreBuildBranch", null);
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            buildServerConfig.SetString("BuildServerUrl", JenkinsServerUrl.Text.NullIfEmpty());
            buildServerConfig.SetString("ProjectName", JenkinsProjectName.Text.NullIfEmpty());

            // While an empty value is valid as as override for lower level settings,
            // the behaviour requiring that the "effective" value is set is considered a worse limitation.
            buildServerConfig.SetString("IgnoreBuildBranch", IgnoreBuildBranch.Text.NullIfEmpty());
        }
    }
}
