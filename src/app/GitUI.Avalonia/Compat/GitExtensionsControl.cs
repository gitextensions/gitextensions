using Avalonia.Controls;
using Avalonia.LogicalTree;
using GitExtUtils;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace ResourceManager;

// Avalonia routes ProcessKeyDown through its containing form, so this class retains the
// control-level hotkey table and exposes ProcessHotkey for that form to call.
public class GitExtensionsControl : TranslatedControl
{
    private IReadOnlyList<HotkeyCommand> _hotkeys = [];

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
    protected virtual IServiceProvider ServiceProvider
        => this.GetLogicalAncestors().OfType<IGitModuleForm>().First().UICommands;

    /// <summary>
        /// Checks if the control wants to handle the key and execute that hotkey
        /// (without propagating an unhandled key to the base class function as in <cref>ProcessCmdKey</cref>).
        /// Can be overridden in order to execute a hotkey for a (visible) subcontrol
        /// (if this focused/queried control does not have such an (active) hotkey itself).
        /// </summary>
    public virtual bool ProcessHotkey(WinFormsShims.Keys keyData)
    {
        // Avalonia maps modifier-only and unsupported key events to None; None is not an assignable hotkey.
        if (!HotkeysEnabled || keyData == WinFormsShims.Keys.None)
        {
            return false;
        }

        HotkeyCommand? hotkey = _hotkeys.FirstOrDefault(hotkey => hotkey.KeyData == keyData);
        return hotkey is not null && ExecuteCommand(hotkey.CommandCode);
    }

    protected IReadOnlyList<HotkeyCommand> Hotkeys => _hotkeys;

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
        _hotkeys = HotkeysEnabled
            ? ServiceProvider.GetRequiredService<IHotkeySettingsLoader>().LoadHotkeys(hotkeySettingsName) ?? []
            : [];
    }

    public string GetShortcutKeyDisplayString<T>(T commandCode)
        where T : struct, Enum
        => _hotkeys.GetShortcutDisplay(commandCode);

    protected void UpdateTooltipWithShortcut<T>(Control control, T commandCode)
        where T : struct, Enum
    {
        string text = ToolTip.GetTip(control) switch
        {
            TextBlock textBlock => textBlock.Text ?? string.Empty,
            string value => value,
            _ => string.Empty,
        };
        ToolTip.SetTip(control, text.UpdateSuffix(_hotkeys.GetShortcutToolTip(commandCode)));
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
    public static bool IsTextEditKey(WinFormsShims.Keys keys, bool multiLine = false)
    {
        keys &= ~WinFormsShims.Keys.Shift;
        switch (keys)
        {
            case WinFormsShims.Keys key when key is
                (>= WinFormsShims.Keys.A and <= WinFormsShims.Keys.Z)
                or (>= WinFormsShims.Keys.D0 and <= WinFormsShims.Keys.D9)
                or (>= WinFormsShims.Keys.Oem1 and <= WinFormsShims.Keys.Oem102):
            case WinFormsShims.Keys.Space:
            case WinFormsShims.Keys.Insert:
                return true;
        }

        keys &= ~WinFormsShims.Keys.Control;
        switch (keys)
        {
            case WinFormsShims.Keys.A:
            case WinFormsShims.Keys.C:
            case WinFormsShims.Keys.V:
            case WinFormsShims.Keys.X:
            case WinFormsShims.Keys.Y:
            case WinFormsShims.Keys.Z:
            case WinFormsShims.Keys.Back:
            case WinFormsShims.Keys.Delete:
            case WinFormsShims.Keys.Left:
            case WinFormsShims.Keys.Right:
            case WinFormsShims.Keys.Home:
            case WinFormsShims.Keys.End:
                return true;

            case WinFormsShims.Keys.Up:
            case WinFormsShims.Keys.Down:
            case WinFormsShims.Keys.PageUp:
            case WinFormsShims.Keys.PageDown:
                return multiLine;
        }

        return false;
    }
}
