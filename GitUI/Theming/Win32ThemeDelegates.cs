using System;
using System.Runtime.InteropServices;
using Windows.Win32.Graphics.Gdi;

namespace GitUI.Theming
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    internal delegate int GetSysColorDelegate(int nindex);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    internal delegate IntPtr GetSysColorBrushDelegate(int nindex);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    internal delegate int DrawThemeBackgroundDelegate(
        IntPtr htheme, IntPtr hdc,
        int partId, int stateId,
        NativeMethods.RECTCLS prect, NativeMethods.RECTCLS pcliprect);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    internal delegate int DrawThemeBackgroundExDelegate(
        IntPtr htheme, IntPtr hdc, int partId, int stateId,
        NativeMethods.RECTCLS prect, ref NativeMethods.DTBGOPTS poptions);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    internal delegate int GetThemeColorDelegate(
        IntPtr htheme, int ipartid, int istateid, int ipropid, out int pcolor);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    internal delegate int DrawThemeTextDelegate(
        IntPtr htheme, IntPtr hdc,
        int partid, int stateId,
        string psztext,
        int cchtext,
        DRAW_TEXT_FORMAT dwtextflags,
        int dwtextflags2,
        IntPtr prect);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    internal delegate int DrawThemeTextExDelegate(
        IntPtr htheme, IntPtr hdc,
        int partid, int stateid,
        string pszText,
        int cchText,
        DRAW_TEXT_FORMAT dwtextflags,
        IntPtr prect,
        ref NativeMethods.DTTOPTS poptions);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    internal delegate IntPtr CreateWindowExDelegate(
        int dwexstyle,
        IntPtr lpclassname, IntPtr lpwindowname, int dwstyle,
        int x, int y,
        int nwidth, int nheight,
        IntPtr hwndparent, IntPtr hmenu, IntPtr hinstance,
        IntPtr lpparam);
}
