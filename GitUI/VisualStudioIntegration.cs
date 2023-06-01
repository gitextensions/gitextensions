using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using GitCommands;
using GitExtUtils;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    internal static class VisualStudioIntegration
    {
        static VisualStudioIntegration()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;

                string vswhere = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Microsoft Visual Studio\Installer\vswhere.exe";
                if (!File.Exists(vswhere))
                {
                    return;
                }

                Executable executable = new(vswhere);
                ArgumentBuilder arguments = new()
                {
                    "-latest",
                    "-property productPath"
                };
                _devEnvPath = await executable.GetOutputAsync(arguments);
            }).FileAndForget();
        }

        public static void Init()
        {
            // just create the static instance
        }

        public static void OpenFile(string filePath)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                if (!await TryOpenFileInRunningInstanceAsync(filePath))
                {
                    if (_devEnvPath is not null)
                    {
                        using IProcess process = new Executable(_devEnvPath).Start(filePath);
                    }
                }
            }).FileAndForget();
        }

        public static async Task<bool> TryOpenFileInRunningInstanceAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // When opening the context menu, we disable this item if the file does not exist.
                // So, we should not experience this situation in practice (barring some exotic race conditions).
                return false;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (DTE dte in GetVisualStudioInstances())
            {
                ProjectItem projectItem = dte.Solution.FindProjectItem(filePath);

                if (projectItem is not null)
                {
                    // Open the file
                    dte.ExecuteCommand("File.OpenFile", filePath);

                    // Bring the Visual Studio window to the front of the desktop
                    NativeMethods.SetForegroundWindow(dte.MainWindow.HWnd);

                    return true;
                }
            }

            return false;
        }

        public static bool IsVisualStudioInstalled => _devEnvPath is not null;

        private static string? _devEnvPath;

        private static IEnumerable<DTE> GetVisualStudioInstances()
        {
            // The DTE object must be retrieved from the main thread. Otherwise it denies to get DTE.MainWindow.HWnd sometimes.
            ThreadHelper.ThrowIfNotOnUIThread();

            int retVal = NativeMethods.GetRunningObjectTable(0, out IRunningObjectTable rot);

            if (retVal != 0)
            {
                yield break;
            }

            rot.EnumRunning(out IEnumMoniker enumMoniker);

            const int count = 1;

            IMoniker[] moniker = new IMoniker[count];

            while (enumMoniker.Next(count, moniker, pceltFetched: IntPtr.Zero) == 0)
            {
                NativeMethods.CreateBindCtx(reserved: 0, out IBindCtx bindCtx);

                string? displayName = null;

                try
                {
                    moniker[0].GetDisplayName(bindCtx, pmkToLeft: null, out displayName);
                }
                catch (UnauthorizedAccessException)
                {
                    // Some ROT objects require elevated permissions.
                }

                // Display name example: "!VisualStudio.DTE.16.0:73424"
                if (displayName?.StartsWith("!VisualStudio") == true)
                {
                    rot.GetObject(moniker[0], out object obj);

                    if (obj is DTE dte)
                    {
                        yield return dte;
                    }
                }
            }
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hwnd);

            [DllImport("ole32.dll")]
            public static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

            [DllImport("ole32.dll")]
            public static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);
        }
    }
}
