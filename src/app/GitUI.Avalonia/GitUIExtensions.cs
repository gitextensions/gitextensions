using Avalonia.Controls;
using Avalonia.Media;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid;

namespace GitUI;

public static partial class GitUIExtensions
{
    public static void Mask(this Control control)
    {
        Panel? host = FindMaskHost(control);
        if (host is not null && FindMaskPanel(control) is null)
        {
            LoadingControl panel = new()
            {
                IsAnimating = true,
                ZIndex = int.MaxValue,
                Background = new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(
                    AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, System.Drawing.KnownColor.AppWorkspace))),
            };
            host.Children.Add(panel);
        }
    }

    public static void UnMask(this Control control)
    {
        Panel? host = FindMaskHost(control);
        LoadingControl? panel = FindMaskPanel(control);
        if (host is not null && panel is not null)
        {
            panel.IsAnimating = false;
            host.Children.Remove(panel);
        }
    }

    private static LoadingControl? FindMaskPanel(Control control)
        => FindMaskHost(control)?.Children.OfType<LoadingControl>().FirstOrDefault();

    // Avalonia exposes a window's child tree through Content instead of Control.Controls.
    private static Panel? FindMaskHost(Control control)
        => control as Panel ?? (control as ContentControl)?.Content as Panel;
}
