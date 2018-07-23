using System.Drawing;
using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGrid
{
    internal static class MenuUtil
    {
        private static readonly object _captionTag = new object();
        private static readonly CaptionCustomMenuRenderer _customMenuRenderer = new CaptionCustomMenuRenderer();
        private static Font _disabledFont;

        /// <summary>
        /// set the menu item disabled and remove mouse hover effect
        /// </summary>
        public static void SetAsCaptionMenuItem(ToolStripMenuItem menuItem, ToolStrip menu)
        {
            menu.Renderer = _customMenuRenderer;

            menuItem.Tag = _captionTag;
            menuItem.Enabled = false;

            if (_disabledFont == null)
            {
                _disabledFont = new Font(menuItem.Font, FontStyle.Italic);
            }

            menuItem.Font = _disabledFont;
        }

        /// <summary>
        /// no mouse over effect for disabled menu items, if the Tag is "caption"
        /// </summary>
        private sealed class CaptionCustomMenuRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                // Only render the background for non-caption menu items
                if (!ReferenceEquals(e.Item.Tag, _captionTag))
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }
        }
    }
}
