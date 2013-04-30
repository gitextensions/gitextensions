using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GitCommands.Settings
{
    /// <summary>
    /// Settings that can be distributed with repository
    /// they can be overridden for a particular repository
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

        public override void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            //Settings stored in RepoSettings always have to be set directly
            if (LowerPriority == null 
                || LowerPriority.LowerPriority == null
                || SettingsCache.HasValue(name) 
                || LowerPriority.SettingsCache.HasValue(name))
                SettingsCache.SetValue(name, value, encode);
            else
                LowerPriority.LowerPriority.SetValue(name, value, encode);
        }

#region RepoSettingsContainer

        private class RepoSettingsContainer : SettingsContainer
        {
            internal RepoSettingsContainer(GitModule aModule)
                : base(AppSettings.SettingsContainer,
                SettingsCache.FromCache(Path.Combine(aModule.WorkingDir, AppSettings.SettingsFileName)))
            { }
        }

#endregion

        public bool NoFastForwardMerge
        {
            get { return GetBool("NoFastForwardMerge", false); }
            set { SetBool("NoFastForwardMerge", value); }
        }

        public GitCommands.PullAction LastPullAction
        {
            get { return GetEnum("LastPullAction", GitCommands.PullAction.None); }
            set { SetEnum("LastPullAction", value); }
        }
    }

}
