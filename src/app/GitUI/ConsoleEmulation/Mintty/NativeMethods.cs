using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.JobObjects;
using Windows.Win32.UI.WindowsAndMessaging;

namespace GitUI.ConsoleEmulation.Mintty;

internal static class NativeMethods
{
    internal const uint WM_CHAR = 0x0102;
    internal const uint WM_KEYDOWN = 0x0100;
    internal const uint WM_KEYUP = 0x0101;

    internal static HWND FindMinttyWindowForProcess(int processId)
    {
        HWND found = HWND.Null;
        PInvoke.EnumWindows((hwnd, _) =>
        {
            PInvoke.GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid != (uint)processId)
            {
                return true;
            }

            Span<char> classNameBuf = stackalloc char[20];
            unsafe
            {
                fixed (char* pBuf = classNameBuf)
                {
                    int len = PInvoke.GetClassName(hwnd, pBuf, classNameBuf.Length);
                    if (len > 0 && classNameBuf[..len] is "mintty")
                    {
                        found = hwnd;
                        return false;
                    }
                }
            }

            return true;
        }, 0);
        return found;
    }

    /// <summary>
    /// Pings the target window with WM_NULL via SendMessageTimeout to verify its
    /// message pump is alive. SMTO_ABORTIFHUNG returns immediately if the OS has
    /// already marked the window as hung; otherwise the call returns once the pump
    /// dispatches the no-op message or the timeout expires. Returns false on
    /// failure — caller must not proceed with synchronous cross-process calls.
    /// </summary>
    internal static bool IsWindowResponsive(HWND hwnd, TimeSpan timeout)
    {
        nint result = PInvoke.SendMessageTimeout(
            hwnd,
            Msg: 0, // WM_NULL
            wParam: default,
            lParam: default,
            SEND_MESSAGE_TIMEOUT_FLAGS.SMTO_ABORTIFHUNG,
            (uint)timeout.TotalMilliseconds,
            out _);
        return result != 0;
    }

    /// <summary>
    /// SetFocus only works when the target hwnd belongs to the calling thread, or when
    /// the calling thread's input queue is attached to the target window's thread.
    /// Mintty runs in another process, so we must attach input queues for the call to
    /// take effect — otherwise SetFocus is silently dropped.
    /// </summary>
    internal static void FocusWindowWithAttachedInput(HWND hwnd)
    {
        uint targetThreadId = PInvoke.GetWindowThreadProcessId(hwnd, out _);
        if (targetThreadId == 0)
        {
            return;
        }

        uint currentThreadId = PInvoke.GetCurrentThreadId();
        if (targetThreadId == currentThreadId)
        {
            PInvoke.SetFocus(hwnd);
            return;
        }

        if (!PInvoke.AttachThreadInput(currentThreadId, targetThreadId, true))
        {
            return;
        }

        try
        {
            PInvoke.SetFocus(hwnd);
        }
        finally
        {
            PInvoke.AttachThreadInput(currentThreadId, targetThreadId, false);
        }
    }

    /// <summary>
    ///  Moves keyboard focus from the window currently holding it, which may belong to another thread, to <paramref name="hwnd"/>.
    /// </summary>
    internal static void MoveFocusFromForeignThread(HWND hwnd)
    {
        GUITHREADINFO info = new() { cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<GUITHREADINFO>() };
        if (!PInvoke.GetGUIThreadInfo(0, ref info) || info.hwndFocus.IsNull)
        {
            FocusWindowWithAttachedInput(hwnd);
            return;
        }

        uint focusThreadId = PInvoke.GetWindowThreadProcessId(info.hwndFocus, out _);
        uint currentThreadId = PInvoke.GetCurrentThreadId();
        if (focusThreadId == currentThreadId || !PInvoke.AttachThreadInput(currentThreadId, focusThreadId, true))
        {
            FocusWindowWithAttachedInput(hwnd);
            return;
        }

        try
        {
            PInvoke.SetFocus(hwnd);
        }
        finally
        {
            PInvoke.AttachThreadInput(currentThreadId, focusThreadId, false);
        }
    }

    internal static List<HWND> EnumerateProcessWindows(int processId)
    {
        List<HWND> found = [];
        PInvoke.EnumWindows((hwnd, _) =>
        {
            PInvoke.GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == (uint)processId)
            {
                found.Add(hwnd);
            }

            return true;
        }, 0);
        return found;
    }

    internal static unsafe HANDLE CreateKillOnCloseJob()
    {
        HANDLE job = PInvoke.CreateJobObject(default(Windows.Win32.Security.SECURITY_ATTRIBUTES*), null);
        if (job.IsNull)
        {
            return HANDLE.Null;
        }

        JOBOBJECT_EXTENDED_LIMIT_INFORMATION info = default;
        info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
        if (!PInvoke.SetInformationJobObject(
                job,
                JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation,
                &info,
                (uint)sizeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION)))
        {
            PInvoke.CloseHandle(job);
            return HANDLE.Null;
        }

        return job;
    }
}
