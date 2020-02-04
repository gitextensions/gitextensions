using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd,
                                                int msg,
                                                IntPtr wParam,
                                                ref LVHITTESTINFO lParam);

        [DllImport(Libraries.User32, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd,
                                                int msg,
                                                IntPtr wParam,
                                                ref LVGROUP lParam);

        [DllImport(Libraries.User32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wp, ref FORMATRANGE lp);

        [DllImport(Libraries.User32, CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessageInt(
            IntPtr handle,
            uint msg,
            IntPtr wParam,
            IntPtr lParam);
    }
}
