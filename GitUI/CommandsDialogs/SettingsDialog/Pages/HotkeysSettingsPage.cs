using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class HotkeysSettingsPage : SettingsPageWithHeader
    {
        public HotkeysSettingsPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
            InitializeComplete();
        }

        private IScriptsManager ScriptsManager
        {
            get
            {
                if (!TryGetUICommands(out IGitUICommands commands))
                {
                    throw new InvalidOperationException("IGitUICommands should have been assigned");
                }

                return commands.GetRequiredService<IScriptsManager>();
            }
        }

        protected override void SettingsToPage()
        {
            controlHotkeys.ReloadSettings(ScriptsManager);

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            controlHotkeys.SaveSettings();

            base.PageToSettings();
        }
    }
}
