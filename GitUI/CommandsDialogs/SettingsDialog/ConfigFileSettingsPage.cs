using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class ConfigFileSettingsPage : SettingsPageBase
    {
        protected ConfigFileSettingsSet ConfigFileSettingsSet { get { return CommonLogic.ConfigFileSettingsSet; } }
        protected ConfigFileSettings CurrentSettings { get; private set; }

        public ConfigFileSettingsPage()
        {
            header = new ConfigFileSettingsPageHeader(this);
        }

        protected override void Init(ISettingsPageHost aPageHost)
        {
            base.Init(aPageHost);

            CurrentSettings = CommonLogic.ConfigFileSettingsSet.EffectiveSettings;
        }

        private ConfigFileSettingsPageHeader header;

        public override Control GuiControl
        {
            get
            {
                return header;
            }
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

        public void SetGlobalSettings()
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
