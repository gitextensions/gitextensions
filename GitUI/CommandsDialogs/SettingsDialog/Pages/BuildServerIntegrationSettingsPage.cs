using System.Linq;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class BuildServerIntegrationSettingsPage : SettingsPageBase
    {
        public BuildServerIntegrationSettingsPage()
        {
            InitializeComponent();
            Text = "Build Server Integration";
            Translate();
        }

        public override void OnPageShown()
        {
            BuildServerType.SelectedItem = Settings.ActiveBuildServerType;

            ActivateBuildServerSettingsControl();
        }

        private void ActivateBuildServerSettingsControl()
        {
            var previousControl = buildServerSettingsPanel.Controls.Count > 0 ? buildServerSettingsPanel.Controls[0] : null;
            if (previousControl != null) previousControl.Dispose();

            var control = CreateBuildServerSettingsUserControl();

            buildServerSettingsPanel.Controls.Clear();

            if (control != null)
            {
                control.OnPageShown();

                buildServerSettingsPanel.Controls.Add(control);
            }
        }

        private TeamCitySettingsUserControl CreateBuildServerSettingsUserControl()
        {
            switch (GetSelectedBuildServerType())
            {
                case Settings.BuildServerType.TeamCity:
                    return new TeamCitySettingsUserControl();
                default:
                    return null;
            }
        }

        protected override void OnLoadSettings()
        {
            BuildServerType.SelectedIndex = (int)Settings.ActiveBuildServerType;

            var control = buildServerSettingsPanel.Controls.Cast<ISettingsPage>().SingleOrDefault();
            if (control != null) control.LoadSettings();
        }

        public override void SaveSettings()
        {
            Settings.ActiveBuildServerType = GetSelectedBuildServerType();

            var control = buildServerSettingsPanel.Controls.Cast<ISettingsPage>().SingleOrDefault();
            if (control != null) control.SaveSettings();
        }

        private Settings.BuildServerType GetSelectedBuildServerType()
        {
            return (Settings.BuildServerType)BuildServerType.SelectedIndex;
        }

        private void BuildServerType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ActivateBuildServerSettingsControl();
        }
    }
}
