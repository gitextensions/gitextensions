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
        // No mouse over effect for disabled menu items whose Tag is "caption".
        // Avalonia renders the background only for non-caption menu items because the caption class disables hit testing.
        SetAsCaptionMenuItem(menuItem);
    }
}
