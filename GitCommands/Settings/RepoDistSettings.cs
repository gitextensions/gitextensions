using System;
using System.IO;

namespace GitCommands.Settings
{
    /// <summary>
    /// Settings that can be distributed with repository
    /// they can be overridden for a particular repository
    /// </summary>
    public class RepoDistSettings : SettingsContainer<RepoDistSettings, GitExtSettingsCache>
    {
        public RepoDistSettings(RepoDistSettings lowerPriority, GitExtSettingsCache settingsCache)
            : base(lowerPriority, settingsCache)
        {
            BuildServer = new BuildServer(this);
            Detailed = new DetailedGroup(this);
        }

        #region CreateXXX

        public static RepoDistSettings CreateEffective(GitModule module)
        {
            return CreateLocal(module, CreateDistributed(module, CreateGlobal()));
        }

        private static RepoDistSettings CreateLocal(GitModule module, RepoDistSettings lowerPriority, bool allowCache = true)
        {
            ////if (module.IsBareRepository()
            return new RepoDistSettings(lowerPriority,
                GitExtSettingsCache.Create(Path.Combine(module.GitCommonDirectory, AppSettings.SettingsFileName), allowCache));
        }

        public static RepoDistSettings CreateLocal(GitModule module, bool allowCache = true)
        {
            return CreateLocal(module, null, allowCache);
        }

        private static RepoDistSettings CreateDistributed(GitModule module, RepoDistSettings lowerPriority, bool allowCache = true)
        {
            return new RepoDistSettings(lowerPriority,
                GitExtSettingsCache.Create(Path.Combine(module.WorkingDir, AppSettings.SettingsFileName), allowCache));
        }

        public static RepoDistSettings CreateDistributed(GitModule module, bool allowCache = true)
        {
            return CreateDistributed(module, null, allowCache);
        }

        public static RepoDistSettings CreateGlobal(bool allowCache = true)
        {
            return new RepoDistSettings(null, GitExtSettingsCache.Create(AppSettings.SettingsFilePath, allowCache));
        }

        #endregion

        public override void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            bool isEffectiveLevel = LowerPriority?.LowerPriority != null;
            bool isDetachedOrGlobal = LowerPriority == null;

            if (isDetachedOrGlobal || SettingsCache.HasValue(name))
            {
                // there is no lower level
                // or the setting is assigned on this level
                SettingsCache.SetValue(name, value, encode);
            }
            else if (isEffectiveLevel)
            {
                // Settings stored at the Distributed level always have to be set directly
                // so I do not pass the control to the LowerPriority(Distributed)
                // in order to not overwrite the setting
                if (LowerPriority.SettingsCache.HasValue(name))
                {
                    // if the setting is set at the Distributed level, do not overwrite it
                    // instead of that, set the setting at the Local level to make it effective
                    // but only if the effective value is different from the new value
                    if (LowerPriority.SettingsCache.HasADifferentValue(name, value, encode))
                    {
                        SettingsCache.SetValue(name, value, encode);
                    }
                }
                else
                {
                    // if the setting isn't set at the Distributed level, do not set it there
                    // instead of that, set the setting at the Global level (it becomes effective then)
                    LowerPriority.LowerPriority.SetValue(name, value, encode);
                }
            }
            else
            {
                // the settings is not assigned on this level, recurse to the lower level
                LowerPriority.SetValue(name, value, encode);
            }
        }

        public readonly BuildServer BuildServer;
        public readonly DetailedGroup Detailed;

        public bool NoFastForwardMerge
        {
            get => GetBool("NoFastForwardMerge", false);
            set => SetBool("NoFastForwardMerge", value);
        }

        public string Dictionary
        {
            get => GetString("dictionary", "en-US");
            set => SetString("dictionary", value);
        }
    }

    public class BuildServer : SettingsPath
    {
        public readonly StringSetting Type;
        public readonly BoolNullableSetting EnableIntegration;
        public readonly BoolNullableSetting ShowBuildResultPage;

        public BuildServer(RepoDistSettings container)
            : base(container, "BuildServer")
        {
            Type = new StringSetting("Type", this, null);
            EnableIntegration = new BoolNullableSetting("EnableIntegration", this, defaultValue: false);
            ShowBuildResultPage = new BoolNullableSetting("ShowBuildResultPage", this, defaultValue: true);
        }

        public SettingsPath TypeSettings => new SettingsPath(this, Type.ValueOrDefault);
    }

    public class DetailedGroup : SettingsPath
    {
        public readonly BoolNullableSetting GetRemoteBranchesDirectlyFromRemote;
        public readonly BoolNullableSetting AddMergeLogMessages;
        public readonly IntNullableSetting MergeLogMessagesCount;

        public DetailedGroup(RepoDistSettings container)
            : base(container, "Detailed")
        {
            GetRemoteBranchesDirectlyFromRemote = new BoolNullableSetting("GetRemoteBranchesDirectlyFromRemote", this, false);
            AddMergeLogMessages = new BoolNullableSetting("AddMergeLogMessages", this, false);
            MergeLogMessagesCount = new IntNullableSetting("MergeLogMessagesCount", this, 20);
        }
    }
}
