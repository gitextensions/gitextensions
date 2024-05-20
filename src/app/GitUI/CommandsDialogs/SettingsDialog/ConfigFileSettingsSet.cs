using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public readonly record struct ConfigFileSettingsSet(
        ConfigFileSettings EffectiveSettings,
        ConfigFileSettings LocalSettings,
        ConfigFileSettings GlobalSettings);
}
