using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace GitUI.BranchTreePanel
{
    internal static class ContextMenuExtensions
    {
        internal static RepoObjectsTree.NodeBase GetSelectedNode(this ContextMenuStrip menu)
            => (menu.SourceControl as TreeView)?.SelectedNode?.Tag as RepoObjectsTree.NodeBase;

        /// <summary>Inserts <paramref name="items"/> into the <paramref name="menu"/>; optionally <paramref name="before"/> or
        /// <paramref name="after"/> an existing item or at the start of the menu before other existing items if neither is specified.</summary>
        internal static void InsertItems(this ContextMenuStrip menu, IEnumerable<ToolStripItem> items,
            ToolStripItem? before = null, ToolStripItem? after = null)
        {
            Debug.Assert(!(after is not null && before is not null), $"Only {nameof(before)} or {nameof(after)} is allowed.");

            menu.SuspendLayout();
            int index;

            if (before is not null)
            {
                index = Math.Max(0, menu.Items.IndexOf(before) - 1);
                items.ForEach(item => menu.Items.Insert(++index, item));
            }
            else
            {
                index = after is null ? 0 : Math.Max(0, menu.Items.IndexOf(after) + 1);
                items.ForEach(item => menu.Items.Insert(index++, item));
            }

            menu.ResumeLayout();
        }

        /// <summary>Adds the <paramref name="item"/> to the <paramref name="menu"/>
        /// while making sure not to add it more than once and that it is the last item in the menu.
        /// This is useful for maintaining the order of items that are shared among multiple context menus.</summary>
        internal static void SetLastItem(this ContextMenuStrip menu, ToolStripItem item)
        {
            if (!menu.Items.Contains(item))
            {
                menu.Items.Add(item);
            }

            /* Sort item last. This works around other shared items being implicitly removed from menu in front
             * (by adding them to other context menus when they are opened) and re-added after the item. */
            if (menu.Items.IndexOf(item) != menu.Items.Count - 1)
            {
                menu.Items.Remove(item);
                menu.Items.Add(item);
            }
        }

        /// <summary>Toggles the <paramref name="item"/>'s <see cref="ToolStripItem.Visible"/>
        /// as well as <see cref="ToolStripItem.Enabled"/> properties depending on <paramref name="enabled"/>.
        /// Prefer this over only toggling the visibility of an item to enable determining whether the context menu will (once open)
        /// contain any visible items via <see cref="ToolStripItem.Enabled"/> even before the menu itself (as the visual parent)
        /// is visble and <see cref="ToolStripItem.Visible"/> of any item therefore false.</summary>
        internal static void Toggle(this ToolStripItem item, bool enabled)
            => item.Visible = item.Enabled = enabled;
    }
}
