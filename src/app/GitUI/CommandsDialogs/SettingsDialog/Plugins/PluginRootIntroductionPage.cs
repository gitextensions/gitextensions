using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class PluginRootIntroductionPage : SettingsPageBase
    {
        public PluginRootIntroductionPage(IServiceProvider serviceProvider, ISettingsPageHost pageHost)
            : base(serviceProvider, pageHost)
        {
            InitializeComponent();
            InitializeComplete();
        }

        protected override SettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
