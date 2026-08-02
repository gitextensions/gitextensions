using System.Runtime.InteropServices;

namespace WinFormsParityCapture;

internal static partial class NativeMethods
{
    internal const int WmDpiChanged = 0x02E0;
    internal const int WmMouseMove = 0x0200;
    internal const int WmLButtonDown = 0x0201;
    internal const int WmMouseLeave = 0x02A3;
    internal const int WmCancelMode = 0x001F;
    internal const int PwRenderFullContent = 0x00000002;

    private const int MonitorDefaultToNearest = 2;

    internal static IReadOnlyList<CaptureMonitor> GetMonitors()
    {
        List<CaptureMonitor> monitors = [];
        EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            (monitor, _, _, _) =>
            {
                MonitorInfo info = new() { Size = Marshal.SizeOf<MonitorInfo>() };
                if (!GetMonitorInfo(monitor, ref info))
                {
                    return true;
                }

                GetDpiForMonitor(monitor, MonitorDpiType.Effective, out uint dpiX, out uint dpiY);
                monitors.Add(new CaptureMonitor(
                    info.Work.Left,
                    info.Work.Top,
                    info.Work.Right - info.Work.Left,
                    info.Work.Bottom - info.Work.Top,
                    checked((int)dpiX),
                    checked((int)dpiY)));
                return true;
            },
            IntPtr.Zero);
        return monitors;
    }

    internal static int GetWindowDpi(IntPtr handle) => checked((int)GetDpiForWindow(handle));

    internal static Rectangle GetWindowRectangle(IntPtr handle)
    {
        if (!GetWindowRect(handle, out NativeRectangle rectangle))
        {
            throw new InvalidOperationException("GetWindowRect failed.");
        }

        return Rectangle.FromLTRB(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
    }

    internal static bool PrintWindowContent(IntPtr handle, IntPtr deviceContext) =>
        PrintWindow(handle, deviceContext, PwRenderFullContent);

    internal static void SendDpiChanged(IntPtr handle, int dpi, Rectangle suggestedBounds)
    {
        NativeRectangle rectangle = new()
        {
            Left = suggestedBounds.Left,
            Top = suggestedBounds.Top,
            Right = suggestedBounds.Right,
            Bottom = suggestedBounds.Bottom
        };
        IntPtr wParam = (IntPtr)((dpi & 0xFFFF) | (dpi << 16));
        SendMessage(handle, WmDpiChanged, wParam, ref rectangle);
    }

    internal static void SendMouseMessage(IntPtr handle, int message, int x, int y)
    {
        IntPtr lParam = (IntPtr)((x & 0xFFFF) | (y << 16));
        SendMessage(handle, message, IntPtr.Zero, lParam);
    }

    internal static CaptureMonitor GetNearestMonitor(IntPtr window)
    {
        IntPtr monitor = MonitorFromWindow(window, MonitorDefaultToNearest);
        MonitorInfo info = new() { Size = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfo(monitor, ref info))
        {
            throw new InvalidOperationException("GetMonitorInfo failed.");
        }

        GetDpiForMonitor(monitor, MonitorDpiType.Effective, out uint dpiX, out uint dpiY);
        return new CaptureMonitor(
            info.Work.Left,
            info.Work.Top,
            info.Work.Right - info.Work.Left,
            info.Work.Bottom - info.Work.Top,
            checked((int)dpiX),
            checked((int)dpiY));
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumDisplayMonitors(
        IntPtr deviceContext,
        IntPtr clippingRectangle,
        MonitorEnumProc callback,
        IntPtr data);

    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

    [LibraryImport("shcore.dll")]
    private static partial int GetDpiForMonitor(IntPtr monitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(IntPtr window);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr window, out NativeRectangle rectangle);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PrintWindow(IntPtr window, IntPtr deviceContext, uint flags);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, ref NativeRectangle lParam);

    [LibraryImport("user32.dll")]
    private static partial IntPtr MonitorFromWindow(IntPtr window, int flags);

    private delegate bool MonitorEnumProc(IntPtr monitor, IntPtr deviceContext, IntPtr rectangle, IntPtr data);

    private enum MonitorDpiType
    {
        Effective = 0
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRectangle Monitor;
        public NativeRectangle Work;
        public uint Flags;
    }
}
