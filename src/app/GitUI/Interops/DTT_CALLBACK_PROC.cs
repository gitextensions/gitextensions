using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        public delegate int DTT_CALLBACK_PROC(IntPtr hdc,
            [MarshalAs(UnmanagedType.LPWStr)] string text, int textLen, ref RECTCLS rc, int flags,
            IntPtr lParam);
    }
}
