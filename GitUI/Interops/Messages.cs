namespace System
{
    internal static partial class NativeMethods
    {
        public const int WM_USER = 0x0400;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_MOUSEACTIVATE = 0x21;
        public const int WM_SYSCOLORCHANGE = 21;

        public const int WM_HSCROLL = 276;
        public const int WM_VSCROLL = 0x115;
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WM_KEYDOWN = 0x0100;

        public const int LVM_FIRST = 0x1000;
        public const int LVM_HITTEST = LVM_FIRST + 18;
        public const int LVM_SETGROUPINFO = LVM_FIRST + 147;
        public const int LVM_SUBITEMHITTEST = LVM_FIRST + 57;
        public const int LVM_INSERTGROUP = LVM_FIRST + 145;

        public const uint MA_ACTIVATE = 1;
        public const uint MA_ACTIVATEANDEAT = 2;
        public const uint MA_NOACTIVATE = 3;
        public const uint MA_NOACTIVATEANDEAT = 4;
        public const int CP_NOCLOSE_BUTTON = 0x200;

        // from vsstyle.h
        public const int TEXT_MAININSTRUCTION = 1;

        // from vssym32.h
        public const int TMT_TEXTCOLOR = 3803;
        public const int TMT_FONT = 210;

        public const int EM_FORMATRANGE = WM_USER + 57;
        public const int EM_LINEINDEX = 0x00BB;
        public const int EM_LINELENGTH = 0x00C1;
        public const int EM_POSFROMCHAR = 0x00D6;
        public const int EM_CHARFROMPOS = 0x00D7;
        public const int EM_GETFIRSTVISIBLELINE = 0xCE;
    }
}
