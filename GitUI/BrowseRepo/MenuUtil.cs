using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GitUI.BrowseRepo
{
    class MenuUtil
    {
        public static void SetCustomMenuRenderer(MenuStripEx menuStrip)
        {
            menuStrip.Renderer = new CustomMenuRenderer();
        }

        public static void SetAsSubsectionMenuItem(ToolStripMenuItem menuItem)
        {
            menuItem.Enabled = false;
            menuItem.Font = new Font(menuItem.Font, FontStyle.Italic); // TODO: can be optimized later by caching this font
        }
    }
}
