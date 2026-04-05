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
        chkAmend.Checked = !AppSettings.DontConfirmAmend.Value;
        chkUndoLastCommitConfirmation.Checked = !AppSettings.DontConfirmUndoLastCommit.Value;
        chkCommitIfNoBranch.Checked = !AppSettings.DontConfirmCommitIfNoBranch.Value;
        chkRebaseOnTopOfSelectedCommit.Checked = !AppSettings.DontConfirmRebase.Value;

        // Branches:
        chkFetchAndPruneAllConfirmation.Checked = !AppSettings.DontConfirmFetchAndPruneAll.Value;
        chkPushNewBranch.Checked = !AppSettings.DontConfirmPushNewBranch.Value;
        chkAddTrackingRef.Checked = !AppSettings.DontConfirmAddTrackingRef.Value;
        chkBranchDeleteUnmerged.Checked = !AppSettings.DontConfirmDeleteUnmergedBranch.Value;
        chkBranchCheckoutConfirmation.Checked = AppSettings.ConfirmBranchCheckout.Value;

        // Stashes:
        chkAutoPopStashAfterPull.CheckState = ToCheckboxStateInverted(AppSettings.AutoPopStashAfterPull);
        chkAutoPopStashAfterCheckout.CheckState = ToCheckboxStateInverted(AppSettings.AutoPopStashAfterCheckoutBranch);
        chkConfirmStashDrop.Checked = !AppSettings.DontConfirmStashDrop;

        // Conflict resolution:
        chkResolveConflicts.Checked = !AppSettings.DontConfirmResolveConflicts.Value;
        chkCommitAfterConflictsResolved.Checked = !AppSettings.DontConfirmCommitAfterConflictsResolved.Value;
        chkSecondAbortConfirmation.Checked = !AppSettings.DontConfirmSecondAbortConfirmation.Value;

        // Submodules:
        chkUpdateModules.CheckState = ToCheckboxStateInverted(AppSettings.DontConfirmUpdateSubmodulesOnCheckout);

        // Worktrees:
        chkSwitchWorktree.Checked = !AppSettings.DontConfirmSwitchWorktree.Value;

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
        AppSettings.DontConfirmStashDrop = !chkConfirmStashDrop.Checked;

        // Conflict resolution:
        AppSettings.DontConfirmResolveConflicts.Value = !chkResolveConflicts.Checked;
        AppSettings.DontConfirmCommitAfterConflictsResolved.Value = !chkCommitAfterConflictsResolved.Checked;
        AppSettings.DontConfirmSecondAbortConfirmation.Value = !chkSecondAbortConfirmation.Checked;

        // Submodules:
        AppSettings.DontConfirmUpdateSubmodulesOnCheckout = ToBooleanInverted(chkUpdateModules.CheckState);

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
