using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    // level 0
    private static readonly SettingsPath RootSettingsPath = new AppSettingsPath(pathName: "");

    // level 1
    private static readonly SettingsPath AppearanceSettingsPath = new AppSettingsPath("Appearance");
    private static readonly SettingsPath ConfirmationsSettingsPath = new AppSettingsPath("Confirmations");
    private static readonly SettingsPath DetailedSettingsPath = new AppSettingsPath("Detailed");
    private static readonly SettingsPath DialogSettingsPath = new AppSettingsPath("Dialogs");
    private static readonly SettingsPath HiddenSettingsPath = new AppSettingsPath("Hidden");
    private static readonly SettingsPath RecentRepositories = new AppSettingsPath("RecentRepositories");

    // level 2 AppearanceSettingsPath
    private static readonly SettingsPath FileStatusSettingsPath = new AppSettingsPath(AppearanceSettingsPath, "FileStatus");
    private static readonly SettingsPath RevisionGraphSettingsPath = new AppSettingsPath(AppearanceSettingsPath, "RevisionGraph");

    // level 2 DetailedSettingsPath
    private static readonly SettingsPath ExperimentalSettingsPath = new AppSettingsPath(DetailedSettingsPath, "Experimental");

    // level 2 HiddenSettingsPath
    private static readonly SettingsPath MigrationSettingsPath = new AppSettingsPath(HiddenSettingsPath, "Migration");
}
