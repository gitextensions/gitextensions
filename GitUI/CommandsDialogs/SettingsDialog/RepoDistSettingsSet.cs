using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class RepoDistSettingsSet
    {
        public readonly RepoDistSettings EffectiveSettings;
        public readonly RepoDistSettings LocalSettings;
        public readonly RepoDistSettings RepoDistSettings;
        public readonly RepoDistSettings GlobalSettings;

        public RepoDistSettingsSet(
            RepoDistSettings effectiveSettings,
            RepoDistSettings localSettings,
            RepoDistSettings pulledSettings,
            RepoDistSettings globalSettings)
        {
            EffectiveSettings = effectiveSettings;
            LocalSettings = localSettings;
            RepoDistSettings = pulledSettings;
            GlobalSettings = globalSettings;
        }
    }
}
