using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();
    }
}
