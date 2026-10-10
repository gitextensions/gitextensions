using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Loads and saves the strongly-typed toolbar layout configuration, backed by the raw XML
// stored in AppSettings.ToolbarLayoutXml. Toolbar customization is only meaningful to GitUI,
// so the typed model and its (de)serialization live here rather than in GitCommands.
internal static class ToolbarLayoutStore
{
    // Never returns null: an absent, unreadable or invalid setting yields an empty configuration.
    public static ToolbarLayoutConfig Load()
    {
        ToolbarLayoutConfig? config = ToolbarXmlSerializer.Deserialize<ToolbarLayoutConfig>(AppSettings.ToolbarLayoutXml);

        // The single validation boundary for the layout: past this point every caller works with
        // data whose invariants have been checked, so no other code needs to re-check them.
        return ToolbarLayoutValidator.TryNormalize(config) ? config : new ToolbarLayoutConfig();
    }

    public static void Save(ToolbarLayoutConfig config)
    {
        AppSettings.ToolbarLayoutXml = ToolbarXmlSerializer.Serialize(config);
    }
}
