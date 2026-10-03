using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Preserves the main horizontal ToolStrip's item ownership and split-stack overflow layout.
/// </summary>
/// <remarks>
///  WinForms items retain their Owner while their current parent changes to ToolStripOverflow.
///  The logical Items stay here; visual-only presenters move the actual controls without
///  cloning commands, rewriting their visibility, or losing their owner font and resources.
///  This is the Browse toolbar adapter, not a general WinForms control emulation layer.
/// </remarks>
public sealed class NativeToolStrip : Control, IDisposable
{
    private const int DefaultHeight = 25;
    private const int DefaultWidth = 100;
    private const int OverflowWidth = 16;
    private const int VisualStyleGripWidth = 5;
    private readonly NativeToolStripPresenter _mainPresenter;
    private readonly NativeToolStripOverflowPresenter _overflowPresenter;
    private readonly Popup _popup;
    private readonly Dictionary<Control, Size> _preferredSizes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Control, NativeToolStripItemPlacement> _placements = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Control, IDisposable?[]> _autoSizeValues = new(ReferenceEqualityComparer.Instance);
    private IDisposable?[] _overflowValues = [];
    private Button _overflowButton;
    private bool _measuring;
    private bool _disposed;

    public static readonly StyledProperty<FontFamily> FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner<NativeToolStrip>();
    public static readonly StyledProperty<double> FontSizeProperty = TextElement.FontSizeProperty.AddOwner<NativeToolStrip>();
    public static readonly StyledProperty<FontStyle> FontStyleProperty = TextElement.FontStyleProperty.AddOwner<NativeToolStrip>();
    public static readonly StyledProperty<FontWeight> FontWeightProperty = TextElement.FontWeightProperty.AddOwner<NativeToolStrip>();
    public static readonly StyledProperty<IBrush?> ForegroundProperty = TextElement.ForegroundProperty.AddOwner<NativeToolStrip>();
    public static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<NativeToolStrip>();
    public static readonly StyledProperty<Thickness> PaddingProperty = Border.PaddingProperty.AddOwner<NativeToolStrip>();
    public static readonly AttachedProperty<bool> ItemAutoSizeProperty =
        AvaloniaProperty.RegisterAttached<NativeToolStrip, Control, bool>("ItemAutoSize", true);
    public static readonly AttachedProperty<double> ItemHeightProperty =
        AvaloniaProperty.RegisterAttached<NativeToolStrip, Control, double>("ItemHeight", 22);
    public static readonly AttachedProperty<double> ItemPreferredHeightProperty =
        AvaloniaProperty.RegisterAttached<NativeToolStrip, Control, double>("ItemPreferredHeight", double.NaN);
    public static readonly AttachedProperty<NativeToolStripItemOverflow> ItemOverflowProperty =
        AvaloniaProperty.RegisterAttached<NativeToolStrip, Control, NativeToolStripItemOverflow>("ItemOverflow");

    static NativeToolStrip()
    {
        AffectsRender<NativeToolStrip>(BackgroundProperty);
    }

    public NativeToolStrip()
    {
        // ToolStrip.Font uses ToolStripManager's menu default, not its Form's configured font.
        // The original Browse's unassigned menu font remains Segoe UI 9pt at every configured
        // form-font size. Avalonia resolves the family normally on each desktop platform.
        SetCurrentValue(FontFamilyProperty, new FontFamily("Segoe UI"));
        SetCurrentValue(FontSizeProperty, 12);
        SetCurrentValue(FontStyleProperty, FontStyle.Normal);
        SetCurrentValue(FontWeightProperty, FontWeight.Normal);
        ClipToBounds = true;
        _mainPresenter = new NativeToolStripPresenter();
        VisualChildren.Add(_mainPresenter);
        _overflowPresenter = new NativeToolStripOverflowPresenter(this);
        _popup = new Popup
        {
            Child = _overflowPresenter,
            Placement = PlacementMode.BottomEdgeAlignedRight,
            IsLightDismissEnabled = true,
        };

        // PopupRoot receives its control theme through the Popup's logical owner.
        // A detached Popup can request IsOpen while leaving its presenter unstyled
        // and unarranged, even when its placement target has a visual root.
        LogicalChildren.Add(_popup);
        _popup.Closed += Popup_Closed;

        _overflowButton = new Button();
        AttachOverflowButton(_overflowButton);
        Items.CollectionChanged += Items_CollectionChanged;
    }

    /// <summary>
    ///  Raised when the source item preferred sizes change the owner's preferred size.
    /// </summary>
    public event EventHandler? PreferredSizeChanged;

    /// <summary>
    ///  Gets the stable logical collection, including items currently in the overflow.
    /// </summary>
    [Content]
    public AvaloniaList<Control> Items { get; } = new() { ResetBehavior = ResetBehavior.Remove };

    /// <summary>
    ///  Gets the original Avalonia collection alias for existing toolbar consumers.
    /// </summary>
    public AvaloniaList<Control> Children => Items;

    /// <summary>
    ///  Gets or sets the owner's independently assigned menu font family.
    /// </summary>
    public FontFamily FontFamily { get => GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }

    /// <summary>
    ///  Gets or sets the owner's menu font size in device-independent pixels.
    /// </summary>
    public double FontSize { get => GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }

    /// <summary>
    ///  Gets or sets the owner's menu font style.
    /// </summary>
    public FontStyle FontStyle { get => GetValue(FontStyleProperty); set => SetValue(FontStyleProperty, value); }

    /// <summary>
    ///  Gets or sets the owner's menu font weight.
    /// </summary>
    public FontWeight FontWeight { get => GetValue(FontWeightProperty); set => SetValue(FontWeightProperty, value); }

    /// <summary>
    ///  Gets or sets the inherited item foreground.
    /// </summary>
    public IBrush? Foreground { get => GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }

    /// <summary>
    ///  Gets or sets the owner's background.
    /// </summary>
    public IBrush? Background { get => GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }

    /// <summary>
    ///  Gets or sets the source owner's display padding.
    /// </summary>
    public Thickness Padding { get => GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }

    /// <summary>
    ///  Gets or sets the source ToolStrip translation text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    ///  Gets the unconstrained source owner preferred size.
    /// </summary>
    public Size PreferredSize { get; private set; }

    /// <summary>
    ///  Gets whether the current split-stack layout has overflow items.
    /// </summary>
    public bool HasOverflow => OverflowItems.Count > 0;

    /// <summary>
    ///  Gets whether the actual overflow popup is open.
    /// </summary>
    public bool IsOverflowOpen => _popup.IsOpen;

    /// <summary>
    ///  Gets the source-order items whose current placement is overflow.
    /// </summary>
    public IReadOnlyList<Control> OverflowItems { get; private set; } = [];

    /// <summary>
    ///  Gets the current overflow visual parent.
    /// </summary>
    public Control OverflowContent => _overflowPresenter;

    /// <summary>
    ///  Gets or sets the named overflow button without replacing source command items.
    /// </summary>
    public Button OverflowButton
    {
        get => _overflowButton;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(value, _overflowButton))
            {
                return;
            }

            CloseOverflow();
            _mainPresenter.Remove(_overflowButton);
            _overflowButton.Click -= OverflowButton_Click;
            ReleaseOverflowValues();
            LogicalChildren.Remove(_overflowButton);
            _overflowButton = value;
            AttachOverflowButton(value);
            InvalidateMeasure();
        }
    }

    /// <summary>
    ///  Gets whether the source item stretches to its main owner's display height.
    /// </summary>
    public static bool GetItemAutoSize(Control control) => control.GetValue(ItemAutoSizeProperty);

    /// <summary>
    ///  Sets whether the source item stretches to its main owner's display height.
    /// </summary>
    public static void SetItemAutoSize(Control control, bool value) => control.SetValue(ItemAutoSizeProperty, value);

    /// <summary>
    ///  Gets the source height of an item whose AutoSize is false.
    /// </summary>
    public static double GetItemHeight(Control control) => control.GetValue(ItemHeightProperty);

    /// <summary>
    ///  Sets the source height of an item whose AutoSize is false.
    /// </summary>
    public static void SetItemHeight(Control control, double value) => control.SetValue(ItemHeightProperty, value);

    /// <summary>
    ///  Gets an original item-specific preferred height independent of its stretched bounds.
    /// </summary>
    public static double GetItemPreferredHeight(Control control) => control.GetValue(ItemPreferredHeightProperty);

    /// <summary>
    ///  Sets an original item-specific preferred height independent of its stretched bounds.
    /// </summary>
    public static void SetItemPreferredHeight(Control control, double value) => control.SetValue(ItemPreferredHeightProperty, value);

    /// <summary>
    ///  Gets the source overflow policy.
    /// </summary>
    public static NativeToolStripItemOverflow GetItemOverflow(Control control) => control.GetValue(ItemOverflowProperty);

    /// <summary>
    ///  Sets the source overflow policy.
    /// </summary>
    public static void SetItemOverflow(Control control, NativeToolStripItemOverflow value) => control.SetValue(ItemOverflowProperty, value);

    /// <summary>
    ///  Gets the current source split-stack placement without changing item visibility.
    /// </summary>
    public NativeToolStripItemPlacement GetItemPlacement(Control control)
        => _placements.GetValueOrDefault(control);

    /// <summary>
    ///  Gets the source current parent; the logical owner remains this strip.
    /// </summary>
    public Control? GetCurrentParent(Control control)
        => GetItemPlacement(control) switch
        {
            NativeToolStripItemPlacement.Main => this,
            NativeToolStripItemPlacement.Overflow => _overflowPresenter,
            _ => null,
        };

    /// <summary>
    ///  Opens the wrapped overflow with the same item instances.
    /// </summary>
    public void ShowOverflow()
    {
        if (!HasOverflow || _disposed || !IsEffectivelyVisible)
        {
            return;
        }

        _overflowPresenter.SetItems(OverflowItems);
        _overflowPresenter.InvalidateMeasure();
        _popup.PlacementTarget = OverflowButton;
        _popup.Placement = FlowDirection == FlowDirection.RightToLeft
            ? PlacementMode.BottomEdgeAlignedLeft : PlacementMode.BottomEdgeAlignedRight;
        _popup.IsOpen = true;
    }

    /// <summary>
    ///  Closes the overflow without modifying the source item collection.
    /// </summary>
    public void CloseOverflow() => _popup.IsOpen = false;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        CloseOverflow();
        _popup.Closed -= Popup_Closed;
        _popup.Child = null;
        LogicalChildren.Remove(_popup);
        Items.CollectionChanged -= Items_CollectionChanged;
        foreach (Control item in Items)
        {
            DetachItem(item);
        }

        _overflowPresenter.SetItems([]);
        _mainPresenter.SetItems([]);
        _overflowButton.Click -= OverflowButton_Click;
        ReleaseOverflowValues();
        LogicalChildren.Remove(_overflowButton);
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Background ?? Brushes.Transparent, new Rect(Bounds.Size));
        base.Render(context);
    }

    protected override bool BypassFlowDirectionPolicies => true;

    protected override Size MeasureOverride(Size availableSize)
    {
        _measuring = true;
        try
        {
            foreach (Control item in Items)
            {
                ApplyItemAutoSize(item);
                item.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Thickness margin = item.Margin;
                double preferredHeight = GetItemPreferredHeight(item);
                _preferredSizes[item] = new Size(Pixel(item.DesiredSize.Width - margin.Left - margin.Right),
                    GetItemAutoSize(item)
                        ? Pixel(double.IsNaN(preferredHeight) ? item.DesiredSize.Height - margin.Top - margin.Bottom : preferredHeight)
                        : Pixel(GetItemHeight(item)));
            }
        }
        finally
        {
            _measuring = false;
        }

        Control[] participating = Items.Where(item => item.IsVisible).ToArray();
        double width = participating.Length == 0 ? DefaultWidth : 0;
        double height = DefaultHeight - Padding.Top - Padding.Bottom;
        foreach (Control item in participating.Where(item => GetItemOverflow(item) != NativeToolStripItemOverflow.Always))
        {
            Size itemSize = _preferredSizes[item];
            width += itemSize.Width + item.Margin.Left + item.Margin.Right;
            height = Math.Max(height, itemSize.Height + item.Margin.Top + item.Margin.Bottom);
        }

        width += participating.Any(item => GetItemOverflow(item) == NativeToolStripItemOverflow.Always) ? OverflowWidth : 2;
        Size preferred = new(width + VisualStyleGripWidth + Padding.Left + Padding.Right,
            Math.Max(0, height) + Padding.Top + Padding.Bottom);
        if (PreferredSize != preferred)
        {
            PreferredSize = preferred;
            PreferredSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        _mainPresenter.Measure(availableSize);
        return new Size(Math.Min(preferred.Width, availableSize.Width), preferred.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double width = Pixel(finalSize.Width);
        double height = Pixel(finalSize.Height);
        bool rightToLeft = FlowDirection == FlowDirection.RightToLeft;
        Rect display = new(Padding.Left + (rightToLeft ? 0 : VisualStyleGripWidth), Padding.Top,
            Math.Max(0, width - Padding.Left - Padding.Right - VisualStyleGripWidth),
            Math.Max(0, height - Padding.Top - Padding.Bottom));
        CalculatePlacements(display.Width);
        Control[] main = Items.Where(item => item.IsVisible && GetItemPlacement(item) == NativeToolStripItemPlacement.Main).ToArray();
        Control[] overflow = Items.Where(item => item.IsVisible && GetItemPlacement(item) == NativeToolStripItemPlacement.Overflow).ToArray();
        OverflowItems = overflow;
        OverflowButton.IsVisible = overflow.Length > 0;
        if (!HasOverflow)
        {
            CloseOverflow();
        }

        _overflowPresenter.SetItems(_popup.IsOpen ? overflow : []);
        _mainPresenter.SetItems(HasOverflow ? [.. main, OverflowButton] : main);
        double x = rightToLeft ? display.Right : display.Left;
        foreach (Control item in main)
        {
            Thickness margin = item.Margin;
            Size source = _preferredSizes[item];
            double itemHeight = GetItemAutoSize(item) ? Math.Max(0, display.Height - margin.Top - margin.Bottom) : source.Height;
            double y = GetItemAutoSize(item) ? display.Top + margin.Top : display.Top + (int)((display.Height - itemHeight) / 2);
            double itemX = rightToLeft ? x - margin.Right - source.Width : x + margin.Left;
            _mainPresenter.SetBounds(item, new Rect(itemX, y, source.Width, itemHeight));
            x = rightToLeft ? itemX - margin.Left : itemX + source.Width + margin.Right;
        }

        if (HasOverflow)
        {
            _mainPresenter.SetBounds(OverflowButton, new Rect(rightToLeft ? 0 : Math.Max(0, width - OverflowWidth), 0, OverflowWidth, height));
        }

        _mainPresenter.Arrange(new Rect(finalSize));
        _overflowPresenter.InvalidateMeasure();
        return finalSize;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsVisibleProperty && !IsVisible)
        {
            CloseOverflow();
        }

        if (change.Property == FontFamilyProperty || change.Property == FontSizeProperty
            || change.Property == FontStyleProperty || change.Property == FontWeightProperty
            || change.Property == FlowDirectionProperty || change.Property == PaddingProperty)
        {
            if (change.Property == FontFamilyProperty || change.Property == FontSizeProperty
                || change.Property == FontStyleProperty || change.Property == FontWeightProperty)
            {
                _measuring = true;
                try
                {
                    foreach (Control item in Items)
                    {
                        ReleaseAutoSize(item);
                    }
                }
                finally
                {
                    _measuring = false;
                }
            }

            InvalidateMeasure();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CloseOverflow();
        base.OnDetachedFromVisualTree(e);
    }

    internal Size GetItemPreferredSize(Control item) => _preferredSizes.GetValueOrDefault(item);

    private static double Pixel(double value) => Math.Max(0, Math.Ceiling(value));

    private void AttachOverflowButton(Button button)
    {
        LogicalChildren.Add(button);
        _overflowValues =
        [
            button.SetValue(HeightProperty, double.NaN, BindingPriority.StyleTrigger),
            button.SetValue(MinHeightProperty, 0d, BindingPriority.StyleTrigger),
            button.SetValue(VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Stretch, BindingPriority.StyleTrigger),
            button.SetValue(HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Stretch, BindingPriority.StyleTrigger),
        ];
        button.Click += OverflowButton_Click;
        _popup.PlacementTarget = button;
    }

    private void ReleaseOverflowValues()
    {
        foreach (IDisposable? value in _overflowValues)
        {
            value?.Dispose();
        }

        _overflowValues = [];
    }

    private void OverflowButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_popup.IsOpen)
        {
            CloseOverflow();
        }
        else
        {
            ShowOverflow();
        }
    }

    private void Popup_Closed(object? sender, EventArgs e) => _overflowPresenter.SetItems([]);

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (Control item in e.OldItems)
            {
                if (!Items.Contains(item))
                {
                    DetachItem(item);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (Control item in e.NewItems)
            {
                if (!LogicalChildren.Contains(item))
                {
                    LogicalChildren.Add(item);
                    item.PropertyChanged += Item_PropertyChanged;
                    item.AddHandler(Button.ClickEvent, Item_Click, RoutingStrategies.Bubble);
                    item.AddHandler(SplitButton.ClickEvent, Item_Click, RoutingStrategies.Bubble);
                }
            }
        }

        InvalidateMeasure();
    }

    private void DetachItem(Control item)
    {
        _mainPresenter.Remove(item);
        _overflowPresenter.Remove(item);
        item.PropertyChanged -= Item_PropertyChanged;
        item.RemoveHandler(Button.ClickEvent, Item_Click);
        item.RemoveHandler(SplitButton.ClickEvent, Item_Click);
        LogicalChildren.Remove(item);
        _preferredSizes.Remove(item);
        _placements.Remove(item);
        ReleaseAutoSize(item);
    }

    private void ApplyItemAutoSize(Control item)
    {
        if (_autoSizeValues.ContainsKey(item))
        {
            return;
        }

        List<IDisposable?> values =
        [
            item.SetValue(HeightProperty, GetItemAutoSize(item) ? double.NaN : GetItemHeight(item), BindingPriority.StyleTrigger),
            item.SetValue(MinHeightProperty, 0d, BindingPriority.StyleTrigger),
            item.SetValue(VerticalAlignmentProperty, Avalonia.Layout.VerticalAlignment.Stretch, BindingPriority.StyleTrigger),
            item.SetValue(HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Stretch, BindingPriority.StyleTrigger),
        ];
        if (item is TemplatedControl control)
        {
            // The source item inherits ToolStrip.Font, not the application's generic
            // control-font style. Explicit local item fonts still take precedence.
            values.Add(control.SetValue(TemplatedControl.FontFamilyProperty, FontFamily, BindingPriority.StyleTrigger));
            values.Add(control.SetValue(TemplatedControl.FontSizeProperty, FontSize, BindingPriority.StyleTrigger));
            values.Add(control.SetValue(TemplatedControl.FontStyleProperty, FontStyle, BindingPriority.StyleTrigger));
            values.Add(control.SetValue(TemplatedControl.FontWeightProperty, FontWeight, BindingPriority.StyleTrigger));
        }

        _autoSizeValues[item] = values.ToArray();
    }

    private void ReleaseAutoSize(Control item)
    {
        if (_autoSizeValues.Remove(item, out IDisposable?[]? values))
        {
            foreach (IDisposable? value in values)
            {
                value?.Dispose();
            }
        }
    }

    private void Item_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_measuring || sender is not Control item || e.Property == BoundsProperty)
        {
            return;
        }

        if (e.Property == ItemAutoSizeProperty || e.Property == ItemHeightProperty)
        {
            _measuring = true;
            try
            {
                ReleaseAutoSize(item);
            }
            finally
            {
                _measuring = false;
            }
        }

        if (e.Property != IsPointerOverProperty && e.Property != IsFocusedProperty)
        {
            InvalidateMeasure();
        }
    }

    private void Item_Click(object? sender, RoutedEventArgs e)
    {
        if (!_popup.IsOpen || sender is not Control item || GetItemPlacement(item) != NativeToolStripItemPlacement.Overflow)
        {
            return;
        }

        // ToolStripSplitButton.DismissWhenClicked remains false while its own drop-down
        // is open. Preserve that command's existing Flyout rather than closing its parent.
        if (item is SplitButton { Flyout.IsOpen: true } || item is DropDownButton { Flyout.IsOpen: true })
        {
            return;
        }

        CloseOverflow();
    }

    private void CalculatePlacements(double availableWidth)
    {
        _placements.Clear();
        double currentWidth = 0;
        bool overflowRequired = false;
        int backward = -1;
        for (int forward = 0; forward < Items.Count; forward++)
        {
            Control item = Items[forward];
            if (!item.IsVisible)
            {
                continue;
            }

            if (GetItemOverflow(item) == NativeToolStripItemOverflow.Always)
            {
                overflowRequired = true;
                _placements[item] = NativeToolStripItemPlacement.Overflow;
                continue;
            }

            if (GetItemPlacement(item) == NativeToolStripItemPlacement.Overflow)
            {
                continue;
            }

            currentWidth += ItemWidth(item);
            double overflowWidth = overflowRequired ? OverflowWidth : 0;
            if (currentWidth <= availableWidth - overflowWidth)
            {
                continue;
            }

            double required = currentWidth + overflowWidth - availableWidth;
            double recovered = 0;
            backward = backward < 0 ? Items.Count - 1 : backward - 1;
            for (; backward >= 0; backward--)
            {
                Control candidate = Items[backward];
                if (!candidate.IsVisible)
                {
                    continue;
                }

                if (GetItemOverflow(candidate) == NativeToolStripItemOverflow.AsNeeded
                    && GetItemPlacement(candidate) != NativeToolStripItemPlacement.Overflow)
                {
                    if (backward <= forward)
                    {
                        recovered += ItemWidth(candidate);
                    }

                    _placements[candidate] = NativeToolStripItemPlacement.Overflow;
                    if (!overflowRequired)
                    {
                        required += OverflowWidth;
                        overflowRequired = true;
                    }
                }

                // The source backward walker deliberately uses strict greater-than.
                if (recovered > required)
                {
                    break;
                }
            }

            currentWidth -= recovered;
        }

        foreach (Control item in Items.Where(item => item.IsVisible))
        {
            if (GetItemPlacement(item) == NativeToolStripItemPlacement.None)
            {
                _placements[item] = NativeToolStripItemPlacement.Main;
            }
        }
    }

    private double ItemWidth(Control item)
        => _preferredSizes[item].Width + item.Margin.Left + item.Margin.Right;
}

/// <summary>
///  Defines the source ToolStrip item overflow policy.
/// </summary>
public enum NativeToolStripItemOverflow
{
    /// <summary>
    ///  Moves the item only when the source split-stack layout requires space.
    /// </summary>
    AsNeeded,

    /// <summary>
    ///  Always places the item in the overflow.
    /// </summary>
    Always,

    /// <summary>
    ///  Keeps the item on the main owner.
    /// </summary>
    Never,
}

/// <summary>
///  Identifies the source current parent placement.
/// </summary>
public enum NativeToolStripItemPlacement
{
    /// <summary>
    ///  The item does not participate in the current layout.
    /// </summary>
    None,

    /// <summary>
    ///  The item is displayed on the main owner.
    /// </summary>
    Main,

    /// <summary>
    ///  The item belongs to the wrapped overflow.
    /// </summary>
    Overflow,
}

internal class NativeToolStripPresenter : Control
{
    private readonly Dictionary<Control, Rect> _bounds = new(ReferenceEqualityComparer.Instance);

    protected override bool BypassFlowDirectionPolicies => true;

    public void SetItems(IReadOnlyList<Control> items)
    {
        foreach (Control existing in VisualChildren.OfType<Control>().Where(item => !items.Contains(item)).ToArray())
        {
            Remove(existing);
        }

        for (int index = 0; index < items.Count; index++)
        {
            Control item = items[index];
            if (item.GetVisualParent() is NativeToolStripPresenter previous && !ReferenceEquals(previous, this))
            {
                previous.Remove(item);
            }

            if (!VisualChildren.Contains(item))
            {
                VisualChildren.Insert(index, item);
            }
            else if (VisualChildren.IndexOf(item) != index)
            {
                VisualChildren.Move(VisualChildren.IndexOf(item), index);
            }
        }
    }

    public void Remove(Control item)
    {
        VisualChildren.Remove(item);
        _bounds.Remove(item);
    }

    public void SetBounds(Control item, Rect bounds) => _bounds[item] = bounds;

    protected override Size MeasureOverride(Size availableSize) => default;

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (Control item in VisualChildren.OfType<Control>())
        {
            if (_bounds.TryGetValue(item, out Rect bounds))
            {
                Thickness margin = item.Margin;
                item.Arrange(new Rect(bounds.X - margin.Left, bounds.Y - margin.Top,
                    bounds.Width + margin.Left + margin.Right, bounds.Height + margin.Top + margin.Bottom));
            }
        }

        return finalSize;
    }
}

internal sealed class NativeToolStripOverflowPresenter : NativeToolStripPresenter
{
    private const int PreferredWidthConstraint = 200;
    private readonly NativeToolStrip _owner;
    private readonly Thickness _sourcePadding = new(1, 2, 1, 2);
    private IReadOnlyList<Control> _items = [];

    public static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<NativeToolStripOverflowPresenter>();
    public static readonly StyledProperty<IBrush?> BorderBrushProperty = Border.BorderBrushProperty.AddOwner<NativeToolStripOverflowPresenter>();

    static NativeToolStripOverflowPresenter()
    {
        AffectsRender<NativeToolStripOverflowPresenter>(BackgroundProperty, BorderBrushProperty);
    }

    public NativeToolStripOverflowPresenter(NativeToolStrip owner)
    {
        _owner = owner;
        this[!BackgroundProperty] = new DynamicResourceExtension("GitExtensionsKnownColorControlBrush");
        this[!BorderBrushProperty] = new DynamicResourceExtension("GitExtensionsKnownColorControlDarkBrush");
        KeyDown += (_, e) =>
        {
            if (!e.Handled && e.Key == Key.Escape)
            {
                _owner.CloseOverflow();
                e.Handled = true;
            }
        };
    }

    public new void SetItems(IReadOnlyList<Control> items)
    {
        _items = items;
        base.SetItems(items);
    }

    public override void Render(DrawingContext context)
    {
        IBrush background = GetValue(BackgroundProperty) ?? Brushes.Transparent;
        IBrush border = GetValue(BorderBrushProperty) ?? Brushes.Transparent;
        context.FillRectangle(background, new Rect(Bounds.Size));
        context.FillRectangle(border, new Rect(0, 0, Bounds.Width, 1));
        context.FillRectangle(border, new Rect(0, Math.Max(0, Bounds.Height - 1), Bounds.Width, 1));
        context.FillRectangle(border, new Rect(0, 0, 1, Bounds.Height));
        context.FillRectangle(border, new Rect(Math.Max(0, Bounds.Width - 1), 0, 1, Bounds.Height));
        base.Render(context);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // FlowLayout.GetPreferredSize makes a second pass if an oversized first item
        // exceeded its constraint. ToolStripOverflow constrains preferred width to200,
        // but never clips a source item merely because that item itself is wider.
        Size first = LayoutRows(PreferredWidthConstraint - _sourcePadding.Left - _sourcePadding.Right, arrange: false);
        Size measured = first.Width > PreferredWidthConstraint - _sourcePadding.Left - _sourcePadding.Right
            ? LayoutRows(first.Width, arrange: false) : first;
        return measured + new Size(_sourcePadding.Left + _sourcePadding.Right, _sourcePadding.Top + _sourcePadding.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        LayoutRows(Math.Max(0, finalSize.Width - _sourcePadding.Left - _sourcePadding.Right), arrange: true);
        return base.ArrangeOverride(finalSize);
    }

    private Size LayoutRows(double width, bool arrange)
    {
        double x = 0;
        double y = 0;
        double rowHeight = 0;
        double widest = 0;
        foreach (Control item in _items)
        {
            Size size = _owner.GetItemPreferredSize(item);
            Thickness margin = item.Margin;
            double requiredWidth = size.Width + margin.Left + margin.Right;
            double requiredHeight = size.Height + margin.Top + margin.Bottom;
            if (x > 0 && x + requiredWidth > width)
            {
                widest = Math.Max(widest, x);
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            if (arrange)
            {
                double sourceX = _sourcePadding.Left + x + margin.Left;
                if (_owner.FlowDirection == FlowDirection.RightToLeft)
                {
                    // FlowLayout.ContainerProxy translates against DisplayRect.Right;
                    // it does not add the display rectangle's leading inset back in.
                    sourceX = _sourcePadding.Left + width - sourceX - size.Width;
                }

                SetBounds(item, new Rect(sourceX, _sourcePadding.Top + y + margin.Top, size.Width, size.Height));
            }

            x += requiredWidth;
            rowHeight = Math.Max(rowHeight, requiredHeight);
        }

        return new Size(Math.Max(widest, x), y + rowHeight);
    }
}
