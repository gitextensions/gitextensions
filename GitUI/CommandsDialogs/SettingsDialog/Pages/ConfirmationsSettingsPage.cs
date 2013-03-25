using GitCommands;
using GitCommands.Properties;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ConfirmationsSettingsPage : SettingsPageBase
    {
        public ConfirmationsSettingsPage()
        {
            InitializeComponent();
            Text = "Confirmations";
            Translate();
        }

        protected override void OnLoadSettings()
        {
            chkAmend.Checked = Settings.Default.DontConfirmAmend;
            chkAutoPopStashAfterPull.CheckState = Settings.Default.AutoPopStashAfterPull.ToCheckboxState();
            chkAutoPopStashAfterCheckout.CheckState = Settings.Default.AutoPopStashAfterCheckoutBranch.ToCheckboxState();
            chkPushNewBranch.Checked = Settings.Default.DontConfirmPushNewBranch;
            chkAddTrackingRef.Checked = Settings.Default.DontConfirmAddTrackingRef;
        }

        public override void SaveSettings()
        {
            Settings.Default.DontConfirmAmend = chkAmend.Checked;
            Settings.Default.AutoPopStashAfterPull = chkAutoPopStashAfterPull.CheckState.ToBoolean();
            Settings.Default.AutoPopStashAfterCheckoutBranch = chkAutoPopStashAfterCheckout.CheckState.ToBoolean();
            Settings.Default.DontConfirmPushNewBranch = chkPushNewBranch.Checked;
            Settings.Default.DontConfirmAddTrackingRef = chkAddTrackingRef.Checked;
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
