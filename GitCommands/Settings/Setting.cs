using System.Collections.Generic;

namespace GitCommands.Settings
{
    public abstract class Setting<T>
    {
        public readonly SettingsPath SettingsSource;
        public readonly T DefaultValue;
        public readonly string Name;

        protected Setting(string name, SettingsPath settingsSource, T defaultValue)
        {
            Name = name;
            SettingsSource = settingsSource;
            DefaultValue = defaultValue;
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

        public virtual bool ValueIsEmpty(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default);
        }

        public string FullPath => SettingsSource.PathFor(Name);
    }
}
