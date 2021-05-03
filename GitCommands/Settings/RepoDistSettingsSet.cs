using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    public sealed class RepoDistSettingsSet
    {
        public readonly ISettingsSource EffectiveSettings;
        public readonly ISettingsSource LocalSettings;
        public readonly ISettingsSource RepoDistSettings;
        public readonly ISettingsSource GlobalSettings;

        public RepoDistSettingsSet(
            ISettingsSource effectiveSettings,
            ISettingsSource localSettings,
            ISettingsSource pulledSettings,
            ISettingsSource globalSettings)
        {
            EffectiveSettings = effectiveSettings;
            LocalSettings = localSettings;
            RepoDistSettings = pulledSettings;
            GlobalSettings = globalSettings;
        }
    }
}
