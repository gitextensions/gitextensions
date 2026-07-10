using System.Globalization;

namespace GitCommands.Settings;

/// <summary>
///  Factory methods for <see cref="ISetting{T}"/> instances.
/// </summary>
public static class Setting
{
    /// <summary>
    ///  Creates a <see cref="string"/> setting.
    /// </summary>
    public static ISetting<string> Create(SettingsPath settingsSource, string name, string defaultValue)
    {
        return new SettingOf<string>(
            settingsSource,
            name,
            defaultValue,
            read: static s => (true, s),
            store: static v => v);
    }

    /// <summary>
    ///  Creates a nullable <see cref="string"/> setting.
    /// </summary>
    public static ISetting<string?> CreateNullableString(SettingsPath settingsSource, string name)
    {
        return new SettingOf<string?>(
            settingsSource,
            name,
            defaultValue: null,
            read: static s => (true, s),
            store: static v => v);
    }

    /// <summary>
    ///  Creates a <see cref="bool"/> setting with a default value.
    /// </summary>
    public static ISetting<bool> Create(SettingsPath settingsSource, string name, bool defaultValue)
    {
        return new SettingOf<bool>(
            settingsSource,
            name,
            defaultValue,
            read: static s => s switch { "true" or "True" => (true, true), "false" or "False" => (true, false), _ => (false, default) },
            store: static v => v ? "true" : "false");
    }

    /// <summary>
    ///  Creates a nullable <see cref="bool"/> setting.
    /// </summary>
    public static ISetting<bool?> CreateNullableBool(SettingsPath settingsSource, string name)
    {
        return new SettingOf<bool?>(
            settingsSource,
            name,
            defaultValue: null,
            read: static s => s switch { "true" or "True" => (true, true), "false" or "False" => (true, false), _ => (false, default) },
            store: static v => v switch { true => "true", false => "false", _ => null });
    }

    /// <summary>
    ///  Creates an <see cref="int"/> setting with a default value.
    /// </summary>
    public static ISetting<int> Create(SettingsPath settingsSource, string name, int defaultValue)
    {
        return new SettingOf<int>(
            settingsSource,
            name,
            defaultValue,
            read: static s => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? (true, v) : (false, default),
            store: static v => v.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    ///  Creates a <see cref="float"/> setting with a default value.
    /// </summary>
    public static ISetting<float> Create(SettingsPath settingsSource, string name, float defaultValue)
    {
        return new SettingOf<float>(
            settingsSource,
            name,
            defaultValue,
            read: static s => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? (true, v) : (false, default),
            store: static v => v.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    ///  Creates an <see cref="Enum"/> setting with a default value.
    /// </summary>
    public static ISetting<T> Create<T>(SettingsPath settingsSource, string name, T defaultValue)
        where T : struct, Enum
    {
        return new SettingOf<T>(
            settingsSource,
            name,
            defaultValue,
            read: static s => Enum.TryParse(s, out T v) ? (true, v) : (false, default),
            store: static v => v.ToString());
    }

    /// <summary>
    ///  Creates a nullable <see cref="Enum"/> setting.
    /// </summary>
    public static ISetting<T?> CreateNullableEnum<T>(SettingsPath settingsSource, string name)
        where T : struct, Enum
    {
        return new SettingOf<T?>(
            settingsSource,
            name,
            defaultValue: null,
            read: static s => Enum.TryParse(s, out T v) ? (true, v) : (false, default),
            store: static v => v?.ToString());
    }

    /// <summary>
    ///  Creates a setting with custom conversion between the stored string and the exposed type.
    /// </summary>
    /// <typeparam name="T">The type exposed to callers.</typeparam>
    /// <param name="settingsSource">The settings path to store the value under.</param>
    /// <param name="name">The settings key name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="read">
    ///  Converts from the stored string to <typeparamref name="T"/>, returning a tuple
    ///  of <c>(success, value)</c>. When <c>success</c> is <see langword="false"/>, the default is used.
    /// </param>
    /// <param name="store">Converts from <typeparamref name="T"/> to the stored string.</param>
    public static ISetting<T> Create<T>(
        SettingsPath settingsSource,
        string name,
        T defaultValue,
        Func<string, (bool success, T value)> read,
        Func<T, string?> store)
    {
        return new SettingOf<T>(settingsSource, name, defaultValue, read, store);
    }

    /// <summary>
    ///  Concrete implementation of <see cref="ISetting{T}"/> that stores immutable metadata
    ///  and baked-in conversion functions for reading/writing string-based storage.
    /// </summary>
    private sealed class SettingOf<T> : ISetting<T>
    {
        private readonly SettingsPath _settingsSource;
        private readonly Func<string, (bool success, T value)> _read;
        private readonly Func<T, string?> _store;

        public SettingOf(
            SettingsPath settingsSource,
            string name,
            T defaultValue,
            Func<string, (bool success, T value)> read,
            Func<T, string?> store)
        {
            _settingsSource = settingsSource;
            Name = name;
            Default = defaultValue;
            _read = read;
            _store = store;
        }

        public string Name { get; }

        public T Default { get; }

        public T Value
        {
            get
            {
                string? raw = _settingsSource.GetValue(Name);
                if (raw is null)
                {
                    return Default;
                }

                (bool success, T value) = _read(raw);
                return success ? value : Default;
            }

            set
            {
                string? raw = value is null ? null : _store(value);
                _settingsSource.SetValue(Name, raw);
            }
        }

        public string FullPath => _settingsSource.PathFor(Name);
    }
}
