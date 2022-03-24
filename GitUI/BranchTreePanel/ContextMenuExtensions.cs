using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace GitUI.BranchTreePanel
{
    internal static class ContextMenuExtensions
    {
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

        /// <summary>Toggles the <paramref name="item"/>'s <see cref="ToolStripItem.Visible"/>
        /// as well as <see cref="ToolStripItem.Enabled"/> properties depending on <paramref name="enabled"/>.
        /// Prefer this over only toggling the visibility of an item to enable determining whether the context menu will (once open)
        /// contain any visible items via <see cref="ToolStripItem.Enabled"/> even before the menu itself (as the visual parent)
        /// is visble and <see cref="ToolStripItem.Visible"/> of any item therefore false.</summary>
        internal static void Enable(this ToolStripItem item, bool enabled)
            => item.Visible = item.Enabled = enabled;

        /// <summary>Toggles <see cref="ToolStripSeparator"/>s in between <paramref name="contextMenu"/>'s items
        /// preventing separators from preceding or trailing the list or being displayed without any items in between them.
        /// Relies on the items' <see cref="ToolStripItem.Visible"/> and <see cref="ToolStripItem.Enabled"/>
        /// being toggled using <see cref="Enable(ToolStripItem, bool)"/>.</summary>
        internal static void ToggleSeparators(this ContextMenuStrip contextMenu)
        {
            var items = contextMenu.Items.Cast<ToolStripItem>().ToArray();
            ToolStripItem precedingEnabledItem = null;

            // toggle all separators looking behind for enabled items other than separators
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripSeparator)
                {
                    item.Enable(precedingEnabledItem != null && precedingEnabledItem is not ToolStripSeparator);
                }

                if (item.Enabled)
                {
                    precedingEnabledItem = item;
                }
            }

            // hide the last Enabled separator that may remain
            var lastEnabled = items.LastOrDefault(i => i.Enabled);

            if (lastEnabled != null && lastEnabled is ToolStripSeparator)
            {
                lastEnabled.Enable(false);
            }
        }
    }
}
