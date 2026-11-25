using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static BooleanSetting AutoNormaliseBranchName { get; } = new(RootSettingsPath, nameof(AutoNormaliseBranchName), defaultValue: true);

    public static BoolRuntimeSetting BranchFilterEnabled { get; } = new(RootSettingsPath, nameof(BranchFilterEnabled), defaultValue: false);

    public static EnumRuntimeSetting<DiffDisplayAppearance> DiffDisplayAppearance { get; } = new(RootSettingsPath, nameof(DiffDisplayAppearance), defaultValue: Settings.DiffDisplayAppearance.Patch);

    public static BooleanSetting DontConfirmAddTrackingRef { get; } = new(RootSettingsPath, nameof(DontConfirmAddTrackingRef), defaultValue: false);

    public static BooleanSetting DontConfirmAmend { get; } = new(RootSettingsPath, nameof(DontConfirmAmend), defaultValue: false);

    public static BooleanSetting DontConfirmCommitAfterConflictsResolved { get; } = new(RootSettingsPath, nameof(DontConfirmCommitAfterConflictsResolved), defaultValue: false);

    public static BooleanSetting DontConfirmCommitIfNoBranch { get; } = new(RootSettingsPath, nameof(DontConfirmCommitIfNoBranch), defaultValue: false);

    public static BooleanSetting DontConfirmDeleteUnmergedBranch { get; } = new(RootSettingsPath, nameof(DontConfirmDeleteUnmergedBranch), defaultValue: false);

    public static BooleanSetting DontConfirmFetchAndPruneAll { get; } = new(RootSettingsPath, nameof(DontConfirmFetchAndPruneAll), defaultValue: false);

    public static BooleanSetting DontConfirmPushNewBranch { get; } = new(RootSettingsPath, nameof(DontConfirmPushNewBranch), defaultValue: false);

    public static BooleanSetting DontConfirmRebase { get; } = new(RootSettingsPath, nameof(DontConfirmRebase), defaultValue: false);

    public static BooleanSetting DontConfirmResolveConflicts { get; } = new(RootSettingsPath, nameof(DontConfirmResolveConflicts), defaultValue: false);

    public static BooleanSetting DontConfirmSecondAbortConfirmation { get; } = new(RootSettingsPath, nameof(DontConfirmSecondAbortConfirmation), defaultValue: false);

    // History: The setting had originally been called 'StashConfirmDropShow', and then it was inverted.
    // To maintain the compatibility with the existing user settings, negate the retrieved value.
    public static InvertedBooleanSetting DontConfirmStashDrop { get; } = new(RootSettingsPath, "stashconfirmdropshow", defaultValue: false);

    public static BooleanSetting DontConfirmSwitchWorktree { get; } = new(RootSettingsPath, nameof(DontConfirmSwitchWorktree), defaultValue: false);

    public static BooleanSetting DontConfirmUndoLastCommit { get; } = new(RootSettingsPath, nameof(DontConfirmUndoLastCommit), defaultValue: false);

    public static NullableBooleanSetting DontConfirmUpdateSubmodulesOnCheckout { get; } = new(RootSettingsPath, "dontConfirmUpdateSubmodulesOnCheckout");

    public static EnumRuntimeSetting<IgnoreWhitespaceKind> IgnoreWhitespaceKind { get; } = new(RootSettingsPath, nameof(IgnoreWhitespaceKind), defaultValue: Settings.IgnoreWhitespaceKind.None);

    public static EnumRuntimeSetting<RevisionSortOrder> RevisionSortOrder { get; } = new(RootSettingsPath, nameof(RevisionSortOrder), defaultValue: GitCommands.RevisionSortOrder.GitDefault);

    public static BoolRuntimeSetting ShowCurrentBranchOnly { get; } = new(RootSettingsPath, nameof(ShowCurrentBranchOnly), defaultValue: false);

    public static BoolRuntimeSetting ShowEntireFile { get; } = new(RootSettingsPath, nameof(ShowEntireFile), defaultValue: false);

    public static BoolRuntimeSetting ShowNonPrintingChars { get; } = new(RootSettingsPath, nameof(ShowNonPrintingChars), defaultValue: false);

    public static BoolRuntimeSetting ShowReflogReferences { get; } = new(RootSettingsPath, nameof(ShowReflogReferences), defaultValue: false);

    public static BoolRuntimeSetting ShowSyntaxHighlightingInDiff { get; } = new(RootSettingsPath, nameof(ShowSyntaxHighlightingInDiff), defaultValue: true);

    public static BooleanSetting UseConsoleEmulatorForCommands { get; } = new(RootSettingsPath, nameof(UseConsoleEmulatorForCommands), defaultValue: true);
}
