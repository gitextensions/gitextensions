using GitExtUtils;
using GitUIPluginInterfaces;
using ResourceManager.Hotkey;

namespace ResourceManager
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>
    ///  Provides hotkey plumbing for Git Extensions <see cref="UserControl"/>s.
    /// </summary>
    public class GitExtensionsControl : TranslatedControl
    {
        private IReadOnlyList<HotkeyCommand>? _hotkeys;

        private bool _serviceProviderLoaded = false;

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();
            _serviceProviderLoaded = true;
        }

        /// <summary>
        ///  Attempts to find an instance of <see cref="IServiceProvider"/>.
        /// </summary>
        /// <remark>
        ///  The instance of <see cref="IServiceProvider"/>
        ///  either directly assigned to the control (if the control implements <see cref="IGitModuleControl"/>)
        ///  or to the parent form (if the form implements <see cref="IGitModuleForm"/>).
        /// </remark>>
        /// <returns>
        ///  The found instance of <see cref="IServiceProvider"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///  If this control is not a <see cref="IGitModuleControl"/>) and is not placed on a <see cref="IGitModuleForm"/>.
        /// </exception>
        protected IServiceProvider ServiceProvider
            => this is IGitModuleControl control ? control.UICommands
                : FindForm() is IGitModuleForm form ? form.UICommands
                : throw new InvalidOperationException($"no chance to get {nameof(ServiceProvider)}");

        #region Hotkeys

        /// <summary>
        /// Checks if the control wants to handle the key and execute that hotkey
        /// (without propagating an unhandled key to the base class function as in <cref>ProcessCmdKey</cref>).
        /// Can be overridden in order to execute a hotkey for a (visible) subcontrol
        /// (if this focused/queried control does not have such an (active) hotkey itself).
        /// </summary>
        public virtual bool ProcessHotkey(Keys keyData)
        {
            if (!HotkeysEnabled)
            {
                return false;
            }

            HotkeyCommand? hotkey = _hotkeys?.FirstOrDefault(hotkey => hotkey?.KeyData == keyData);
            return hotkey is not null && ExecuteCommand(hotkey.CommandCode);
        }

        /// <summary>
        ///  Gets the currently loaded hotkeys.
        /// </summary>
        protected bool HotkeysEnabled { get; set; }

        /// <summary>
        ///  Loads hotkeys for the specified configuration setting.
        /// </summary>
        /// <param name="hotkeySettingsName">The setting name.</param>
        protected void LoadHotkeys(string hotkeySettingsName)
        {
            _hotkeys = GetHotkeys(hotkeySettingsName);
        }

        /// <summary>
        ///  Get hotkeys for the specified configuration setting.
        /// </summary>
        /// <param name="hotkeySettingsName">The setting name.</param>
        protected IReadOnlyList<HotkeyCommand> GetHotkeys(string hotkeySettingsName)
        {
            if (!HotkeysEnabled || !_serviceProviderLoaded)
            {
                // Hotkeys shall be loaded by all controls in OnRuntimeLoad
                return [];
            }

            return ServiceProvider.GetRequiredService<IHotkeySettingsLoader>().LoadHotkeys(hotkeySettingsName);
        }

        /// <summary>Checks if a hotkey wants to handle the key before letting the message propagate.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return ProcessHotkey(keyData) || base.ProcessCmdKey(ref msg, keyData);
        }

        protected Keys GetShortcutKeys(int commandCode)
            => _hotkeys.GetShortcutKey(commandCode);

        public string GetShortcutKeyDisplayString<T>(T commandCode) where T : struct, Enum
            => _hotkeys.GetShortcutDisplay(commandCode);

        protected void UpdateTooltipWithShortcut<T>(ToolStripItem button, T commandCode) where T : struct, Enum
        {
            button.ToolTipText = button.ToolTipText.UpdateSuffix(_hotkeys.GetShortcutToolTip(commandCode));
        }

        /// <summary>
        /// Override this method to handle form-specific Hotkey commands.
        /// <remarks>This base method does nothing and returns <see langword="false"/>.</remarks>
        /// </summary>
        protected virtual bool ExecuteCommand(int command)
        {
            return false;
        }

        /// <summary>
        /// Returns whether the given [combination of] key[s] represents a keypress which is used for text input by default.
        /// <remarks>Can be used to ignore hotkeys which would prevent from typing text into an input control if it's focused.</remarks>
        /// </summary>
        /// <param name="multiLine">Should be set to true for multi-line input controls to match keys for vertical movement, too.</param>
        public static bool IsTextEditKey(Keys keys, bool multiLine = false)
        {
            keys &= ~Keys.Shift; // ignore the SHIFT key as modifier
            switch (keys)
            {
                case Keys key when key is
                    (>= Keys.A and <= Keys.Z)
                    or (>= Keys.D0 and <= Keys.D9)
                    or (>= Keys.Oem1 and <= Keys.Oem102):
                case Keys.Space:
                case Keys.Insert:
                    return true;
            }

            keys &= ~Keys.Control; // ignore the CONTROL key as modifier
            switch (keys)
            {
                case Keys.A:
                case Keys.C:
                case Keys.V:
                case Keys.X:
                case Keys.Y:
                case Keys.Z:
                case Keys.Back:
                case Keys.Delete:
                case Keys.Left:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                    return true;

                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                    return multiLine;
            }

            return false;
        }

        #endregion
    }
}
