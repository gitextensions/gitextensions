using GitExtensions.Extensibility.Settings;

namespace GitCommands.Settings;

public static class DetailedSettings
{
    private static readonly SettingsPath _settingsPath = new(parent: null, "Detailed");

    public static BoolSetting GetRemoteBranchesDirectlyFromRemote { get; } = new(_settingsPath.PathFor(nameof(GetRemoteBranchesDirectlyFromRemote)), defaultValue: false);
    public static BoolSetting AddMergeLogMessages { get; } = new(_settingsPath.PathFor(nameof(AddMergeLogMessages)), defaultValue: false);
    public static NumberSetting<int> MergeLogMessagesCount { get; } = new(_settingsPath.PathFor(nameof(MergeLogMessagesCount)), defaultValue: 20);
}
