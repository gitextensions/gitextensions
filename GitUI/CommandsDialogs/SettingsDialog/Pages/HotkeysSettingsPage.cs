namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class HotkeysSettingsPage : SettingsPageBase
    {
        public HotkeysSettingsPage(CommonLogic aCommonLogic)
            : base(aCommonLogic)
        {
            InitializeComponent();
            Text = "Hotkeys";
            Translate();
        }

        protected override void SettingsToPage()
        {
            controlHotkeys.ReloadSettings();
        }

        protected override void PageToSettings()
        {
            controlHotkeys.SaveSettings();
        }
    }
}
