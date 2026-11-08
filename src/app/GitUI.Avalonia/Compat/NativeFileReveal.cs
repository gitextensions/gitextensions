using System.Diagnostics;
using GitExtensions.Extensibility;

namespace GitUI.Compat;

internal static class NativeFileReveal
{
    // parity-scaffolding: replace only the external file-manager process in headless tests.
    private static Func<ProcessStartInfo, bool, Task<bool>> _startProcess = StartProcessAsync;

    internal static Task<bool> RevealAsync(string target)
        => OperatingSystem.IsWindows()
            ? RevealOnWindowsAsync(target)
            : OperatingSystem.IsMacOS()
                ? RevealOnMacOSAsync(target)
                : Task.FromResult(false);

    internal static async Task<bool> RevealOnWindowsAsync(string target)
    {
        try
        {
            ProcessStartInfo startInfo = new("explorer.exe")
            {
                Arguments = $"/select, {Path.GetFullPath(target).Quote()}",
                UseShellExecute = false,
            };
            return await _startProcess(startInfo, false);
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static async Task<bool> RevealOnMacOSAsync(string target)
    {
        try
        {
            ProcessStartInfo startInfo = new("/usr/bin/open") { UseShellExecute = false };
            startInfo.ArgumentList.Add("-R");
            startInfo.ArgumentList.Add(Path.GetFullPath(target));
            return await _startProcess(startInfo, true);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<bool> StartProcessAsync(ProcessStartInfo startInfo, bool waitForExit)
    {
        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            return false;
        }

        if (!waitForExit)
        {
            return true;
        }

        await process.WaitForExitAsync();
        return process.ExitCode == 0;
    }

    // parity-scaffolding: observe the native request without opening a file manager.
    internal static class TestAccessor
    {
        public static Func<ProcessStartInfo, bool, Task<bool>> StartProcess
        {
            get => _startProcess;
            set => _startProcess = value;
        }
    }
}
