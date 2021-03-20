using System;
using System.Collections.Generic;
using System.IO;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    /// <summary>
    /// Settings that can be distributed with repository.
    /// They can be overridden for a particular repository.
    /// </summary>
    public sealed class RepoDistSettings : SettingsContainer<RepoDistSettings, GitExtSettingsCache>
    {
        private static Dictionary<(SettingLevel settingLevel, bool allowCache), (string path, RepoDistSettings settingsContainer)> CachedSettingsContainers = new();

        public RepoDistSettings(RepoDistSettings? lowerPriority, GitExtSettingsCache settingsCache, SettingLevel settingLevel)
            : base(lowerPriority, settingsCache)
        {
            BuildServer = new BuildServer(this);
            Detailed = new DetailedGroup(this);
            SettingLevel = settingLevel;
        }

        #region CreateXXX

        public static RepoDistSettings CreateEffective(GitModule module)
        {
            var globalSettingsContainer = CreateGlobal();
            var distributedSettingsContainer = CreateDistributed(module, lowerPriority: globalSettingsContainer);

            string settingsFilePath = Path.Combine(module.GitCommonDirectory, AppSettings.SettingsFileName);

            return new RepoDistSettings(
                lowerPriority: distributedSettingsContainer,
                settingsCache: GitExtSettingsCache.Create(settingsFilePath, allowCache: true),
                settingLevel: SettingLevel.Effective);
        }

        public static RepoDistSettings CreateGlobal(bool allowCache = true)
        {
            string settingsFilePath = AppSettings.SettingsFilePath;

            return CreateSettingsContainer(SettingLevel.Global, settingsFilePath, allowCache);
        }

        public static RepoDistSettings CreateDistributed(GitModule module, bool allowCache = true, RepoDistSettings? lowerPriority = null)
        {
            string settingsFilePath = Path.Combine(module.WorkingDir, AppSettings.SettingsFileName);

            return CreateSettingsContainer(SettingLevel.Distributed, settingsFilePath, allowCache, lowerPriority);
        }

        public static RepoDistSettings CreateLocal(GitModule module, bool allowCache = true, RepoDistSettings? lowerPriority = null)
        {
            string settingsFilePath = Path.Combine(module.GitCommonDirectory, AppSettings.SettingsFileName);

            return CreateSettingsContainer(SettingLevel.Local, settingsFilePath, allowCache, lowerPriority);
        }

        private static RepoDistSettings CreateSettingsContainer(SettingLevel settingLevel, string settingsFilePath, bool allowCache, RepoDistSettings? lowerPriority = null)
        {
            if (!CachedSettingsContainers.TryGetValue((settingLevel, allowCache), out var settingsContainerWithPath)
                || settingsContainerWithPath.path != settingsFilePath)
            {
                var settingsContainer = new RepoDistSettings(
                    lowerPriority,
                    settingsCache: GitExtSettingsCache.Create(settingsFilePath, allowCache),
                    settingLevel);

                CachedSettingsContainers[(settingLevel, allowCache)] = (settingsFilePath, settingsContainer);
            }

            return CachedSettingsContainers[(settingLevel, allowCache)].settingsContainer;
        }

        #endregion

        public override void SetValue<T>(string name, T value, Func<T, string?> encode)
        {
            bool isEffectiveLevel = LowerPriority?.LowerPriority is not null;
            bool isDetachedOrGlobal = LowerPriority is null;

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
                if (LowerPriority!.SettingsCache.HasValue(name))
                {
                    // if the setting is set at the Distributed level, do not overwrite it
                    // instead of that, set the setting at the Local level to make it effective
                    // but only if the effective value is different from the new value
                    if (LowerPriority!.SettingsCache.HasADifferentValue(name, value, encode))
                    {
                        SettingsCache.SetValue(name, value, encode);
                    }
                }
                else
                {
                    // if the setting isn't set at the Distributed level, do not set it there
                    // instead of that, set the setting at the Global level (it becomes effective then)
                    LowerPriority!.LowerPriority!.SetValue(name, value, encode);
                }
            }
            else
            {
                // the settings is not assigned on this level, recurse to the lower level
                LowerPriority!.SetValue(name, value, encode);
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
        public readonly ISetting<string> Type;
        public readonly ISetting<bool> EnableIntegration;
        public readonly ISetting<bool> ShowBuildResultPage;

        public BuildServer(RepoDistSettings container)
            : base(container, "BuildServer")
        {
            Type = Setting.Create(this, nameof(Type), null);
            EnableIntegration = Setting.Create(this, nameof(EnableIntegration), false);
            ShowBuildResultPage = Setting.Create(this, nameof(ShowBuildResultPage), true);
        }

        public SettingsPath TypeSettings => new SettingsPath(this, Type.Value!);
    }

    public class DetailedGroup : SettingsPath
    {
        public readonly ISetting<bool> GetRemoteBranchesDirectlyFromRemote;
        public readonly ISetting<bool> AddMergeLogMessages;
        public readonly ISetting<int> MergeLogMessagesCount;

        public DetailedGroup(RepoDistSettings container)
            : base(container, "Detailed")
        {
            GetRemoteBranchesDirectlyFromRemote = Setting.Create(this, nameof(GetRemoteBranchesDirectlyFromRemote), false);
            AddMergeLogMessages = Setting.Create(this, nameof(AddMergeLogMessages), false);
            MergeLogMessagesCount = Setting.Create(this, nameof(MergeLogMessagesCount), 20);
        }
    }
}
