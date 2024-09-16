using System.Runtime.InteropServices;

namespace System;

internal static partial class NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct COMBOBOXINFO
    {
        public uint cbSize;
        public RECT rcItem;
        public RECT rcButton;
        public ComboBoxButtonState buttonState;
        public nint hwndCombo;
        public nint hwndEdit;
        public nint hwndList;
    }
}
