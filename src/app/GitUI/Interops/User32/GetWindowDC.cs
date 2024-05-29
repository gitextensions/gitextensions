using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.User32)]
        public static partial IntPtr GetWindowDC(IntPtr hwnd);
    }
}
