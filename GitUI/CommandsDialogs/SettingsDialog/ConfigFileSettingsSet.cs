using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class ConfigFileSettingsSet
    {
        public readonly ConfigFileSettings EffectiveSettings;
        public readonly ConfigFileSettings LocalSettings;
        public readonly ConfigFileSettings GlobalSettings;

        public ConfigFileSettingsSet(
            ConfigFileSettings effectiveSettings,
            ConfigFileSettings localSettings,
            ConfigFileSettings globalSettings)
        {
            EffectiveSettings = effectiveSettings;
            LocalSettings = localSettings;
            GlobalSettings = globalSettings;
        }
    }
}
