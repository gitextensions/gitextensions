using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace GitCommands.Settings
{
    public interface ISetting<T>
    {
        /// <summary>
        /// Event triggered after settings update.
        /// </summary>
        event EventHandler Updated;

        /// <summary>
        /// Settings provider.
        /// </summary>
        SettingsPath SettingsSource { get; }

        /// <summary>
        /// Name of the setting.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Default value for setting type.
        /// For nullable except "string" is default(T).
        /// For "string" is the defaultValue ?? string.Empty from constructor.
        /// For non nullable is the defaultValue from constructor.
        /// </summary>
        T? Default { get; }

        /// <summary>
        /// Value of the setting.
        /// For nullable except "string" is the value from storage.
        /// For "string" is the value from storage or <see cref="Default"/>.
        /// For non nullable is the value from storage or <see cref="Default"/>.
        /// </summary>
        T? Value { get; set; }

        /// <summary>
        /// Value of the setting.
        /// For nullable except "string" always false (null is value too).
        /// For "string" is true when the stored value is null or is false when the stored value not null.
        /// For non nullable is true when the stored value is null or is false when the stored value not null.
        /// </summary>
        bool IsUnset { get; }

        /// <summary>
        /// Full name of the setting.
        /// Includes section name and setting name.
        /// </summary>
        string FullPath { get; }
    }

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
                    var storedValue = GetValue(Name);

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
                    var storedValue = GetValue(Name);

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

                    var storedValue = GetValue(Name);

                    return storedValue is null;
                }
            }

            /// <inheritdoc />
            public string FullPath => SettingsSource.PathFor(Name);

            private object? GetValue(string name)
            {
                string? stringValue = SettingsSource.GetValue(name);

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
