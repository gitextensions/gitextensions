namespace System
{
    internal static partial class NativeMethods
    {
        public struct FORMATRANGE
        {
            public IntPtr hdc;             // Actual DC to draw on
            public IntPtr hdcTarget;       // Target DC for determining text formatting
            public RECT rc;                // Region of the DC to draw to (in twips)
            public RECT rcPage;            // Region of the whole DC (page size) (in twips)
            public CHARRANGE chrg;         // Range of text to draw (see earlier declaration)
        }
    }
}
