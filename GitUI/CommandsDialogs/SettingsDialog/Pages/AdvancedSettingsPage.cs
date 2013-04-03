using GitCommands;
using GitCommands.Properties;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class AdvancedSettingsPage : SettingsPageBase
    {
        public AdvancedSettingsPage()
        {
            InitializeComponent();
            Text = "Advanced";
            Translate();
        }

        protected override void OnLoadSettings()
        {
            chkAlwaysShowCheckoutDlg.Checked = Settings.Default.AlwaysShowCheckoutBranchDlg;
            chkUseLocalChangesAction.Checked = Settings.Default.UseDefaultCheckoutBranchAction;
            chkDontSHowHelpImages.Checked = Settings.Default.DontShowHelpImages;
        }

        public override void SaveSettings()
        {
            Settings.Default.AlwaysShowCheckoutBranchDlg = chkAlwaysShowCheckoutDlg.Checked;
            Settings.Default.UseDefaultCheckoutBranchAction = chkUseLocalChangesAction.Checked;
            Settings.Default.DontShowHelpImages = chkDontSHowHelpImages.Checked;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(AdvancedSettingsPage));
        }        
    }
}
