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
    private const uint _altContextBit = 1u << 29;
    private const nuint _keyDownMessage = 0x0100;
    private const int _keyPressedMask = 0x8000;
    private const nuint _keyUpMessage = 0x0101;
    private const nuint _sysKeyDownMessage = 0x0104;
    private const nuint _sysKeyUpMessage = 0x0105;
    private const uint _scanCodeShift = 16;

    private readonly HashSet<VIRTUAL_KEY> _consumedKeyDowns = [];
    private readonly Func<HWND> _getFormWindow;
    private readonly Func<HWND> _getMinttyWindow;
    private HHOOK _hook;
    private readonly HOOKPROC _hookProc;
    private readonly bool _interceptTabPress;

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
    }

    private LRESULT HookProc(int code, WPARAM messageId, LPARAM hookData)
    {
        if (code >= 0 && TryHandle(messageId, hookData))
        {
            return (LRESULT)1;
        }

        return PInvoke.CallNextHookEx(_hook, code, messageId, hookData);
    }

    private static bool IsAltGrOrCtrlDown()
        => (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_CONTROL) & _keyPressedMask) != 0;

    private static bool IsFocusedWindow(HWND window)
    {
        GUITHREADINFO info = new() { cbSize = (uint)Marshal.SizeOf<GUITHREADINFO>() };

        // Thread id 0 queries the foreground thread.
        return PInvoke.GetGUIThreadInfo(0, ref info) && info.hwndFocus == window;
    }

    private bool ShouldIntercept(VIRTUAL_KEY key, bool altDown)
    {
        if (key is VIRTUAL_KEY.VK_TAB)
        {
            // Tab and Shift+Tab must reach the shell for tab completion in the console tab.
            // Alt+Tab is handled by the OS.
            return _interceptTabPress && !altDown;
        }

        if (key is >= VIRTUAL_KEY.VK_F1 and <= VIRTUAL_KEY.VK_F24)
        {
            return true;
        }

        // Ctrl + digit is used for focus control. Ctrl+Alt is AltGr, i.e. text input.
        if (key is >= VIRTUAL_KEY.VK_0 and <= VIRTUAL_KEY.VK_9 && !altDown && IsAltGrOrCtrlDown())
        {
            return true;
        }

        // Alt + character (mnemonic). Ctrl+Alt is AltGr, i.e. text input.
        return altDown && !IsAltGrOrCtrlDown() && key is not (VIRTUAL_KEY.VK_MENU or VIRTUAL_KEY.VK_LMENU or VIRTUAL_KEY.VK_RMENU);
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

        // Posting (not sending) lets the message loop run the regular WinForms pre-processing:
        // dialog keys (Tab), shortcuts (ProcessCmdKey) and mnemonics.
        nuint message = altDown ? _sysKeyDownMessage : _keyDownMessage;
        nint keyDownLParam = (nint)(1u | (data->scanCode << (int)_scanCodeShift) | (altDown ? _altContextBit : 0));
        PInvoke.PostMessage(formWindow, (uint)message, (nuint)key, keyDownLParam);

        return true;
    }
}
