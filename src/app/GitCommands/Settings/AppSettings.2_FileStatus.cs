using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static IntegerSetting FileStatusFindInFilesGitGrepTypeIndex { get; } = new(_fileStatusSettingsPath, nameof(FileStatusFindInFilesGitGrepTypeIndex), defaultValue: 1);

    public static BooleanSetting FileStatusMergeSingleItemWithFolder { get; } = new(_fileStatusSettingsPath, nameof(FileStatusMergeSingleItemWithFolder), defaultValue: false);

    public static BooleanSetting FileStatusShowGroupNodesInFlatList { get; } = new(_fileStatusSettingsPath, nameof(FileStatusShowGroupNodesInFlatList), defaultValue: false);
}
