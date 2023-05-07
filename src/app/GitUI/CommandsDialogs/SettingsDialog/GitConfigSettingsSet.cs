#nullable enable

using GitCommands.Settings;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog;

public readonly record struct GitConfigSettingsSet(
    SettingsSource<IConfigValueStore> EffectiveSettings,
    SettingsSource<IPersistentConfigValueStore> LocalSettings,
    SettingsSource<IPersistentConfigValueStore> GlobalSettings,
    SettingsSource SystemSettings)
{
    public void Save()
    {
        LocalSettings.ConfigValueStore.Save();
        GlobalSettings.ConfigValueStore.Save();
        EffectiveSettings.ConfigValueStore.Invalidate();
    }
}
