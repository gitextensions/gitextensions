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
///  Preserves the source link identity while using Avalonia's routed button input and accessibility.
/// </summary>
public class LinkLabel : Button
{
    /// <summary>
    ///  Gets the link caption independently of its image/content renderer.
    /// </summary>
    public string Text => Content is Grid grid
        ? grid.Children.OfType<TextBlock>().Single().Text ?? string.Empty
        : Content as string ?? string.Empty;

    protected override Type StyleKeyOverride => typeof(Button);
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
/// Describes the source picture-box image placement used by the portable control.
/// </summary>
public enum PictureBoxSizeMode
{
    Normal,
    CenterImage,
    Zoom,
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
    private PictureBoxSizeMode _sizeMode;

    public PictureBox()
    {
        Child = _image;
    }

    public IImage? Source
    {
        get => _image.Source;
        set
        {
            _image.Source = value;
            InvalidateArrange();
        }
    }

    public PictureBoxSizeMode SizeMode
    {
        get => _sizeMode;
        set
        {
            _sizeMode = value;
            bool center = value == PictureBoxSizeMode.CenterImage;

            // The owner applies Zoom's integer rectangle. A second Uniform fit would
            // discard its independently truncated width and height.
            _image.Stretch = value == PictureBoxSizeMode.Zoom ? Stretch.Fill : Stretch.None;
            _image.HorizontalAlignment = value == PictureBoxSizeMode.Zoom
                ? Avalonia.Layout.HorizontalAlignment.Stretch
                : center
                ? Avalonia.Layout.HorizontalAlignment.Center
                : Avalonia.Layout.HorizontalAlignment.Left;
            _image.VerticalAlignment = value == PictureBoxSizeMode.Zoom
                ? Avalonia.Layout.VerticalAlignment.Stretch
                : center
                ? Avalonia.Layout.VerticalAlignment.Center
                : Avalonia.Layout.VerticalAlignment.Top;
            InvalidateArrange();
        }
    }

    protected override Type StyleKeyOverride => typeof(Border);

    protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
    {
        if (SizeMode != PictureBoxSizeMode.Zoom || Source is not { Size.Width: > 0, Size.Height: > 0 } source)
        {
            return base.ArrangeOverride(finalSize);
        }

        // WinForms PictureBox.Zoom truncates the scaled image rectangle before integer
        // centering; Uniform stretch leaves fractional image edges and a different crop.
        int clientWidth = (int)Math.Round(Math.Max(0, finalSize.Width - BorderThickness.Left - BorderThickness.Right));
        int clientHeight = (int)Math.Round(Math.Max(0, finalSize.Height - BorderThickness.Top - BorderThickness.Bottom));
        float ratio = Math.Min(clientWidth / (float)source.Size.Width,
            clientHeight / (float)source.Size.Height);
        int width = (int)((float)source.Size.Width * ratio);
        int height = (int)((float)source.Size.Height * ratio);
        double left = BorderThickness.Left + ((clientWidth - width) / 2);
        double top = BorderThickness.Top + ((clientHeight - height) / 2);
        _image.Arrange(new Avalonia.Rect(left, top, width, height));
        return finalSize;
    }
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
    /// <summary>
    ///  Identifies the source ToolStripMenuItem padding used for native preferred-size calculation.
    /// </summary>
    public static readonly Avalonia.AttachedProperty<Avalonia.Thickness> NativeItemPaddingProperty =
        Avalonia.AvaloniaProperty.RegisterAttached<MenuStripEx, MenuItem, Avalonia.Thickness>(
            "NativeItemPadding", new Avalonia.Thickness(4, 0, 4, 0));

    private const int NativeItemBorderWidth = 2;
    private const int NativeItemImageWidth = 16;
    private readonly Dictionary<MenuItem, IDisposable?> _autoWidthItems = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MenuItem, (TemplatedControl Owner, IDisposable?[] Values)> _itemFontValues = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<MenuItem> _pendingAutoWidthItems = new(ReferenceEqualityComparer.Instance);
    private bool _updatingAutoWidth;
    private bool _updatingItemFonts;
    private bool _itemFontsDirty;
    private bool _itemUpdateQueued;

    public MenuStripEx()
    {
        Items.CollectionChanged += OnItemCollectionChanged;
        PropertyChanged += OnOwnedItemPropertyChanged;
        LayoutUpdated += (_, _) =>
        {
            ProcessPendingItemChanges();
            RoundAutoItemWidths();
        };
    }

    /// <summary>
    ///  Gets the source padding, which is independent of the Avalonia renderer's text insets.
    /// </summary>
    public static Avalonia.Thickness GetNativeItemPadding(MenuItem item)
        => item.GetValue(NativeItemPaddingProperty);

    /// <summary>
    ///  Sets authored or runtime source padding for one top-level item.
    /// </summary>
    public static void SetNativeItemPadding(MenuItem item, Avalonia.Thickness padding)
        => item.SetValue(NativeItemPaddingProperty, padding);

    private static IEnumerable<(MenuItem Item, TemplatedControl Owner)> EnumerateOwnedItems(ItemsControl owner)
    {
        foreach (MenuItem item in owner.Items.OfType<MenuItem>())
        {
            yield return (item, owner);
            foreach ((MenuItem descendant, TemplatedControl parent) in EnumerateOwnedItems(item))
            {
                yield return (descendant, parent);
            }
        }
    }

    private void UpdateItemFonts(bool ownerFontChanged = false)
    {
        if (_updatingItemFonts)
        {
            return;
        }

        _updatingItemFonts = true;
        try
        {
            (MenuItem Item, TemplatedControl Owner)[] ownedItems = [.. EnumerateOwnedItems(this)];
            HashSet<MenuItem> currentItems = new(ownedItems.Select(pair => pair.Item), ReferenceEqualityComparer.Instance);
            foreach (MenuItem item in _itemFontValues.Keys.Where(item => !currentItems.Contains(item)).ToArray())
            {
                ReleaseAutoItemWidth(item);
                item.Items.CollectionChanged -= OnItemCollectionChanged;
                item.PropertyChanged -= OnOwnedItemPropertyChanged;
                foreach (IDisposable? value in _itemFontValues[item].Values)
                {
                    value?.Dispose();
                }

                _itemFontValues.Remove(item);
            }

            foreach ((MenuItem item, TemplatedControl owner) in ownedItems)
            {
                if (!ReferenceEquals(owner, this))
                {
                    ReleaseAutoItemWidth(item);
                }

                if (_itemFontValues.TryGetValue(item, out (TemplatedControl Owner, IDisposable?[] Values) previous))
                {
                    if (!ownerFontChanged && ReferenceEquals(previous.Owner, owner))
                    {
                        continue;
                    }

                    foreach (IDisposable? value in previous.Values)
                    {
                        value?.Dispose();
                    }
                }
                else
                {
                    item.Items.CollectionChanged += OnItemCollectionChanged;
                    item.PropertyChanged += OnOwnedItemPropertyChanged;
                }

                // Framework constraint: MenuItem's generic style masks Avalonia font inheritance.
                // Native autogenerated dropdowns inherit their owner item's effective font.
                // Template priority overrides generic defaults, not local or conditional item fonts.
                _itemFontValues[item] = (owner,
                [
                    item.SetValue(MenuItem.FontFamilyProperty, owner.FontFamily, Avalonia.Data.BindingPriority.Template),
                    item.SetValue(MenuItem.FontSizeProperty, owner.FontSize, Avalonia.Data.BindingPriority.Template),
                    item.SetValue(MenuItem.FontStyleProperty, owner.FontStyle, Avalonia.Data.BindingPriority.Template),
                    item.SetValue(MenuItem.FontWeightProperty, owner.FontWeight, Avalonia.Data.BindingPriority.Template),
                ]);
            }
        }
        finally
        {
            _updatingItemFonts = false;
        }
    }

    private void OnItemCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        => UpdateItemFonts();

    private void OnOwnedItemPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs args)
    {
        if (!_updatingItemFonts && (args.Property == FontFamilyProperty
            || args.Property == FontSizeProperty
            || args.Property == FontStyleProperty
            || args.Property == FontWeightProperty))
        {
            _itemFontsDirty = true;
            QueueItemUpdate();
        }
    }

    private void QueueItemUpdate()
    {
        if (_itemUpdateQueued || _updatingItemFonts)
        {
            return;
        }

        _itemUpdateQueued = true;

        // Framework constraint: font changes also fire inside Avalonia's style-frame iteration.
        // Defer owned font/width entry mutation until that iteration has returned.
        Dispatcher.UIThread.Post(() =>
        {
            _itemUpdateQueued = false;
            ProcessPendingItemChanges();
            RoundAutoItemWidths();
        }, DispatcherPriority.Loaded);
    }

    private void ProcessPendingItemChanges()
    {
        bool ownerFontChanged = _itemFontsDirty;
        _itemFontsDirty = false;
        UpdateItemFonts(ownerFontChanged);
        foreach (MenuItem item in _pendingAutoWidthItems.ToArray())
        {
            _pendingAutoWidthItems.Remove(item);
            if (!_autoWidthItems.TryGetValue(item, out IDisposable? value))
            {
                continue;
            }

            _updatingAutoWidth = true;
            try
            {
                value?.Dispose();
                _autoWidthItems[item] = null;
            }
            finally
            {
                _updatingAutoWidth = false;
            }

            if (!double.IsNaN(item.Width))
            {
                ReleaseAutoItemWidth(item);
            }
        }
    }

    private void RoundAutoItemWidths()
    {
        foreach (MenuItem item in _autoWidthItems.Keys.Where(item => !Items.Contains(item)).ToArray())
        {
            ReleaseAutoItemWidth(item);
        }

        foreach (MenuItem item in Items.OfType<MenuItem>())
        {
            if (!_autoWidthItems.ContainsKey(item))
            {
                if (!double.IsNaN(item.Width))
                {
                    continue;
                }

                _autoWidthItems.Add(item, null);
                item.PropertyChanged += OnAutoItemPropertyChanged;
            }

            if (double.IsNaN(item.Width) && item.DesiredSize.Width > 0)
            {
                // Framework constraint: ToolStripItem preferred size adds its internal border
                // and source padding to TextRenderer's mnemonic-aware, overhang-padded text.
                // The Avalonia renderer's own Padding is not that source sizing input.
                double width = item.DesiredSize.Width;
                if (item.Header is null or string)
                {
                    string text = AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(item.Header as string ?? string.Empty);
                    if (OperatingSystem.IsWindows())
                    {
                        text = text.Replace("&", "&&", StringComparison.Ordinal);
                    }

                    Avalonia.Thickness padding = GetNativeItemPadding(item);
                    double imageWidth = item.Icon is null ? 0 : NativeItemImageWidth;
                    width = WinFormsTextMeasurer.MeasureTextRenderer(item, text).Width
                        + imageWidth + (NativeItemBorderWidth * 2) + padding.Left + padding.Right;
                }

                _updatingAutoWidth = true;
                try
                {
                    // Keep automatic width disposable so removing an owner cannot clear an
                    // explicitly authored Width, including one equal to the cached width.
                    item.ClearValue(MenuItem.WidthProperty);
                    _autoWidthItems[item]?.Dispose();
                    _autoWidthItems[item] = item.SetValue(
                        MenuItem.WidthProperty,
                        Math.Ceiling(Math.Max(item.MinWidth, width)),
                        Avalonia.Data.BindingPriority.Template);
                }
                finally
                {
                    _updatingAutoWidth = false;
                }
            }
        }
    }

    private void ReleaseAutoItemWidth(MenuItem item)
    {
        _pendingAutoWidthItems.Remove(item);
        if (_autoWidthItems.Remove(item, out IDisposable? value))
        {
            item.PropertyChanged -= OnAutoItemPropertyChanged;
            value?.Dispose();
        }
    }

    private void OnAutoItemPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs args)
    {
        if (sender is not MenuItem item || _updatingAutoWidth)
        {
            return;
        }

        if (args.Property == MenuItem.WidthProperty
                 || args.Property == MenuItem.HeaderProperty
                 || args.Property == MenuItem.IconProperty
                 || args.Property == MenuItem.FontFamilyProperty
                 || args.Property == MenuItem.FontSizeProperty
                 || args.Property == MenuItem.FontStyleProperty
                 || args.Property == MenuItem.FontWeightProperty
                 || args.Property == MenuItem.MinWidthProperty
                 || args.Property == NativeItemPaddingProperty)
        {
            _pendingAutoWidthItems.Add(item);
            QueueItemUpdate();
        }
    }
}

/// <summary>
/// Preserves the source toolbar-container identity and its single-row overflow layout.
/// </summary>
public class ToolStripContainer : Avalonia.Controls.Panel
{
    private const double MinimumContentTop = 27;
    private double _contentTop = MinimumContentTop;

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
    {
        Control? topPanel = Children.FirstOrDefault(child => child.Name == "_topPanel");
        topPanel?.Measure(new Avalonia.Size(availableSize.Width, double.PositiveInfinity));

        // Framework constraint: the source ToolStripPanel grows with an explicitly assigned
        // toolbar font. Its authored 27-pixel row remains the minimum, not a clipping height.
        _contentTop = Math.Max(MinimumContentTop, topPanel?.DesiredSize.Height ?? MinimumContentTop);
        foreach (Control child in Children)
        {
            if (ReferenceEquals(child, topPanel))
            {
                continue;
            }

            child.Measure(child.Name switch
            {
                "_contentPanel" => new Avalonia.Size(
                    availableSize.Width,
                    Math.Max(0, availableSize.Height - _contentTop)),
                _ => new Avalonia.Size(0, 0),
            });
        }

        return availableSize;
    }

    protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
    {
        foreach (Control child in Children)
        {
            Avalonia.Rect bounds = child.Name switch
            {
                "_topPanel" => new Avalonia.Rect(0, 0, finalSize.Width, _contentTop),
                "_contentPanel" => new Avalonia.Rect(
                    0,
                    _contentTop,
                    finalSize.Width,
                    Math.Max(0, finalSize.Height - _contentTop)),
                "_leftPanel" => new Avalonia.Rect(0, 0, 0, 175),
                "_rightPanel" => new Avalonia.Rect(150, 0, 0, 175),
                "_bottomPanel" => new Avalonia.Rect(0, 175, 150, 0),
                _ => default,
            };
            child.Arrange(bounds);
        }

        return finalSize;
    }
}

/// <summary>
/// Retains the source top ToolStripPanel as the actual parent of its three toolbars.
/// </summary>
public class ToolStripPanel : Avalonia.Controls.Panel
{
    private const double LeadingInset = 7;
    private const double MinimumFilterWidth = 50;
    private const double ScriptsWidth = 50;
    private const double TrailingInset = 4;
    private const double ToolStripHeight = 25;
    private const double FilterToolStripHeight = 27;

    public double MainPreferredWidth { get; set; } = double.PositiveInfinity;

    public double MainPreferredHeight { get; set; } = ToolStripHeight;

    private static (double MainWidth, double FilterWidth) GetToolbarWidths(double totalWidth, double mainPreferredWidth)
    {
        double availableWidth = Math.Max(0, totalWidth - LeadingInset - ScriptsWidth - TrailingInset);
        double filterWidth = Math.Min(availableWidth, Math.Max(MinimumFilterWidth, availableWidth - mainPreferredWidth));
        return (availableWidth - filterWidth, filterWidth);
    }

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
    {
        (double mainWidth, double filterWidth) = GetToolbarWidths(availableSize.Width, MainPreferredWidth);
        foreach (Control child in Children)
        {
            child.Measure(child.Name switch
            {
                "toolStripMainHost" => new Avalonia.Size(mainWidth, MainPreferredHeight),
                "toolStripFiltersHost" => new Avalonia.Size(filterWidth, FilterToolStripHeight),
                "ToolStripScripts" => new Avalonia.Size(ScriptsWidth, ToolStripHeight),
                _ => default,
            });
        }

        return new Avalonia.Size(availableSize.Width, Math.Max(FilterToolStripHeight, MainPreferredHeight));
    }

    protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
    {
        (double mainWidth, double filterWidth) = GetToolbarWidths(finalSize.Width, MainPreferredWidth);
        double filterX = LeadingInset + mainWidth;
        foreach (Control child in Children)
        {
            Avalonia.Rect bounds = child.Name switch
            {
                "toolStripMainHost" => new Avalonia.Rect(LeadingInset, 0, mainWidth, MainPreferredHeight),
                "toolStripFiltersHost" => new Avalonia.Rect(filterX, 0, filterWidth, FilterToolStripHeight),
                "ToolStripScripts" => new Avalonia.Rect(
                    Math.Max(0, finalSize.Width - ScriptsWidth - TrailingInset),
                    0,
                    ScriptsWidth,
                    ToolStripHeight),
                _ => default,
            };
            child.Arrange(bounds);
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
