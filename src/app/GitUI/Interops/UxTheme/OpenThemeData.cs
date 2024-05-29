using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.UxTheme, StringMarshalling = StringMarshalling.Utf16)]
        public static partial IntPtr OpenThemeData(IntPtr hWnd, string classList);
    }
}
