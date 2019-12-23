using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace GitUI
{
    internal static class NativeMethods
    {
        #region Unmanaged Code

        [StructLayout(LayoutKind.Sequential)]
        internal readonly struct POINT
        {
            public readonly int X;
            public readonly int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public static implicit operator Rectangle(RECT rect) =>
                Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
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

        internal delegate int DTT_CALLBACK_PROC(IntPtr hdc,
            [MarshalAs(UnmanagedType.LPWStr)] string text, int textLen, ref RECT rc, int flags,
            IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        internal struct DTTOPTS
        {
            public int dwSize;
            public DTT dwFlags;
            public int crText;
            public int crBorder;
            public int crShadow;
            public TEXTSHADOWTYPE iTextShadowType;
            public POINT ptShadowOffset;
            public int iBorderSize;
            public int iFontPropId;
            public int iColorPropId;
            public int iStateId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fApplyOverlay;
            public int iGlowSize;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public DTT_CALLBACK_PROC pfnDrawTextCallback;
            public IntPtr lParam;
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

        [Flags]
        internal enum DTT : int
        {
            TextColor = 1,
            BorderColor = 1 << 1,
            ShadowColor = 1 << 2,
            ShadowType = 1 << 3,
            ShadowOffset = 1 << 4,
            BorderSize = 1 << 5,
            FontProp = 1 << 6,
            ColorProp = 1 << 7,
            StateID = 1 << 8,
            CalcRect = 1 << 9,
            ApplyOverlay = 1 << 10,
            GlowSize = 1 << 11,
            Callback = 1 << 12,
            Composited = 1 << 13
        }

        internal enum DT : int
        {
            DT_LEFT = 0x0,
            DT_TOP = 0x0,
            DT_CENTER = 0x1,
            DT_RIGHT = 0x2,
            DT_VCENTER = 0x4,
            DT_BOTTOM = 0x8,
            DT_WORDBREAK = 0x10,
            DT_SINGLELINE = 0x20,
            DT_EXPANDTABS = 0x40,
            DT_TABSTOP = 0x80,
            DT_NOCLIP = 0x100,
            DT_EXTERNALLEADING = 0x200,
            DT_CALCRECT = 0x400,
            DT_NOPREFIX = 0x800,
            DT_INTERNAL = 0x1000,
            DT_EDITCONTROL = 0x2000,
            DT_PATH_ELLIPSIS = 0x4000,
            DT_END_ELLIPSIS = 0x8000,
            DT_MODIFYSTRING = 0x10000,
            DT_RTLREADING = 0x20000,
            DT_WORD_ELLIPSIS = 0x40000,
            DT_NOFULLWIDTHCHARBREAK = 0x80000,
            DT_HIDEPREFIX = 0x100000,
            DT_PREFIXONLY = 0x200000,
        }

        internal enum TEXTSHADOWTYPE : int
        {
            None = 0,
            Single = 1,
            Continuous = 2,
        }

        [DllImport("user32", EntryPoint = "ShowCaret")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowCaretAPI(IntPtr hwnd);

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

        [DllImport("wininet.dll")]
        internal static extern bool InternetGetConnectedState(out int description, int reservedValue);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateSolidBrush(int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        #endregion
    }

    internal static class NativeConstants
    {
        public const uint WM_MOUSEACTIVATE = 0x21;
        public const uint MA_ACTIVATE = 1;
        public const uint MA_ACTIVATEANDEAT = 2;
        public const uint MA_NOACTIVATE = 3;
        public const uint MA_NOACTIVATEANDEAT = 4;
        public const int CP_NOCLOSE_BUTTON = 0x200;
        public const int WM_SYSCOLORCHANGE = 21;
    }
}
