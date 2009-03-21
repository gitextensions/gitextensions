using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GitUI
{
    internal class NativeMethods
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
            public int cpMin;         //First character of range (0 for start of doc)
            public int cpMax;           //Last character of range (-1 for end of doc)
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FORMATRANGE
        {
            public IntPtr hdc;             //Actual DC to draw on
            public IntPtr hdcTarget;       //Target DC for determining text formatting
            public RECT rc;                //Region of the DC to draw to (in twips)
            public RECT rcPage;            //Region of the whole DC (page size) (in twips)
            public CHARRANGE chrg;         //Range of text to draw (see earlier declaration)
        }

        internal const int WM_USER = 0x0400;
        internal const int EM_FORMATRANGE = WM_USER + 57;

        [DllImport("user32")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wp, ref FORMATRANGE lp);



        [DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        internal extern static IntPtr SendMessageInt(
            IntPtr handle,
            uint msg,
            IntPtr wParam,
            IntPtr lParam
            );
        internal const int EM_LINEINDEX = 0x00BB;
        internal const int EM_LINELENGTH = 0x00C1;
        internal const int EM_POSFROMCHAR = 0x00D6;
        internal const int EM_CHARFROMPOS = 0x00D7;
        internal const int EM_GETFIRSTVISIBLELINE = 0xCE;

        [DllImport("user32", EntryPoint = "ShowCaret")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool ShowCaretAPI(
            IntPtr hwnd);
        #endregion

        private NativeMethods() { }
    }
}
