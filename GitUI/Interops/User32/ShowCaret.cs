using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32, ExactSpelling = true)]
        public static extern BOOL ShowCaret(IntPtr hWnd);
    }
}
