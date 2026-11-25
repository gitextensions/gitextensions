using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static BooleanSetting IsEditorSettingsMigrated { get; } = new(MigrationSettingsPath, nameof(IsEditorSettingsMigrated), defaultValue: false);
}
