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

        public override bool IsInstantSavePage
        {
            get
            {
                return true;
            }
        }
    }
}
