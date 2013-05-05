using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GitCommands.Settings
{
    public class ConfigFileSettings : SettingsContainer
    {
        protected ConfigFileSettings(SettingsContainer aLowerPriority, SettingsCache aSettingsCache)
            : base(aLowerPriority, aSettingsCache)
        {
            core = new CorePath(this);
        }


        public static ConfigFileSettings CreateEffective(GitModule aModule)
        {
            return CreateLocal(aModule, CreateGlobal());
        }

        public static ConfigFileSettings CreateLocal(GitModule aModule)
        {
            return CreateLocal(aModule, null);
        }

        private static ConfigFileSettings CreateLocal(GitModule aModule, ConfigFileSettings aLowerPriority)
        {
            return new ConfigFileSettings(aLowerPriority,
                ConfigFileSettingsCache.FromCache(Path.Combine(aModule.WorkingDirGitDir(), "config"), true));
        }

        public static ConfigFileSettings CreateGlobal()
        {
            string configPath = Path.Combine(GitCommandHelpers.GetHomeDir(), ".config", "git", "config");
            if (!File.Exists(configPath))
                configPath = Path.Combine(GitCommandHelpers.GetHomeDir(), ".gitconfig");

            return new ConfigFileSettings(null,
                ConfigFileSettingsCache.FromCache(configPath, false));
        }


        public readonly CorePath core;

    }

    public class CorePath : SettingsPath
    {
        public readonly EnumSetting<AutoCRLFType> autocrlf;

        public CorePath(ConfigFileSettings container)
            : base(container, "core")
        {
            autocrlf = new EnumSetting<AutoCRLFType>("autocrlf", this, AutoCRLFType.False);
        }
    }
}
