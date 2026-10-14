namespace GitUI.CommandsDialogs;

/// <summary>
/// Implemented by both <see cref="ToolStripPushButton"/> (the original on the Standard toolbar)
/// and <see cref="ToolStripPushButtonClone"/> (independent copies on custom toolbars).
/// Allows <see cref="FormBrowse"/> infrastructure to treat both uniformly without resolving
/// the original from a clone's Tag.
/// </summary>
internal interface IPushLabelItem
{
    string? LabelText { get; set; }

    bool ShowLabel { get; set; }
}
