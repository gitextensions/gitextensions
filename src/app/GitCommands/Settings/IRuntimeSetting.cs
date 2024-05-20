namespace GitCommands.Settings;

/// <summary>
///  Interface for settings which are persisted on explicit request only.
/// </summary>
public interface IRuntimeSetting
{
    /// <summary>
    ///  Reloads the value from the settings file.
    /// </summary>
    void Reload();

    /// <summary>
    ///  Resets the value to the hard-coded default value.
    /// </summary>
    void ResetToDefault();

    /// <summary>
    ///  Saves the value to the persistent setting,
    ///  which is not written to the settings file at once but latest on app exit.
    /// </summary>
    void Save();
}
