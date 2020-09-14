using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GitCommands.Settings
{
    public interface ISetting<T>
    {
        event EventHandler Updated;

        SettingsPath SettingsSource { get; }

        string Name { get; }

        T Default { get; }

        T ValueOrDefault { get; }

        T Value { get; set; }

        string FullPath { get; }
    }

    public static class Setting
    {
        public static ISetting<string> Create(SettingsPath settingsSource, string name, string defaultValue)
        {
            return new SettingOf<string>(settingsSource, name, defaultValue);
        }

        public static ISetting<T> Create<T>(SettingsPath settingsSource, string name, T defaultValue)
            where T : struct
        {
            return new SettingOf<T>(settingsSource, name, defaultValue);
        }

        public static ISetting<T?> Create<T>(SettingsPath settingsSource, string name)
            where T : struct
        {
            return new SettingOf<T?>(settingsSource, name);
        }

        private sealed class SettingOf<T> : ISetting<T>
        {
            public event EventHandler Updated;

            public SettingOf(SettingsPath settingsSource, string name, T defaultValue = default)
            {
                SettingsSource = settingsSource;
                Name = name;
                Default = defaultValue;
            }

            public SettingsPath SettingsSource { get; }

            public string Name { get; }

            public T Default { get; }

            public T ValueOrDefault
            {
                get
                {
                    T v = Value;

                    if (IsDefault(v))
                    {
                        return Default;
                    }
                    else
                    {
                        return v;
                    }
                }
            }

            public T Value
            {
                get => GetValue(Name, Default);
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

            public string FullPath => SettingsSource.PathFor(Name);

            private bool IsDefault(T value)
            {
                return EqualityComparer<T>.Default.Equals(value, default);
            }

            private T GetValue(string name, T defaultValue = default)
            {
                return SettingsSource
                    .GetValue(name, defaultValue, value =>
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
                SettingsSource
                    .SetValue(name, value, value =>
                    {
                        switch (Type.GetTypeCode(typeof(T)))
                        {
                            case TypeCode.String:
                                return (string)(object)value;
                            default:
                                return JsonConvert
                                    .SerializeObject(value);
                        }
                    });
            }
        }
    }
}
