namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// What the settings page knows of a toolbar when it saves the layout: its name, whether it is one
// of the user's own, and how it currently looks.
internal readonly record struct ToolbarState(string Name, bool IsCustom, bool Visible, bool AllIconsShowText);
