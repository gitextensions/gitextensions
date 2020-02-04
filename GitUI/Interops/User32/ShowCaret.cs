using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32, EntryPoint = "ShowCaret")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowCaretAPI(IntPtr hwnd);
    }
}
