using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        internal const int TVIF_STATE = 0x8;
        internal const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        internal const int TVM_SETITEM = TV_FIRST + 63;

        /// <summary>
        /// A helper for hiding the checkboxes of nodes in a TreeView with <see cref="System.Windows.Forms.TreeView.CheckBoxes"/> enabled.
        /// Inspired by https://stackoverflow.com/a/4826740 .
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        internal struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }

        [DllImport(Libraries.User32, CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref TVITEM lParam);
    }
}
