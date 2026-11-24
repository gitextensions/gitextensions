using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace GitUI.Compat;

// Framework constraint: WinForms Form.Handle and Form.ActiveForm have no direct Avalonia equivalents.
internal static class ApplicationForms
{
    // parity-scaffolding: substitutes only native window handles in a headless host.
    private static Func<TopLevel, nint>? _handleForTesting;

    // parity-scaffolding: substitutes only the native active-window query in a headless host.
    private static Func<nint>? _activeWindowForTesting;

    private static readonly ConditionalWeakTable<TopLevel, StrongBox<nint>> StandInHandles = new();
    private static long _lastStandInHandle;

    /// <summary>
    /// Gets the application's window that is currently active, like WinForms <c>Form.ActiveForm</c>.
    /// On Windows this reads the native active window, which is already updated while the previously
    /// active window handles its deactivation.
    /// </summary>
    internal static Window? ActiveForm
    {
        get
        {
            IEnumerable<Window> windows = GetWindows();
            if (_activeWindowForTesting is null && !OperatingSystem.IsWindows())
            {
                return windows.FirstOrDefault(window => window.IsActive);
            }

            nint activeWindow = _activeWindowForTesting?.Invoke() ?? GetActiveWindow();
            return activeWindow == 0
                ? null
                : windows.FirstOrDefault(window => GetHandle(window) == activeWindow);
        }
    }

    /// <summary>
    /// Gets the first shown window of the application, which stands for the WinForms main window.
    /// </summary>
    internal static Window? FindShownWindow() => GetWindows().FirstOrDefault(window => window.IsVisible);

    /// <summary>
    /// Gets the application's window with the given native handle, or <see langword="null"/>.
    /// </summary>
    internal static Window? FindWindow(nint windowHandle)
        => windowHandle == 0 ? null : GetWindows().FirstOrDefault(window => GetHandle(window) == windowHandle);

    /// <summary>
    /// Gets the native window handle, like WinForms <c>Form.Handle</c>. A WinForms form always has a
    /// handle, so where the platform gives windows none (Wayland, the headless host) each window gets a
    /// stable negative stand-in that no native handle uses.
    /// </summary>
    internal static nint GetHandle(TopLevel window)
    {
        if (_handleForTesting is not null)
        {
            return _handleForTesting(window);
        }

        return window.TryGetPlatformHandle() is { Handle: not 0 } handle
            ? handle.Handle
            : StandInHandles.GetValue(window, static _ => new StrongBox<nint>((nint)Interlocked.Decrement(ref _lastStandInHandle))).Value;
    }

    private static IEnumerable<Window> GetWindows()
    {
        IEnumerable<Window> windows = WindowsCloseRequests.GetOpenForms();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            windows = desktop.Windows.Concat(windows).Distinct();
        }

        return windows;
    }

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern nint GetActiveWindow();

    // parity-scaffolding: replaces only native window handles and the native active-window query.
    internal readonly struct TestAccessor
    {
        internal static Func<TopLevel, nint>? Handle
        {
            get => _handleForTesting;
            set => _handleForTesting = value;
        }

        internal static Func<nint>? ActiveWindow
        {
            get => _activeWindowForTesting;
            set => _activeWindowForTesting = value;
        }
    }
}
