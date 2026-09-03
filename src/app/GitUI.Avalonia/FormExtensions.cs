using Avalonia.Controls;

namespace GitUI;

public static class FormExtensions
{
    /// <summary>
    ///  Brings the window to the front and activates it. Needed on drag drop and on button click from desktop launchers.
    /// </summary>
    public static void ForceActivate(this Window? form)
    {
        if (form is null)
        {
            return;
        }

        // Avalonia exposes no cross-platform BringToFront API. Briefly toggling Topmost
        // requests the same z-order transition before activation on supported desktops.
        form.Topmost = true;
        form.Topmost = false;
        form.Activate();
    }
}
