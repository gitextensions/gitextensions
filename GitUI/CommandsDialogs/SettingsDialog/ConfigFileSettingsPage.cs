using GitCommands.Settings;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class ConfigFileSettingsPage : SettingsPageWithHeader, ILocalSettingsPage
    {
        protected ConfigFileSettingsSet ConfigFileSettingsSet => CommonLogic.ConfigFileSettingsSet;
        protected ConfigFileSettings? CurrentSettings { get; private set; }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            CurrentSettings = CommonLogic.ConfigFileSettingsSet.EffectiveSettings;
        }

        protected override ISettingsSource GetCurrentSettings()
        {
            Validates.NotNull(CurrentSettings);
            return CurrentSettings;
        }

        public void SetEffectiveSettings()
        {
            if (ConfigFileSettingsSet is not null)
            {
                SetCurrentSettings(ConfigFileSettingsSet.EffectiveSettings);
            }
        }

        public void SetLocalSettings()
        {
            if (ConfigFileSettingsSet is not null)
            {
                SetCurrentSettings(ConfigFileSettingsSet.LocalSettings);
            }
        }

        public override void SetGlobalSettings()
        {
            if (ConfigFileSettingsSet is not null)
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
