using System;
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
            this.SetString(setting, value);
        }

        public void SetPathValue(string setting, string value)
        {
            this.SetString(setting, ConfigSection.FixPath(value));
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
