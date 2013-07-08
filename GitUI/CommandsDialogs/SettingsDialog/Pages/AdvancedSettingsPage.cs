using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class AdvancedSettingsPage : SettingsPageWithHeader
    {
        public AdvancedSettingsPage()
        {
            InitializeComponent();
            Text = "Advanced";
            Translate();
        }

        protected override void SettingsToPage()
        {
            chkAlwaysShowCheckoutDlg.Checked = AppSettings.AlwaysShowCheckoutBranchDlg;
            chkUseLocalChangesAction.Checked = AppSettings.UseDefaultCheckoutBranchAction;
            chkDontSHowHelpImages.Checked = AppSettings.DontShowHelpImages;
        }

        protected override void PageToSettings()
        {
            AppSettings.AlwaysShowCheckoutBranchDlg = chkAlwaysShowCheckoutDlg.Checked;
            AppSettings.UseDefaultCheckoutBranchAction = chkUseLocalChangesAction.Checked;
            AppSettings.DontShowHelpImages = chkDontSHowHelpImages.Checked;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(AdvancedSettingsPage));
        }        
    }
}
