using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using GitCommands.Config;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Settings
{
    public class ConfigFileSettings : SettingsContainer<ConfigFileSettings, ConfigFileSettingsCache>, IConfigFileSettings
    {
        public ConfigFileSettings(ConfigFileSettings lowerPriority, ConfigFileSettingsCache settingsCache)
            : base(lowerPriority, settingsCache)
        {
            core = new CorePath(this);
            mergetool = new MergeToolPath(this);
        }

        public static ConfigFileSettings CreateEffective(GitModule module)
        {
            return CreateLocal(module, CreateGlobal(CreateSystemWide()));
        }

        public static ConfigFileSettings CreateLocal(GitModule module, bool allowCache = true)
        {
            return CreateLocal(module, null, allowCache);
        }

        private static ConfigFileSettings CreateLocal(GitModule module, ConfigFileSettings lowerPriority, bool allowCache = true)
        {
            return new ConfigFileSettings(lowerPriority,
                ConfigFileSettingsCache.Create(Path.Combine(module.GitCommonDirectory, "config"), true, allowCache));
        }

        public static ConfigFileSettings CreateGlobal(bool allowCache = true)
        {
            return CreateGlobal(null, allowCache);
        }

        public static ConfigFileSettings CreateGlobal(ConfigFileSettings lowerPriority, bool allowCache = true)
        {
            string configPath = Path.Combine(EnvironmentConfiguration.GetHomeDir(), ".config", "git", "config");
            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(EnvironmentConfiguration.GetHomeDir(), ".gitconfig");
            }

            return new ConfigFileSettings(lowerPriority,
                ConfigFileSettingsCache.Create(configPath, false, allowCache));
        }

        [CanBeNull]
        public static ConfigFileSettings CreateSystemWide(bool allowCache = true)
        {
            // Git 2.xx
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Git", "config");
            if (!File.Exists(configPath))
            {
                // Git 1.xx
                configPath = Path.Combine(AppSettings.GitBinDir, "..", "etc", "gitconfig");
                if (!File.Exists(configPath))
                {
                    return null;
                }
            }

            return new ConfigFileSettings(null,
                ConfigFileSettingsCache.Create(configPath, false, allowCache));
        }

        public readonly CorePath core;
        public readonly MergeToolPath mergetool;

        public string GetValue(string setting)
        {
            return GetString(setting, string.Empty);
        }

        public IReadOnlyList<string> GetValues(string setting)
        {
            return SettingsCache.GetValues(setting);
        }

        public void SetValue(string setting, [CanBeNull] string value)
        {
            if (value.IsNullOrEmpty())
            {
                // to remove setting
                value = null;
            }

            SetString(setting, value);
        }

        public void SetPathValue(string setting, [NotNull] string value)
        {
            SetValue(setting, ConfigSection.FixPath(value));
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

        [CanBeNull]
        public Encoding FilesEncoding
        {
            get => GetEncoding("i18n.filesEncoding");
            set => SetEncoding("i18n.filesEncoding", value);
        }

        [CanBeNull]
        public Encoding CommitEncoding => GetEncoding("i18n.commitEncoding");

        [CanBeNull]
        public Encoding LogOutputEncoding => GetEncoding("i18n.logoutputencoding");

        [CanBeNull]
        private Encoding GetEncoding(string settingName)
        {
            string encodingName = GetValue(settingName);

            if (string.IsNullOrEmpty(encodingName))
            {
                return null;
            }

            if (AppSettings.AvailableEncodings.TryGetValue(encodingName, out var result))
            {
                return result;
            }

            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine(
                    "Unsupported encoding set in git config file: {0}\n" +
                    "Please check the setting {1} in config file.", encodingName, settingName);
                return null;
            }
        }

        private void SetEncoding(string settingName, [CanBeNull] Encoding encoding)
        {
            SetValue(settingName, encoding?.HeaderName);
        }
    }

    public class CorePath : SettingsPath
    {
        public readonly EnumNullableSetting<AutoCRLFType> autocrlf;

        public CorePath(ConfigFileSettings container)
            : base(container, "core")
        {
            autocrlf = new EnumNullableSetting<AutoCRLFType>("autocrlf", this);
        }
    }

    public class MergeToolPath : SettingsPath
    {
        public readonly BoolNullableSetting keepBackup;

        public MergeToolPath(ConfigFileSettings container)
            : base(container, "mergetool")
        {
            keepBackup = new BoolNullableSetting("keepBackup", this, true);
        }
    }
}
