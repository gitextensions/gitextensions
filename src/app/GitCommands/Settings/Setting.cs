using System.ComponentModel;
using System.Text.Json;

namespace GitCommands.Settings;

/// <summary>
///  Factory and accessor methods for <see cref="ISetting{T}"/> instances.
/// </summary>
public static class Setting
{
    /// <summary>
    ///  Creates a string setting.
    /// </summary>
    public static ISetting<string> Create(SettingsPath settingsSource, string name, string? defaultValue)
    {
        return new SettingOf<string>(settingsSource, name, defaultValue ?? string.Empty);
    }

    /// <summary>
    ///  Creates a non-nullable value-type setting with a default value.
    /// </summary>
    public static ISetting<T> Create<T>(SettingsPath settingsSource, string name, T defaultValue)
        where T : struct
    {
        return new SettingOf<T>(settingsSource, name, defaultValue);
    }

    /// <summary>
    ///  Creates a nullable value-type setting with no default.
    /// </summary>
    public static ISetting<T?> Create<T>(SettingsPath settingsSource, string name)
        where T : struct
    {
        return new SettingOf<T?>(settingsSource, name);
    }

    /// <summary>
    ///  Creates a setting that transforms values between a storage representation and the exposed type.
    /// </summary>
    /// <typeparam name="TStored">The type used for storage (serialized form).</typeparam>
    /// <typeparam name="TExposed">The type exposed to callers.</typeparam>
    /// <param name="settingsSource">The settings path to store the value under.</param>
    /// <param name="name">The settings key name.</param>
    /// <param name="defaultValue">The default value in <typeparamref name="TExposed"/> form.</param>
    /// <param name="read">Converts from stored to exposed form.</param>
    /// <param name="store">Converts from exposed to stored form.</param>
    public static ISetting<TExposed> CreateIntercepted<TStored, TExposed>(
        SettingsPath settingsSource,
        string name,
        TExposed defaultValue,
        Func<TStored, TExposed> read,
        Func<TExposed, TStored> store)
    {
        ISetting<TStored> inner = new SettingOf<TStored>(settingsSource, name, store(defaultValue));
        return new InterceptedSetting<TStored, TExposed>(inner, defaultValue, read, store);
    }

    /// <summary>
    ///  Reads the current value of a non-nullable value-type setting from storage.
    ///  Returns the setting's default when no value is stored.
    /// </summary>
    internal static T GetValue<T>(ISetting<T> setting) where T : struct
    {
        return ((ISettingAccessor<T>)setting).GetValue();
    }

    /// <summary>
    ///  Reads the current value of a string setting from storage.
    ///  Returns the setting's default when no value is stored.
    /// </summary>
    internal static string GetValue(ISetting<string> setting)
    {
        return ((ISettingAccessor<string>)setting).GetValue();
    }

    /// <summary>
    ///  Reads the current value of a nullable value-type setting from storage.
    /// </summary>
    internal static T? GetNullableValue<T>(ISetting<T?> setting) where T : struct
    {
        return ((ISettingAccessor<T?>)setting).GetValue();
    }

    /// <summary>
    ///  Reads the current value of a setting from storage (unconstrained).
    ///  Used internally by <see cref="RuntimeSetting{T}"/> and <see cref="InterceptedSetting{TStored, TExposed}"/>.
    /// </summary>
    internal static T GetRawValue<T>(ISetting<T> setting)
    {
        return ((ISettingAccessor<T>)setting).GetValue();
    }

    /// <summary>
    ///  Writes a value to storage for the given setting.
    /// </summary>
    internal static void SetValue<T>(ISetting<T> setting, T? value)
    {
        ((ISettingAccessor<T>)setting).SetValue(value);
    }

    /// <summary>
    ///  Gets whether the setting has no value in storage.
    /// </summary>
    internal static bool IsUnset<T>(ISetting<T> setting)
    {
        return ((ISettingAccessor<T>)setting).IsUnset;
    }

    private sealed class SettingOf<T> : ISetting<T>, ISettingAccessor<T>
    {
        public SettingOf(SettingsPath settingsSource, string name, T? defaultValue = default)
        {
            SettingsSource = settingsSource;
            Name = name;
            Default = defaultValue;
        }

        public SettingsPath SettingsSource { get; }

        public string Name { get; }

        public T? Default { get; }

        public string FullPath => SettingsSource.PathFor(Name);

        T ISettingAccessor<T>.GetValue()
        {
            object? storedValue = ReadFromStorage(Name);

            if (default(T) is null)
            {
                if (Type.GetTypeCode(typeof(T)) != TypeCode.String)
                {
                    return (T)(object?)storedValue!;
                }
            }

            if (storedValue is null)
            {
                return Default!;
            }

            return (T)storedValue;
        }

        void ISettingAccessor<T>.SetValue(T? value)
        {
            object? storedValue = ReadFromStorage(Name);

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
                WriteToStorage(Name, (object?)value ?? string.Empty);
            }
            else
            {
                WriteToStorage(Name, value);
            }
        }

        bool ISettingAccessor<T>.IsUnset
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

                object? storedValue = ReadFromStorage(Name);

                return storedValue is null;
            }
        }

        private object? ReadFromStorage(string name)
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
                        return JsonSerializer.Deserialize<T>(stringValue);
                    }
                    catch
                    {
                        return null;
                    }

                default:
                    TypeConverter converter = TypeDescriptor.GetConverter(underlyingType);

                    try
                    {
                        return converter.ConvertFromInvariantString(stringValue);
                    }
                    catch
                    {
                        return null;
                    }
            }
        }

        private void WriteToStorage(string name, object? value)
        {
            string? stringValue;

            Type type = typeof(T);
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            switch (Type.GetTypeCode(underlyingType))
            {
                case TypeCode.String:
                    stringValue = (string?)value;
                    break;
                case TypeCode.Object:
                    stringValue = JsonSerializer.Serialize(value);
                    break;
                default:
                    TypeConverter converter = TypeDescriptor.GetConverter(underlyingType);
                    stringValue = converter.ConvertToInvariantString(value);
                    break;
            }

            SettingsSource.SetValue(name, stringValue);
        }
    }

    private sealed class InterceptedSetting<TStored, TExposed>(
        ISetting<TStored> inner,
        TExposed defaultValue,
        Func<TStored, TExposed> read,
        Func<TExposed, TStored> store) : ISetting<TExposed>, ISettingAccessor<TExposed>
    {
        public string Name => inner.Name;

        public SettingsPath SettingsSource => inner.SettingsSource;

        public TExposed? Default => defaultValue;

        public string FullPath => inner.FullPath;

        TExposed ISettingAccessor<TExposed>.GetValue()
        {
            return IsUnset(inner) ? defaultValue! : read(GetRawValue(inner)!);
        }

        void ISettingAccessor<TExposed>.SetValue(TExposed? value)
        {
            Setting.SetValue(inner, value is not null ? store(value) : default);
        }

        bool ISettingAccessor<TExposed>.IsUnset => Setting.IsUnset(inner);
    }
}
