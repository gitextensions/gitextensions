using System.Runtime.InteropServices;
using static System.Interop;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.Gdi32)]
        public static partial BOOL DeleteObject(IntPtr hObject);
    }
}
