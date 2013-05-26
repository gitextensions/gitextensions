using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    class FormBrowseMenus
    {
        MenuStrip _menuStrip;
        ToolStripMenuItem _navigateToolStripMenuItem;
        ToolStripMenuItem _viewToolStripMenuItem;

        public FormBrowseMenus(MenuStrip menuStrip)
        {
            _menuStrip = menuStrip;
        }

        /// <summary>
        /// inserts 
        /// - Navigate (after Repository)
        /// - View
        /// </summary>
        public void InsertAdditionalMainMenuItems(ToolStripMenuItem insertAfterMenuItem)
        {
            RemoveAdditionalMainMenuItems();

            _navigateToolStripMenuItem = new ToolStripMenuItem();
            // navigateToolStripMenuItem.DropDownItems
            _navigateToolStripMenuItem.Name = "navigateToolStripMenuItem";
            // navigateToolStripMenuItem.Size
            _navigateToolStripMenuItem.Text = "Navigate";
            _menuStrip.Items.Insert(_menuStrip.Items.IndexOf(insertAfterMenuItem) + 1, _navigateToolStripMenuItem);

            _viewToolStripMenuItem = new ToolStripMenuItem();
            // navigateToolStripMenuItem.DropDownItems
            _viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            _viewToolStripMenuItem.Text = "View";
            _menuStrip.Items.Insert(_menuStrip.Items.IndexOf(_navigateToolStripMenuItem) + 1, _viewToolStripMenuItem);
        }

        public void RemoveAdditionalMainMenuItems()
        {
            _menuStrip.Items.Remove(_navigateToolStripMenuItem);
            _menuStrip.Items.Remove(_viewToolStripMenuItem);
        }
    }
}
