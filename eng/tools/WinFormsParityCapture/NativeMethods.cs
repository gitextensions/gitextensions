using System.Runtime.InteropServices;

namespace WinFormsParityCapture;

internal static partial class NativeMethods
{
    internal const int WmDpiChanged = 0x02E0;
    internal const int WmDpiChangedBeforeParent = 0x02E2;
    internal const int WmDpiChangedAfterParent = 0x02E3;
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

    internal static Rectangle GetComboBoxListRectangle(IntPtr handle)
        => GetWindowRectangle(GetComboBoxListHandle(handle));

    internal static IntPtr GetComboBoxListHandle(IntPtr handle)
    {
        ComboBoxInfo info = new() { Size = Marshal.SizeOf<ComboBoxInfo>() };
        if (!GetComboBoxInfo(handle, ref info) || info.ListHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("GetComboBoxInfo did not expose the native list window.");
        }

        return info.ListHandle;
    }

    internal static bool IsUnoccluded(IntPtr target, Rectangle bounds, IReadOnlySet<IntPtr> capturedWindows)
    {
        const uint gwHwndPrev = 3;
        for (IntPtr above = GetWindow(target, gwHwndPrev); above != IntPtr.Zero; above = GetWindow(above, gwHwndPrev))
        {
            if (!capturedWindows.Contains(above) && !IsAssociatedPopupShadow(above, capturedWindows)
                && (IsWindowVisible(above) || IsRedrawDisabled(above)) && !IsIconic(above)
                && GetWindowRectangle(above).IntersectsWith(bounds))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool IsOwnedNativeSurfaceUnoccluded(IntPtr surface, IntPtr host, Rectangle bounds, IReadOnlySet<IntPtr> capturedWindows)
    {
        HashSet<IntPtr> visited = [];
        for (IntPtr current = surface; current != host;)
        {
            if (current == IntPtr.Zero || !visited.Add(current)
                || !IsUnoccluded(current, bounds, capturedWindows))
            {
                return false;
            }

            IntPtr parent = GetParent(current);
            if (parent == IntPtr.Zero || !IsWindowShown(parent)
                || !TryGetNativeClientRectangle(parent, out Rectangle clientBounds)
                || !clientBounds.Contains(bounds))
            {
                return false;
            }

            // Only the acquired frozen HWND is exempt from WS_VISIBLE. Check every
            // ancestor's siblings and client clipping, not merely its outer bounds.
            current = parent;
        }

        return IsWindowShown(host) && IsUnoccluded(host, bounds, capturedWindows);
    }

    internal static IReadOnlyList<NativeWindowOccluder> GetOccludingNativeWindows(IntPtr target, Rectangle bounds, IReadOnlySet<IntPtr> capturedWindows)
    {
        const uint previousWindow = 3;
        const uint ownerWindow = 4;
        List<NativeWindowOccluder> blockers = [];
        for (IntPtr above = GetWindow(target, previousWindow); above != IntPtr.Zero; above = GetWindow(above, previousWindow))
        {
            bool shown = IsWindowVisible(above);
            bool redrawDisabled = IsRedrawDisabled(above);
            if (capturedWindows.Contains(above) || IsAssociatedPopupShadow(above, capturedWindows)
                || (!shown && !redrawDisabled) || IsIconic(above))
            {
                continue;
            }

            Rectangle windowBounds = GetWindowRectangle(above);
            if (!windowBounds.IntersectsWith(bounds))
            {
                continue;
            }

            uint threadId = GetWindowThreadProcessId(above, out uint processId);
            blockers.Add(new NativeWindowOccluder(above.ToString(), GetParent(above).ToString(),
                GetWindow(above, ownerWindow).ToString(), GetNativeWindowClass(above), processId,
                threadId, GetClassLong(above, -26), GetWindow(above, previousWindow).ToString(),
                GetWindow(above, 2).ToString(), windowBounds, shown, redrawDisabled));
        }

        return blockers;
    }

    internal static IReadOnlyList<NativePopupShadow> GetAssociatedPopupShadows(IEnumerable<IntPtr> requestedPopups)
    {
        const uint nextWindow = 2;
        const uint previousWindow = 3;
        List<NativePopupShadow> shadows = [];
        foreach (IntPtr popup in requestedPopups.Distinct())
        {
            IntPtr shadow = GetWindow(popup, nextWindow);
            if (!IsAssociatedPopupShadow(shadow, popup))
            {
                continue;
            }

            uint threadId = GetWindowThreadProcessId(popup, out uint processId);
            uint shadowThreadId = GetWindowThreadProcessId(shadow, out uint shadowProcessId);
            shadows.Add(new NativePopupShadow(shadow.ToString(), popup.ToString(), processId, threadId,
                shadowProcessId, shadowThreadId, GetClassLong(popup, -26), GetWindow(popup, nextWindow).ToString(),
                GetWindow(shadow, previousWindow).ToString(), GetWindowRectangle(shadow), GetWindowRectangle(popup)));
        }

        return shadows;
    }

    internal static IReadOnlyList<NativeWindowOccluder> GetNativeWindowAndNeighborReceipts(IntPtr handle)
    {
        List<NativeWindowOccluder> windows = [];
        foreach (IntPtr window in new[] { handle, GetWindow(handle, 3), GetWindow(handle, 2) }.Where(window => window != IntPtr.Zero).Distinct())
        {
            uint threadId = GetWindowThreadProcessId(window, out uint processId);
            windows.Add(new NativeWindowOccluder(window.ToString(), GetParent(window).ToString(),
                GetWindow(window, 4).ToString(), GetNativeWindowClass(window), processId, threadId,
                GetClassLong(window, -26), GetWindow(window, 3).ToString(), GetWindow(window, 2).ToString(),
                GetWindowRectangle(window), IsWindowShown(window), IsRedrawDisabled(window)));
        }

        return windows;
    }

    private static bool IsAssociatedPopupShadow(IntPtr shadow, IReadOnlySet<IntPtr> capturedWindows)
        => capturedWindows.Any(popup => IsAssociatedPopupShadow(shadow, popup));

    private static bool IsAssociatedPopupShadow(IntPtr shadow, IntPtr popup)
    {
        const int classStyle = -26;
        const uint dropShadow = 0x00020000;
        const uint nextWindow = 2;
        const uint previousWindow = 3;
        if (shadow == IntPtr.Zero || popup == IntPtr.Zero
            || !IsWindowShown(popup) || !IsWindowShown(shadow)
            || (Control.FromHandle(popup) is not ToolStripDropDown && GetNativeWindowClass(popup) != "ComboLBox")
            || (GetClassLong(popup, classStyle) & dropShadow) == 0
            || GetNativeWindowClass(shadow) != "SysShadow"
            || GetWindow(popup, nextWindow) != shadow || GetWindow(shadow, previousWindow) != popup)
        {
            return false;
        }

        uint popupThread = GetWindowThreadProcessId(popup, out uint popupProcess);
        uint shadowThread = GetWindowThreadProcessId(shadow, out uint shadowProcess);
        if (popupThread == 0 || popupProcess == 0 || popupThread != shadowThread || popupProcess != shadowProcess)
        {
            return false;
        }

        // CS_DROPSHADOW creates native decoration outside the popup HWND. Accept
        // only its actual adjacent SysShadow, never a class/process-wide exception.
        Rectangle popupBounds = GetWindowRectangle(popup);
        Rectangle shadowBounds = GetWindowRectangle(shadow);
        return shadowBounds.Location == popupBounds.Location && shadowBounds.Contains(popupBounds);
    }

    internal static IntPtr GetNativeParentWindow(IntPtr handle) => GetParent(handle);

    internal static bool TryGetNativeClientRectangle(IntPtr handle, out Rectangle bounds)
    {
        bounds = Rectangle.Empty;
        if (!GetClientRect(handle, out NativeRectangle rectangle))
        {
            return false;
        }

        // Map both RECT corners together so native RTL window layouts retain their
        // physical client bounds; MapWindowPoints may validly return a zero offset.
        int offset = MapWindowPoints(handle, IntPtr.Zero, ref rectangle, 2);
        if (offset == 0 && Marshal.GetLastPInvokeError() != 0)
        {
            return false;
        }

        bounds = Rectangle.FromLTRB(Math.Min(rectangle.Left, rectangle.Right), Math.Min(rectangle.Top, rectangle.Bottom),
            Math.Max(rectangle.Left, rectangle.Right), Math.Max(rectangle.Top, rectangle.Bottom));
        return true;
    }

    private static unsafe string GetNativeWindowClass(IntPtr handle)
    {
        const int maximumClassNameLength = 256;
        Span<char> name = stackalloc char[maximumClassNameLength];
        fixed (char* buffer = name)
        {
            int length = GetClassName(handle, buffer, name.Length);
            return new string(name[..length]);
        }
    }

    internal static bool PrintWindowContent(IntPtr handle, IntPtr deviceContext) =>
        PrintWindow(handle, deviceContext, PwRenderFullContent);

    // DefWindowProc removes WS_VISIBLE for WM_SETREDRAW(FALSE), but leaves the
    // previously painted pixels on screen. Do not infer this state from Visible.
    internal static bool IsRedrawDisabled(IntPtr handle) => GetProp(handle, "SysSetRedraw") != IntPtr.Zero;

    internal static bool IsWindowShown(IntPtr handle) => IsWindowVisible(handle) && !IsIconic(handle);

    internal static bool IsEntirelyOnScreen(Rectangle bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return false;
        }

        using Region uncovered = new(bounds);
        foreach (Screen screen in Screen.AllScreens)
        {
            uncovered.Exclude(screen.Bounds);
        }

        using Bitmap pixel = new(1, 1);
        using Graphics graphics = Graphics.FromImage(pixel);
        return uncovered.IsEmpty(graphics);
    }

    internal static void FocusWindow(IntPtr handle) => SetFocus(handle);

    internal static Point GetCursorPosition()
    {
        if (!GetCursorPos(out NativePoint point))
        {
            throw new InvalidOperationException("GetCursorPos failed.");
        }

        return new Point(point.X, point.Y);
    }

    internal static void SetCursorPosition(Point point)
    {
        if (!SetCursorPos(point.X, point.Y))
        {
            throw new InvalidOperationException("SetCursorPos failed.");
        }
    }

    internal static void SendDpiChanged(IntPtr handle, int dpi, Rectangle suggestedBounds)
    {
        IReadOnlyList<IntPtr> descendants = GetDescendantWindows(handle);
        for (int index = descendants.Count - 1; index >= 0; index--)
        {
            SendDpiMessage(descendants[index], WmDpiChangedBeforeParent, dpi);
        }

        NativeRectangle rectangle = new()
        {
            Left = suggestedBounds.Left,
            Top = suggestedBounds.Top,
            Right = suggestedBounds.Right,
            Bottom = suggestedBounds.Bottom
        };
        IntPtr wParam = (IntPtr)((dpi & 0xFFFF) | (dpi << 16));
        SendMessage(handle, WmDpiChanged, wParam, ref rectangle);

        foreach (IntPtr descendant in descendants)
        {
            SendDpiMessage(descendant, WmDpiChangedAfterParent, dpi);
        }
    }

    internal static IReadOnlyList<IntPtr> GetDescendantWindows(IntPtr handle)
    {
        List<IntPtr> descendants = [];
        AddDescendantWindows(handle, descendants);
        return descendants;
    }

    internal static void SendDpiChangedBeforeParentTree(IntPtr handle, int dpi)
    {
        List<IntPtr> windows = [handle, .. GetDescendantWindows(handle)];
        for (int index = windows.Count - 1; index >= 0; index--)
        {
            SendDpiMessage(windows[index], WmDpiChangedBeforeParent, dpi);
        }
    }

    internal static void SendDpiChangedAfterParentTree(IntPtr handle, int dpi)
    {
        SendDpiMessage(handle, WmDpiChangedAfterParent, dpi);
        foreach (IntPtr descendant in GetDescendantWindows(handle))
        {
            SendDpiMessage(descendant, WmDpiChangedAfterParent, dpi);
        }
    }

    private static void AddDescendantWindows(IntPtr parent, List<IntPtr> descendants)
    {
        const uint gwChild = 5;
        const uint gwHwndNext = 2;

        for (IntPtr child = GetWindow(parent, gwChild);
             child != IntPtr.Zero;
             child = GetWindow(child, gwHwndNext))
        {
            descendants.Add(child);
            AddDescendantWindows(child, descendants);
        }
    }

    private static void SendDpiMessage(IntPtr handle, int message, int dpi)
    {
        // WinForms deliberately accepts the target DPI in WM_DPICHANGED_BEFOREPARENT's
        // otherwise-unused wParam for test-driven monitor transitions. A real PMv2 move
        // updates GetDpiForWindow before sending the message; the fallback cannot change
        // the physical monitor, so it supplies the same value through that supported path.
        IntPtr wParam = (IntPtr)((dpi & 0xFFFF) | (dpi << 16));
        SendMessage(handle, message, wParam, IntPtr.Zero);
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
    private static partial bool GetClientRect(IntPtr window, out NativeRectangle rectangle);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int MapWindowPoints(IntPtr source, IntPtr target, ref NativeRectangle rectangle, uint pointCount);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetParent(IntPtr window);

    [LibraryImport("user32.dll", EntryPoint = "GetClassNameW")]
    private static unsafe partial int GetClassName(IntPtr window, char* name, int length);

    [LibraryImport("user32.dll")]
    private static partial uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [LibraryImport("user32.dll", EntryPoint = "GetClassLongW")]
    private static partial uint GetClassLong(IntPtr window, int index);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetComboBoxInfo(IntPtr comboBox, ref ComboBoxInfo info);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PrintWindow(IntPtr window, IntPtr deviceContext, uint flags);

    [LibraryImport("user32.dll")]
    private static partial IntPtr SetFocus(IntPtr window);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out NativePoint point);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetCursorPos(int x, int y);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, ref NativeRectangle lParam);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetWindow(IntPtr window, uint command);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(IntPtr window);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(IntPtr window);

    [LibraryImport("user32.dll", EntryPoint = "GetPropW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr GetProp(IntPtr window, string name);

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

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ComboBoxInfo
    {
        public int Size;
        public NativeRectangle ItemRectangle;
        public NativeRectangle ButtonRectangle;
        public uint ButtonState;
        public IntPtr ComboBoxHandle;
        public IntPtr EditHandle;
        public IntPtr ListHandle;
    }
}

internal sealed record NativeWindowOccluder(
    string Handle,
    string Parent,
    string Owner,
    string ClassName,
    uint ProcessId,
    uint ThreadId,
    uint ClassStyle,
    string PreviousWindow,
    string NextWindow,
    Rectangle Bounds,
    bool Shown,
    bool RedrawDisabled);

internal sealed record NativePopupShadow(
    string Handle,
    string PopupHandle,
    uint ProcessId,
    uint ThreadId,
    uint ShadowProcessId,
    uint ShadowThreadId,
    uint PopupClassStyle,
    string PopupNextWindow,
    string ShadowPreviousWindow,
    Rectangle Bounds,
    Rectangle PopupBounds);
