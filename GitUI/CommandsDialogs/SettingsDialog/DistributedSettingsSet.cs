using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public readonly record struct DistributedSettingsSet(
        DistributedSettings EffectiveSettings,
        DistributedSettings LocalSettings,
        DistributedSettings DistributedSettings,
        DistributedSettings GlobalSettings);
}
