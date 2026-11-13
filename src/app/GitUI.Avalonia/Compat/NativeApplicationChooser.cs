using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace GitUI.Compat;

internal static partial class NativeApplicationChooser
{
    private const uint Execute = 0x00000004;
    private const int Cancelled = unchecked((int)0x800704C7);
    private const int Aborted = unchecked((int)0x80004004);

    // parity-scaffolding: replace only the external shell dialog in headless tests.
    private static Func<nint, string, uint, int> _showWindowsDialog = ShowWindowsDialog;

    // parity-scaffolding: replace only the external application launch in headless tests.
    private static Func<ProcessStartInfo, Task<bool>> _startApplication = StartApplicationAsync;

    internal static Task<bool> OpenAsync(TopLevel owner, string target)
        => OperatingSystem.IsWindows()
            ? Task.FromResult(OpenOnWindows(owner, target))
            : OperatingSystem.IsMacOS()
                ? OpenOnMacOSAsync(owner, target)
                : Task.FromResult(false);

    internal static bool OpenOnWindows(TopLevel owner, string target)
    {
        try
        {
            string fileName = Path.GetFullPath(target);
            int result = _showWindowsDialog(owner.TryGetPlatformHandle()?.Handle ?? 0, fileName, Execute);
            return result >= 0 || result is Cancelled or Aborted;
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static async Task<bool> OpenOnMacOSAsync(TopLevel owner, string target)
    {
        try
        {
            string fileName = Path.GetFullPath(target);
            string? application = await PortalPickerGuard.OpenApplicationPickerAsync(owner);
            if (application is null)
            {
                return true;
            }

            if (!Directory.Exists(application)
                || !string.Equals(Path.GetExtension(application), ".app", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            ProcessStartInfo startInfo = new("/usr/bin/open") { UseShellExecute = false };
            startInfo.ArgumentList.Add("-a");
            startInfo.ArgumentList.Add(application);
            startInfo.ArgumentList.Add(fileName);
            return await _startApplication(startInfo);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static int ShowWindowsDialog(nint owner, string target, uint flags)
    {
        nint fileName = Marshal.StringToCoTaskMemUni(target);
        try
        {
            OpenAsInfo info = new() { FileName = fileName, ClassName = 0, Flags = flags };
            return SHOpenWithDialog(owner, in info);
        }
        finally
        {
            Marshal.FreeCoTaskMem(fileName);
        }
    }

    private static async Task<bool> StartApplicationAsync(ProcessStartInfo startInfo)
    {
        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            return false;
        }

        await process.WaitForExitAsync();
        return process.ExitCode == 0;
    }

    [LibraryImport("shell32.dll", EntryPoint = "SHOpenWithDialog")]
    private static partial int SHOpenWithDialog(nint owner, in OpenAsInfo info);

    [StructLayout(LayoutKind.Sequential)]
    private struct OpenAsInfo
    {
        public nint FileName;
        public nint ClassName;
        public uint Flags;
    }

    // parity-scaffolding: expose only external OS calls, preserving the production chooser wiring.
    internal static class TestAccessor
    {
        public static Func<nint, string, uint, int> ShowWindowsDialog
        {
            get => _showWindowsDialog;
            set => _showWindowsDialog = value;
        }

        public static Func<ProcessStartInfo, Task<bool>> StartApplication
        {
            get => _startApplication;
            set => _startApplication = value;
        }
    }
}
