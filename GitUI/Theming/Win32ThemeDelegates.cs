using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Controls;

namespace GitUI.Theming
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    internal delegate int GetSysColorDelegate(int nindex);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    internal delegate IntPtr GetSysColorBrushDelegate(int nindex);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    internal unsafe delegate int DrawThemeBackgroundDelegate(
        IntPtr htheme, HDC hdc,
        int partId, int stateId,
        in RECT prect, [In] RECT* pcliprect);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    internal unsafe delegate int DrawThemeBackgroundExDelegate(
        IntPtr htheme, HDC hdc, int partId, int stateId,
        in RECT prect, [In] DTBGOPTS* poptions);

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
        ref DTTOPTS poptions);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    internal delegate IntPtr CreateWindowExDelegate(
        int dwexstyle,
        IntPtr lpclassname, IntPtr lpwindowname, int dwstyle,
        int x, int y,
        int nwidth, int nheight,
        IntPtr hwndparent, IntPtr hmenu, IntPtr hinstance,
        IntPtr lpparam);
}
