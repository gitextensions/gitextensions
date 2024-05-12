using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class DistributedSettingsPage : SettingsPageWithHeader, IDistributedSettingsPage
    {
        public DistributedSettingsPage(IServiceProvider serviceProvider)
           : base(serviceProvider)
        {
        }

        protected DistributedSettingsSet DistributedSettingsSet => CommonLogic.DistributedSettingsSet;
        protected DistributedSettings? CurrentSettings { get; private set; }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            CurrentSettings = DistributedSettingsSet.EffectiveSettings;
        }

        protected override SettingsSource GetCurrentSettings()
        {
            Validates.NotNull(CurrentSettings);

            return CurrentSettings;
        }

        public void SetEffectiveSettings()
        {
            if (DistributedSettingsSet.EffectiveSettings is not null)
            {
                SetCurrentSettings(DistributedSettingsSet.EffectiveSettings);
            }
        }

        public void SetLocalSettings()
        {
            if (DistributedSettingsSet.LocalSettings is not null)
            {
                SetCurrentSettings(DistributedSettingsSet.LocalSettings);
            }
        }

        public void SetDistributedSettings()
        {
            if (DistributedSettingsSet.DistributedSettings is not null)
            {
                SetCurrentSettings(DistributedSettingsSet.DistributedSettings);
            }
        }

        public override void SetGlobalSettings()
        {
            if (DistributedSettingsSet.GlobalSettings is not null)
            {
                SetCurrentSettings(DistributedSettingsSet.GlobalSettings);
            }
        }

        private void SetCurrentSettings(DistributedSettings settings)
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
