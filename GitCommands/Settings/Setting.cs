using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GitCommands.Settings
{
    public abstract class Setting<T>
    {
        public readonly SettingsPath SettingsSource;
        public readonly T DefaultValue;
        public readonly string Name;

        public event EventHandler Updated;

        protected Setting(string name, SettingsPath settingsSource, T defaultValue)
        {
            Name = name;
            SettingsSource = settingsSource;
            DefaultValue = defaultValue;
        }

        public virtual T Value
        {
            get => GetValue(Name, DefaultValue);
            set
            {
                if (Value.Equals(value))
                {
                    return;
                }

                SetValue(Name, value);
                Updated?.Invoke(this, EventArgs.Empty);
            }
        }

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

        private T GetValue(string name, T defaultValue = default)
        {
            return SettingsSource.GetValue(name, defaultValue, value =>
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.String:
                        return (T)(object)value;
                    default:
                        return JsonConvert
                            .DeserializeObject<T>(value) ?? defaultValue;
                }
            });
        }

        private void SetValue(string name, T value)
        {
            SettingsSource.SetValue(name, value, value =>
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.String:
                        var stringValue = (string)(object)value;

                        return string.IsNullOrEmpty(stringValue)
                            ? null
                            : stringValue;
                    default:
                        return JsonConvert
                            .SerializeObject(value);
                }
            });
        }
    }
}
