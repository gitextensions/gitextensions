using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;

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
        chkAmend.IsChecked = !AppSettings.DontConfirmAmend;
        chkUndoLastCommitConfirmation.IsChecked = !AppSettings.DontConfirmUndoLastCommit;
        chkCommitIfNoBranch.IsChecked = !AppSettings.DontConfirmCommitIfNoBranch;
        chkRebaseOnTopOfSelectedCommit.IsChecked = !AppSettings.DontConfirmRebase;

        // Branches:
        chkFetchAndPruneAllConfirmation.IsChecked = !AppSettings.DontConfirmFetchAndPruneAll;
        chkPushNewBranch.IsChecked = !AppSettings.DontConfirmPushNewBranch;
        chkAddTrackingRef.IsChecked = !AppSettings.DontConfirmAddTrackingRef;
        chkBranchDeleteUnmerged.IsChecked = !AppSettings.DontConfirmDeleteUnmergedBranch;
        chkBranchCheckoutConfirmation.IsChecked = AppSettings.ConfirmBranchCheckout.Value;

        // Stashes:
        chkAutoPopStashAfterPull.IsChecked = ToCheckboxStateInverted(AppSettings.AutoPopStashAfterPull);
        chkAutoPopStashAfterCheckout.IsChecked = ToCheckboxStateInverted(AppSettings.AutoPopStashAfterCheckoutBranch);
        chkConfirmStashDrop.IsChecked = !AppSettings.DontConfirmStashDrop;

        // Conflict resolution:
        chkResolveConflicts.IsChecked = !AppSettings.DontConfirmResolveConflicts;
        chkCommitAfterConflictsResolved.IsChecked = !AppSettings.DontConfirmCommitAfterConflictsResolved;
        chkSecondAbortConfirmation.IsChecked = !AppSettings.DontConfirmSecondAbortConfirmation;

        // Submodules:
        chkUpdateModules.IsChecked = ToCheckboxStateInverted(AppSettings.DontConfirmUpdateSubmodulesOnCheckout);

        // Worktrees:
        chkSwitchWorktree.IsChecked = !AppSettings.DontConfirmSwitchWorktree;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        // Commits:
        AppSettings.DontConfirmAmend = chkAmend.IsChecked != true;
        AppSettings.DontConfirmUndoLastCommit = chkUndoLastCommitConfirmation.IsChecked != true;
        AppSettings.DontConfirmCommitIfNoBranch = chkCommitIfNoBranch.IsChecked != true;
        AppSettings.DontConfirmRebase = chkRebaseOnTopOfSelectedCommit.IsChecked != true;

        // Branches:
        AppSettings.DontConfirmFetchAndPruneAll = chkFetchAndPruneAllConfirmation.IsChecked != true;
        AppSettings.DontConfirmPushNewBranch = chkPushNewBranch.IsChecked != true;
        AppSettings.DontConfirmAddTrackingRef = chkAddTrackingRef.IsChecked != true;
        AppSettings.DontConfirmDeleteUnmergedBranch = chkBranchDeleteUnmerged.IsChecked != true;
        AppSettings.ConfirmBranchCheckout.Value = chkBranchCheckoutConfirmation.IsChecked == true;

        // Stashes:
        AppSettings.AutoPopStashAfterPull = ToBooleanInverted(chkAutoPopStashAfterPull.IsChecked);
        AppSettings.AutoPopStashAfterCheckoutBranch = ToBooleanInverted(chkAutoPopStashAfterCheckout.IsChecked);
        AppSettings.DontConfirmStashDrop = chkConfirmStashDrop.IsChecked != true;

        // Conflict resolution:
        AppSettings.DontConfirmResolveConflicts = chkResolveConflicts.IsChecked != true;
        AppSettings.DontConfirmCommitAfterConflictsResolved = chkCommitAfterConflictsResolved.IsChecked != true;
        AppSettings.DontConfirmSecondAbortConfirmation = chkSecondAbortConfirmation.IsChecked != true;

        // Submodules:
        AppSettings.DontConfirmUpdateSubmodulesOnCheckout = ToBooleanInverted(chkUpdateModules.IsChecked);

        // Worktrees:
        AppSettings.DontConfirmSwitchWorktree = chkSwitchWorktree.IsChecked != true;

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(ConfirmationsSettingsPage));

    private static bool? ToCheckboxStateInverted(bool? booleanValue)
        => booleanValue.HasValue ? !booleanValue.Value : null;

    private static bool? ToBooleanInverted(bool? state)
        => state.HasValue ? !state.Value : null;

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(
            nameof(ConfirmationsSettingsPage),
            "$this",
            "Text",
            Text ?? "Confirmations");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string neutralText = Text ?? "Confirmations";
        Text = translation.TranslateItem(
            nameof(ConfirmationsSettingsPage),
            "$this",
            "Text",
            () => neutralText) ?? neutralText;
    }

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
