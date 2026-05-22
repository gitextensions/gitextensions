using System.ComponentModel;
using System.Text.Json;

namespace GitCommands.Settings;

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

    public static ISetting<T> Create<T>(SettingsPath settingsSource, string name)
        where T : struct
    {
        return new SettingOf<T>(settingsSource, name, default);
    }

    private sealed class SettingOf<T>(SettingsPath settingsSource, string name, T defaultValue) : ISetting<T>
    {
        public string Name { get; } = name;

        public T Default { get; } = defaultValue;

        public T Value
        {
            get => GetValue(Name) is { } value ? (T)value : Default;

            set
            {
                object? valueToBeStored = value?.Equals(Default) is true ? null : value;
                if (valueToBeStored == GetValue(Name))
                {
                    return;
                }

                SetValue(Name, valueToBeStored);
            }
        }

        public string FullPath => settingsSource.PathFor(Name);

        private object? GetValue(string name)
        {
            string? stringValue = settingsSource.GetValue(name);
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
                        return JsonSerializer
                            .Deserialize<T>(stringValue);
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
                    stringValue = JsonSerializer
                        .Serialize(value);
                    break;
                default:
                    TypeConverter converter = TypeDescriptor
                        .GetConverter(underlyingType);

                    stringValue = converter
                        .ConvertToInvariantString(value);
                    break;
            }

            settingsSource.SetValue(name, stringValue);
        }
    }
}
