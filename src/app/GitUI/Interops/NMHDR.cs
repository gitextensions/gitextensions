namespace System
{
#if SUPPORT_THEME_HOOKS
    internal static partial class NativeMethods
    {
        public struct NMHDR
        {
            public IntPtr hwndFrom;
            public IntPtr idFrom;
            public int code;
        }
    }
 #endif
}
