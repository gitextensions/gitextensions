using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using GitExtensions.Extensibility;

namespace GitUI.LeftPanel.ContextMenu;

internal static class ContextMenuExtensions
{
    /// <summary>Inserts <paramref name="items"/> into the <paramref name="menu"/>; optionally <paramref name="before"/> or
    /// <paramref name="after"/> an existing item or at the start of the menu before other existing items if neither is specified.</summary>
    internal static void InsertItems(this Avalonia.Controls.ContextMenu menu, IEnumerable<Control> items,
        Control? before = null, Control? after = null)
    {
        DebugHelpers.Assert(after is null || before is null, $"Only {nameof(before)} or {nameof(after)} is allowed, not both.");

        int index;

        if (before is not null)
        {
            index = Math.Max(0, menu.Items.IndexOf(before) - 1);

            foreach (Control item in items)
            {
                menu.Items.Insert(++index, item);
            }
        }
        else
        {
            index = after is null ? 0 : Math.Max(0, menu.Items.IndexOf(after) + 1);

            foreach (Control item in items)
            {
                menu.Items.Insert(index++, item);
            }
        }
    }

    /// <summary>Toggles the <paramref name="item"/>'s <see cref="Visual.IsVisible"/>
    /// as well as <see cref="InputElement.IsEnabled"/> properties depending on <paramref name="enabled"/>.
    /// This may be a useful shorthand for scenarios in which you want to make sure that items are only enabled if they're also visible;
    /// e.g. to enable determining whether the context menu will (once open) contain any visible items via <see cref="InputElement.IsEnabled"/>
    /// even before the menu itself is visible.</summary>
    internal static void Enable(this Control item, bool enabled)
        => item.IsVisible = item.IsEnabled = enabled;

    /// <summary>Toggles <see cref="Separator"/>s in between <paramref name="contextMenu"/>'s items
    /// preventing separators from preceding or trailing the list or being displayed without any items in between them.
    /// Relies on the items' <see cref="Visual.IsVisible"/> property, which Avalonia exposes before
    /// the popup is opened, so it can be used while preparing the native context menu.</summary>
    internal static void ToggleSeparators(this Avalonia.Controls.ContextMenu contextMenu)
    {
        Control[] items = [.. contextMenu.Items.OfType<Control>()];

        // toggle all separators (but the last) looking behind for visible items other than separators
        Control? lastPrecedingVisibleItem = null;

        foreach (Control item in items)
        {
            if (item is Separator)
            {
                // show separator if last preceding visible item is not also a separator to avoid stacking them
                item.Enable(lastPrecedingVisibleItem is not null && lastPrecedingVisibleItem is not Separator);
            }

            if (item.IsVisible)
            {
                // remember this as the last visible item before continuing
                lastPrecedingVisibleItem = item;
            }
        }

        // hide the last visible separator that above look-behind loop may have left over
        Control? lastVisible = items.LastOrDefault(i => i.IsVisible);

        if (lastVisible is Separator)
        {
            lastVisible.Enable(false);
        }
    }
}
