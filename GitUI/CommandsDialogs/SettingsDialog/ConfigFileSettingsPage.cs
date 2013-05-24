using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class ConfigFileSettingsPage : SettingsPageWithHeader, ILocalSettingsPage
    {
        protected ConfigFileSettingsSet ConfigFileSettingsSet { get { return CommonLogic.ConfigFileSettingsSet; } }
        protected ConfigFileSettings CurrentSettings { get; private set; }

        protected override void Init(ISettingsPageHost aPageHost)
        {
            base.Init(aPageHost);

            CurrentSettings = CommonLogic.ConfigFileSettingsSet.EffectiveSettings;
        }

        public void SetEffectiveSettings()
        {
            if (ConfigFileSettingsSet != null)
                SetCurrentSettings(ConfigFileSettingsSet.EffectiveSettings);
        }

        public void SetLocalSettings()
        {
            if (ConfigFileSettingsSet != null)
                SetCurrentSettings(ConfigFileSettingsSet.LocalSettings);
        }

        public override void SetGlobalSettings()
        {
            if (ConfigFileSettingsSet != null)
                SetCurrentSettings(ConfigFileSettingsSet.GlobalSettings);
        }

        private void SetCurrentSettings(ConfigFileSettings settings)
        {            
            if (CurrentSettings != null)
                SaveSettings();

            CurrentSettings = settings;

            LoadSettings();
        }

    }
}
