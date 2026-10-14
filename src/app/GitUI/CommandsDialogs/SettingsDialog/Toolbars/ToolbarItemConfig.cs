using System.Runtime.Serialization;

namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Configuration for a single toolbar item (button, menu, etc.)
[DataContract]
internal class ToolbarItemConfig
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
