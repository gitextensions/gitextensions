using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;

namespace GitUI
{
    internal static class VisualStudioIntegration
    {
        public static bool TryOpenFile(string filePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!File.Exists(filePath))
            {
                // When opening the context menu, we disable this item if the file does not exist.
                // So, we should not experience this situation in practice (barring some exotic race conditions).
                return false;
            }

            foreach (DTE dte in GetVisualStudioInstances())
            {
                ProjectItem projectItem = dte.Solution.FindProjectItem(filePath);

                if (projectItem != null)
                {
                    // Open the file
                    dte.ExecuteCommand("File.OpenFile", filePath);

                    // Bring the Visual Studio window to the front of the desktop
                    NativeMethods.SetForegroundWindow(new IntPtr(dte.MainWindow.HWnd));

                    return true;
                }
            }

            return false;
        }

        public static bool IsVisualStudioRunning => GetVisualStudioInstances().Any();

        private static IEnumerable<DTE> GetVisualStudioInstances()
        {
            int retVal = NativeMethods.GetRunningObjectTable(0, out IRunningObjectTable rot);

            if (retVal != 0)
            {
                yield break;
            }

            rot.EnumRunning(out IEnumMoniker enumMoniker);

            const int count = 1;

            var moniker = new IMoniker[count];

            while (enumMoniker.Next(count, moniker, pceltFetched: IntPtr.Zero) == 0)
            {
                NativeMethods.CreateBindCtx(0, out IBindCtx bindCtx);

                string? displayName = null;

                try
                {
                    moniker[0].GetDisplayName(bindCtx, null, out displayName);
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
