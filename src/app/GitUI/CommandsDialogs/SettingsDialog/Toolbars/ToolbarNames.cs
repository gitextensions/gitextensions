using System.Collections.Frozen;

namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Names of the toolbars that always exist, as they appear in the saved layout. Custom toolbars
// are named by the user instead and are listed in ToolbarLayoutConfig.CustomToolbars.
internal static class ToolbarNames
{
    public const string Standard = "Standard";
    public const string Filters = "Filters";
    public const string Scripts = "Scripts";

    // A built-in toolbar exists whether or not the saved layout mentions it, so items may
    // reference one of these names even when the visibility list has never been written.
    public static readonly FrozenSet<string> BuiltIn = FrozenSet.ToFrozenSet([Standard, Filters, Scripts], StringComparer.OrdinalIgnoreCase);
}
