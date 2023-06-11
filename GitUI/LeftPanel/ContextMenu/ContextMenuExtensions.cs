using System.Diagnostics;

namespace GitUI.LeftPanel.ContextMenu
{
    internal static class ContextMenuExtensions
    {
        /// <summary>Inserts <paramref name="items"/> into the <paramref name="menu"/>; optionally <paramref name="before"/> or
        /// <paramref name="after"/> an existing item or at the start of the menu before other existing items if neither is specified.</summary>
        internal static void InsertItems(this ContextMenuStrip menu, IEnumerable<ToolStripItem> items,
            ToolStripItem? before = null, ToolStripItem? after = null)
        {
            Debug.Assert(after is null || before is null, $"Only {nameof(before)} or {nameof(after)} is allowed, not both.");

            menu.SuspendLayout();
            int index;

            if (before is not null)
            {
                index = Math.Max(0, menu.Items.IndexOf(before) - 1);

                foreach (ToolStripItem item in items)
                {
                    menu.Items.Insert(++index, item);
                }
            }
            else
            {
                index = after is null ? 0 : Math.Max(0, menu.Items.IndexOf(after) + 1);

                foreach (ToolStripItem item in items)
                {
                    menu.Items.Insert(index++, item);
                }
            }

            menu.ResumeLayout();
        }

        /// <summary>Toggles the <paramref name="item"/>'s <see cref="ToolStripItem.Visible"/>
        /// as well as <see cref="ToolStripItem.Enabled"/> properties depending on <paramref name="enabled"/>.
        /// This may be a useful shorthand for scenarios in which you want to make sure that items are only enabled if they're also visible;
        /// e.g. to enable determining whether the context menu will (once open) contain any visible items via <see cref="ToolStripItem.Enabled"/>
        /// even before the menu itself (as the visual parent) is visble and <see cref="ToolStripItem.Visible"/> of any item therefore returns false.</summary>
        internal static void Enable(this ToolStripItem item, bool enabled)
            => item.Visible = item.Enabled = enabled;

        /// <summary>Toggles <see cref="ToolStripSeparator"/>s in between <paramref name="contextMenu"/>'s items
        /// preventing separators from preceding or trailing the list or being displayed without any items in between them.
        /// Relies on the items' <see cref="ToolStripItem.Visible"/> property and therefore only works
        /// if the <paramref name="contextMenu"/> as the visual parent is visible itself.
        /// So you'll probably want to use this on its <see cref="ToolStripDropDown.Opened"/> event
        /// or after toggling the visibility of any of its items while it is open.</summary>
        internal static void ToggleSeparators(this ContextMenuStrip contextMenu)
        {
            contextMenu.SuspendLayout();
            var items = contextMenu.Items.Cast<ToolStripItem>().ToArray();

            // toggle all separators (but the last) looking behind for visible items other than separators
            ToolStripItem lastPrecedingVisibleItem = null;

            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripSeparator)
                {
                    // show separator if last preceding visible item is not also a separator to avoid stacking them
                    item.Enable(lastPrecedingVisibleItem != null && lastPrecedingVisibleItem is not ToolStripSeparator);
                }

                if (item.Visible)
                {
                    // remember this as the last visible item before continuing
                    lastPrecedingVisibleItem = item;
                }
            }

            // hide the last visible separator that above look-behind loop may have left over
            var lastVisible = items.LastOrDefault(i => i.Visible);

            if (lastVisible is ToolStripSeparator)
            {
                lastVisible.Enable(false);
            }

            contextMenu.ResumeLayout();
        }
    }
}
