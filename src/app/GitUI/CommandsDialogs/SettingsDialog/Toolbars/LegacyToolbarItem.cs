namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// What LegacyToolbarVisibility needs to know of an item on a toolbar of the main window: the name
// and group its old visibility setting was stored under, and whether it is a separator.
internal readonly record struct LegacyToolbarItem(string? Name, string? Group, bool IsSeparator);
