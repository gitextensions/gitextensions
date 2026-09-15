namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// One toolbar as the layout window's grid sees it: where it sits, and how it is drawn there.
// Deliberately free of any WinForms type, so the arithmetic that moves it around
// (see ToolbarGridArrangement) can be exercised without a window.
internal sealed class ToolbarLayoutItem
{
    // Identifies the toolbar in the saved layout.
    public string Name { get; set; } = string.Empty;

    // What the grid shows for it, which for now is its name.
    public string DisplayName { get; set; } = string.Empty;

    // Row of the toolbar panel it sits on, 0 being the topmost.
    public int Row { get; set; }

    // Position within that row, 0 being the leftmost.
    public int OrderInRow { get; set; }

    // Whether this is one of the toolbars that always exist, which cannot be deleted and keep a
    // fixed order among themselves.
    public bool IsBuiltIn { get; set; }

    public bool IsVisible { get; set; } = true;

    public int IconSize { get; set; } = 16;
}
