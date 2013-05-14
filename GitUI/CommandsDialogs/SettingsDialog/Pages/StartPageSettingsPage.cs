namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class StartPageSettingsPage : SettingsPageBase
    {
        public StartPageSettingsPage(CommonLogic aCommonLogic)
            : base(aCommonLogic)
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

        protected override void SettingsToPage()
        {
        }

        protected override void PageToSettings()
        {
        }


    }
}
