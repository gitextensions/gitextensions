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
    public class RepoDistSettings : SettingsContainer<RepoDistSettings, GitExtSettingsCache>
    {

        public GitModule Module { get; private set; }

        public RepoDistSettings(RepoDistSettings aLowerPriority, GitExtSettingsCache aSettingsCache)
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
                GitExtSettingsCache.Create(Path.Combine(aModule.GetGitDirectory(), AppSettings.SettingsFileName), allowCache));
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
            bool isEffectiveLevel = LowerPriority != null && LowerPriority.LowerPriority != null;
            bool isDetachedOrGlobal = LowerPriority == null;

            if (isDetachedOrGlobal || //there is no lower level
                SettingsCache.HasValue(name))//or the setting is assigned on this level
            {
                SettingsCache.SetValue(name, value, encode);
            }
            else if (isEffectiveLevel)
            {
                //Settings stored at the Distributed level always have to be set directly
                //so I do not pass the control to the LowerPriority(Distributed)
                //in order to not overwrite the setting
                if (LowerPriority.SettingsCache.HasValue(name))
                {
                    //if the setting is set at the Distributed level, do not overwrite it
                    //instead of that, set the setting at the Local level to make it effective
                    //but only if the effective value is different from the new value
                    if (LowerPriority.SettingsCache.HasADifferentValue(name, value, encode))
                    {
                        SettingsCache.SetValue(name, value, encode);
                    }
                }
                else
                {
                    //if the setting isn't set at the Distributed level, do not set it there
                    //instead of that, set the setting at the Global level (it becomes effective then)
                    LowerPriority.LowerPriority.SetValue(name, value, encode);
                }
            }
            else//the settings is not assigned on this level, recurse to the lower level
            {
                LowerPriority.SetValue(name, value, encode);
            }
        }

        public readonly BuildServer BuildServer;
        
        public bool NoFastForwardMerge
        {
            get { return this.GetBool("NoFastForwardMerge", false); }
            set { this.SetBool("NoFastForwardMerge", value); }
        }

        public string Dictionary
        {
            get { return this.GetString("dictionary", "en-US"); }
            set { this.SetString("dictionary", value); }
        }

    }


    public class BuildServer : SettingsPath
    {
        public readonly StringSetting Type;
        public readonly BoolNullableSetting EnableIntegration;
        public readonly BoolNullableSetting ShowBuildSummaryInGrid;

        public BuildServer(RepoDistSettings container)
            : base(container, "BuildServer")
        {
            Type = new StringSetting("Type", this, null);
            EnableIntegration = new BoolNullableSetting("EnableIntegration", this, false);
            ShowBuildSummaryInGrid = new BoolNullableSetting("ShowBuildSummaryInGrid", this, true);
        }

        public SettingsPath TypeSettings
        {
            get
            {
                return new SettingsPath(this, Type.Value);
            }
        }
    }

}
