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
            InitializeComplete();
        }

        protected override void SettingsToPage()
        {
            chkAmend.Checked = AppSettings.DontConfirmAmend;
            chkCommitIfNoBranch.Checked = AppSettings.DontConfirmCommitIfNoBranch;
            chkAutoPopStashAfterPull.CheckState = AppSettings.AutoPopStashAfterPull.ToCheckboxState();
            chkAutoPopStashAfterCheckout.CheckState = AppSettings.AutoPopStashAfterCheckoutBranch.ToCheckboxState();
            chkConfirmStashDrop.Checked = !AppSettings.StashConfirmDropShow;
            chkPushNewBranch.Checked = AppSettings.DontConfirmPushNewBranch;
            chkAddTrackingRef.Checked = AppSettings.DontConfirmAddTrackingRef;
            chkUpdateModules.CheckState = AppSettings.UpdateSubmodulesOnCheckout.ToCheckboxState();
            chkResolveConflicts.Checked = AppSettings.DontConfirmResolveConflicts;
            chkCommitAfterConflictsResolved.Checked = AppSettings.DontConfirmCommitAfterConflictsResolved;
            chkSecondAbortConfirmation.Checked = AppSettings.DontConfirmSecondAbortConfirmation;
            chkRebaseOnTopOfSelectedCommit.Checked = AppSettings.DontConfirmRebase;
            chkUndoLastCommitConfirmation.Checked = AppSettings.DontConfirmUndoLastCommit;
            chkFetchAndPruneAllConfirmation.Checked = AppSettings.DontConfirmFetchAndPruneAll;
        }

        protected override void PageToSettings()
        {
            AppSettings.DontConfirmAmend = chkAmend.Checked;
            AppSettings.DontConfirmCommitIfNoBranch = chkCommitIfNoBranch.Checked;
            AppSettings.AutoPopStashAfterPull = chkAutoPopStashAfterPull.CheckState.ToBoolean();
            AppSettings.AutoPopStashAfterCheckoutBranch = chkAutoPopStashAfterCheckout.CheckState.ToBoolean();
            AppSettings.StashConfirmDropShow = !chkConfirmStashDrop.Checked;
            AppSettings.DontConfirmPushNewBranch = chkPushNewBranch.Checked;
            AppSettings.DontConfirmAddTrackingRef = chkAddTrackingRef.Checked;
            AppSettings.UpdateSubmodulesOnCheckout = chkUpdateModules.CheckState.ToBoolean();
            AppSettings.DontConfirmResolveConflicts = chkResolveConflicts.Checked;
            AppSettings.DontConfirmCommitAfterConflictsResolved = chkCommitAfterConflictsResolved.Checked;
            AppSettings.DontConfirmSecondAbortConfirmation = chkSecondAbortConfirmation.Checked;
            AppSettings.DontConfirmRebase = chkRebaseOnTopOfSelectedCommit.Checked;
            AppSettings.DontConfirmUndoLastCommit = chkUndoLastCommitConfirmation.Checked;
            AppSettings.DontConfirmFetchAndPruneAll = chkFetchAndPruneAllConfirmation.Checked;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(ConfirmationsSettingsPage));
        }
    }

    public static class CheckboxExtension
    {
        public static CheckState ToCheckboxState(this bool? booleanValue)
        {
            if (!booleanValue.HasValue)
            {
                return CheckState.Indeterminate;
            }

            return booleanValue == true ? CheckState.Checked : CheckState.Unchecked;
        }

        public static bool? ToBoolean(this CheckState state)
        {
            if (state == CheckState.Indeterminate)
            {
                return null;
            }

            return state == CheckState.Checked;
        }
    }
}
