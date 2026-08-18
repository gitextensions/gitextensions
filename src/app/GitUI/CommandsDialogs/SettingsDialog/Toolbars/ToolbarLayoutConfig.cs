using System.Runtime.Serialization;

namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Complete configuration for all toolbars layout
[DataContract]
internal class ToolbarLayoutConfig
{
    // List of all toolbar items with their positions
    [DataMember]
    public List<ToolbarItemConfig> Items { get; set; } = new();

    // List of custom toolbars metadata
    [DataMember]
    public List<ToolbarCustomMetadata> CustomToolbars { get; set; } = new();

    // List of all toolbars visibility metadata (built-in and custom)
    [DataMember]
    public List<ToolbarBuiltInMetadata> ToolbarsVisibility { get; set; } = new();

    /// <summary>
    /// Writes layout and visibility metadata for a custom toolbar into both
    /// <see cref="ToolbarsVisibility"/> and <see cref="CustomToolbars"/> in one call,
    /// keeping the two lists always in sync. Prefer this over mutating the lists directly.
    /// </summary>
    /// <param name="name">Toolbar display name.</param>
    /// <param name="row">Row index in the toolbar panel (0-based).</param>
    /// <param name="orderInRow">Position within the row (0 = leftmost).</param>
    /// <param name="visible">Whether the toolbar is visible.</param>
    /// <param name="iconSize">Icon size in pixels; snapped to the nearest supported size.</param>
    /// <param name="index">
    /// Sort key ordering the custom toolbars, unique across <see cref="CustomToolbars"/>. When
    /// <see langword="null"/> the next free index is taken, which is one past the highest in use.
    /// </param>
    /// <param name="allIconsShowText">Whether all icons show their text label.</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or blank.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/>, <paramref name="orderInRow"/> or <paramref name="index"/> is negative or beyond the supported range.</exception>
    public void SetCustomToolbarMetadata(
        string name,
        int row,
        int orderInRow,
        bool visible,
        int iconSize,
        int? index = null,
        bool allIconsShowText = false)
    {
        // Reject a layout that could not be represented, so an invalid value is caught here rather
        // than surfacing as a corrupt setting on the next load.
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(row, ToolbarLayoutValidator.MaxRow);
        ArgumentOutOfRangeException.ThrowIfNegative(orderInRow);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(orderInRow, ToolbarLayoutValidator.MaxOrderInRow);

        if (index is not null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index.Value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index.Value, ToolbarLayoutValidator.MaxIndex);
        }

        // Unlike the positions above, an icon size is normalized rather than rejected: one read
        // from a live DPI-scaled ToolStrip is a legitimate in-between value.
        iconSize = ToolbarLayoutValidator.NormalizeIconSize(iconSize);

        // --- ToolbarsVisibility ---
        ToolbarBuiltInMetadata? visMeta = ToolbarsVisibility.FirstOrDefault(t => t.Name == name);
        if (visMeta is not null)
        {
            visMeta.Row = row;
            visMeta.OrderInRow = orderInRow;
            visMeta.Visible = visible;
            visMeta.IconSize = iconSize;
            visMeta.AllIconsShowText = allIconsShowText;
        }
        else
        {
            ToolbarsVisibility.Add(new ToolbarBuiltInMetadata
            {
                Name = name,
                Row = row,
                OrderInRow = orderInRow,
                Visible = visible,
                IconSize = iconSize,
                AllIconsShowText = allIconsShowText
            });
        }

        // --- CustomToolbars ---
        ToolbarCustomMetadata? customMeta = CustomToolbars.FirstOrDefault(c => c.Name == name);
        if (customMeta is not null)
        {
            customMeta.Row = row;
            customMeta.OrderInRow = orderInRow;
            customMeta.Visible = visible;
            customMeta.IconSize = iconSize;
            customMeta.AllIconsShowText = allIconsShowText;
        }
        else
        {
            CustomToolbars.Add(new ToolbarCustomMetadata
            {
                Name = name,

                // One past the highest index in use, rather than one past the count: the two agree
                // only while the indices form an unbroken run, and adding then removing a toolbar
                // is enough to break that - after which counting would hand out an index another
                // toolbar already holds.
                Index = index ?? NextFreeIndex(),
                Row = row,
                OrderInRow = orderInRow,
                Visible = visible,
                IconSize = iconSize,
                AllIconsShowText = allIconsShowText
            });
        }
    }

    // The three built-in toolbars occupy 0-2, so a custom one starts at 3.
    private int NextFreeIndex()
        => CustomToolbars.Count == 0
            ? 3
            : Math.Max(3, CustomToolbars.Max(c => c.Index) + 1);

    // Removes all metadata for a custom toolbar from both <see cref="ToolbarsVisibility"/>
    // and <see cref="CustomToolbars"/> in one call.
    public void RemoveCustomToolbarMetadata(string name)
    {
        ToolbarsVisibility.RemoveAll(t => t.Name == name);
        CustomToolbars.RemoveAll(c => c.Name == name);

        // Drop the items too: they are unreachable once their toolbar is gone, and leaving them
        // behind would grow the setting on every add/remove cycle.
        Items.RemoveAll(i => i.ToolbarName == name);
    }
}
