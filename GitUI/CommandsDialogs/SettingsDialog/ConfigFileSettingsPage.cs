using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class ConfigFileSettingsPage : SettingsPageWithHeader, ILocalSettingsPage
    {
        public ConfigFileSettingsPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected ConfigFileSettingsSet ConfigFileSettingsSet => CommonLogic.ConfigFileSettingsSet;
        protected ConfigFileSettings? CurrentSettings { get; private set; }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            CurrentSettings = CommonLogic.ConfigFileSettingsSet.EffectiveSettings;
        }

        protected override SettingsSource GetCurrentSettings()
        {
            Validates.NotNull(CurrentSettings);
            return CurrentSettings;
        }

        public void SetEffectiveSettings()
        {
            if (ConfigFileSettingsSet.EffectiveSettings is not null)
            {
                SetCurrentSettings(ConfigFileSettingsSet.EffectiveSettings);
            }
        }

        public void SetLocalSettings()
        {
            if (ConfigFileSettingsSet.LocalSettings is not null)
            {
                SetCurrentSettings(ConfigFileSettingsSet.LocalSettings);
            }
        }

        public override void SetGlobalSettings()
        {
            if (ConfigFileSettingsSet.GlobalSettings is not null)
            {
                SetCurrentSettings(ConfigFileSettingsSet.GlobalSettings);
            }
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
