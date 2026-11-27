using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace GitUI.ConsoleEmulation.Mintty;

/// <summary>
///  Intercepts keys that would otherwise be consumed by the embedded mintty window
///  (which lives in another process and never lets WinForms see its keystrokes)
///  and forwards them to the host form, so that focus traversal (<c>Tab</c> optionally),
///  mnemonics (<c>Alt</c> + char) and function-key shortcuts keep working.
///  Text input, <c>Enter</c> and terminal control keys (e.g. <c>Ctrl</c> + C) are left to mintty.
/// </summary>
internal sealed class MinttyKeyboardHook : IDisposable
{
    private const uint _wmApp = 0x8000;
    internal const uint InterceptedKeyMessage = _wmApp + 1;
    private const nuint _keyDownMessage = 0x0100;
    private const nuint _keyUpMessage = 0x0101;
    private const nuint _sysKeyDownMessage = 0x0104;
    private const nuint _sysKeyUpMessage = 0x0105;

    private readonly HashSet<VIRTUAL_KEY> _consumedKeyDowns = [];
    private readonly Func<HWND> _getFormWindow;
    private readonly Func<HWND> _getMinttyWindow;
    private HHOOK _hook;
    private readonly HOOKPROC _hookProc;
    private readonly bool _interceptTabPress;
    private bool _ctrlDown;
    private bool _shiftDown;

    public MinttyKeyboardHook(Func<HWND> getFormWindow, Func<HWND> getMinttyWindow, bool interceptTabPress)
    {
        _getFormWindow = getFormWindow;
        _getMinttyWindow = getMinttyWindow;
        _interceptTabPress = interceptTabPress;

        // Keep the delegate in a field so it is not garbage collected while the hook is installed.
        _hookProc = HookProc;
    }

    public void Dispose() => Uninstall();

    public void Install()
    {
        if (!_hook.IsNull)
        {
            return;
        }

        // A low-level hook is required because mintty's thread belongs to another process;
        // its callback is dispatched on the installing (UI) thread.
        HINSTANCE module = new(Marshal.GetHINSTANCE(typeof(MinttyKeyboardHook).Module));
        _hook = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _hookProc, module, 0);
    }

    public void Uninstall()
    {
        if (_hook.IsNull)
        {
            return;
        }

        PInvoke.UnhookWindowsHookEx(_hook);
        _hook = HHOOK.Null;
        _consumedKeyDowns.Clear();
        _ctrlDown = false;
        _shiftDown = false;
    }

    private LRESULT HookProc(int code, WPARAM messageId, LPARAM hookData)
    {
        if (code >= 0 && TryHandle(messageId, hookData))
        {
            return (LRESULT)1;
        }

        return PInvoke.CallNextHookEx(_hook, code, messageId, hookData);
    }

    private static bool IsFocusedWindow(HWND window)
    {
        GUITHREADINFO info = new() { cbSize = (uint)Marshal.SizeOf<GUITHREADINFO>() };

        // Thread id 0 queries the foreground thread.
        return PInvoke.GetGUIThreadInfo(0, ref info) && info.hwndFocus == window;
    }

    private Keys GetModifiers(bool altDown)
        => (_ctrlDown ? Keys.Control : Keys.None) | (_shiftDown ? Keys.Shift : Keys.None) | (altDown ? Keys.Alt : Keys.None);

    private bool ShouldIntercept(VIRTUAL_KEY key, bool altDown)
    {
        if (key is VIRTUAL_KEY.VK_TAB)
        {
            // Unmodified Tab must reach the shell for tab completion; Ctrl+Tab switches tabs.
            // Alt+Tab is handled by the OS.
            return !altDown && (_interceptTabPress || _ctrlDown);
        }

        if (key is >= VIRTUAL_KEY.VK_F1 and <= VIRTUAL_KEY.VK_F24)
        {
            return true;
        }

        // Ctrl + digit is used for focus control. Ctrl+Alt is AltGr, i.e. text input.
        if (key is >= VIRTUAL_KEY.VK_0 and <= VIRTUAL_KEY.VK_9 && _ctrlDown && !altDown)
        {
            return true;
        }

        // Alt + character (mnemonic). Ctrl+Alt is AltGr, i.e. text input.
        return altDown && !_ctrlDown && key is (>= VIRTUAL_KEY.VK_A and <= VIRTUAL_KEY.VK_Z) or (>= VIRTUAL_KEY.VK_0 and <= VIRTUAL_KEY.VK_9);
    }

    private unsafe bool TryHandle(WPARAM messageId, LPARAM hookData)
    {
        if (messageId.Value is not (_keyDownMessage or _sysKeyDownMessage or _keyUpMessage or _sysKeyUpMessage))
        {
            return false;
        }

        KBDLLHOOKSTRUCT* data = (KBDLLHOOKSTRUCT*)hookData.Value;
        VIRTUAL_KEY key = (VIRTUAL_KEY)data->vkCode;
        bool altDown = (data->flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_ALTDOWN) != 0;
        bool isKeyUp = (data->flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_UP) != 0;

        // The low-level hook observes all keyboard input, unlike GetKeyState which is per thread.
        if (key is VIRTUAL_KEY.VK_CONTROL or VIRTUAL_KEY.VK_LCONTROL or VIRTUAL_KEY.VK_RCONTROL)
        {
            _ctrlDown = !isKeyUp;
        }
        else if (key is VIRTUAL_KEY.VK_SHIFT or VIRTUAL_KEY.VK_LSHIFT or VIRTUAL_KEY.VK_RSHIFT)
        {
            _shiftDown = !isKeyUp;
        }

        // Swallow the matching key-up independent of current focus and modifiers
        // so no window ever sees an unpaired event.
        if (isKeyUp)
        {
            return _consumedKeyDowns.Remove(key);
        }

        HWND minttyWindow = _getMinttyWindow();
        if (minttyWindow.IsNull
            || !ShouldIntercept(key, altDown)
            || !IsFocusedWindow(minttyWindow))
        {
            return false;
        }

        HWND formWindow = _getFormWindow();
        if (formWindow.IsNull)
        {
            return false;
        }

        _consumedKeyDowns.Add(key);

        // Posting (not sending) avoids re-entering the hook. The modifiers are carried explicitly,
        // as WinForms reads them from the keyboard state of its own thread, which mintty does not update.
        Keys keyData = (Keys)(int)key | GetModifiers(altDown);
        nuint message = altDown ? _sysKeyDownMessage : _keyDownMessage;
        PInvoke.PostMessage(formWindow, InterceptedKeyMessage, (nuint)(uint)keyData, (nint)message);

        return true;
    }
}
