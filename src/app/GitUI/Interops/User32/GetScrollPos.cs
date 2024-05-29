using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.User32, SetLastError = true)]
        public static partial int GetScrollPos(IntPtr hWnd, SB nBar);
    }
}
