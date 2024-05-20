namespace GitCommands.Settings;

/// <inheritdoc />
public sealed class BoolRuntimeSetting : RuntimeSetting<bool>
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="BoolRuntimeSetting"/> class with the settings details.
    /// </summary>
    /// <param name="settingsSource">The source containing the value.</param>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default value.</param>
    public BoolRuntimeSetting(SettingsPath settingsSource, string name, bool defaultValue)
        : base(Setting.Create(settingsSource, name, defaultValue))
    {
    }

    /// <summary>
    ///  Toggles the setting value.
    /// </summary>
    public void Toggle()
    {
        Value = !Value;
    }
}
