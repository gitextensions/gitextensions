using Windows.Win32;

namespace System
{
    internal static partial class NativeMethods
    {
        /// <summary>
        ///  Scroll bar values (SB_) that indicates the user's scrolling request in a horizontal scrollbar.
        ///  Used by WM_HSCROLL message.
        /// </summary>
        public enum SBH : uint
        {
            LINELEFT = Constants.SB_LINELEFT,
            LINERIGHT = Constants.SB_LINERIGHT,
            PAGELEFT = Constants.SB_PAGELEFT,
            PAGERIGHT = Constants.SB_PAGERIGHT,
            THUMBPOSITION = Constants.SB_THUMBPOSITION,
            THUMBTRACK = Constants.SB_THUMBTRACK,
            LEFT = Constants.SB_LEFT,
            RIGHT = Constants.SB_RIGHT,
            ENDSCROLL = Constants.SB_ENDSCROLL,
        }
    }
}
