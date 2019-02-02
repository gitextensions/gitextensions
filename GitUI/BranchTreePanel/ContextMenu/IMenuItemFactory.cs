using System;
using System.Drawing;
using System.Windows.Forms;
using GitUI.BranchTreePanel.Interfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
{
    internal interface IMenuItemFactory
    {
        /// <summary>
        /// Creates a menu item control
        /// </summary>
        /// <typeparam name="TMenuItem">The menu item control type</typeparam>
        /// <typeparam name="TNode">The node type (branch, tag etc)</typeparam>
        /// <param name="onClick">The action to execute on click</param>
        /// <param name="text">The menu item text</param>
        /// <param name="toolTip">Menu item tooltip</param>
        /// <param name="icon">The image to show on the menu item</param>
        /// <returns>A new instance of the menu item control</returns>
        TMenuItem CreateMenuItem<TMenuItem, TNode>(Action<TNode> onClick, TranslationString text, TranslationString toolTip, Bitmap icon = null)
            where TMenuItem : ToolStripItem, new()
            where TNode : class, INode;
    }
}
