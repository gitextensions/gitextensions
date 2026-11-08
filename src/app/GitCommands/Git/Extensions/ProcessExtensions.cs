using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;
namespace GitCommands.Git.Extensions;

public static class ProcessExtensions
{
    public static void TerminateTree(this Process process)
    {
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

        if (!process.HasExited)
        {
            process.Kill();
        }
    }

    /// <summary>
    ///  Gets the command line the process was started with.
    /// </summary>
    /// <remarks>
    ///  Unlike WMI (Win32_Process.CommandLine), this is cheap enough to be called while polling.
    /// </remarks>
    /// <returns>The command line, or <see langword="null"/> if it cannot be read, e.g. if the process has exited or access is denied.</returns>
    [SupportedOSPlatform("windows")]
    internal static string? TryGetCommandLine(this Process process)
    {
        try
        {
            using SafeProcessHandle handle = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_LIMITED_INFORMATION, inheritHandle: false, process.Id);
            if (handle.IsInvalid)
            {
                return null;
            }

            // Query the required buffer size first.
            int status = NativeMethods.NtQueryInformationProcess(handle, NativeMethods.ProcessCommandLineInformation, IntPtr.Zero, 0, out int length);
            if (status != NativeMethods.STATUS_INFO_LENGTH_MISMATCH || length < Marshal.SizeOf<NativeMethods.UNICODE_STRING>() || length > NativeMethods.MaxCommandLineInformationLength)
            {
                return null;
            }

            IntPtr buffer = Marshal.AllocHGlobal(length);
            try
            {
                if (NativeMethods.NtQueryInformationProcess(handle, NativeMethods.ProcessCommandLineInformation, buffer, length, out _) < 0)
                {
                    return null;
                }

                // The string data follows the UNICODE_STRING header in the same buffer.
                NativeMethods.UNICODE_STRING commandLine = Marshal.PtrToStructure<NativeMethods.UNICODE_STRING>(buffer);
                if (commandLine.Length > length - Marshal.SizeOf<NativeMethods.UNICODE_STRING>())
                {
                    return null;
                }

                return Marshal.PtrToStringUni(commandLine.Buffer, commandLine.Length / sizeof(char));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return null;
        }
    }

    private static class NativeMethods
    {
        public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        public const int ProcessCommandLineInformation = 60;
        public const int STATUS_INFO_LENGTH_MISMATCH = unchecked((int)0xC0000004);

        // UNICODE_STRING.Length is a ushort, so a command line can't be longer than that.
        public const int MaxCommandLineInformationLength = ushort.MaxValue + 64;

        [StructLayout(LayoutKind.Sequential)]
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, int dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeProcessHandle OpenProcess(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);

        [DllImport("ntdll.dll")]
        public static extern int NtQueryInformationProcess(SafeProcessHandle processHandle, int processInformationClass, IntPtr processInformation, int processInformationLength, out int returnLength);
    }
}
