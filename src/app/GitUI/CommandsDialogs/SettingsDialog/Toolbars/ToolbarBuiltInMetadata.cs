using System.Runtime.Serialization;

namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Metadata for a built-in or custom toolbar
[DataContract]
internal class ToolbarBuiltInMetadata
{
    // Display name of the toolbar. Used as the lookup key between the layout lists;
    // not validated or restricted to a fixed set of values.
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

    // Icon size in pixels for this toolbar. Normalized to a supported size on load
    // (see ToolbarLayoutValidator.IconSizes).
    [DataMember]
    public int IconSize { get; set; } = 16;

    // Whether all icons in this toolbar show their text label ("For all icons" mode).
    [DataMember]
    public bool AllIconsShowText { get; set; }
}
