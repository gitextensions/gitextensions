using System.Runtime.InteropServices;
using static System.Interop;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.User32)]
        public static partial BOOL ShowCaret(IntPtr hWnd);
    }
}
