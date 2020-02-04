using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.Gdi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateSolidBrush(int nIndex);
    }
}
