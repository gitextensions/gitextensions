using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.JobObjects;

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
