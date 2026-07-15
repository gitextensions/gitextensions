using System.Diagnostics;
using System.Runtime.InteropServices;
namespace GitCommands.Git.Extensions;

public static class ProcessExtensions
{
    /// <summary>Starts a process in its own process group on Linux and macOS.</summary>
    /// <remarks>
    ///  The Linux wrapper uses the <c>setsid</c> executable from util-linux when available.
    ///  macOS uses the native launcher built with the Avalonia application. Other platforms,
    ///  or installations without their launcher, use <see cref="Process.Start()"/> unchanged.
    /// </remarks>
    public static bool StartInOwnProcessGroup(this Process process)
    {
        ArgumentNullException.ThrowIfNull(process);

        if (OperatingSystem.IsLinux())
        {
            string? setsidPath = GetSetsidPath();
            if (setsidPath is not null)
            {
                WrapStartInfo(process.StartInfo, setsidPath);
            }
            else
            {
                Trace.WriteLine("setsid was not found; process cleanup will use descendant traversal only.");
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            string launcherPath = Path.Combine(AppContext.BaseDirectory, "GitExtensions.ProcessGroupLauncher");
            if (File.Exists(launcherPath))
            {
                WrapStartInfo(process.StartInfo, launcherPath);
            }
            else
            {
                Trace.WriteLine("The macOS process-group launcher was not found; process cleanup will use descendant traversal only.");
            }
        }

        return process.Start();
    }

    public static void TerminateTree(this Process process)
    {
        ArgumentNullException.ThrowIfNull(process);

        if (OperatingSystem.IsWindows())
        {
            // Send Ctrl+C
            NativeMethods.AttachConsole(process.Id);
            NativeMethods.SetConsoleCtrlHandler(IntPtr.Zero, add: true);
            NativeMethods.GenerateConsoleCtrlEvent(0, 0);

            if (!process.HasExited)
            {
                process.WaitForExit(500);
            }
        }
        else
        {
            int processGroupId = NativeMethods.GetProcessGroupId(process.Id);
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            // Only signal a group created specifically for this process. Signalling an
            // inherited group could terminate Git Extensions or its launching terminal.
            if (processGroupId == process.Id)
            {
                NativeMethods.Kill(-processGroupId, NativeMethods.SigKill);
            }

            return;
        }

        if (!process.HasExited)
        {
            process.Kill();
        }
    }

    private static string? GetSetsidPath()
    {
        string[] searchDirectories = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        foreach (string directory in searchDirectories)
        {
            string candidate = Path.Combine(directory, "setsid");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static void WrapStartInfo(ProcessStartInfo startInfo, string wrapperPath)
    {
        string fileName = startInfo.FileName;
        string arguments = startInfo.Arguments;
        string[] argumentList = [.. startInfo.ArgumentList];

        startInfo.FileName = wrapperPath;
        if (argumentList.Length > 0)
        {
            startInfo.ArgumentList.Clear();
            startInfo.ArgumentList.Add(fileName);
            foreach (string argument in argumentList)
            {
                startInfo.ArgumentList.Add(argument);
            }
        }
        else
        {
            startInfo.Arguments = string.IsNullOrEmpty(arguments)
                ? fileName.Quote()
                : $"{fileName.Quote()} {arguments}";
        }
    }

    private static class NativeMethods
    {
        public const int SigKill = 9;

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, int dwProcessGroupId);

        [DllImport("libc", EntryPoint = "getpgid", SetLastError = true)]
        public static extern int GetProcessGroupId(int processId);

        [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
        public static extern int Kill(int processId, int signal);
    }
}
