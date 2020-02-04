using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.UxTheme, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int GetThemeFont(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, out LOGFONT pFont);
    }
}
