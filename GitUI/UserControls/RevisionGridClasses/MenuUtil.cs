using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GitUI.UserControls.RevisionGridClasses
{
    class MenuUtil
    {
        private static CaptionCustomMenuRenderer customMenuRenderer = new CaptionCustomMenuRenderer();
        private static Font disabledFont;

        /// <summary>
        /// set the menu item disabled and remove mouse hover effect
        /// </summary>
        /// <param name="menuItem"></param>
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
