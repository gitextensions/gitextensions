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
            // Commits:
            chkAmend.Checked = !AppSettings.DontConfirmAmend;
            chkUndoLastCommitConfirmation.Checked = !AppSettings.DontConfirmUndoLastCommit;
            chkCommitIfNoBranch.Checked = !AppSettings.DontConfirmCommitIfNoBranch;
            chkRebaseOnTopOfSelectedCommit.Checked = !AppSettings.DontConfirmRebase;

            // Branches:
            chkFetchAndPruneAllConfirmation.Checked = !AppSettings.DontConfirmFetchAndPruneAll;
            chkPushNewBranch.Checked = !AppSettings.DontConfirmPushNewBranch;
            chkAddTrackingRef.Checked = !AppSettings.DontConfirmAddTrackingRef;
            chkBranchDeleteUnmerged.Checked = !AppSettings.DontConfirmDeleteUnmergedBranch;

            // Stashes:
            chkAutoPopStashAfterPull.CheckState = ToCheckboxStateInverted(AppSettings.AutoPopStashAfterPull);
            chkAutoPopStashAfterCheckout.CheckState = ToCheckboxStateInverted(AppSettings.AutoPopStashAfterCheckoutBranch);
            chkConfirmStashDrop.Checked = !AppSettings.DontConfirmStashDrop;

            // Conflict resolution:
            chkResolveConflicts.Checked = !AppSettings.DontConfirmResolveConflicts;
            chkCommitAfterConflictsResolved.Checked = !AppSettings.DontConfirmCommitAfterConflictsResolved;
            chkSecondAbortConfirmation.Checked = !AppSettings.DontConfirmSecondAbortConfirmation;

            // Submodules:
            chkUpdateModules.CheckState = ToCheckboxStateInverted(AppSettings.DontConfirmUpdateSubmodulesOnCheckout);

            // Worktrees:
            chkSwitchWorktree.Checked = !AppSettings.DontConfirmSwitchWorktree;

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            // Commits:
            AppSettings.DontConfirmAmend = !chkAmend.Checked;
            AppSettings.DontConfirmUndoLastCommit = !chkUndoLastCommitConfirmation.Checked;
            AppSettings.DontConfirmCommitIfNoBranch = !chkCommitIfNoBranch.Checked;
            AppSettings.DontConfirmRebase = !chkRebaseOnTopOfSelectedCommit.Checked;

            // Branches:
            AppSettings.DontConfirmFetchAndPruneAll = !chkFetchAndPruneAllConfirmation.Checked;
            AppSettings.DontConfirmPushNewBranch = !chkPushNewBranch.Checked;
            AppSettings.DontConfirmAddTrackingRef = !chkAddTrackingRef.Checked;
            AppSettings.DontConfirmDeleteUnmergedBranch = !chkBranchDeleteUnmerged.Checked;

            // Stashes:
            AppSettings.AutoPopStashAfterPull = ToBooleanInverted(chkAutoPopStashAfterPull.CheckState);
            AppSettings.AutoPopStashAfterCheckoutBranch = ToBooleanInverted(chkAutoPopStashAfterCheckout.CheckState);
            AppSettings.DontConfirmStashDrop = !chkConfirmStashDrop.Checked;

            // Conflict resolution:
            AppSettings.DontConfirmResolveConflicts = !chkResolveConflicts.Checked;
            AppSettings.DontConfirmCommitAfterConflictsResolved = !chkCommitAfterConflictsResolved.Checked;
            AppSettings.DontConfirmSecondAbortConfirmation = !chkSecondAbortConfirmation.Checked;

            // Submodules:
            AppSettings.DontConfirmUpdateSubmodulesOnCheckout = ToBooleanInverted(chkUpdateModules.CheckState);

            // Worktrees:
            AppSettings.DontConfirmSwitchWorktree = !chkSwitchWorktree.Checked;

            base.PageToSettings();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(ConfirmationsSettingsPage));
        }

        private static CheckState ToCheckboxStateInverted(bool? booleanValue)
        {
            if (!booleanValue.HasValue)
            {
                return CheckState.Indeterminate;
            }

            return booleanValue == false ? CheckState.Checked : CheckState.Unchecked;
        }

        private static bool? ToBooleanInverted(CheckState state)
        {
            if (state == CheckState.Indeterminate)
            {
                return null;
            }

            return state != CheckState.Checked;
        }
    }
}
