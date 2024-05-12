using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitRootIntroductionPage : SettingsPageBase
    {
        public GitRootIntroductionPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
            InitializeComplete();
        }

        protected override void SettingsToPage()
        {
        }

        protected override void PageToSettings()
        {
        }

        protected override SettingsSource GetCurrentSettings()
        {
            return AppSettings.SettingsContainer;
        }
    }
}
