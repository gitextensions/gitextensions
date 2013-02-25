namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class StartPageSettingsPage : SettingsPageBase
    {
        public StartPageSettingsPage()
        {
            InitializeComponent();
            Text = "Start Page";
            Translate();
        }

        public override bool IsInstantSavePage
        {
            get
            {
                return true;
            }
        }
    }
}
