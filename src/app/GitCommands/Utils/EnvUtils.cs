using System.Diagnostics;

namespace GitCommands.Utils;

public static class EnvUtils
{
    public static bool RunningOnWindowsWithMainWindow()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        Process currentProcess = Process.GetCurrentProcess();
        if (currentProcess is null)
        {
            return false;
        }

        return currentProcess.MainWindowHandle != IntPtr.Zero;
    }

    public static string? ReplaceLinuxNewLinesDependingOnPlatform(string? s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        if (!OperatingSystem.IsWindows())
        {
            return s;
        }

        return s.Replace("\n", Environment.NewLine);
    }
}
