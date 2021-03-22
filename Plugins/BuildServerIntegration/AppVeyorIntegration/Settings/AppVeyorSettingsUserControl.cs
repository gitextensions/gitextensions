using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace AppVeyorIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(AppVeyorAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class AppVeyorSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string? _defaultProjectName;

        public AppVeyorSettingsUserControl()
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
            AppVeyorProjectName.Text = buildServerConfig.GetValue("AppVeyorProjectName", _defaultProjectName ?? string.Empty);
            AppVeyorAccountName.Text = buildServerConfig.GetValue("AppVeyorAccountName", string.Empty);
            AppVeyorAccountToken.Text = buildServerConfig.GetValue("AppVeyorAccountToken", string.Empty);
            cbLoadTestResults.Checked = buildServerConfig.GetValue("AppVeyorLoadTestsResults", false);
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            buildServerConfig.SetValue("AppVeyorProjectName", AppVeyorProjectName.Text);
            buildServerConfig.SetValue("AppVeyorAccountName", AppVeyorAccountName.Text);
            buildServerConfig.SetValue("AppVeyorAccountToken", AppVeyorAccountToken.Text);
            buildServerConfig.SetValue("AppVeyorLoadTestsResults", cbLoadTestResults.Checked);
        }
    }
}
