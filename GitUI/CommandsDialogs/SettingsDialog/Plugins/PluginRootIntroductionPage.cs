using GitCommands;
using GitUIPluginInterfaces;

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

        protected override ISettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
