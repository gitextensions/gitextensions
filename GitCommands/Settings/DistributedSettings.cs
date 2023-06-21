using System.Diagnostics;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    /// <summary>
    /// Settings that can be distributed with repository.
    /// They can be overridden for a particular repository.
    /// </summary>
    [DebuggerDisplay("{" + nameof(SettingLevel) + "}: {" + nameof(SettingsCache) + "} << {" + nameof(LowerPriority) + "}")]
    public class DistributedSettings : SettingsContainer<DistributedSettings, GitExtSettingsCache>
    {
        public DistributedSettings(DistributedSettings? lowerPriority, GitExtSettingsCache settingsCache, SettingLevel settingLevel)
            : base(lowerPriority, settingsCache)
        {
            SettingLevel = settingLevel;
        }

        #region CreateDistributedSettings

        /// <summary>
        /// Returns an effective setting object where Git Extensions settings are read from the first available of:
        /// * Local (in .git dir).
        /// * Distributed (stored in repo).
        /// * Global (user home).
        /// </summary>
        /// <param name="module">The GitModule.</param>
        /// <returns>the settings.</returns>
        public static DistributedSettings CreateEffective(GitModule module)
        {
            return CreateLocal(module, CreateDistributed(module, CreateGlobal()), SettingLevel.Effective);
        }

        private static DistributedSettings CreateLocal(GitModule module, DistributedSettings? lowerPriority, SettingLevel settingLevel, bool useSharedCache = true)
        {
            return new DistributedSettings(lowerPriority,
                GitExtSettingsCache.Create(Path.Combine(module.GitCommonDirectory, AppSettings.SettingsFileName), useSharedCache),
                settingLevel);
        }

        public static DistributedSettings CreateLocal(GitModule module, bool useSharedCache = true)
        {
            return CreateLocal(module, lowerPriority: null, SettingLevel.Local, useSharedCache);
        }

        private static DistributedSettings CreateDistributed(GitModule module, DistributedSettings? lowerPriority, bool useSharedCache = true)
        {
            return new DistributedSettings(lowerPriority,
                GitExtSettingsCache.Create(Path.Combine(module.WorkingDir, AppSettings.SettingsFileName), useSharedCache),
                SettingLevel.Distributed);
        }

        public static DistributedSettings CreateDistributed(GitModule module, bool useSharedCache = true)
        {
            return CreateDistributed(module, lowerPriority: null, useSharedCache);
        }

        public static DistributedSettings CreateGlobal(bool useSharedCache = true)
        {
            return new DistributedSettings(lowerPriority: null,
                GitExtSettingsCache.Create(AppSettings.SettingsFilePath, useSharedCache),
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
