using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ConfirmationsSettingsPage : SettingsPageWithHeader
    {
        public ConfirmationsSettingsPage()
        {
            InitializeComponent();
            Text = "Confirmations";
            Translate();
        }

        protected override void SettingsToPage()
        {
            chkAmend.Checked = AppSettings.DontConfirmAmend;
            chkAutoPopStashAfterPull.CheckState = AppSettings.AutoPopStashAfterPull.ToCheckboxState();
            chkAutoPopStashAfterCheckout.CheckState = AppSettings.AutoPopStashAfterCheckoutBranch.ToCheckboxState();
            chkPushNewBranch.Checked = AppSettings.DontConfirmPushNewBranch;
            chkAddTrackingRef.Checked = AppSettings.DontConfirmAddTrackingRef;
            chkUpdateModules.CheckState = AppSettings.UpdateSubmodulesOnCheckout.ToCheckboxState();
        }

        protected override void PageToSettings()
        {
            AppSettings.DontConfirmAmend = chkAmend.Checked;
            AppSettings.AutoPopStashAfterPull = chkAutoPopStashAfterPull.CheckState.ToBoolean();
            AppSettings.AutoPopStashAfterCheckoutBranch = chkAutoPopStashAfterCheckout.CheckState.ToBoolean();
            AppSettings.DontConfirmPushNewBranch = chkPushNewBranch.Checked;
            AppSettings.DontConfirmAddTrackingRef = chkAddTrackingRef.Checked;
            AppSettings.UpdateSubmodulesOnCheckout = chkUpdateModules.CheckState.ToBoolean();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(ConfirmationsSettingsPage));
        }
    }

    public static class CheckboxExtension
    {
        public static CheckState ToCheckboxState(this bool booleanValue)
        {
            return booleanValue.ToCheckboxState();
        }

        public static CheckState ToCheckboxState(this bool? booleanValue)
        {
            if (!booleanValue.HasValue)
                return CheckState.Indeterminate;
            return booleanValue == true ? CheckState.Checked : CheckState.Unchecked;
        }

        public static bool? ToBoolean(this CheckState state)
        {
            if (state == CheckState.Indeterminate)
                return null;
            return state == CheckState.Checked;
        }
    }
}
