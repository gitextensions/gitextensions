using Avalonia.LogicalTree;
using GitExtUtils;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace ResourceManager;

// Twin of ResourceManager/GitExtensionsControl.cs. Avalonia routes ProcessKeyDown through
// its containing form, so this class retains the original control-level hotkey table and
// exposes ProcessHotkey for that form to call.
public class GitExtensionsControl : TranslatedControl
{
    private IReadOnlyList<HotkeyCommand> _hotkeys = [];

    protected virtual IServiceProvider ServiceProvider
        => this.GetLogicalAncestors().OfType<IGitModuleForm>().First().UICommands;

    protected bool HotkeysEnabled { get; set; }

    protected void LoadHotkeys(string hotkeySettingsName)
    {
        _hotkeys = HotkeysEnabled
            ? ServiceProvider.GetRequiredService<IHotkeySettingsLoader>().LoadHotkeys(hotkeySettingsName) ?? []
            : [];
    }

    public virtual bool ProcessHotkey(WinFormsShims.Keys keyData)
    {
        if (!HotkeysEnabled)
        {
            return false;
        }

        HotkeyCommand? hotkey = _hotkeys.FirstOrDefault(hotkey => hotkey.KeyData == keyData);
        return hotkey is not null && ExecuteCommand(hotkey.CommandCode);
    }

    public string GetShortcutKeyDisplayString<T>(T commandCode)
        where T : struct, Enum
        => _hotkeys.GetShortcutDisplay(commandCode);

    protected virtual bool ExecuteCommand(int command)
    {
        return false;
    }

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
