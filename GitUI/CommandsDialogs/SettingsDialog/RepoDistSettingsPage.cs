using GitCommands.Settings;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class RepoDistSettingsPage : SettingsPageWithHeader, IRepoDistSettingsPage
    {
        protected RepoDistSettingsSet? RepoDistSettingsSet => CommonLogic.RepoDistSettingsSet;
        protected RepoDistSettings? CurrentSettings { get; private set; }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            CurrentSettings = RepoDistSettingsSet?.EffectiveSettings;
        }

        protected override ISettingsSource GetCurrentSettings()
        {
            Validates.NotNull(CurrentSettings);

            return CurrentSettings;
        }

        public void SetEffectiveSettings()
        {
            if (RepoDistSettingsSet is not null)
            {
                SetCurrentSettings(RepoDistSettingsSet.EffectiveSettings);
            }
        }

        public void SetLocalSettings()
        {
            if (RepoDistSettingsSet is not null)
            {
                SetCurrentSettings(RepoDistSettingsSet.LocalSettings);
            }
        }

        public override void SetGlobalSettings()
        {
            if (RepoDistSettingsSet is not null)
            {
                SetCurrentSettings(RepoDistSettingsSet.GlobalSettings);
            }
        }

        public void SetRepoDistSettings()
        {
            if (RepoDistSettingsSet is not null)
            {
                SetCurrentSettings(RepoDistSettingsSet.RepoDistSettings);
            }
        }

        private void SetCurrentSettings(RepoDistSettings settings)
        {
            if (CurrentSettings is not null)
            {
                SaveSettings();
            }

            CurrentSettings = settings;

            LoadSettings();
        }
    }
}
