using System.IO;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    /// <summary>
    /// Settings that can be distributed with repository.
    /// They can be overridden for a particular repository.
    /// </summary>
    public class RepoDistSettings : SettingsContainer<RepoDistSettings, GitExtSettingsCache>
    {
        public RepoDistSettings(RepoDistSettings? lowerPriority, GitExtSettingsCache settingsCache, SettingLevel settingLevel)
            : base(lowerPriority, settingsCache)
        {
            SettingLevel = settingLevel;
        }

        #region CreateRepoDistSettings

        /// <summary>
        /// Returns an effective setting object where Git Extensions settings are read from the first available of:
        /// * Local (in .git dir).
        /// * Distributed (stored in repo).
        /// * Global (user home).
        /// </summary>
        /// <param name="module">The GitModule.</param>
        /// <returns>the settings.</returns>
        public static RepoDistSettings CreateEffective(GitModule module)
        {
            return CreateLocal(module, CreateDistributed(module, CreateGlobal()), SettingLevel.Effective);
        }

        private static RepoDistSettings CreateLocal(GitModule module, RepoDistSettings? lowerPriority,
            SettingLevel settingLevel, bool allowCache = true)
        {
            return new RepoDistSettings(lowerPriority,
                GitExtSettingsCache.Create(Path.Combine(module.GitCommonDirectory, AppSettings.SettingsFileName), allowCache),
                settingLevel);
        }

        public static RepoDistSettings CreateLocal(GitModule module, bool allowCache = true)
        {
            return CreateLocal(module, null, SettingLevel.Local, allowCache);
        }

        private static RepoDistSettings CreateDistributed(GitModule module, RepoDistSettings? lowerPriority, bool allowCache = true)
        {
            return new RepoDistSettings(lowerPriority,
                GitExtSettingsCache.Create(Path.Combine(module.WorkingDir, AppSettings.SettingsFileName), allowCache),
                SettingLevel.Distributed);
        }

        public static RepoDistSettings CreateDistributed(GitModule module, bool allowCache = true)
        {
            return CreateDistributed(module, null, allowCache);
        }

        public static RepoDistSettings CreateGlobal(bool allowCache = true)
        {
            return new RepoDistSettings(null, GitExtSettingsCache.Create(AppSettings.SettingsFilePath, allowCache),
                SettingLevel.Global);
        }

        #endregion

        public override void SetValue(string name, string? value)
        {
            // For the effective level read the effective value explicitly
            // as the SetValue below just check the explicit level
            if (SettingLevel == SettingLevel.Effective && value == GetValue(name))
            {
                return;
            }

            bool isEffectiveLevel = LowerPriority?.LowerPriority is not null;
            bool isDetachedOrGlobal = LowerPriority is null;

            if (isDetachedOrGlobal || SettingsCache.HasValue(name))
            {
                // there is no lower level
                // or the setting is assigned on this level
                SettingsCache.SetValue(name, value);
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
                    if (LowerPriority!.SettingsCache.HasADifferentValue(name, value))
                    {
                        SettingsCache.SetValue(name, value);
                    }
                }
                else
                {
                    // if the setting isn't set at the Distributed level, do not set it there
                    // instead of that, set the setting at the Global level (it becomes effective then)
                    LowerPriority!.LowerPriority!.SetValue(name, value);
                }
            }
            else
            {
                // the settings is not assigned on this level, recurse to the lower level
                LowerPriority!.SetValue(name, value);
            }
        }
    }
}
