using GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class RepoDistSettingsPage : SettingsPageWithHeader, IRepoDistSettingsPage
    {
        protected RepoDistSettingsSet RepoDistSettingsSet => CommonLogic.RepoDistSettingsSet;
        protected RepoDistSettings CurrentSettings { get; private set; }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            CurrentSettings = RepoDistSettingsSet.EffectiveSettings;
        }

        protected override bool AreEffectiveSettings => CurrentSettings == RepoDistSettingsSet.EffectiveSettings;

        protected override ISettingsSource GetCurrentSettings()
        {
            return CurrentSettings;
        }

        public void SetEffectiveSettings()
        {
            if (RepoDistSettingsSet != null)
            {
                SetCurrentSettings(RepoDistSettingsSet.EffectiveSettings);
            }
        }

        public void SetLocalSettings()
        {
            if (RepoDistSettingsSet != null)
            {
                SetCurrentSettings(RepoDistSettingsSet.LocalSettings);
            }
        }

        public override void SetGlobalSettings()
        {
            if (RepoDistSettingsSet != null)
            {
                SetCurrentSettings(RepoDistSettingsSet.GlobalSettings);
            }
        }

        public void SetRepoDistSettings()
        {
            if (RepoDistSettingsSet != null)
            {
                SetCurrentSettings(RepoDistSettingsSet.RepoDistSettings);
            }
        }

        private void SetCurrentSettings(RepoDistSettings settings)
        {
            if (CurrentSettings != null)
            {
                SaveSettings();
            }

            CurrentSettings = settings;

            LoadSettings();
        }
    }
}
