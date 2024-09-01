using System.Runtime.InteropServices;

namespace System;

internal static partial class NativeMethods
{
    [DllImport("user32.dll")]
    internal static extern int GetWindowLong(IntPtr hwnd, int nIndex);
}
