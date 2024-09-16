using System.Runtime.InteropServices;
using static System.Interop;

namespace System;

internal static partial class NativeMethods
{
    [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
    internal static unsafe extern BOOL GetComboBoxInfo(nint hWnd, COMBOBOXINFO* pcbi);
}
