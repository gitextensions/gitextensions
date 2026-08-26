using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;

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
/// Preserves the source panel identity for a single native Avalonia child.
/// </summary>
public class Panel : Border
{
    protected override Type StyleKeyOverride => typeof(Border);
}

/// <summary>
/// Preserves the source flow-layout identity around the native Avalonia layout selected by each twin.
/// </summary>
public class FlowLayoutPanel : Decorator
{
}

/// <summary>
/// Preserves the source menu-strip identity while using Avalonia menu behavior.
/// </summary>
public class MenuStrip : Menu
{
    protected override Type StyleKeyOverride => typeof(Menu);
}

/// <summary>
/// Preserves Git Extensions' source menu-strip subtype while using Avalonia menu behavior.
/// </summary>
public class MenuStripEx : MenuStrip
{
}

/// <summary>
/// Preserves the source toolbar-container identity while using Avalonia's wrapping layout.
/// </summary>
public class ToolStripContainer : WrapPanel
{
}

/// <summary>
/// Preserves the source drop-down-item boundary while using Avalonia menu behavior.
/// </summary>
public class ToolStripDropDownItem : MenuItem
{
    protected override Type StyleKeyOverride => typeof(MenuItem);

    protected override void OnSubmenuOpened(RoutedEventArgs e)
    {
        base.OnSubmenuOpened(e);

        // Framework constraint: Avalonia measures each popup from its own template. WinForms
        // instead gives every item the widest ToolStrip text-and-shortcut layout when it opens.
        WinFormsToolStripMenuSizer.Apply(this);
        Dispatcher.UIThread.Post(
            () => WinFormsToolStripMenuSizer.Apply(this),
            DispatcherPriority.Loaded);
    }
}

/// <summary>
/// Preserves the source menu-item identity while using Avalonia menu behavior.
/// </summary>
public class ToolStripMenuItem : ToolStripDropDownItem
{
}

/// <summary>
/// Preserves the source menu-separator identity while using Avalonia separator rendering.
/// </summary>
public class ToolStripSeparator : Separator
{
    protected override Type StyleKeyOverride => typeof(Separator);
}

/// <summary>
/// Preserves the source context-menu identity while using Avalonia popup behavior.
/// </summary>
public class ContextMenuStrip : ContextMenu
{
    protected override Type StyleKeyOverride => typeof(ContextMenu);
}
