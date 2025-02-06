#nullable enable

using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class GitConfigBaseSettingsPage : SettingsPageWithHeader, ILocalSettingsPage
    {
        public GitConfigBaseSettingsPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected GitConfigSettingsSet GitConfigSettingsSet => CommonLogic.GitConfigSettingsSet;
        protected ConfigFileSettings? CurrentSettings { get; private set; }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            CurrentSettings = GitConfigSettingsSet.EffectiveSettings;
        }

        protected override SettingsSource GetCurrentSettings()
        {
            Validates.NotNull(CurrentSettings);
            return CurrentSettings;
        }

        public void SetEffectiveSettings()
        {
            SetCurrentSettings(GitConfigSettingsSet.EffectiveSettings);
        }

        public void SetLocalSettings()
        {
            SetCurrentSettings(GitConfigSettingsSet.LocalSettings);
        }

        public override void SetGlobalSettings()
        {
            SetCurrentSettings(GitConfigSettingsSet.GlobalSettings);
        }

        private void SetCurrentSettings(ConfigFileSettings settings)
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
