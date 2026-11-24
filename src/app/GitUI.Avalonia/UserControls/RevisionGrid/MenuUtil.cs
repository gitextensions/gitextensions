using Avalonia.Controls;
using GitUI.Compat.WinFormsControls;

namespace GitUI.UserControls.RevisionGrid;

internal static class MenuUtil
{
    private static readonly object _captionTag = new();

    /// <summary>
    /// set the menu item disabled.
    /// </summary>
    public static void SetAsCaptionMenuItem(ToolStripMenuItem menuItem)
    {
        menuItem.Tag = _captionTag;
        menuItem.IsEnabled = false;
        menuItem.Focusable = false;

        // no mouse over effect for disabled menu items, if the Tag is "caption".
        // Only render the background for non-caption menu items
        menuItem.IsHitTestVisible = false;
        menuItem.Classes.Add("gitextensions-menu-caption");
    }

    /// <summary>
    /// set the menu item disabled and remove mouse hover effect.
    /// </summary>
    public static void SetAsCaptionMenuItem(ToolStripMenuItem menuItem, ItemsControl menu)
    {
        // Avalonia applies the caption style to the item without a parent renderer.
        SetAsCaptionMenuItem(menuItem);
    }
}
