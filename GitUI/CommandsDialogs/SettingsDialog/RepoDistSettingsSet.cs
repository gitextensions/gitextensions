using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;
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
            RepoDistSettings aEffectiveSettings,
            RepoDistSettings aLocalSettings,
            RepoDistSettings aPulledSettings,
            RepoDistSettings aGlobalSettings)
        {
            EffectiveSettings = aEffectiveSettings;
            LocalSettings = aLocalSettings;
            RepoDistSettings = aPulledSettings;
            GlobalSettings = aGlobalSettings;
        }


    }
}
