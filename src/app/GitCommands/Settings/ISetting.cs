namespace GitCommands.Settings;

/// <summary>
///  Immutable metadata describing a persisted setting.
///  Use <see cref="Setting.GetValue{T}"/> and <see cref="Setting.SetValue{T}"/> to read and write values.
/// </summary>
/// <typeparam name="T">The type of the setting value.</typeparam>
public interface ISetting<T>
{
    /// <summary>
    ///  The settings key name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///  The settings path that provides hierarchical grouping and storage access.
    /// </summary>
    SettingsPath SettingsSource { get; }

    /// <summary>
    ///  The default value for the setting, used when no value is stored.
    /// </summary>
    T? Default { get; }

    /// <summary>
    ///  The full settings path, including the section prefix and setting name.
    /// </summary>
    string FullPath { get; }
}
