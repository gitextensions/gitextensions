using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static IntegerSetting FileStatusFindInFilesGitGrepTypeIndex { get; } = new(FileStatusSettingsPath, nameof(FileStatusFindInFilesGitGrepTypeIndex), defaultValue: 1);

    public static BooleanSetting FileStatusMergeSingleItemWithFolder { get; } = new(FileStatusSettingsPath, nameof(FileStatusMergeSingleItemWithFolder), defaultValue: false);

    public static BooleanSetting FileStatusShowGroupNodesInFlatList { get; } = new(FileStatusSettingsPath, nameof(FileStatusShowGroupNodesInFlatList), defaultValue: false);
}
