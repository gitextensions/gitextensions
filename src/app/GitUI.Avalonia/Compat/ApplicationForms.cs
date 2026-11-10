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

    /// <summary>
    /// Gets the application's window that is currently active, like WinForms <c>Form.ActiveForm</c>.
    /// On Windows this reads the native active window, which is already updated while the previously
    /// active window handles its deactivation.
    /// </summary>
    internal static Window? ActiveForm
    {
        get
        {
            IEnumerable<Window> windows = WindowsCloseRequests.GetOpenForms();
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                windows = desktop.Windows.Concat(windows).Distinct();
            }

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
    /// Gets the native window handle, like WinForms <c>Form.Handle</c>, or zero when the window has none.
    /// </summary>
    internal static nint GetHandle(TopLevel window)
        => _handleForTesting?.Invoke(window) ?? window.TryGetPlatformHandle()?.Handle ?? 0;

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
