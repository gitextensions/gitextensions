using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.UxTheme)]
        public static partial int GetThemeColor(IntPtr hTheme, int iPartId, int iStateId, int iPropId, out COLORREF pColor);
    }
}
