using GitCommands;
using GitExtensions.Core.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class PluginRootIntroductionPage : SettingsPageBase
    {
        public PluginRootIntroductionPage()
        {
            InitializeComponent();
            Text = "Plugins Settings";
            InitializeComplete();
        }

        protected override void SettingsToPage()
        {
        }

        protected override void PageToSettings()
        {
        }

        protected override ISettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
