namespace GitCommands.Settings;

/// <summary>
///  Concrete implementation of an <see cref="ISetting{T}"/> instance
///  that stores immutable metadata and baked-in conversion functions for reading/writing string-based storage.
/// </summary>
/// <typeparam name="T">The type exposed to callers.</typeparam>
public class Setting<T> : ISetting<T>
{
    private readonly SettingsPath _settingsSource;
    private readonly Func<string, (bool success, T value)> _read;
    private readonly Func<T, string?> _store;

    /// <summary>
    ///  Creates a setting with custom conversion between the stored string and the exposed type.
    /// </summary>
    /// <param name="settingsSource">The settings path to store the value under.</param>
    /// <param name="name">The settings key name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="read">
    ///  Converts from the stored string to <typeparamref name="T"/>, returning a tuple
    ///  of <c>(success, value)</c>. When <c>success</c> is <see langword="false"/>, the default is used.
    /// </param>
    /// <param name="store">Converts from <typeparamref name="T"/> to the stored string.</param>
    public Setting(
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

    /// <summary>
    ///  Implicit conversion for direct get-access to the <see cref="Setting{T}.Value"/> property.
    /// </summary>
    /// <param name="setting">The <see cref="Setting{T}"/> whose value is returned as conversion result.</param>
    public static implicit operator T(Setting<T> setting) => setting.Value;
}
