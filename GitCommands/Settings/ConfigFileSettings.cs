﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitCommands.Config;

namespace GitCommands.Settings
{
    public class ConfigFileSettings : SettingsContainer
    {
        public ConfigFileSettings(SettingsContainer aLowerPriority, SettingsCache aSettingsCache)
            : base(aLowerPriority, aSettingsCache)
        {
            core = new CorePath(this);
        }


        public static ConfigFileSettings CreateEffective(GitModule aModule)
        {
            return CreateLocal(aModule, CreateGlobal());
        }

        public static ConfigFileSettings CreateLocal(GitModule aModule, bool allowCache = true)
        {
            return CreateLocal(aModule, null, allowCache);
        }

        private static ConfigFileSettings CreateLocal(GitModule aModule, ConfigFileSettings aLowerPriority, bool allowCache = true)
        {
            return new ConfigFileSettings(aLowerPriority,
                ConfigFileSettingsCache.Create(Path.Combine(aModule.WorkingDirGitDir(), "config"), true, allowCache));
        }

        public static ConfigFileSettings CreateGlobal(bool allowCache = true)
        {
            string configPath = Path.Combine(GitCommandHelpers.GetHomeDir(), ".config", "git", "config");
            if (!File.Exists(configPath))
                configPath = Path.Combine(GitCommandHelpers.GetHomeDir(), ".gitconfig");

            return new ConfigFileSettings(null,
                ConfigFileSettingsCache.Create(configPath, false, allowCache));
        }


        public readonly CorePath core;

        public string GetValue(string setting)
        {
            return this.GetString(setting, string.Empty);
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
                    Debug.WriteLine(string.Format("Unsupported encoding set in git config file: {0}\nPlease check the setting {1} in config file.", encodingName, settingName));
                    result = null;
                }
            }

            return result;
        }

        private void SetEncoding(string settingName, Encoding encoding)
        {
            SetValue(settingName, encoding.HeaderName);
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
}
