using System.Diagnostics;
using System.Runtime.InteropServices;
using GitCommands.Utils;

namespace GitCommands.Git.Extensions
{
    public static class ProcessExtensions
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

        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool AttachConsole(int dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, int dwProcessGroupId);
        }
    }
}
