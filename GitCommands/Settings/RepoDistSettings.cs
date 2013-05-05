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

        protected RepoDistSettings(SettingsContainer aLowerPriority, SettingsCache aSettingsCache)
            : base(aLowerPriority, aSettingsCache)
        {
        }

        #region CreateXXX

        public static RepoDistSettings CreateEffective(GitModule aModule)
        {
            return CreateLocal(aModule, CreateDistributed(aModule, CreateGlobal()));
        }

        private static RepoDistSettings CreateLocal(GitModule aModule, RepoDistSettings aLowerPriority)
        {
            //if (aModule.IsBareRepository()
            return new RepoDistSettings(aLowerPriority, 
                GitExtSettingsCache.FromCache(Path.Combine(aModule.WorkingDirGitDir(), AppSettings.SettingsFileName)));
        }

        public static RepoDistSettings CreateLocal(GitModule aModule)
        {
            return CreateLocal(aModule, null);
        }

        private static RepoDistSettings CreateDistributed(GitModule aModule, RepoDistSettings aLowerPriority)
        {
            return new RepoDistSettings(aLowerPriority,
                GitExtSettingsCache.FromCache(Path.Combine(aModule.WorkingDir, AppSettings.SettingsFileName)));
        }

        public static RepoDistSettings CreateDistributed(GitModule aModule)
        {
            return CreateDistributed(aModule, null);
        }

        public static RepoDistSettings CreateGlobal()
        {
            return new RepoDistSettings(null, AppSettings.SettingsContainer.SettingsCache);
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
    }

}
