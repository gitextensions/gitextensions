namespace System
{
    internal static partial class NativeMethods
    {
        /// <summary>
        ///  Scroll bar values (SB_) that indicates the user's scrolling request in a horizontal scrollbar.
        ///  Used by WM_HSCROLL message.
        /// </summary>
        public enum SBH : int
        {
            LINELEFT = 0,
            LINERIGHT = 1,
            PAGELEFT = 2,
            PAGERIGHT = 3,
            THUMBPOSITION = 4,
            THUMBTRACK = 5,
            LEFT = 6,
            RIGHT = 7,
            ENDSCROLL = 8,
        }
    }
}
