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

        public static readonly IntPtr FALSE = IntPtr.Zero;
        public static readonly IntPtr TRUE = new(1);

        /// <summary>
        /// The WM_NULL message performs no operation. An application sends the WM_NULL message if it wants to post a message that the recipient window will ignore.
        /// </summary>
        public const int WM_NULL = 0x0000;

        /// <summary>
        /// The WM_SETREDRAW message is sent to a window to allow changes in that window to be redrawn, or to prevent changes in that window from being redrawn.
        /// </summary>
        public const int WM_SETREDRAW = 0x000B;

        /// <summary>
        /// The WM_PAINT message is sent when the system or another application makes a request to paint a portion of an application's window. The message is sent when the UpdateWindow or RedrawWindow function is called, or by the DispatchMessage function when the application obtains a WM_PAINT message by using the GetMessage or PeekMessage function.
        /// </summary>
        public const int WM_PAINT = 0x000F;

        /// <summary>
        /// The WM_NCPAINT message is sent to a window when its frame must be painted.
        /// </summary>
        public const int WM_NCPAINT = 0x0085;

        /// <summary>
        /// The WM_SYSCOMMAND message is sent when the user chooses a command from the Window menu
        /// or when the user chooses the maximize button, minimize button, restore button, or close button.
        /// </summary>
        public const int WM_SYSCOMMAND = 0x0112;

        /// <summary>
        /// The WM_CTLCOLORSCROLLBAR message is sent to the parent window of a scroll bar control when the control is about to be drawn.
        /// </summary>
        public const int WM_CTLCOLORSCROLLBAR = 0x0137;

        /// <summary>
        /// The WM_MOUSEHOVER message is posted to a window when the cursor hovers over the client area of the window for the period of time specified in a prior call to TrackMouseEvent.
        /// </summary>
        public const int WM_MOUSEHOVER = 0x02A1;

        /// <summary>
        /// The WM_MOUSELEAVE message is posted to a window when the cursor leaves the client area of the window specified in a prior call to TrackMouseEvent.
        /// </summary>
        public const int WM_MOUSELEAVE = 0x02A3;

        /// <summary>
        /// The WM_NCMOUSEHOVER message is posted to a window when the cursor hovers over the nonclient area of the window for the period of time specified in a prior call to TrackMouseEvent.
        /// </summary>
        public const int WM_NCMOUSEHOVER = 0x02A0;

        /// <summary>
        /// The WM_NCMOUSELEAVE message is posted to a window when the cursor leaves the nonclient area of the window specified in a prior call to TrackMouseEvent.
        /// </summary>
        public const int WM_NCMOUSELEAVE = 0x02A2;

        /// <summary>
        /// The type of a WM_SYSCOMMAND (given in WParam).
        /// </summary>
        public const nint SC_CLOSE = 0xf060;
    }
}
