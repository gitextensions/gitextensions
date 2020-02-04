using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LVGROUP
        {
            public int CbSize;
            public ListViewGroupMask Mask;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PszHeader;
            public int CchHeader;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PszFooter;
            public int CchFooter;
            public int IGroupId;
            public int StateMask;
            public ListViewGroupState State;
            public uint UAlign;
        }
    }
}
