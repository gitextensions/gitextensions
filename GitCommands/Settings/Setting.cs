using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands.Settings
{
    public abstract class Setting<T>
    {
        public readonly ISettingsSource SettingsSource;
        public readonly T DefaultValue;
        public readonly string Name;

        public Setting(string aName, ISettingsSource aSettingsSource, T aDefaultValue)
        {
            Name = aName;
            SettingsSource = aSettingsSource;
            DefaultValue = aDefaultValue;
        }

        public abstract T Value { get; set; }
    }
}
