using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    // level 0
    private static readonly SettingsPath _rootSettingsPath = new AppSettingsPath(pathName: "");

    // level 1
    private static readonly SettingsPath _appearanceSettingsPath = new AppSettingsPath("Appearance");
    private static readonly SettingsPath _confirmationsSettingsPath = new AppSettingsPath("Confirmations");
    private static readonly SettingsPath _detailedSettingsPath = new AppSettingsPath("Detailed");
    private static readonly SettingsPath _dialogSettingsPath = new AppSettingsPath("Dialogs");
    private static readonly SettingsPath _hiddenSettingsPath = new AppSettingsPath("Hidden");
    private static readonly SettingsPath _recentRepositories = new AppSettingsPath("RecentRepositories");

    // level 2 _appearanceSettingsPath
    private static readonly SettingsPath _fileStatusSettingsPath = new AppSettingsPath(_appearanceSettingsPath, "FileStatus");
    private static readonly SettingsPath _revisionGraphSettingsPath = new AppSettingsPath(_appearanceSettingsPath, "RevisionGraph");

    // level 2 _detailedSettingsPath
    private static readonly SettingsPath _experimentalSettingsPath = new AppSettingsPath(_detailedSettingsPath, "Experimental");

    // level 2 _hiddenSettingsPath
    private static readonly SettingsPath _migrationSettingsPath = new AppSettingsPath(_hiddenSettingsPath, "Migration");
}
