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
    /// <param name="iconSize">Icon size in pixels.</param>
    /// <param name="index">
    /// Toolbar index (3+ for custom toolbars). When <see langword="null"/> the next
    /// available index is computed automatically from the current <see cref="CustomToolbars"/> count.
    /// </param>
    /// <param name="allIconsShowText">Whether all icons show their text label.</param>
    public void SetCustomToolbarMetadata(
        string name,
        int row,
        int orderInRow,
        bool visible,
        int iconSize,
        int? index = null,
        bool allIconsShowText = false)
    {
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
                Index = index ?? (3 + CustomToolbars.Count),
                Row = row,
                OrderInRow = orderInRow,
                Visible = visible,
                IconSize = iconSize,
                AllIconsShowText = allIconsShowText
            });
        }
    }

    // Removes all metadata for a custom toolbar from both <see cref="ToolbarsVisibility"/>
    // and <see cref="CustomToolbars"/> in one call.
    public void RemoveCustomToolbarMetadata(string name)
    {
        ToolbarsVisibility.RemoveAll(t => t.Name == name);
        CustomToolbars.RemoveAll(c => c.Name == name);
    }
}
