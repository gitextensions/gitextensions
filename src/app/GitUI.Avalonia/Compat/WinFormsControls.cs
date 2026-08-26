using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace GitUI.Compat.WinFormsControls;

/// <summary>
/// Native Avalonia counterparts for WinForms controls whose source identity carries layout semantics.
/// </summary>
/// <remarks>
/// These types keep Designer field signatures recognizable without emulating WinForms. Each control
/// uses the corresponding Avalonia control's layout, styling, input, and accessibility behavior.
/// </remarks>
public class Label : TextBlock
{
    protected override Type StyleKeyOverride => typeof(TextBlock);
}

/// <summary>
/// Preserves the source group-box identity while using Avalonia headered-content rendering.
/// </summary>
public class GroupBox : HeaderedContentControl
{
    protected override Type StyleKeyOverride => typeof(HeaderedContentControl);
}

/// <summary>
/// Preserves the source list-column identity while using an Avalonia content presenter.
/// </summary>
public class ColumnHeader : ContentControl
{
    internal Action? ResizeToFitContentAction { get; set; }

    protected override Type StyleKeyOverride => typeof(ContentControl);

    internal void ResizeToFitContent()
        => (ResizeToFitContentAction
            ?? throw new InvalidOperationException("The column header has no content-sizing owner."))();
}

/// <summary>
/// Preserves the source table-layout identity while using Avalonia grid sizing.
/// </summary>
public class TableLayoutPanel : Grid
{
}

/// <summary>
/// Preserves the source tab-page identity while using Avalonia tab-item behavior.
/// </summary>
public class TabPage : TabItem
{
    protected override Type StyleKeyOverride => typeof(TabItem);
}

/// <summary>
/// Preserves the source split-container identity while using an Avalonia grid and splitter.
/// </summary>
public class SplitContainer : Grid
{
}

/// <summary>
/// Preserves the source flow-layout identity around the native Avalonia layout selected by each twin.
/// </summary>
public class FlowLayoutPanel : Decorator
{
}
