using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32)]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);
    }
}
