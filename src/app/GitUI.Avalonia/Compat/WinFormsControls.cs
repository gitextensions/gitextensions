using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
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

    public string Text
    {
        get => Content as string ?? string.Empty;
        set => Content = value;
    }

    public object? Header
    {
        get => Content is TextBlock textBlock ? textBlock.Text : Content;
        set => Content = value;
    }

    public int DisplayIndex { get; internal set; }

    public double SourceWidth { get; set; } = 100;

    public bool CanUserResize { get; set; } = true;

    public string SortMode { get; set; } = "Automatic";

    public string CellAlignment { get; set; } = "NotSet";

    protected override Type StyleKeyOverride => typeof(ContentControl);

    internal void ResizeToFitContent()
        => (ResizeToFitContentAction
            ?? throw new InvalidOperationException("The column header has no content-sizing owner."))();
}

/// <summary>
/// Preserves the source text-column identity while a native Avalonia header and row template render it.
/// </summary>
public class DataGridViewTextBoxColumn : ColumnHeader
{
}

/// <summary>
/// Preserves the source checkbox-column identity while a native Avalonia header and row template render it.
/// </summary>
public class DataGridViewCheckBoxColumn : ColumnHeader
{
    public DataGridViewCheckBoxColumn()
    {
        SortMode = "NotSortable";
        CellAlignment = "MiddleCenter";
    }
}

/// <summary>
/// Preserves the source data-grid boundary while Avalonia owns list virtualization and row rendering.
/// </summary>
public class DataGridView : ListBox
{
    public IList<ColumnHeader> Columns { get; } = [];

    public bool IsReadOnly { get; set; }

    protected override Type StyleKeyOverride => typeof(ListBox);

    public void AddColumns(params ColumnHeader[] columns)
    {
        for (int index = 0; index < columns.Length; index++)
        {
            columns[index].DisplayIndex = index;
            Columns.Add(columns[index]);
        }
    }
}

/// <summary>
/// Preserves the source picture-box boundary while an inner Avalonia image performs native rendering.
/// </summary>
public class PictureBox : Border
{
    private readonly Image _image = new()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        Stretch = Stretch.None,
    };

    public PictureBox()
    {
        Child = _image;
    }

    public IImage? Source
    {
        get => _image.Source;
        set => _image.Source = value;
    }

    protected override Type StyleKeyOverride => typeof(Border);
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
    public string Text { get; set; } = string.Empty;

    protected override Type StyleKeyOverride => typeof(Border);
}

/// <summary>
/// Preserves the source flow-layout identity around the native Avalonia layout used by ported views.
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
/// Preserves the source toolbar-container identity and its single-row overflow layout.
/// </summary>
public class ToolStripContainer : Avalonia.Controls.Panel
{
    private const double LeadingInset = 7;
    private const double FilterWidth = 50;
    private const double FilterTrailingReserve = 104;
    private const double ScriptsTrailingInset = 30;
    private const double ContentTop = 27;
    private const double ToolStripHeight = 25;

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
    {
        if (double.IsInfinity(availableSize.Width))
        {
            foreach (Control child in Children)
            {
                child.Measure(new Avalonia.Size(double.PositiveInfinity, ToolStripHeight));
            }

            return new Avalonia.Size(Children.Sum(child => child.DesiredSize.Width), ToolStripHeight);
        }

        double filterX = Math.Max(LeadingInset, availableSize.Width - FilterTrailingReserve);
        if (Children.Count > 0)
        {
            Children[0].Measure(new Avalonia.Size(filterX - LeadingInset, ToolStripHeight));
        }

        if (Children.Count > 1)
        {
            Children[1].Measure(new Avalonia.Size(FilterWidth, ToolStripHeight));
        }

        if (Children.Count > 2)
        {
            Children[2].Measure(new Avalonia.Size(double.PositiveInfinity, ToolStripHeight));
        }

        if (Children.Count > 3)
        {
            Children[3].Measure(new Avalonia.Size(
                availableSize.Width,
                Math.Max(0, availableSize.Height - ContentTop)));
        }

        return availableSize;
    }

    protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
    {
        if (Children.Count > 0)
        {
            double filterX = Math.Max(LeadingInset, finalSize.Width - FilterTrailingReserve);
            Children[0].Arrange(new Avalonia.Rect(LeadingInset, 0, filterX - LeadingInset, ToolStripHeight));

            if (Children.Count > 1)
            {
                Children[1].Arrange(new Avalonia.Rect(filterX, 0, FilterWidth, ToolStripHeight));
            }

            if (Children.Count > 2)
            {
                Children[2].Arrange(new Avalonia.Rect(
                    Math.Max(0, finalSize.Width - ScriptsTrailingInset),
                    0,
                    Children[2].DesiredSize.Width,
                    ToolStripHeight));
            }
        }

        if (Children.Count > 3)
        {
            Children[3].Arrange(new Avalonia.Rect(
                0,
                ContentTop,
                finalSize.Width,
                Math.Max(0, finalSize.Height - ContentTop)));
        }

        return finalSize;
    }
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
