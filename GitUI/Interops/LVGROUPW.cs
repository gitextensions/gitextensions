using System.Runtime.InteropServices;

namespace System
{
#if SUPPORT_THEME_HOOKS
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public unsafe struct LVGROUPW
        {
            public uint cbSize;
            public LVGF mask;
            public char* pszHeader;
            public int cchHeader;
            public char* pszFooter;
            public int cchFooter;
            public int iGroupId;
            public LVGS stateMask;
            public LVGS state;
            public LVGA uAlign;
        }
    }
#endif
}
