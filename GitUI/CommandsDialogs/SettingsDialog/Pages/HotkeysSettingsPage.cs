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

        protected override void SettingsToPage()
        {
            controlHotkeys.ReloadSettings();

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            controlHotkeys.SaveSettings();

            base.PageToSettings();
        }
    }
}
