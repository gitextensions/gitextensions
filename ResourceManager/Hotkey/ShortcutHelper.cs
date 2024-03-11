namespace ResourceManager.Hotkey
{
    public static class ShortcutHelper
    {
        /// <summary>
        ///  Returns the string representation of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
        /// </summary>
        /// <param name="hotkeys">The collection of configured shortcut keys.</param>
        /// <param name="commandCode">The required shortcut identifier.</param>
        /// <returns>The string representation of the shortcut, if exists; otherwise, the string representation of <see cref="Keys.None"/>.</returns>
        public static string GetShortcutToolTip<T>(this IEnumerable<HotkeyCommand>? hotkeys, T commandCode) where T : struct, Enum
            => hotkeys.GetShortcutKey(commandCode).ToShortcutKeyToolTipString();

        /// <summary>
        ///  Returns the string representation of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
        /// </summary>
        /// <param name="hotkeys">The collection of configured shortcut keys.</param>
        /// <param name="commandCode">The required shortcut identifier.</param>
        /// <returns>The string representation of the shortcut, if exists; otherwise, the string representation of <see cref="Keys.None"/>.</returns>
        public static string GetShortcutDisplay<T>(this IEnumerable<HotkeyCommand>? hotkeys, T commandCode) where T : struct, Enum
            => hotkeys.GetShortcutKey(commandCode).ToShortcutKeyDisplayString();

        /// <summary>
        ///  Returns the string representation of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
        /// </summary>
        /// <param name="hotkeys">The collection of configured shortcut keys.</param>
        /// <param name="commandCode">The required shortcut identifier.</param>
        /// <returns>The string representation of the shortcut, if exists; otherwise, the string representation of <see cref="Keys.None"/>.</returns>
        public static Keys GetShortcutKey<T>(this IEnumerable<HotkeyCommand>? hotkeys, T commandCode) where T : struct, Enum
            => hotkeys?.FirstOrDefault(h => h.CommandCode == (int)(object)commandCode)?.KeyData ?? Keys.None;
    }
}
