namespace ResourceManager.Hotkey;

public static class ShortcutHelper
{
    /// <summary>
    ///  Returns the assigned shortcut key of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
    /// </summary>
    /// <param name="hotkeys">The collection of configured shortcut keys.</param>
    /// <param name="commandCode">The required shortcut identifier.</param>
    /// <returns>The shortcut key code, if exists; otherwise, <see cref="Keys.None"/>.</returns>
    public static Keys GetShortcutKey<T>(this IEnumerable<HotkeyCommand>? hotkeys, T commandCode) where T : notnull
        => hotkeys?.FirstOrDefault(h => h.CommandCode == (int)(object)commandCode)?.KeyData ?? Keys.None;

    /// <summary>
    ///  Returns the menu string representation of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
    /// </summary>
    /// <param name="hotkeys">The collection of configured shortcut keys.</param>
    /// <param name="commandCode">The required shortcut identifier.</param>
    /// <returns>The string representation of the shortcut, if exists; otherwise, the string representation of <see cref="Keys.None"/>, which is empty.</returns>
    public static string GetShortcutDisplay<T>(this IEnumerable<HotkeyCommand>? hotkeys, T commandCode) where T : struct, Enum
        => hotkeys.GetShortcutKey(commandCode).ToShortcutKeyDisplayString();

    /// <summary>
    ///  Returns the tooltip string representation of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
    /// </summary>
    /// <param name="hotkeys">The collection of configured shortcut keys.</param>
    /// <param name="commandCode">The required shortcut identifier.</param>
    /// <returns>The string representation of the shortcut, if exists; otherwise, the string representation of <see cref="Keys.None"/>, which is empty.</returns>
    public static string GetShortcutToolTip<T>(this IEnumerable<HotkeyCommand>? hotkeys, T commandCode) where T : struct, Enum
        => hotkeys.GetShortcutKey(commandCode).ToShortcutKeyToolTipString();
}
