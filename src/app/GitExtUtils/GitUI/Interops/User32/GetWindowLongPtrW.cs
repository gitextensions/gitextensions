using System.Runtime.InteropServices;

namespace System;

internal static partial class NativeMethods
{
    [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
    internal static extern nint GetWindowLongPtrW(nint hWnd, GWL nIndex);
}
