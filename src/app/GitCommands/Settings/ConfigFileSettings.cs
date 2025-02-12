#nullable enable

using System.Diagnostics;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;

namespace GitCommands.Settings
{
    [DebuggerDisplay("{" + nameof(SettingLevel) + "}: {" + nameof(SettingsCache) + "} << {" + nameof(LowerPriority) + "}")]
    public sealed class ConfigFileSettings : SettingsContainer<ConfigFileSettings, ConfigFileSettingsCache>, IConfigFileSettings
    {
        public ConfigFileSettings(ConfigFileSettings? lowerPriority, ConfigFileSettingsCache settingsCache, SettingLevel settingLevel)
            : base(lowerPriority, settingsCache)
        {
            SettingLevel = settingLevel;
        }

        public static ConfigFileSettings CreateLocal(IGitModule module, bool useSharedCache = true)
        {
            return CreateLocal(module, lowerPriority: null, SettingLevel.Local, useSharedCache);
        }

        private static ConfigFileSettings CreateLocal(IGitModule module, ConfigFileSettings? lowerPriority, SettingLevel settingLevel, bool useSharedCache = true)
        {
            return new ConfigFileSettings(lowerPriority,
                ConfigFileSettingsCache.Create(Path.Combine(module.GitCommonDirectory, "config"), useSharedCache),
                settingLevel);
        }

        public static ConfigFileSettings CreateGlobal(bool useSharedCache = true)
        {
            return CreateGlobal(lowerPriority: null, useSharedCache);
        }

        public static ConfigFileSettings CreateGlobal(ConfigFileSettings? lowerPriority, bool useSharedCache = true)
        {
            string configPath = Path.Combine(EnvironmentConfiguration.GetHomeDir(), ".config", "git", "config");
            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(EnvironmentConfiguration.GetHomeDir(), ".gitconfig");
            }

            return new ConfigFileSettings(lowerPriority,
                ConfigFileSettingsCache.Create(configPath, useSharedCache),
                SettingLevel.Global);
        }

        /// <summary>
        /// Gets all configured values for a git setting that accepts multiple values for the same key.
        /// </summary>
        /// <param name="setting">The git setting key</param>
        /// <returns>The collection of all the <see cref="string"/> values.</returns>
        public IReadOnlyList<string> GetValues(string setting) => SettingsCache.GetValues(setting);

        public new void SetValue(string setting, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // to remove setting
                value = null;
            }

            SetString(setting, value);
        }

        public IReadOnlyList<IConfigSection> GetConfigSections()
        {
            return SettingsCache.GetConfigSections();
        }

        /// <summary>
        /// Adds the specific configuration section to the .git/config file.
        /// </summary>
        /// <param name="configSection">The configuration section.</param>
        public void AddConfigSection(IConfigSection configSection)
        {
            SettingsCache.AddConfigSection(configSection);
        }

        /// <summary>
        /// Removes the specific configuration section from the .git/config file.
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section.</param>
        /// <param name="performSave">If <see langword="true"/> the configuration changes will be saved immediately.</param>
        public void RemoveConfigSection(string configSectionName, bool performSave = false)
        {
            SettingsCache.RemoveConfigSection(configSectionName, performSave);
        }
    }
}
