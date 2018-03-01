using System.Collections.Generic;

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

        public T ValueOrDefault
        {
            get
            {
                T v = Value;
                if (ValueIsEmpty(v))
                {
                    return DefaultValue;
                }
                else
                {
                    return v;
                }
            }
        }

        public virtual bool ValueIsEmpty(T aValue)
        {
            return EqualityComparer<T>.Default.Equals(aValue, default);
        }

        public string FullPath => SettingsSource.PathFor(Name);
    }
}
