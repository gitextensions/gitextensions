using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class ConfirmationsSettingsPage : SettingsPageWithHeader
{
    public ConfirmationsSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public ConfirmationsSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        // Commits:
        chkAmend.IsChecked = !AppSettings.DontConfirmAmend.Value;
        chkUndoLastCommitConfirmation.IsChecked = !AppSettings.DontConfirmUndoLastCommit.Value;
        chkCommitIfNoBranch.IsChecked = !AppSettings.DontConfirmCommitIfNoBranch;
        chkRebaseOnTopOfSelectedCommit.IsChecked = !AppSettings.DontConfirmRebase.Value;

        // Branches:
        chkFetchAndPruneAllConfirmation.IsChecked = !AppSettings.DontConfirmFetchAndPruneAll.Value;
        chkPushNewBranch.IsChecked = !AppSettings.DontConfirmPushNewBranch.Value;
        chkAddTrackingRef.IsChecked = !AppSettings.DontConfirmAddTrackingRef;
        chkBranchDeleteUnmerged.IsChecked = !AppSettings.DontConfirmDeleteUnmergedBranch.Value;
        chkBranchCheckoutConfirmation.IsChecked = AppSettings.ConfirmBranchCheckout.Value;

        // Stashes:
        chkAutoPopStashAfterPull.IsChecked = ToCheckboxStateInverted(AppSettings.AutoPopStashAfterPull);
        chkAutoPopStashAfterCheckout.IsChecked = ToCheckboxStateInverted(AppSettings.AutoPopStashAfterCheckoutBranch);
        chkConfirmStashDrop.IsChecked = !AppSettings.DontConfirmStashDrop;

        // Conflict resolution:
        chkResolveConflicts.IsChecked = !AppSettings.DontConfirmResolveConflicts.Value;
        chkCommitAfterConflictsResolved.IsChecked = !AppSettings.DontConfirmCommitAfterConflictsResolved.Value;
        chkSecondAbortConfirmation.IsChecked = !AppSettings.DontConfirmSecondAbortConfirmation.Value;

        // Submodules:
        chkUpdateModules.IsChecked = ToCheckboxStateInverted(AppSettings.DontConfirmUpdateSubmodulesOnCheckout);

        // Worktrees:
        chkSwitchWorktree.IsChecked = !AppSettings.DontConfirmSwitchWorktree.Value;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        // Commits:
        AppSettings.DontConfirmAmend.Value = chkAmend.IsChecked != true;
        AppSettings.DontConfirmUndoLastCommit.Value = chkUndoLastCommitConfirmation.IsChecked != true;
        AppSettings.DontConfirmCommitIfNoBranch = chkCommitIfNoBranch.IsChecked != true;
        AppSettings.DontConfirmRebase.Value = chkRebaseOnTopOfSelectedCommit.IsChecked != true;

        // Branches:
        AppSettings.DontConfirmFetchAndPruneAll.Value = chkFetchAndPruneAllConfirmation.IsChecked != true;
        AppSettings.DontConfirmPushNewBranch.Value = chkPushNewBranch.IsChecked != true;
        AppSettings.DontConfirmAddTrackingRef = chkAddTrackingRef.IsChecked != true;
        AppSettings.DontConfirmDeleteUnmergedBranch.Value = chkBranchDeleteUnmerged.IsChecked != true;
        AppSettings.ConfirmBranchCheckout.Value = chkBranchCheckoutConfirmation.IsChecked == true;

        // Stashes:
        AppSettings.AutoPopStashAfterPull = ToBooleanInverted(chkAutoPopStashAfterPull.IsChecked);
        AppSettings.AutoPopStashAfterCheckoutBranch = ToBooleanInverted(chkAutoPopStashAfterCheckout.IsChecked);
        AppSettings.DontConfirmStashDrop = chkConfirmStashDrop.IsChecked != true;

        // Conflict resolution:
        AppSettings.DontConfirmResolveConflicts.Value = chkResolveConflicts.IsChecked != true;
        AppSettings.DontConfirmCommitAfterConflictsResolved.Value = chkCommitAfterConflictsResolved.IsChecked != true;
        AppSettings.DontConfirmSecondAbortConfirmation.Value = chkSecondAbortConfirmation.IsChecked != true;

        // Submodules:
        AppSettings.DontConfirmUpdateSubmodulesOnCheckout = ToBooleanInverted(chkUpdateModules.IsChecked);

        // Worktrees:
        AppSettings.DontConfirmSwitchWorktree.Value = chkSwitchWorktree.IsChecked != true;

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(ConfirmationsSettingsPage));

    private static bool? ToCheckboxStateInverted(bool? booleanValue)
        => booleanValue.HasValue ? !booleanValue.Value : null;

    private static bool? ToBooleanInverted(bool? state)
        => state.HasValue ? !state.Value : null;

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ConfirmationsSettingsPage page)
    {
        public CheckBox Amend => page.chkAmend;

        public CheckBox UndoLastCommit => page.chkUndoLastCommitConfirmation;

        public CheckBox CommitIfNoBranch => page.chkCommitIfNoBranch;

        public CheckBox RebaseOnTop => page.chkRebaseOnTopOfSelectedCommit;

        public CheckBox FetchAndPruneAll => page.chkFetchAndPruneAllConfirmation;

        public CheckBox PushNewBranch => page.chkPushNewBranch;

        public CheckBox AddTrackingRef => page.chkAddTrackingRef;

        public CheckBox DeleteUnmergedBranch => page.chkBranchDeleteUnmerged;

        public CheckBox BranchCheckout => page.chkBranchCheckoutConfirmation;

        public CheckBox AutoPopStashAfterCheckout => page.chkAutoPopStashAfterCheckout;

        public CheckBox AutoPopStashAfterPull => page.chkAutoPopStashAfterPull;

        public CheckBox ConfirmStashDrop => page.chkConfirmStashDrop;

        public CheckBox ResolveConflicts => page.chkResolveConflicts;

        public CheckBox CommitAfterConflictsResolved => page.chkCommitAfterConflictsResolved;

        public CheckBox SecondAbort => page.chkSecondAbortConfirmation;

        public CheckBox UpdateModules => page.chkUpdateModules;

        public CheckBox SwitchWorktree => page.chkSwitchWorktree;
    }
}
