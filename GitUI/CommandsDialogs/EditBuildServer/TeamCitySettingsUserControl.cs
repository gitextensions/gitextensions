using System.Windows.Forms;
using Nini.Config;

namespace GitUI.CommandsDialogs.EditBuildServer
{
    public partial class TeamCitySettingsUserControl : GitExtensionsControl
    {
        private readonly string _defaultProjectName;

        public TeamCitySettingsUserControl(string defaultProjectName)
        {
            _defaultProjectName = defaultProjectName;
            InitializeComponent();
            Translate();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
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
