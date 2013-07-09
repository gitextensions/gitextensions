namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class PluginRootIntroductionPage : SettingsPageBase
    {
        public PluginRootIntroductionPage()
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
