using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.User32)]
        public static partial void ReleaseDC(IntPtr hwnd, IntPtr dc);
    }
}
