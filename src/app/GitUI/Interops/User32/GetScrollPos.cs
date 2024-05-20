using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32, ExactSpelling = true, SetLastError = true)]
        public static extern int GetScrollPos(IntPtr hWnd, SB nBar);
    }
}
