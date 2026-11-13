using Avalonia.Controls.ApplicationLifetimes;
using GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
///  Installs the Avalonia implementations of the shim services
///  (see <see cref="ShimHost"/>).
/// </summary>
public static class ShimServices
{
    public static void Install(IClassicDesktopStyleApplicationLifetime desktop)
        => Install(desktop, new XdgDesktopPortal());

    // parity-scaffolding: substitute only external OS detection and portal I/O in headless tests.
    internal static void Install(IClassicDesktopStyleApplicationLifetime desktop, IXdgDesktopPortal portal, Func<bool>? isLinux = null)
    {
        ShimHost.MessageBoxHost = new AvaloniaMessageBoxHost(desktop);
        ShimHost.TaskDialogHost = new AvaloniaTaskDialogHost();
        ShimHost.Clipboard = new AvaloniaClipboard(desktop);
        ShimHost.FolderPicker = new AvaloniaFolderPicker(desktop);
        ShimHost.OsShell = new AvaloniaOsShell(desktop, portal, isLinux ?? OperatingSystem.IsLinux);
        ShimHost.TextMeasurer = new AvaloniaTextMeasurer();
        ShimHost.IconExtractor = new AssociatedFileIconExtractor();
    }
}
