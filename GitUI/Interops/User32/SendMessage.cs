using Windows.Win32.Foundation;

#pragma warning disable SA1305 // Field names should not use Hungarian notation
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Windows.Win32
{
    partial class PInvoke
    {
        public static unsafe LRESULT SendMessage<T>(
            HWND hWnd,
            uint Msg,
            WPARAM wParam,
            ref T lParam) where T : unmanaged
        {
            fixed (void* l = &lParam)
            {
                return SendMessage(hWnd, Msg, wParam, (nint)l);
            }
        }
    }
}
