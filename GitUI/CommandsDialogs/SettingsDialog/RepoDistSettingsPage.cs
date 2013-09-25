using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public class RepoDistSettingsPage : SettingsPageWithHeader, IRepoDistSettingsPage
    {
        protected RepoDistSettingsSet RepoDistSettingsSet { get { return CommonLogic.RepoDistSettingsSet; } }
        protected RepoDistSettings CurrentSettings { get; private set; }

        protected override void Init(ISettingsPageHost aPageHost)
        {
            base.Init(aPageHost);

            CurrentSettings = RepoDistSettingsSet.EffectiveSettings;
        }

        public void SetEffectiveSettings()
        {
            if (RepoDistSettingsSet != null)
                SetCurrentSettings(RepoDistSettingsSet.EffectiveSettings);
        }

        public void SetLocalSettings()
        {
            if (RepoDistSettingsSet != null)
                SetCurrentSettings(RepoDistSettingsSet.LocalSettings);
        }

        public override void SetGlobalSettings()
        {
            if (RepoDistSettingsSet != null)
                SetCurrentSettings(RepoDistSettingsSet.GlobalSettings);
        }

        public void SetRepoDistSettings()
        {
            if (RepoDistSettingsSet != null)
                SetCurrentSettings(RepoDistSettingsSet.RepoDistSettings);
        }

        private void SetCurrentSettings(RepoDistSettings settings)
        {
            if (CurrentSettings != null)
                SaveSettings();

            CurrentSettings = settings;

            LoadSettings();
        }

    }
}
