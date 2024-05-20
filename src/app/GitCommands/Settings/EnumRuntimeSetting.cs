namespace GitCommands.Settings;

/// <inheritdoc />
public sealed class EnumRuntimeSetting<T> : RuntimeSetting<T> where T : struct, Enum
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="BoolRuntimeSetting"/> class with the settings details.
    /// </summary>
    /// <param name="settingsSource">The source containing the value.</param>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultValue">The default value.</param>
    public EnumRuntimeSetting(SettingsPath settingsSource, string name, T defaultValue)
        : base(Setting.Create(settingsSource, name, defaultValue))
    {
    }
}
