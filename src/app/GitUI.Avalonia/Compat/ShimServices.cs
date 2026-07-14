using Avalonia.Controls.ApplicationLifetimes;
using GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
///  Installs the Avalonia implementations of the shim services
///  (see <see cref="ShimHost"/> and AGENTS.md, "The shim contract").
/// </summary>
public static class ShimServices
{
    public static void Install(IClassicDesktopStyleApplicationLifetime desktop)
    {
        ShimHost.MessageBoxHost = new AvaloniaMessageBoxHost(desktop);
        ShimHost.Clipboard = new AvaloniaClipboard(desktop);
        ShimHost.FolderPicker = new AvaloniaFolderPicker(desktop);
        ShimHost.TextMeasurer = new AvaloniaTextMeasurer();
    }
}
