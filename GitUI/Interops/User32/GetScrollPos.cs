using System.Runtime.InteropServices;
using static System.Interop;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32, ExactSpelling = true, SetLastError = true)]
        public static extern int GetScrollPos(IntPtr hWnd, SB nBar);
    }
}
