using System.ComponentModel;
using Newtonsoft.Json;

namespace GitCommands.Settings
{
    public static class Setting
    {
        public static ISetting<string> Create(SettingsPath settingsSource, string name, string? defaultValue)
        {
            return new SettingOf<string>(settingsSource, name, defaultValue ?? string.Empty);
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
            /// <inheritdoc />
            public event EventHandler? Updated;

            public SettingOf(SettingsPath settingsSource, string name, T? defaultValue = default)
            {
                SettingsSource = settingsSource;
                Name = name;
                Default = defaultValue;
            }

            /// <inheritdoc />
            public SettingsPath SettingsSource { get; }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public T? Default { get; }

            /// <inheritdoc />
            public T? Value
            {
                get
                {
                    object storedValue = GetValue(Name);

                    if (default(T) is null)
                    {
                        if (Type.GetTypeCode(typeof(T)) != TypeCode.String)
                        {
                            return (T?)storedValue!;
                        }
                    }

                    if (storedValue is null)
                    {
                        return Default;
                    }

                    return (T)storedValue;
                }

                set
                {
                    object storedValue = GetValue(Name);

                    if (Type.GetTypeCode(typeof(T)) == TypeCode.String)
                    {
                        if (storedValue?.Equals((object?)value ?? string.Empty) ?? false)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (storedValue?.Equals(value) ?? ((default(T) is null) && (value is null)))
                        {
                            return;
                        }
                    }

                    if (Type.GetTypeCode(typeof(T)) == TypeCode.String)
                    {
                        SetValue(Name, (object?)value ?? string.Empty);
                    }
                    else
                    {
                        SetValue(Name, value);
                    }

                    Updated?.Invoke(this, EventArgs.Empty);
                }
            }

            /// <inheritdoc />
            public bool IsUnset
            {
                get
                {
                    if (default(T) is null)
                    {
                        if (Type.GetTypeCode(typeof(T)) != TypeCode.String)
                        {
                            return false;
                        }
                    }

                    object storedValue = GetValue(Name);

                    return storedValue is null;
                }
            }

            /// <inheritdoc />
            public string FullPath => SettingsSource.PathFor(Name);

            private object? GetValue(string name)
            {
                string? stringValue = SettingsSource.GetValue(name);
                if (stringValue is null)
                {
                    return null;
                }

                Type type = typeof(T);
                Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

                switch (Type.GetTypeCode(underlyingType))
                {
                    case TypeCode.String:
                        return stringValue;
                    case TypeCode.Object:
                        try
                        {
                            return JsonConvert
                                .DeserializeObject<T>(stringValue);
                        }
                        catch
                        {
                            return null;
                        }

                    default:
                        TypeConverter converter = TypeDescriptor
                            .GetConverter(underlyingType);

                        try
                        {
                            return converter
                                .ConvertFromInvariantString(stringValue);
                        }
                        catch
                        {
                            return null;
                        }
                }
            }

            private void SetValue(string name, object? value)
            {
                string? stringValue;

                Type type = typeof(T);
                Type underlyingType = Nullable
                    .GetUnderlyingType(type) ?? type;

                switch (Type.GetTypeCode(underlyingType))
                {
                    case TypeCode.String:
                        stringValue = (string?)value;
                        break;
                    case TypeCode.Object:
                        stringValue = JsonConvert
                            .SerializeObject(value);
                        break;
                    default:
                        TypeConverter converter = TypeDescriptor
                            .GetConverter(underlyingType);

                        stringValue = converter
                            .ConvertToInvariantString(value);
                        break;
                }

                SettingsSource.SetValue(name, stringValue);
            }
        }
    }
}
