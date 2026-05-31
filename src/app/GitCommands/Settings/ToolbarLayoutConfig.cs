using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GitCommands.Settings;

// Configuration for a single toolbar item (button, menu, etc.)
[DataContract]
public class ToolbarItemConfig
{
    // The name of the ToolStripItem (e.g., "toolStripButtonCommit")
    [DataMember]
    public string ItemName { get; set; } = string.Empty;

    // Name of the toolbar this item belongs to (e.g., "Standard", "Filters", "Scripts", "Custom 01").
    // This is the authoritative key used to match items to their toolbar at load time.
    [DataMember]
    public string ToolbarName { get; set; } = string.Empty;

    // Position/order of the item within its toolbar (0-based index)
    [DataMember]
    public int Order { get; set; }

    // Whether the icon text label is shown next to the icon for this item.
    [DataMember]
    public bool ShowText { get; set; }
}

// Metadata for a custom toolbar
[DataContract]
public class CustomToolbarMetadata
{
    // Name of the custom toolbar (e.g., "Custom 01", "Custom 02")
    [DataMember]
    public string Name { get; set; } = string.Empty;

    // Index of the toolbar (3+ for custom toolbars)
    [DataMember]
    public int Index { get; set; }

    // Whether the toolbar is visible
    [DataMember]
    public bool Visible { get; set; } = true;

    // Row where the toolbar is positioned (0-based)
    [DataMember]
    public int Row { get; set; }

    // Order/position within the row (0 = leftmost)
    [DataMember]
    public int OrderInRow { get; set; }

    // Icon size in pixels for this toolbar (16, 24, 32, 40, 48, 56, 64, 72, 80, 88, 96)
    [DataMember]
    public int IconSize { get; set; } = 16;

    // Whether all icons in this toolbar show their text label ("For all icons" mode).
    [DataMember]
    public bool AllIconsShowText { get; set; }
}

// Metadata for a built-in or custom toolbar
[DataContract]
public class ToolbarMetadata
{
    // Name of the toolbar (e.g., "Standard", "Filters", "Scripts", "Custom 01")
    [DataMember]
    public string Name { get; set; } = string.Empty;

    // Whether the toolbar is visible
    [DataMember]
    public bool Visible { get; set; } = true;

    // Row where the toolbar is positioned (0-based)
    [DataMember]
    public int Row { get; set; }

    // Order/position within the row (0 = leftmost)
    [DataMember]
    public int OrderInRow { get; set; }

    // Icon size in pixels for this toolbar (16, 24, 32, 40, 48, 56, 64, 72, 80, 88, 96)
    [DataMember]
    public int IconSize { get; set; } = 16;

    // Whether all icons in this toolbar show their text label ("For all icons" mode).
    [DataMember]
    public bool AllIconsShowText { get; set; }
}

// Complete configuration for all toolbars layout
[DataContract]
public class ToolbarLayoutConfig
{
    // List of all toolbar items with their positions
    [DataMember]
    public List<ToolbarItemConfig> Items { get; set; } = new();

    // List of custom toolbars metadata
    [DataMember]
    public List<CustomToolbarMetadata> CustomToolbars { get; set; } = new();

    // List of all toolbars visibility metadata (built-in and custom)
    [DataMember]
    public List<ToolbarMetadata> ToolbarsVisibility { get; set; } = new();

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
        ToolbarMetadata? visMeta = ToolbarsVisibility.FirstOrDefault(t => t.Name == name);
        if (visMeta != null)
        {
            visMeta.Row = row;
            visMeta.OrderInRow = orderInRow;
            visMeta.Visible = visible;
            visMeta.IconSize = iconSize;
            visMeta.AllIconsShowText = allIconsShowText;
        }
        else
        {
            ToolbarsVisibility.Add(new ToolbarMetadata
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
        CustomToolbarMetadata? customMeta = CustomToolbars.FirstOrDefault(c => c.Name == name);
        if (customMeta != null)
        {
            customMeta.Row = row;
            customMeta.OrderInRow = orderInRow;
            customMeta.Visible = visible;
            customMeta.IconSize = iconSize;
            customMeta.AllIconsShowText = allIconsShowText;
        }
        else
        {
            CustomToolbars.Add(new CustomToolbarMetadata
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
