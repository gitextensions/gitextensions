using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class ConfirmationsSettingsPage : SettingsPageWithHeader
{
    public ConfirmationsSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
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
        chkBranchCheckoutConfirmation.Checked = AppSettings.ConfirmBranchCheckout;

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
        AppSettings.DontConfirmAmend.Value = !chkAmend.Checked;
        AppSettings.DontConfirmUndoLastCommit.Value = !chkUndoLastCommitConfirmation.Checked;
        AppSettings.DontConfirmCommitIfNoBranch.Value = !chkCommitIfNoBranch.Checked;
        AppSettings.DontConfirmRebase.Value = !chkRebaseOnTopOfSelectedCommit.Checked;

        // Branches:
        AppSettings.DontConfirmFetchAndPruneAll.Value = !chkFetchAndPruneAllConfirmation.Checked;
        AppSettings.DontConfirmPushNewBranch.Value = !chkPushNewBranch.Checked;
        AppSettings.DontConfirmAddTrackingRef.Value = !chkAddTrackingRef.Checked;
        AppSettings.DontConfirmDeleteUnmergedBranch.Value = !chkBranchDeleteUnmerged.Checked;
        AppSettings.ConfirmBranchCheckout.Value = chkBranchCheckoutConfirmation.Checked;

        // Stashes:
        AppSettings.AutoPopStashAfterPull = ToBooleanInverted(chkAutoPopStashAfterPull.CheckState);
        AppSettings.AutoPopStashAfterCheckoutBranch = ToBooleanInverted(chkAutoPopStashAfterCheckout.CheckState);
        AppSettings.DontConfirmStashDrop.Value = !chkConfirmStashDrop.Checked;

        // Conflict resolution:
        AppSettings.DontConfirmResolveConflicts.Value = !chkResolveConflicts.Checked;
        AppSettings.DontConfirmCommitAfterConflictsResolved.Value = !chkCommitAfterConflictsResolved.Checked;
        AppSettings.DontConfirmSecondAbortConfirmation.Value = !chkSecondAbortConfirmation.Checked;

        // Submodules:
        AppSettings.DontConfirmUpdateSubmodulesOnCheckout.Value = ToBooleanInverted(chkUpdateModules.CheckState);

        // Worktrees:
        AppSettings.DontConfirmSwitchWorktree.Value = !chkSwitchWorktree.Checked;

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
