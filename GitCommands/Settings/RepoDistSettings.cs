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
    public class RepoDistSettings : SettingsContainer<RepoDistSettings>
    {

        public GitModule Module { get; private set; }

        public RepoDistSettings(RepoDistSettings aLowerPriority, SettingsCache aSettingsCache)
            : base(aLowerPriority, aSettingsCache)
        {
            BuildServer = new BuildServer(this);
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
            //Settings stored in Distributed always have to be set directly
            if (LowerPriority == null 
                || LowerPriority.LowerPriority == null
                || SettingsCache.HasValue(name) 
                || LowerPriority.SettingsCache.HasValue(name))
                SettingsCache.SetValue(name, value, encode);
            else
                LowerPriority.LowerPriority.SetValue(name, value, encode);
        }

        public readonly BuildServer BuildServer;
        
        public bool NoFastForwardMerge
        {
            get { return this.GetBool("NoFastForwardMerge", false); }
            set { this.SetBool("NoFastForwardMerge", value); }
        }
    }


    public class BuildServer : SettingsPath
    {
        public readonly StringSetting Type;

        public BuildServer(RepoDistSettings container)
            : base(container, "BuildServer")
        {
            Type = new StringSetting("Type", this, null);
        }

        public SettingsPath TypeSettings
        {
            get
            {
                return new SettingsPath(this, Type.Value);
            }
        }

        public bool EnableIntegration
        {
            get { return this.GetBool("enablebuildserverintegration", true); }
            set { this.SetBool("enablebuildserverintegration", value); }
        }
    }

}
