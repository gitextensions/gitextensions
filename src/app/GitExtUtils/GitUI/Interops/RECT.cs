using System.Runtime.InteropServices;

namespace System;

internal static partial class NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
