using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GitUI
{
    internal static class NativeMethods
    {
        #region Unmanaged Code

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CHARRANGE
        {
            public int cpMin;         // First character of range (0 for start of doc)
            public int cpMax;           // Last character of range (-1 for end of doc)
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FORMATRANGE
        {
            public IntPtr hdc;             // Actual DC to draw on
            public IntPtr hdcTarget;       // Target DC for determining text formatting
            public RECT rc;                // Region of the DC to draw to (in twips)
            public RECT rcPage;            // Region of the whole DC (page size) (in twips)
            public CHARRANGE chrg;         // Range of text to draw (see earlier declaration)
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct LOGFONT
        {
            public const int LF_FACESIZE = 32;
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
            public string lfFaceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COLORREF
        {
            public byte R;
            public byte G;
            public byte B;
        }

        internal const int WM_USER = 0x0400;
        internal const int EM_FORMATRANGE = WM_USER + 57;
        internal const int WM_HSCROLL = 276;
        internal const int SB_LEFT = 6;

        // from vsstyle.h
        internal const int TEXT_MAININSTRUCTION = 1;

        // from vssym32.h
        internal const int TMT_TEXTCOLOR = 3803;
        internal const int TMT_FONT = 210;

        [DllImport("user32")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wp, ref FORMATRANGE lp);

        [DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        internal static extern IntPtr SendMessageInt(
            IntPtr handle,
            uint msg,
            IntPtr wParam,
            IntPtr lParam);
        internal const int EM_LINEINDEX = 0x00BB;
        internal const int EM_LINELENGTH = 0x00C1;
        internal const int EM_POSFROMCHAR = 0x00D6;
        internal const int EM_CHARFROMPOS = 0x00D7;
        internal const int EM_GETFIRSTVISIBLELINE = 0xCE;

        [DllImport("user32", EntryPoint = "ShowCaret")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowCaretAPI(
            IntPtr hwnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetLongPathName(string lpszShortPath, StringBuilder lpszLongPath, int cchBuffer);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenThemeData(IntPtr hWnd, string classList);

        [DllImport("uxtheme.dll", ExactSpelling = true)]
        internal static extern int CloseThemeData(IntPtr hTheme);

        [DllImport("uxtheme", ExactSpelling = true)]
        internal static extern int GetThemeColor(IntPtr hTheme, int iPartId, int iStateId, int iPropId, out COLORREF pColor);

        [DllImport("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int GetThemeFont(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, out LOGFONT pFont);

        #endregion
    }

    internal static class NativeConstants
    {
        internal const uint WM_MOUSEACTIVATE = 0x21;
        internal const uint MA_ACTIVATE = 1;
        internal const uint MA_ACTIVATEANDEAT = 2;
        internal const uint MA_NOACTIVATE = 3;
        internal const uint MA_NOACTIVATEANDEAT = 4;
        internal const int CP_NOCLOSE_BUTTON = 0x200;
    }
}
