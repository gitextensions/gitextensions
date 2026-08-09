using GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
///  Ensures Linux file pickers use the XDG desktop portal instead of Avalonia's managed fallback.
/// </summary>
public static class PortalPickerGuard
{
    private const string ErrorCaption = "Git Extensions";

    public static bool IsAvailable()
        => DispatcherPump.Wait(IsAvailableAsync);

    public static Task<bool> IsAvailableAsync()
        => IsAvailableAsync(new XdgDesktopPortal(), OperatingSystem.IsLinux, ReportUnavailable);

    internal static async Task<bool> IsAvailableAsync(
        IXdgDesktopPortal portal,
        Func<bool> isLinux,
        Action reportUnavailable)
    {
        if (!isLinux() || await portal.IsInterfaceAvailableAsync(XdgDesktopPortal.FileChooserInterface))
        {
            return true;
        }

        reportUnavailable();
        return false;
    }

    private static void ReportUnavailable()
    {
        _ = ShimHost.MessageBoxHost.Show(
            owner: null,
            "A file dialog could not be opened because no XDG desktop portal backend is available.",
            ErrorCaption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1);
    }
}
