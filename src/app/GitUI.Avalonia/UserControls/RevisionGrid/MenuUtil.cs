using Avalonia.Controls;

namespace GitUI.UserControls.RevisionGrid;

internal static class MenuUtil
{
    private static readonly object _captionTag = new();

    /// <summary>
    /// set the menu item disabled.
    /// </summary>
    public static void SetAsCaptionMenuItem(MenuItem menuItem)
    {
        menuItem.Tag = _captionTag;
        menuItem.IsEnabled = false;
        menuItem.Focusable = false;
        menuItem.IsHitTestVisible = false;
        menuItem.Classes.Add("gitextensions-menu-caption");
    }

    /// <summary>
    /// set the menu item disabled and remove mouse hover effect.
    /// </summary>
    public static void SetAsCaptionMenuItem(MenuItem menuItem, ItemsControl menu)
    {
        // Avalonia's caption class disables hit testing, so no renderer-level background filter is required.
        SetAsCaptionMenuItem(menuItem);
    }
}
