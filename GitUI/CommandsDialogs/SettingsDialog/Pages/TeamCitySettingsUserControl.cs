using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class TeamCitySettingsUserControl : SettingsPageBase
    {
        public TeamCitySettingsUserControl()
        {
            InitializeComponent();
            Translate();
        }

        public void OnPageShown()
        {
            TeamCityServerUrl.Text = Settings.BuildServerUrl;
        }

        protected void OnLoadSettings()
        {
            TeamCityServerUrl.Text = Settings.BuildServerUrl;
        }

        public void SaveSettings()
        {
            Settings.BuildServerUrl = TeamCityServerUrl.Text;
        }
    }
}
