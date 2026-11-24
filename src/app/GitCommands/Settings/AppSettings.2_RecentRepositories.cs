using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static BooleanSetting HideTopRepositoriesFromRecentList { get; } = new(_recentRepositories, nameof(HideTopRepositoriesFromRecentList), defaultValue: false);
}
