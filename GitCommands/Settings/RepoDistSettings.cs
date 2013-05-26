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

        public RepoDistSettings(SettingsContainer aLowerPriority, SettingsCache aSettingsCache)
            : base(aLowerPriority, aSettingsCache)
        {
        }

        #region CreateXXX

        public static RepoDistSettings CreateEffective(GitModule aModule)
        {
            return CreateLocal(aModule, CreateDistributed(aModule, CreateGlobal()));
        }

        private static RepoDistSettings CreateLocal(GitModule aModule, RepoDistSettings aLowerPriority, bool allowCache = true)
        {
            //if (aModule.IsBareRepository()
            return new RepoDistSettings(aLowerPriority, 
                GitExtSettingsCache.Create(Path.Combine(aModule.WorkingDirGitDir(), AppSettings.SettingsFileName), allowCache));
        }

        public static RepoDistSettings CreateLocal(GitModule aModule, bool allowCache = true)
        {
            return CreateLocal(aModule, null, allowCache);
        }

        private static RepoDistSettings CreateDistributed(GitModule aModule, RepoDistSettings aLowerPriority, bool allowCache = true)
        {
            return new RepoDistSettings(aLowerPriority,
                GitExtSettingsCache.Create(Path.Combine(aModule.WorkingDir, AppSettings.SettingsFileName), allowCache));
        }

        public static RepoDistSettings CreateDistributed(GitModule aModule, bool allowCache = true)
        {
            return CreateDistributed(aModule, null, allowCache);
        }

        public static RepoDistSettings CreateGlobal(bool allowCache = true)
        {
            return new RepoDistSettings(null, GitExtSettingsCache.Create(AppSettings.SettingsFilePath, allowCache));
        }

        #endregion

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


        
        
        public bool NoFastForwardMerge
        {
            get { return this.GetBool("NoFastForwardMerge", false); }
            set { this.SetBool("NoFastForwardMerge", value); }
        }

        public GitCommands.PullAction LastPullAction
        {
            get { return GetEnum("LastPullAction", GitCommands.PullAction.None); }
            set { SetEnum("LastPullAction", value); }
        }
    }

}
