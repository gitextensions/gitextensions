using Nini.Config;

namespace GitUI.CommandsDialogs.EditBuildServer
{
    public partial class TeamCitySettingsUserControl : GitExtensionsControl
    {
        public TeamCitySettingsUserControl()
        {
            InitializeComponent();
            Translate();
        }

        public void LoadSettings(IConfig buildServerConfig)
        {
            if (buildServerConfig != null)
            {
                TeamCityServerUrl.Text = buildServerConfig.GetString("BuildServerUrl");
            }
        }

        public void SaveSettings(IConfig buildServerConfig)
        {
            buildServerConfig.Set("BuildServerUrl", TeamCityServerUrl.Text);
        }
    }
}
