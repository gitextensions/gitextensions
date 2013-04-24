using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GitCommands.Settings
{
    /// <summary>
    /// Settings that can be distributed with repository
    /// they can be overriden for a particular repository
    /// </summary>
    public class RepoDistSettings : SettingsContainer
    {

        public GitModule Module { get; private set; }

        public RepoDistSettings(GitModule aModule)
            : base(new RepoSettingsContainer(aModule),
            SettingsCache.FromCache(Path.Combine(aModule.WorkingDirGitDir(), AppSettings.SettingsFileName)))
        {
            Module = aModule;
        }

        public bool NoFastForwardMerge
        {
            get { return GetBool("NoFastForwardMerge", false); }
            set { SetBool("NoFastForwardMerge", value); }
        }


        private class RepoSettingsContainer : SettingsContainer
        {
            internal RepoSettingsContainer(GitModule aModule)
                : base(AppSettings.SettingsContainer,
                SettingsCache.FromCache(Path.Combine(aModule.WorkingDir, AppSettings.SettingsFileName)))
            { }
        }
    }

}
