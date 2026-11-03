using Avalonia.Controls;

namespace GitUI.CommandsDialogs;

/// <summary>
/// Subclass of <see cref="TabControl"/> whose tab items have no gaps with the border.
/// </summary>
public sealed class FullBleedTabControl : TabControl
{
    // A templated-control subclass otherwise looks up a theme keyed by its own type and
    // renders neither the header strip nor selected content.
    protected override Type StyleKeyOverride => typeof(TabControl);
}
