using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;
using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class ConfigFileSettingsSet
    {
        public readonly ConfigFileSettings EffectiveSettings;
        public readonly ConfigFileSettings LocalSettings;
        public readonly ConfigFileSettings GlobalSettings;

        public ConfigFileSettingsSet(
            ConfigFileSettings aEffectiveSettings,
            ConfigFileSettings aLocalSettings,
            ConfigFileSettings aGlobalSettings)
        {
            EffectiveSettings = aEffectiveSettings;
            LocalSettings = aLocalSettings;
            GlobalSettings = aGlobalSettings;
        }

    }
}
