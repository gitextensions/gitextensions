using System;
using System.Windows.Forms;

namespace GitUI.Theming
{
    internal class SystemDialogDetector
    {
        /// <summary>
        /// Determines whether a system dialog such as MessageBox.Show or Debug.Assert is currently
        /// displayed.
        /// </summary>
        /// <remarks>
        /// Performance measurement result:
        /// Always faster than 200 microseconds
        /// 90% cases faster than 5 microseconds, probably `GetActiveWindow` caches result
        /// </remarks>
        public bool IsSystemDialogOpen
        {
            get
            {
                var hwnd = NativeMethods.GetActiveWindow();
                return hwnd != IntPtr.Zero && Control.FromHandle(hwnd) == null;
            }
        }
    }
}
