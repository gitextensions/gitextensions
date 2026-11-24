using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Paints ordinary ToolStripButton state without using the frame as content padding.
/// </summary>
internal sealed class NativeToolStripButtonChrome : Control
{
    public static readonly StyledProperty<bool> UseSystemVisualStyleProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, bool>(nameof(UseSystemVisualStyle));
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, bool>(nameof(IsChecked));
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, bool>(nameof(IsSelected));
    public static readonly StyledProperty<bool> IsPressedProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, bool>(nameof(IsPressed));
    public static readonly StyledProperty<IBrush?> CheckedBrushProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, IBrush?>(nameof(CheckedBrush));
    public static readonly StyledProperty<IBrush?> SelectedBrushProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, IBrush?>(nameof(SelectedBrush));
    public static readonly StyledProperty<IBrush?> PressedBrushProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, IBrush?>(nameof(PressedBrush));
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, IBrush?>(nameof(BorderBrush));
    public static readonly StyledProperty<IBrush?> SelectedCornerBrushProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, IBrush?>(nameof(SelectedCornerBrush));
    public static readonly StyledProperty<IBrush?> SelectedAdjacentBrushProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, IBrush?>(nameof(SelectedAdjacentBrush));
    public static readonly StyledProperty<IBrush?> PressedCornerBrushProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, IBrush?>(nameof(PressedCornerBrush));
    public static readonly StyledProperty<IBrush?> PressedAdjacentBrushProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, IBrush?>(nameof(PressedAdjacentBrush));

    private static readonly AttachedProperty<bool?> OwnerSelectedProperty =
        AvaloniaProperty.RegisterAttached<NativeToolStripButtonChrome, Button, bool?>("OwnerSelected");
    private static readonly StyledProperty<bool> OwnerAllowsPressedPaintProperty =
        AvaloniaProperty.Register<NativeToolStripButtonChrome, bool>("OwnerAllowsPressedPaint", defaultValue: true);
    private Button? _owner;

    static NativeToolStripButtonChrome()
    {
        AffectsRender<NativeToolStripButtonChrome>(UseSystemVisualStyleProperty, IsCheckedProperty,
            IsSelectedProperty, IsPressedProperty, OwnerAllowsPressedPaintProperty, IsEffectivelyEnabledProperty, CheckedBrushProperty,
            SelectedBrushProperty, PressedBrushProperty, BorderBrushProperty, SelectedCornerBrushProperty,
            SelectedAdjacentBrushProperty, PressedCornerBrushProperty, PressedAdjacentBrushProperty);
    }

    /// <summary>Initializes paint-only chrome without another input or focus target.</summary>
    public NativeToolStripButtonChrome()
    {
        IsHitTestVisible = false;
        Focusable = false;
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }

    /// <summary>Gets or sets the original System versus Professional renderer choice.</summary>
    public bool UseSystemVisualStyle { get => GetValue(UseSystemVisualStyleProperty); set => SetValue(UseSystemVisualStyleProperty, value); }

    /// <summary>Gets or sets whether the source check state is not Unchecked.</summary>
    public bool IsChecked { get => GetValue(IsCheckedProperty); set => SetValue(IsCheckedProperty, value); }

    /// <summary>Gets or sets pointer or keyboard item selection, independently of pressing.</summary>
    public bool IsSelected { get => GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

    /// <summary>Gets source pressed paint or sets its framework input without altering the owner's physical button state.</summary>
    public bool IsPressed { get => GetValue(IsPressedProperty) && GetValue(OwnerAllowsPressedPaintProperty); set => SetValue(IsPressedProperty, value); }

    /// <summary>Gets or sets checked fill, including the transparent Professional gradient.</summary>
    public IBrush? CheckedBrush { get => GetValue(CheckedBrushProperty); set => SetValue(CheckedBrushProperty, value); }

    /// <summary>Gets or sets the source selected fill.</summary>
    public IBrush? SelectedBrush { get => GetValue(SelectedBrushProperty); set => SetValue(SelectedBrushProperty, value); }

    /// <summary>Gets or sets the source pressed fill.</summary>
    public IBrush? PressedBrush { get => GetValue(PressedBrushProperty); set => SetValue(PressedBrushProperty, value); }

    /// <summary>Gets or sets raw native Highlight or Professional ButtonSelectedBorder.</summary>
    public IBrush? BorderBrush { get => GetValue(BorderBrushProperty); set => SetValue(BorderBrushProperty, value); }

    /// <summary>Gets or sets the native System hot inner-corner texel.</summary>
    public IBrush? SelectedCornerBrush { get => GetValue(SelectedCornerBrushProperty); set => SetValue(SelectedCornerBrushProperty, value); }

    /// <summary>Gets or sets the native System hot adjacent corner texel.</summary>
    public IBrush? SelectedAdjacentBrush { get => GetValue(SelectedAdjacentBrushProperty); set => SetValue(SelectedAdjacentBrushProperty, value); }

    /// <summary>Gets or sets the native System pressed/checked inner-corner texel.</summary>
    public IBrush? PressedCornerBrush { get => GetValue(PressedCornerBrushProperty); set => SetValue(PressedCornerBrushProperty, value); }

    /// <summary>Gets or sets the native System pressed/checked adjacent corner texel.</summary>
    public IBrush? PressedAdjacentBrush { get => GetValue(PressedAdjacentBrushProperty); set => SetValue(PressedAdjacentBrushProperty, value); }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        bool selected = IsSelected && IsEffectivelyEnabled;
        bool pressed = IsPressed && IsEffectivelyEnabled;
        if ((!IsChecked && !selected && !pressed) || Bounds.Width < 2 || Bounds.Height < 2)
        {
            return;
        }

        // A checked System item overrides disabled/pressed and uses Hot when selected.
        // Professional instead uses its pressed fill for every selected checked item.
        bool hot = UseSystemVisualStyle ? selected && (IsChecked || !pressed) : selected && !IsChecked && !pressed;
        IBrush? fill = IsChecked
            ? selected ? UseSystemVisualStyle ? SelectedBrush : PressedBrush : CheckedBrush
            : pressed ? PressedBrush : SelectedBrush;
        Rect frame = new(0, 0, Bounds.Width, Bounds.Height - (UseSystemVisualStyle ? 1 : 0));
        using DrawingContext.PushedState clip = context.PushClip(new Rect(Bounds.Size));
        context.FillRectangle(fill ?? Brushes.Transparent, frame);
        if (UseSystemVisualStyle && frame.Width >= 6 && frame.Height >= 6)
        {
            // TOOLBAR part 1's native96 inner-corner texels, measured independently
            // over black/white and at three item sizes. These are theme image pixels,
            // not changes to the authored item allocation or a rounded Skia border.
            PaintCorners(context, frame.Size,
                hot ? SelectedCornerBrush : PressedCornerBrush,
                hot ? SelectedAdjacentBrush : PressedAdjacentBrush);
        }

        if (BorderBrush is { } border)
        {
            // System shortens ClientBounds.Height before DrawBorderSimple; its last
            // item row stays untouched. Professional frames the full item instead.
            context.FillRectangle(border, new Rect(0, 0, frame.Width, 1));
            context.FillRectangle(border, new Rect(0, frame.Height - 1, frame.Width, 1));
            context.FillRectangle(border, new Rect(0, 0, 1, frame.Height));
            context.FillRectangle(border, new Rect(frame.Width - 1, 0, 1, frame.Height));
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TemplatedParent is not Button owner)
        {
            return;
        }

        _owner = owner;
        owner.PointerEntered += OwnerPointerEntered;
        owner.PointerMoved += OwnerPointerMoved;
        owner.PointerExited += OwnerPointerExited;
        owner.GotFocus += OwnerGotFocus;
        owner.LostFocus += OwnerLostFocus;
        owner.PropertyChanged += OwnerPropertyChanged;

        // ToolStripItem owns a selected bit, not a persistent focus predicate.
        // Store only that bit on its owner so retemplating cannot resurrect a
        // selection which MouseLeave already cleared while keyboard focus stayed.
        bool selected = owner.GetValue(OwnerSelectedProperty)
            ?? (owner.IsPointerOver || (owner.IsFocused && owner.Classes.Contains(":focus-visible")));
        SetOwnerSelected(selected && owner.IsEffectivelyEnabled);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_owner is { } owner)
        {
            owner.PointerEntered -= OwnerPointerEntered;
            owner.PointerMoved -= OwnerPointerMoved;
            owner.PointerExited -= OwnerPointerExited;
            owner.GotFocus -= OwnerGotFocus;
            owner.LostFocus -= OwnerLostFocus;
            owner.PropertyChanged -= OwnerPropertyChanged;
            _owner = null;
            SetCurrentValue(IsSelectedProperty, false);
            if (_owner is null)
            {
                SetCurrentValue(OwnerAllowsPressedPaintProperty, false);
            }
        }

        base.OnDetachedFromVisualTree(e);
    }

    private static void PaintCorners(DrawingContext context, Avalonia.Size size, IBrush? corner, IBrush? adjacent)
    {
        for (int column = 0; column < 2; column++)
        {
            double x = column == 0 ? 1 : size.Width - 2;
            for (int row = 0; row < 2; row++)
            {
                double y = row == 0 ? 1 : size.Height - 2;
                if (corner is not null)
                {
                    context.FillRectangle(corner, new Rect(x, y, 1, 1));
                }

                if (adjacent is not null)
                {
                    context.FillRectangle(adjacent, new Rect(x == 1 ? 2 : x - 1, y, 1, 1));
                    context.FillRectangle(adjacent, new Rect(x, y == 1 ? 2 : y - 1, 1, 1));
                }
            }
        }
    }

    private void OwnerPointerEntered(object? sender, PointerEventArgs e)
        => SetOwnerSelected(_owner?.IsEffectivelyEnabled == true);

    private void OwnerPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_owner is { IsPointerOver: true, IsEffectivelyEnabled: true })
        {
            // The source can select again on a later move after MouseLeave,
            // lost focus or disabling cleared the same shared state bit.
            SetOwnerSelected(true);
        }
    }

    private void OwnerPointerExited(object? sender, PointerEventArgs e) => SetOwnerSelected(false);

    private void OwnerGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (ReferenceEquals(e.NewFocusedElement, _owner) && e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional)
        {
            SetOwnerSelected(_owner?.IsEffectivelyEnabled == true);
        }
    }

    private void OwnerLostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (ReferenceEquals(e.OldFocusedElement, _owner))
        {
            SetOwnerSelected(false);
        }
    }

    private void OwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsEffectivelyEnabledProperty && _owner?.IsEffectivelyEnabled == false)
        {
            SetOwnerSelected(false);
        }
    }

    private void SetOwnerSelected(bool selected)
    {
        if (_owner is not { } owner)
        {
            return;
        }

        // Publishing a value can synchronously retire this template or change
        // the same owner's state. Never revive a retired or superseded painter.
        owner.SetCurrentValue(OwnerSelectedProperty, selected);
        if (ReferenceEquals(_owner, owner) && owner.GetValue(OwnerSelectedProperty) == selected)
        {
            // Avalonia retains physical IsPressed while the pointer is captured.
            // ToolStripItem.HandleLeave clears pressed paint instead; keyboard
            // selection still permits an outside-pointer Space-key press.
            SetCurrentValue(OwnerAllowsPressedPaintProperty, owner.IsPointerOver || selected);
            if (ReferenceEquals(_owner, owner) && owner.GetValue(OwnerSelectedProperty) == selected)
            {
                SetCurrentValue(IsSelectedProperty, selected);
            }
        }
    }
}
