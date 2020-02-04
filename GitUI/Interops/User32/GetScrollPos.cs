using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32, CharSet = CharSet.Auto)]
        public static extern int GetScrollPos(IntPtr hWnd, int nBar);
    }
}
