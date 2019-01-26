using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GitExtUtils.GitUI
{
    internal static class HighDpiMouseCursors
    {
        /// <summary>
        /// Replaces some .NET Framework 96-dpi .cur file mouse cursors with system cursors.
        /// </summary>
        public static void Enable()
        {
            try
            {
                SetCursor("hand", IDC.HAND);
                SetCursor("hSplit", IDC.SIZENS);
                SetCursor("vSplit", IDC.SIZEWE);
            }
            catch
            {
                // ignore
            }

            void SetCursor(string fieldName, IDC idc)
            {
                var field = typeof(Cursors).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
                field?.SetValue(null, new Cursor(NativeMethods.LoadCursor(IntPtr.Zero, idc)));
            }
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern IntPtr LoadCursor(IntPtr hInstance, IDC lpCursorName);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private enum IDC
        {
            ARROW = 32512,
            IBEAM = 32513,
            WAIT = 32514,
            CROSS = 32515,
            UPARROW = 32516,
            SIZE = 32640,
            ICON = 32641,
            SIZENWSE = 32642,
            SIZENESW = 32643,
            SIZEWE = 32644,
            SIZENS = 32645,
            SIZEALL = 32646,
            NO = 32648,
            HAND = 32649,
            APPSTARTING = 32650,
            HELP = 32651
        }
    }
}