using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct COLORREF
        {
            [FieldOffset(0)]
            public byte R;

            [FieldOffset(1)]
            public byte G;

            [FieldOffset(2)]
            public byte B;

            [FieldOffset(0)]
            public uint Value;

            public COLORREF(byte r, byte g, byte b)
                : this()
            {
                R = r;
                G = g;
                B = b;
            }

            public COLORREF(uint value)
                : this()
            {
                Value = value;
            }
        }
    }
}
