using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using GitCommands;
using GitExtensions.Extensibility;
using Microsoft.VisualStudio.Threading;

namespace GitUI;

internal static class VisualStudioIntegration
{
    private const uint E_FAIL = 0x8000_4005;
    private const uint RPC_E_CALL_REJECTED = 0x8001_0001;

    static VisualStudioIntegration()
    {
        ThreadHelper.FileAndForget(async () =>
        {
            string vswhere = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Microsoft Visual Studio\Installer\vswhere.exe";
            if (!File.Exists(vswhere))
            {
                return;
            }

            Executable executable = new(vswhere);
            ArgumentBuilder arguments =
            [
                "-latest",
                "-property productPath"
            ];
            _devEnvPath = await executable.GetOutputAsync(arguments);
        });
    }

    public static void Init()
    {
        // just create the static instance
    }

    public static void OpenFile(string filePath, int lineNumber = 0)
    {
        ThreadHelper.FileAndForget(async () =>
        {
            while (true)
            {
                try
                {
                    if (await TryOpenFileInRunningInstanceAsync(filePath, lineNumber))
                    {
                        return;
                    }

                    break;
                }
                catch (COMException exception) when ((uint)exception.HResult == RPC_E_CALL_REJECTED)
                {
                    Trace.WriteLine(exception);
                    Form activeForm = Form.ActiveForm;
                    await activeForm.SwitchToMainThreadAsync();
                    if (!MessageBoxes.ConfirmRetryOpenVisualStudio(activeForm))
                    {
                        return;
                    }

                    await TaskScheduler.Default;
                }
            }

            if (_devEnvPath is not null)
            {
                using IProcess process = new Executable(_devEnvPath).Start(filePath);
            }
        });
    }

    public static async Task<bool> TryOpenFileInRunningInstanceAsync(string filePath, int lineNumber = 0)
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
                if (lineNumber > 0)
                {
                    try
                    {
                        dte.ExecuteCommand("Edit.GoTo", lineNumber.ToString());
                    }
                    catch (COMException exception) when ((uint)exception.HResult == E_FAIL)
                    {
                        Debug.WriteLine(exception);
                    }
                }

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
