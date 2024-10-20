using System.Diagnostics;
using System.Runtime.InteropServices;
using GitCommands.Utils;

namespace GitCommands.Git.Extensions
{
    public static partial class ProcessExtensions
    {
        public static void TerminateTree(this Process process)
        {
            if (EnvUtils.RunningOnWindows())
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

            if (!process.HasExited)
            {
                process.Kill();
            }
        }

        private static partial class NativeMethods
        {
            [LibraryImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool SetConsoleCtrlHandler(IntPtr handlerRoutine, [MarshalAs(UnmanagedType.Bool)] bool add);

            [LibraryImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool AttachConsole(int dwProcessId);

            [LibraryImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, int dwProcessGroupId);
        }
    }
}
