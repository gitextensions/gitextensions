using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static BooleanSetting AutoNormaliseBranchName { get; } = new(_rootSettingsPath, nameof(AutoNormaliseBranchName), defaultValue: true);

    public static BoolRuntimeSetting BranchFilterEnabled { get; } = new(_rootSettingsPath, nameof(BranchFilterEnabled), defaultValue: false);

    public static EnumRuntimeSetting<DiffDisplayAppearance> DiffDisplayAppearance { get; } = new(_rootSettingsPath, nameof(DiffDisplayAppearance), defaultValue: Settings.DiffDisplayAppearance.Patch);

    public static BooleanSetting DontConfirmAddTrackingRef { get; } = new(_rootSettingsPath, nameof(DontConfirmAddTrackingRef), defaultValue: false);

    public static BooleanSetting DontConfirmAmend { get; } = new(_rootSettingsPath, nameof(DontConfirmAmend), defaultValue: false);

    public static BooleanSetting DontConfirmCommitAfterConflictsResolved { get; } = new(_rootSettingsPath, nameof(DontConfirmCommitAfterConflictsResolved), defaultValue: false);

    public static BooleanSetting DontConfirmCommitIfNoBranch { get; } = new(_rootSettingsPath, nameof(DontConfirmCommitIfNoBranch), defaultValue: false);

    public static BooleanSetting DontConfirmDeleteUnmergedBranch { get; } = new(_rootSettingsPath, nameof(DontConfirmDeleteUnmergedBranch), defaultValue: false);

    public static BooleanSetting DontConfirmFetchAndPruneAll { get; } = new(_rootSettingsPath, nameof(DontConfirmFetchAndPruneAll), defaultValue: false);

    public static BooleanSetting DontConfirmPushNewBranch { get; } = new(_rootSettingsPath, nameof(DontConfirmPushNewBranch), defaultValue: false);

    public static BooleanSetting DontConfirmRebase { get; } = new(_rootSettingsPath, nameof(DontConfirmRebase), defaultValue: false);

    public static BooleanSetting DontConfirmResolveConflicts { get; } = new(_rootSettingsPath, nameof(DontConfirmResolveConflicts), defaultValue: false);

    public static BooleanSetting DontConfirmSecondAbortConfirmation { get; } = new(_rootSettingsPath, nameof(DontConfirmSecondAbortConfirmation), defaultValue: false);

    // History: The setting had originally been called 'StashConfirmDropShow', and then it was inverted.
    // To maintain the compatibility with the existing user settings, negate the retrieved value.
    public static InvertedBooleanSetting DontConfirmStashDrop { get; } = new(_rootSettingsPath, "stashconfirmdropshow", defaultValue: false);

    public static BooleanSetting DontConfirmSwitchWorktree { get; } = new(_rootSettingsPath, nameof(DontConfirmSwitchWorktree), defaultValue: false);

    public static BooleanSetting DontConfirmUndoLastCommit { get; } = new(_rootSettingsPath, nameof(DontConfirmUndoLastCommit), defaultValue: false);

    public static NullableBooleanSetting DontConfirmUpdateSubmodulesOnCheckout { get; } = new(_rootSettingsPath, "dontConfirmUpdateSubmodulesOnCheckout");

    public static EnumRuntimeSetting<IgnoreWhitespaceKind> IgnoreWhitespaceKind { get; } = new(_rootSettingsPath, nameof(IgnoreWhitespaceKind), defaultValue: Settings.IgnoreWhitespaceKind.None);

    public static EnumRuntimeSetting<RevisionSortOrder> RevisionSortOrder { get; } = new(_rootSettingsPath, nameof(RevisionSortOrder), defaultValue: GitCommands.RevisionSortOrder.GitDefault);

    public static BoolRuntimeSetting ShowCurrentBranchOnly { get; } = new(_rootSettingsPath, nameof(ShowCurrentBranchOnly), defaultValue: false);

    public static BoolRuntimeSetting ShowEntireFile { get; } = new(_rootSettingsPath, nameof(ShowEntireFile), defaultValue: false);

    public static BoolRuntimeSetting ShowNonPrintingChars { get; } = new(_rootSettingsPath, nameof(ShowNonPrintingChars), defaultValue: false);

    public static BoolRuntimeSetting ShowReflogReferences { get; } = new(_rootSettingsPath, nameof(ShowReflogReferences), defaultValue: false);

    public static BoolRuntimeSetting ShowSyntaxHighlightingInDiff { get; } = new(_rootSettingsPath, nameof(ShowSyntaxHighlightingInDiff), defaultValue: true);

    public static BooleanSetting UseConsoleEmulatorForCommands { get; } = new(_rootSettingsPath, nameof(UseConsoleEmulatorForCommands), defaultValue: true);
}
