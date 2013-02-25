namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class HotkeysSettingsPage : SettingsPageBase
    {
        public HotkeysSettingsPage()
        {
            InitializeComponent();
            Text = "Hotkeys";
            Translate();
        }

        protected override void OnLoadSettings()
        {
            controlHotkeys.ReloadSettings();
        }

        public override void SaveSettings()
        {
            controlHotkeys.SaveSettings();
        }
    }
}
