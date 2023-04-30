using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        public static IntPtr HWND_BROADCAST => (IntPtr)0xFFFF;

        [DllImport(Libraries.User32, ExactSpelling = true)]
        public static extern IntPtr PostMessageW(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam = default,
            IntPtr lParam = default);

        public static IntPtr PostMessageW(
            HandleRef hWnd,
            uint Msg,
            IntPtr wParam = default,
            IntPtr lParam = default)
        {
            IntPtr result = PostMessageW(hWnd.Handle, Msg, wParam, lParam);
            GC.KeepAlive(hWnd.Wrapper);
            return result;
        }

        public static unsafe IntPtr PostMessageW<T>(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            ref T lParam) where T : unmanaged
        {
            fixed (void* l = &lParam)
            {
                return PostMessageW(hWnd, Msg, wParam, (IntPtr)l);
            }
        }
    }
}
