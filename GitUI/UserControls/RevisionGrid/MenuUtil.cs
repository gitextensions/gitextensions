using System.Drawing;
using System.Windows.Forms;
using GitExtUtils.GitUI;

namespace GitUI.UserControls.RevisionGrid
{
    internal static class MenuUtil
    {
        private static readonly object _captionTag = new();
        private static readonly MenuItemBackgroundFilter _menuItemBackgroundFilter = new();
        private static Font? _disabledFont;

        /// <summary>
        /// set the menu item disabled and remove mouse hover effect.
        /// </summary>
        public static void SetAsCaptionMenuItem(ToolStripMenuItem menuItem, ToolStrip menu)
        {
            menu.AttachMenuItemBackgroundFilter(_menuItemBackgroundFilter);

            menuItem.Tag = _captionTag;
            menuItem.Enabled = false;

            _disabledFont ??= new Font(menuItem.Font, FontStyle.Italic);

            menuItem.Font = _disabledFont;
        }

        /// <summary>
        /// no mouse over effect for disabled menu items, if the Tag is "caption".
        /// TODO: in Windows 10 both ToolStripProfessionalRenderer and ToolStripSystemRenderer do
        /// not render background of disabled item on mouse over, so MenuItemBackgroundFilter is not needed.
        /// Someone needs to check if MenuItemBackgroundFilter is neither necessary in Windows 7.
        /// If so, remove it together with associated code
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
