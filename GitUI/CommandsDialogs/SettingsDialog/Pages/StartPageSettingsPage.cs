namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class StartPageSettingsPage : SettingsPageWithHeader
    {
        public StartPageSettingsPage()
        {
            InitializeComponent();
            Text = "Start Page";
            Translate();
        }

        public override bool IsInstantSavePage => true;

        protected override void SettingsToPage()
        {
        }

        protected override void PageToSettings()
        {
        }
    }
}
