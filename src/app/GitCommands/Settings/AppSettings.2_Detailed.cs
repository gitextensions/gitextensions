using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static EnumSetting<CommitInfoPosition> CommitInfoPosition { get; } = new(_detailedSettingsPath, nameof(CommitInfoPosition), defaultValue: GitCommands.CommitInfoPosition.BelowList);

    public static TextSetting ConEmuStyle { get; } = new(_detailedSettingsPath, nameof(ConEmuStyle), defaultValue: "Default");

    public static TextSetting ConEmuTerminal { get; } = new(_detailedSettingsPath, nameof(ConEmuTerminal), defaultValue: "bash");

    public static TextSetting ConsoleEmulatorName { get; } = new(_detailedSettingsPath, nameof(ConsoleEmulatorName), defaultValue: "ConEmu");

    public static BooleanSetting MessageEditorWordWrap { get; } = new(_detailedSettingsPath, nameof(MessageEditorWordWrap), defaultValue: false);

    public static IntegerSetting OutputHistoryDepth { get; } = new(_detailedSettingsPath, nameof(OutputHistoryDepth), defaultValue: 20);

    public static BooleanSetting OutputHistoryPanelVisible { get; } = new(_detailedSettingsPath, nameof(OutputHistoryPanelVisible), defaultValue: false);

    public static BooleanSetting ShowConEmuTab { get; } = new(_detailedSettingsPath, nameof(ShowConEmuTab), defaultValue: true);

    public static BooleanSetting ShowGpgInformation { get; } = new(_detailedSettingsPath, nameof(ShowGpgInformation), defaultValue: true);

    public static BooleanSetting ShowOutputHistoryAsTab { get; } = new(_detailedSettingsPath, nameof(ShowOutputHistoryAsTab), defaultValue: true);

    public static BooleanSetting ShowProcessDialogPasswordInput { get; } = new(_detailedSettingsPath, nameof(ShowProcessDialogPasswordInput), defaultValue: false);

    public static BooleanSetting ShowSplitViewLayout { get; } = new(_detailedSettingsPath, nameof(ShowSplitViewLayout), defaultValue: true);

    public static TextSetting UninformativeRepoNameRegex { get; } = new(_detailedSettingsPath, nameof(UninformativeRepoNameRegex), defaultValue: "app|(repo(sitory)?)");

    public static BooleanSetting UseBrowseForFileHistory { get; } = new(_detailedSettingsPath, nameof(UseBrowseForFileHistory), defaultValue: true);

    public static BooleanSetting UseDiffViewerForBlame { get; } = new(_detailedSettingsPath, nameof(UseDiffViewerForBlame), defaultValue: false);
}
