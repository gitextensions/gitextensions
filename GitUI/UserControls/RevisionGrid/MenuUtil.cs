using GitExtUtils.GitUI;

namespace GitUI.UserControls.RevisionGrid
{
    internal static class MenuUtil
    {
        private static readonly object _captionTag = new();
        private static readonly MenuItemBackgroundFilter _menuItemBackgroundFilter = new();
        private static Font? _disabledFont;

        /// <summary>
        /// set the menu item disabled.
        /// </summary>
        public static void SetAsCaptionMenuItem(ToolStripMenuItem menuItem)
        {
            menuItem.Tag = _captionTag;
            menuItem.Enabled = false;

            _disabledFont ??= new Font(menuItem.Font, FontStyle.Italic);

            menuItem.Font = _disabledFont;
        }

        /// <summary>
        /// set the menu item disabled and remove mouse hover effect.
        /// </summary>
        public static void SetAsCaptionMenuItem(ToolStripMenuItem menuItem, ToolStrip menu)
        {
            menu.AttachMenuItemBackgroundFilter(_menuItemBackgroundFilter);
            SetAsCaptionMenuItem(menuItem);
        }

        /// <summary>
        /// no mouse over effect for disabled menu items, if the Tag is "caption".
        /// </summary>
        private sealed class MenuItemBackgroundFilter : IMenuItemBackgroundFilter
        {
            public bool ShouldRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                // Only render the background for non-caption menu items
                return !ReferenceEquals(e.Item.Tag, _captionTag);
            }
        }
    }
}
