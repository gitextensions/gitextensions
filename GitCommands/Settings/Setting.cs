using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    public abstract class Setting<T>
    {
        public readonly SettingsPath SettingsSource;
        public readonly T DefaultValue;
        public readonly string Name;

        public Setting(string aName, SettingsPath aSettingsSource, T aDefaultValue)
        {
            Name = aName;
            SettingsSource = aSettingsSource;
            DefaultValue = aDefaultValue;
        }

        public abstract T Value { get; set; }

        public string FullPath
        {
            get
            {
                return SettingsSource.PathFor(Name);
            }
        }
    }
}
