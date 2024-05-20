using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DTBGOPTS
        {
            public uint dwSize;
            public uint dwFlags;
            public RECT rcClip;
        }
    }
}
