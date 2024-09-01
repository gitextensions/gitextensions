using System.Runtime.InteropServices;

namespace System;

internal static partial class NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct COMBOBOXINFO
    {
        public int cbSize;
        public RECT rcItem;
        public RECT rcButton;
        public ComboBoxButtonState buttonState;
        public IntPtr hwndCombo;
        public IntPtr hwndEdit;
        public IntPtr hwndList;

        public static COMBOBOXINFO Create() => new() { cbSize = Marshal.SizeOf<COMBOBOXINFO>() };
    }
}
