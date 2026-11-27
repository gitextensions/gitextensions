using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static EnumSetting<CommitInfoPosition> CommitInfoPosition { get; } = new(DetailedSettingsPath, nameof(CommitInfoPosition), defaultValue: GitCommands.CommitInfoPosition.BelowList);

    public static TextSetting ConEmuStyle { get; } = new(DetailedSettingsPath, nameof(ConEmuStyle), defaultValue: "Default");

    public static TextSetting ConEmuTerminal { get; } = new(DetailedSettingsPath, nameof(ConEmuTerminal), defaultValue: "bash");

    public static TextSetting ConsoleEmulatorName { get; } = new(DetailedSettingsPath, nameof(ConsoleEmulatorName), defaultValue: "ConEmu");

    public static BooleanSetting MessageEditorWordWrap { get; } = new(DetailedSettingsPath, nameof(MessageEditorWordWrap), defaultValue: false);

    public static IntegerSetting OutputHistoryDepth { get; } = new(DetailedSettingsPath, nameof(OutputHistoryDepth), defaultValue: 20);

    public static BooleanSetting OutputHistoryPanelVisible { get; } = new(DetailedSettingsPath, nameof(OutputHistoryPanelVisible), defaultValue: false);

    public static BooleanSetting ShowConEmuTab { get; } = new(DetailedSettingsPath, nameof(ShowConEmuTab), defaultValue: true);

    public static BooleanSetting ShowGpgInformation { get; } = new(DetailedSettingsPath, nameof(ShowGpgInformation), defaultValue: true);

    public static BooleanSetting ShowOutputHistoryAsTab { get; } = new(DetailedSettingsPath, nameof(ShowOutputHistoryAsTab), defaultValue: true);

    public static BooleanSetting ShowProcessDialogPasswordInput { get; } = new(DetailedSettingsPath, nameof(ShowProcessDialogPasswordInput), defaultValue: false);

    public static BooleanSetting ShowSplitViewLayout { get; } = new(DetailedSettingsPath, nameof(ShowSplitViewLayout), defaultValue: true);

    public static TextSetting UninformativeRepoNameRegex { get; } = new(DetailedSettingsPath, nameof(UninformativeRepoNameRegex), defaultValue: "app|(repo(sitory)?)");

    public static BooleanSetting UseBrowseForFileHistory { get; } = new(DetailedSettingsPath, nameof(UseBrowseForFileHistory), defaultValue: true);

    public static BooleanSetting UseDiffViewerForBlame { get; } = new(DetailedSettingsPath, nameof(UseDiffViewerForBlame), defaultValue: false);
}
