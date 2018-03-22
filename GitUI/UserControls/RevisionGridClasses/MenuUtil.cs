using System.Drawing;
using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGridClasses
{
    internal static class MenuUtil
    {
        private static readonly CaptionCustomMenuRenderer customMenuRenderer = new CaptionCustomMenuRenderer();
        private static Font disabledFont;

        /// <summary>
        /// set the menu item disabled and remove mouse hover effect
        /// </summary>
        public static void SetAsCaptionMenuItem(ToolStripMenuItem menuItem, ToolStrip menu)
        {
            menu.Renderer = customMenuRenderer;
            menuItem.Enabled = false;

            if (disabledFont == null)
            {
                disabledFont = new Font(menuItem.Font, FontStyle.Italic);
            }

            menuItem.Font = disabledFont;
        }
    }
}
