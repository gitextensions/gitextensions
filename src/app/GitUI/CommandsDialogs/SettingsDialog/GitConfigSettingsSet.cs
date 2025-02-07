#nullable enable

using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog;

public readonly record struct GitConfigSettingsSet(
    ConfigFileSettings EffectiveSettings,
    ConfigFileSettings LocalSettings,
    ConfigFileSettings GlobalSettings)
{
    public void Save() => EffectiveSettings.Save();
}
