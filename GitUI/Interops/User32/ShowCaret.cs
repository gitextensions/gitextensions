using System.Runtime.InteropServices;
using static System.Interop;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32, ExactSpelling = true)]
        public static extern BOOL ShowCaret(IntPtr hWnd);
    }
}
