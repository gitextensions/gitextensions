using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GitCommands.Config;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    public class ConfigFileSettings : SettingsContainer<ConfigFileSettings, ConfigFileSettingsCache>, ISettingsValueGetter
    {
        public ConfigFileSettings(ConfigFileSettings aLowerPriority, ConfigFileSettingsCache aSettingsCache)
            : base(aLowerPriority, aSettingsCache)
        {
            core = new CorePath(this);
            mergetool = new MergeToolPath(this);
        }

        public static ConfigFileSettings CreateEffective(GitModule aModule)
        {
            return CreateLocal(aModule, CreateGlobal(CreateSystemWide()));
        }

        public static ConfigFileSettings CreateLocal(GitModule aModule, bool allowCache = true)
        {
            return CreateLocal(aModule, null, allowCache);
        }

        private static ConfigFileSettings CreateLocal(GitModule aModule, ConfigFileSettings aLowerPriority, bool allowCache = true)
        {
            return new ConfigFileSettings(aLowerPriority,
                ConfigFileSettingsCache.Create(Path.Combine(aModule.GetGitDirectory(), "config"), true, allowCache));
        }

        public static ConfigFileSettings CreateGlobal(bool allowCache = true)
        {
            return CreateGlobal(null, allowCache);
        }

        public static ConfigFileSettings CreateGlobal(ConfigFileSettings aLowerPriority, bool allowCache = true)
        {
            string configPath = Path.Combine(GitCommandHelpers.GetHomeDir(), ".config", "git", "config");
            if (!File.Exists(configPath))
                configPath = Path.Combine(GitCommandHelpers.GetHomeDir(), ".gitconfig");

            return new ConfigFileSettings(aLowerPriority,
                ConfigFileSettingsCache.Create(configPath, false, allowCache));
        }

        public static ConfigFileSettings CreateSystemWide(bool allowCache = true)
        {
            // Git 2.xx
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Git", "config");
            if (!File.Exists(configPath))
            {
                // Git 1.xx
                configPath = Path.Combine(AppSettings.GitBinDir, "..", "etc", "gitconfig");
                if (!File.Exists(configPath))
                    return null;
            }

            return new ConfigFileSettings(null,
                ConfigFileSettingsCache.Create(configPath, false, allowCache));
        }


        public readonly CorePath core;
        public readonly MergeToolPath mergetool;

        public string GetValue(string setting)
        {
            return this.GetString(setting, string.Empty);
        }

        public IList<string> GetValues(string setting)
        {
            return SettingsCache.GetValues(setting);
        }

        public void SetValue(string setting, string value)
        {
            if (value.IsNullOrEmpty())
            {
                //to remove setting
                value = null;
            }

            this.SetString(setting, value);
        }

        public void SetPathValue(string setting, string value)
        {
            SetValue(setting, ConfigSection.FixPath(value));
        }

        public IList<ConfigSection> GetConfigSections()
        {
            return SettingsCache.GetConfigSections();
        }

        public void RemoveConfigSection(string configSectionName)
        {
            SettingsCache.RemoveConfigSection(configSectionName);
        }

        public Encoding FilesEncoding
        {
            get
            {
                return GetEncoding("i18n.filesEncoding");
            }

            set
            {
                SetEncoding("i18n.filesEncoding", value);
            }
        }

        public Encoding CommitEncoding
        {
            get
            {
                return GetEncoding("i18n.commitEncoding");
            }
        }

        public Encoding LogOutputEncoding
        {
            get
            {
                return GetEncoding("i18n.logoutputencoding");
            }
        }

        private Encoding GetEncoding(string settingName)
        {
            Encoding result;

            string encodingName = GetValue(settingName);

            if (string.IsNullOrEmpty(encodingName))
                result = null;
            else if (!AppSettings.AvailableEncodings.TryGetValue(encodingName, out result))
            {
                try
                {
                    result = Encoding.GetEncoding(encodingName);
                }
                catch (ArgumentException)
                {
                    Debug.WriteLine("Unsupported encoding set in git config file: {0}\n" +
                        "Please check the setting {1} in config file.", encodingName, settingName);
                    result = null;
                }
            }

            return result;
        }

        private void SetEncoding(string settingName, Encoding encoding)
        {
            SetValue(settingName, encoding == null ? null : encoding.HeaderName);
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
