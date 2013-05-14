namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class PluginRootIntroductionPage : SettingsPageBase
    {
        public PluginRootIntroductionPage(CommonLogic aCommonLogic)
            : base(aCommonLogic)
        {
            InitializeComponent();
            Text = "Plugins Settings";
            Translate();
        }

        protected override void SettingsToPage()
        {            
        }

        protected override void PageToSettings()
        {
        }
    }
}
