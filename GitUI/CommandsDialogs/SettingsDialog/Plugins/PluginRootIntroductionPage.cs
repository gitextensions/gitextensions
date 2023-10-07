using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class PluginRootIntroductionPage : SettingsPageBase
    {
        public PluginRootIntroductionPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
            InitializeComplete();
        }

        protected override ISettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
