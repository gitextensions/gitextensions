using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitCommands.Settings;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace TeamCityIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata("TeamCity")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TeamCitySettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private string _defaultProjectName;

        public TeamCitySettingsUserControl()
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
                TeamCityServerUrl.Text = buildServerConfig.GetString("BuildServerUrl", string.Empty);
                TeamCityProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
            }
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            buildServerConfig.SetString("BuildServerUrl", TeamCityServerUrl.Text);
            buildServerConfig.SetString("ProjectName", TeamCityProjectName.Text);
        }
    }
}
