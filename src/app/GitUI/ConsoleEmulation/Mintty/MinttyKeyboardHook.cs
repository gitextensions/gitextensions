using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace GitUI.ConsoleEmulation.Mintty;

/// <summary>
///  Intercepts keys that would otherwise be consumed by the embedded mintty window
///  (which lives in another process and never lets WinForms see its keystrokes)
///  and forwards them to the host form, so that focus traversal (<c>Tab</c>),
///  mnemonics (<c>Alt</c> + char) and function-key shortcuts keep working.
///  Text input, <c>Enter</c> and terminal control keys (e.g. <c>Ctrl</c> + C) are left to mintty.
/// </summary>
internal sealed class MinttyKeyboardHook : IDisposable
{
    private const uint ScanCodeShift = 16;
    private const uint AltContextBit = 1u << 29;
    private const nuint KeyDownMessage = 0x0100;
    private const nuint SysKeyDownMessage = 0x0104;
    private const nuint KeyUpMessage = 0x0101;
    private const nuint SysKeyUpMessage = 0x0105;
    private const int KeyPressedMask = 0x8000;

    private readonly Func<HWND> _getMinttyWindow;
    private readonly Func<HWND> _getFormWindow;
    private readonly HOOKPROC _hookProc;
    private HHOOK _hook;

    public MinttyKeyboardHook(Func<HWND> getMinttyWindow, Func<HWND> getFormWindow)
    {
        _getMinttyWindow = getMinttyWindow;
        _getFormWindow = getFormWindow;

        // Keep the delegate in a field so it is not garbage collected while the hook is installed.
        _hookProc = HookProc;
    }

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
    }

    public void Dispose() => Uninstall();

    private static bool IsAltGrOrCtrlDown()
        => (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_CONTROL) & KeyPressedMask) != 0;

    private static bool ShouldIntercept(VIRTUAL_KEY key, bool altDown)
    {
        if (key is VIRTUAL_KEY.VK_TAB)
        {
            // Alt+Tab is handled by the OS.
            return !altDown;
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

    private static bool IsFocusedWindow(HWND window)
    {
        GUITHREADINFO info = new() { cbSize = (uint)Marshal.SizeOf<GUITHREADINFO>() };

        // Thread id 0 queries the foreground thread.
        return PInvoke.GetGUIThreadInfo(0, ref info) && info.hwndFocus == window;
    }

    private LRESULT HookProc(int code, WPARAM messageId, LPARAM hookData)
    {
        if (code >= 0 && TryHandle(messageId, hookData))
        {
            return (LRESULT)1;
        }

        return PInvoke.CallNextHookEx(_hook, code, messageId, hookData);
    }

    private unsafe bool TryHandle(WPARAM messageId, LPARAM hookData)
    {
        HWND minttyWindow = _getMinttyWindow();
        if (minttyWindow.IsNull)
        {
            return false;
        }

        KBDLLHOOKSTRUCT* data = (KBDLLHOOKSTRUCT*)hookData.Value;
        VIRTUAL_KEY key = (VIRTUAL_KEY)data->vkCode;
        bool altDown = (data->flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_ALTDOWN) != 0;
        bool isKeyUp = (data->flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_UP) != 0;

        if (messageId.Value is not (KeyDownMessage or SysKeyDownMessage or KeyUpMessage or SysKeyUpMessage)
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

        // Swallow the matching key-up as well so mintty never sees an unpaired event.
        if (!isKeyUp)
        {
            // Posting (not sending) lets the message loop run the regular WinForms pre-processing:
            // dialog keys (Tab), shortcuts (ProcessCmdKey) and mnemonics.
            nuint message = altDown ? SysKeyDownMessage : KeyDownMessage;
            nint keyDownLParam = (nint)(1u | (data->scanCode << (int)ScanCodeShift) | (altDown ? AltContextBit : 0));
            PInvoke.PostMessage(formWindow, (uint)message, (nuint)key, keyDownLParam);
        }

        return true;
    }
}
