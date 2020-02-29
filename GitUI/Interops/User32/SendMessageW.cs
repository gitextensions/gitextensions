using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.User32, ExactSpelling = true)]
        public static extern IntPtr SendMessageW(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam = default,
            IntPtr lParam = default);

        public static IntPtr SendMessageW(
            HandleRef hWnd,
            uint Msg,
            IntPtr wParam = default,
            IntPtr lParam = default)
        {
            IntPtr result = SendMessageW(hWnd.Handle, Msg, wParam, lParam);
            GC.KeepAlive(hWnd.Wrapper);
            return result;
        }

        public static unsafe IntPtr SendMessageW<T>(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            ref T lParam) where T : unmanaged
        {
            fixed (void* l = &lParam)
            {
                return SendMessageW(hWnd, Msg, wParam, (IntPtr)l);
            }
        }
    }
}
