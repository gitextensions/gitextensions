using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class DistributedSettingsSet
    {
        public readonly DistributedSettings EffectiveSettings;
        public readonly DistributedSettings LocalSettings;
        public readonly DistributedSettings DistributedSettings;
        public readonly DistributedSettings GlobalSettings;

        public DistributedSettingsSet(
            DistributedSettings effectiveSettings,
            DistributedSettings localSettings,
            DistributedSettings pulledSettings,
            DistributedSettings globalSettings)
        {
            EffectiveSettings = effectiveSettings;
            LocalSettings = localSettings;
            DistributedSettings = pulledSettings;
            GlobalSettings = globalSettings;
        }
    }
}
