using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DTBGOPTS
        {
            public int dwSize;
            public int dwFlags;
            public RECT rcClip;
        }
    }
}
