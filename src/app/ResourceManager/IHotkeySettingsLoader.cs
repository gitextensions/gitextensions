namespace ResourceManager;

/// <summary>
///  Provides the ability to load hotkeys for the given setting.
/// </summary>
public interface IHotkeySettingsLoader
{
    /// <summary>
    ///  Loads hotkeys configured under the given setting.
    /// </summary>
    /// <param name="hotkeySettingsName">The setting name.</param>
    /// <returns>The collection of the configured hotkeys.</returns>
    IReadOnlyList<HotkeyCommand> LoadHotkeys(string hotkeySettingsName);
}
