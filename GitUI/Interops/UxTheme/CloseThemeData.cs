using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.UxTheme, ExactSpelling = true)]
        public static extern int CloseThemeData(IntPtr hTheme);
    }
}
