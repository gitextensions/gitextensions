using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Loads and saves the strongly-typed toolbar layout configuration, backed by the raw XML
// stored in AppSettings.ToolbarLayoutXml. Toolbar customization is only meaningful to GitUI,
// so the typed model and its (de)serialization live here rather than in GitCommands.
internal static class ToolbarLayoutStore
{
    // Never returns null: an absent or unreadable setting yields an empty configuration.
    public static ToolbarLayoutConfig Load()
    {
        return ToolbarXmlSerializer.Deserialize<ToolbarLayoutConfig>(AppSettings.ToolbarLayoutXml) ?? new ToolbarLayoutConfig();
    }

    public static void Save(ToolbarLayoutConfig config)
    {
        AppSettings.ToolbarLayoutXml = ToolbarXmlSerializer.Serialize(config);
    }
}
