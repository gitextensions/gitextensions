namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Where each toolbar sits in the layout window's grid, and what dragging one around does to the
// others. This is index arithmetic - shift the rows below an insertion point, close the gap the
// dragged toolbar left behind, renumber what is left - and it is what the saved layout is built
// from, so getting it wrong moves toolbars the user did not touch.
//
// It lives apart from the window because none of it needs one: a row is an integer, a position
// within a row is an integer, and the toolbars are a list of ToolbarLayoutItem. The window keeps
// the parts that do need one - hit-testing a drop point, drawing the grid.
//
// Every operation leaves the list packed: rows are numbered from 0 with no gaps, and so are the
// positions within each row. Callers can therefore hand any of these straight to the next one.
internal static class ToolbarGridArrangement
{
    // Order the built-in toolbars are laid out in when nothing says otherwise. Custom toolbars all
    // share the last value, so a stable sort leaves them in the order they were given.
    public static int GetDefaultOrder(string toolbarName)
        => toolbarName switch
        {
            ToolbarNames.Standard => 0,
            ToolbarNames.Filters => 1,
            ToolbarNames.Scripts => 2,
            _ => 99
        };

    /// <summary>
    /// Orders <paramref name="items"/> the way the grid reads them: top row first, and left to
    /// right within a row.
    /// </summary>
    public static void SortByPosition(List<ToolbarLayoutItem> items)
        => items.Sort((a, b) =>
        {
            int rowCompare = a.Row.CompareTo(b.Row);
            return rowCompare != 0 ? rowCompare : a.OrderInRow.CompareTo(b.OrderInRow);
        });

    /// <summary>
    /// Puts the built-in toolbars back on the first row in their usual order, and gives every
    /// custom toolbar a row of its own below, hidden.
    /// </summary>
    /// <remarks>
    /// A custom toolbar is not put back on the first row because the toolbars would then be wider
    /// than the panel, which makes WinForms wrap them and leaves a gap at the start of the row.
    /// </remarks>
    public static void ResetToDefault(List<ToolbarLayoutItem> items)
    {
        int builtInOrder = 0;
        int nextCustomRow = 1;

        foreach (ToolbarLayoutItem item in items.OrderBy(i => GetDefaultOrder(i.Name)))
        {
            if (item.IsBuiltIn)
            {
                item.Row = 0;
                item.OrderInRow = builtInOrder++;
                item.IsVisible = true;
            }
            else
            {
                item.Row = nextCustomRow++;
                item.OrderInRow = 0;
                item.IsVisible = false;
            }
        }

        SortByPosition(items);
    }

    /// <summary>
    /// Drops <paramref name="item"/> into an existing <paramref name="targetRow"/>, at
    /// <paramref name="dropIndex"/> among the toolbars already there.
    /// </summary>
    public static void MoveToRow(List<ToolbarLayoutItem> items, ToolbarLayoutItem item, int targetRow, int dropIndex)
    {
        int sourceRow = item.Row;
        item.Row = targetRow;

        List<ToolbarLayoutItem> itemsInTargetRow = ItemsInRow(items, targetRow, except: item);
        itemsInTargetRow.Insert(Math.Clamp(dropIndex, 0, itemsInTargetRow.Count), item);
        Renumber(itemsInTargetRow);

        if (sourceRow != targetRow)
        {
            Renumber(ItemsInRow(items, sourceRow));
        }

        Compact(items);
    }

    /// <summary>
    /// Drops <paramref name="item"/> onto a row of its own, inserted at <paramref name="newRow"/>.
    /// Everything from that row down moves one row further.
    /// </summary>
    public static void MoveToNewRow(List<ToolbarLayoutItem> items, ToolbarLayoutItem item, int newRow)
    {
        int sourceRow = item.Row;

        foreach (ToolbarLayoutItem other in items)
        {
            if (other != item && other.Row >= newRow)
            {
                other.Row++;
            }
        }

        // The dragged toolbar's own source row may have been shifted by the loop above, so the row
        // to close up afterwards is not necessarily the one it started on.
        int shiftedSourceRow = sourceRow >= newRow ? sourceRow + 1 : sourceRow;

        item.Row = newRow;
        item.OrderInRow = 0;

        if (shiftedSourceRow != newRow)
        {
            Renumber(ItemsInRow(items, shiftedSourceRow, except: item));
        }

        Compact(items);
    }

    /// <summary>
    /// Closes up the grid: drops the rows nothing sits on, and renumbers rows and positions so both
    /// run from 0 without gaps.
    /// </summary>
    public static void Compact(List<ToolbarLayoutItem> items)
    {
        List<int> usedRows = items.Select(i => i.Row).Distinct().OrderBy(row => row).ToList();

        for (int newRow = 0; newRow < usedRows.Count; newRow++)
        {
            int oldRow = usedRows[newRow];
            if (oldRow == newRow)
            {
                continue;
            }

            foreach (ToolbarLayoutItem item in items.Where(i => i.Row == oldRow))
            {
                item.Row = newRow;
            }
        }

        foreach (IGrouping<int, ToolbarLayoutItem> row in items.GroupBy(i => i.Row))
        {
            Renumber(row.OrderBy(i => i.OrderInRow));
        }
    }

    private static List<ToolbarLayoutItem> ItemsInRow(List<ToolbarLayoutItem> items, int row, ToolbarLayoutItem? except = null)
        => items
            .Where(i => i.Row == row && i != except)
            .OrderBy(i => i.OrderInRow)
            .ToList();

    private static void Renumber(IEnumerable<ToolbarLayoutItem> itemsInRow)
    {
        int order = 0;
        foreach (ToolbarLayoutItem item in itemsInRow)
        {
            item.OrderInRow = order++;
        }
    }
}
