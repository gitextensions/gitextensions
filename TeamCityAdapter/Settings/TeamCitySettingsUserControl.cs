using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitUI;
using GitUIPluginInterfaces.BuildServerIntegration;
using Nini.Config;

namespace TeamCityAdapter.Settings
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

        public void LoadSettings(IConfig buildServerConfig)
        {
            if (buildServerConfig != null)
            {
                TeamCityServerUrl.Text = buildServerConfig.GetString("BuildServerUrl");
                TeamCityProjectName.Text = buildServerConfig.GetString("ProjectName", _defaultProjectName);
            }
        }

        public void SaveSettings(IConfig buildServerConfig)
        {
            buildServerConfig.Set("BuildServerUrl", TeamCityServerUrl.Text);
            buildServerConfig.Set("ProjectName", TeamCityProjectName.Text);
        }
    }
}
