using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class HotkeysSettingsPage : SettingsPageWithHeader
    {
        private readonly IScriptsManager _scriptsManager;

        public HotkeysSettingsPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _scriptsManager = serviceProvider.GetRequiredService<IScriptsManager>();

            InitializeComponent();
            InitializeComplete();
        }

        protected override void SettingsToPage()
        {
            controlHotkeys.ReloadSettings(_scriptsManager);

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            controlHotkeys.SaveSettings();

            base.PageToSettings();
        }
    }
}
