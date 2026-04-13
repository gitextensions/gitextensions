namespace GitCommands.Settings;

/// <summary>
///  Represents a setting which has to be explicitly saved, if changed at runtime.
/// </summary>
/// <typeparam name="T">The type of setting.</typeparam>
public interface IRuntimeSetting<T> : ISetting<T>, IRuntimeSetting
{
    /// <summary>
    ///  The current in-memory value.
    /// </summary>
    T? Value { get; set; }

    /// <summary>
    ///  Optionally calls <cref>Reload</cref> and returns the current value.
    /// </summary>
    public T GetValue(bool reload = false);
}

/// <summary>
///  Represents a setting which has to be explicitly saved, if changed at runtime.
/// </summary>
/// <typeparam name="T">The type of setting.</typeparam>
public class RuntimeSetting<T> : IRuntimeSetting<T>
{
    private bool _loaded;
    private readonly ISetting<T> _persistentSetting;
    private T? _value = default;

    /// <summary>
    ///  Initializes a new instance of the <see cref="RuntimeSetting{T}"/> class.
    /// </summary>
    /// <param name="persistentSetting">
    ///  The <see cref="ISetting{T}"/> instance used to persist the default value of the setting.
    /// </param>
    public RuntimeSetting(ISetting<T> persistentSetting)
    {
        _persistentSetting = persistentSetting;
    }

    public T Default => _persistentSetting.Default!;

    public string FullPath => _persistentSetting.FullPath;

    public string Name => _persistentSetting.Name;

    public SettingsPath SettingsSource => _persistentSetting.SettingsSource;

    public T? Value
    {
        get => GetValue();
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
            {
                return;
            }

            _value = value;
            if (_loaded)
            {
                Updated?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? Updated;

    public T GetValue(bool reload = false)
    {
        if (reload || !_loaded)
        {
            Reload();
        }

        return _value!;
    }

    public void Reload()
    {
        Value = Setting.GetRawValue(_persistentSetting);
        _loaded = true;
    }

    public void ResetToDefault() => Value = Default;

    public void Save()
    {
        Setting.SetValue(_persistentSetting, Value);
    }

    /// <summary>
    ///  Implicit conversion for direct access to the RuntimeSetting value.
    /// </summary>
    /// <param name="setting">The RuntimeSetting whose value is returned as conversion result.</param>
    public static implicit operator T(RuntimeSetting<T> setting) => setting.Value!;
}
