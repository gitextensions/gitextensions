namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// One place on a toolbar once the old visibility settings are applied: either an item the toolbar
// already had, by its index, or a menu item the old settings showed as a button of its own.
internal readonly record struct LegacyToolbarSlot(int? SourceIndex, string? InsertedItemName);
