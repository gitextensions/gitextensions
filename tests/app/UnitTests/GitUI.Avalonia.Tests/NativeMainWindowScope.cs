using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// Gives the headless test host a native main window, so the original
/// <c>EnvUtils.RunningOnWindowsWithMainWindow</c> guard and the taskbar owner handle see one.
/// The window is a 1x1 non-activating tool window far outside every monitor: it never takes focus,
/// gets no taskbar button and is not visible. Tests using it must not run in parallel with others,
/// because every caller of that guard sees the window while it exists.
/// </summary>
internal sealed class NativeMainWindowScope : IDisposable
{
    private const int ToolWindow = 0x00000080;
    private const int NoActivate = 0x08000000;
    private const uint Popup = 0x80000000;
    private const int ShowWithoutActivating = 4;

    private NativeMainWindowScope(nint handle)
    {
        Handle = handle;
    }

    internal nint Handle { get; }

    internal static NativeMainWindowScope Create()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }

        nint handle = CreateWindowExW(ToolWindow | NoActivate, "STATIC", string.Empty, Popup, -32000, -32000, 1, 1, 0, 0, 0, 0);
        if (handle == 0)
        {
            throw new InvalidOperationException($"CreateWindowEx failed with {Marshal.GetLastPInvokeError()}.");
        }

        ShowWindow(handle, ShowWithoutActivating);
        using Process process = Process.GetCurrentProcess();
        if (process.MainWindowHandle != handle)
        {
            DestroyWindow(handle);
            throw new InvalidOperationException("The test host already has another main window.");
        }

        return new NativeMainWindowScope(handle);
    }

    public void Dispose() => DestroyWindow(Handle);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern nint CreateWindowExW(int extendedStyle, string className, string windowName, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);

    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(nint window, int command);

    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint window);
}
