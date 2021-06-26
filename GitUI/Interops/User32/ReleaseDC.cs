using System;
using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32)]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);
    }
}
