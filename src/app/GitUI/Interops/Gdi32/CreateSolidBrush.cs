using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.Gdi32, SetLastError = true)]
        public static partial IntPtr CreateSolidBrush(int crColor);
    }
}
