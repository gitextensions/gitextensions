using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.UxTheme)]
        public static partial int CloseThemeData(IntPtr hTheme);
    }
}
