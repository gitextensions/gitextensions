using Avalonia.Controls;

namespace GitUI.CommandsDialogs;

/// <summary>
/// Tab control used by the repository browser whose content meets the surrounding pane borders.
/// </summary>
public sealed class FullBleedTabControl : TabControl
{
    // A templated-control subclass otherwise looks up a theme keyed by its own type and
    // renders neither the header strip nor selected content.
    protected override Type StyleKeyOverride => typeof(TabControl);
}
