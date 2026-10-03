using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
/// An Avalonia button whose content remains the original translatable text while an image
/// is presented beside it by the shared visual-parity style.
/// </summary>
public class IconButton : Button
{
    public IconButton()
    {
        Classes.Add("gitextensions-icon-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(Button);
}

/// <summary>
/// A toggle button whose string content retains the original translation key while the
/// shared toolbar template presents only its image.
/// </summary>
public class IconToggleButton : ToggleButton
{
    public IconToggleButton()
    {
        Classes.Add("gitextensions-icon-toggle-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconToggleButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(ToggleButton);
}

/// <summary>
/// An Avalonia split button whose string content keeps the original translation key while
/// the shared template presents the corresponding WinForms toolbar image.
/// </summary>
public class IconSplitButton : NativeToolStripSplitButton
{
    public IconSplitButton()
    {
        Classes.Add("gitextensions-icon-split-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconSplitButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public void ShowDropDown() => OpenFlyout();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FlyoutProperty && Flyout is MenuFlyout { Items.Count: 0, Items.IsReadOnly: false } menu)
        {
            // Avalonia 12.1's presenter unwraps ItemCollection to its current backing list.
            // Initialize that list before Opening populates a dynamic, initially empty menu.
            menu.Items.Clear();
        }
    }

    protected override Type StyleKeyOverride => typeof(SplitButton);
}

/// <summary>
/// Keeps the framework split-button parts while adapting the source ToolStripSplitButton's
/// integer segment allocation and mouse-down drop-down route for explicitly opted-in owners.
/// </summary>
public class NativeToolStripSplitButton : SplitButton
{
    private readonly List<IDisposable?> _partValues = [];
    private Button? _primaryButton;
    private Button? _secondaryButton;
    private Border? _splitter;
    private Grid? _partGrid;
    private NativeToolStripSplitButtonFrame? _systemFrame;
    private IDisposable? _overlayPassThrough;
    private bool _buttonPressed;
    private bool _dropDownButtonPressed;
    private bool _secondaryPointerSequence;
    private long _mouseId;
    private long _openingMouseId = -1;

    /// <summary>Defines whether this owner uses the native ToolStrip split-button adapter.</summary>
    public static readonly StyledProperty<bool> UseNativeToolStripLayoutProperty =
        AvaloniaProperty.Register<NativeToolStripSplitButton, bool>(nameof(UseNativeToolStripLayout));

    public static readonly StyledProperty<bool> UseSystemVisualStyleProperty =
        AvaloniaProperty.Register<NativeToolStripSplitButton, bool>(nameof(UseSystemVisualStyle), true);

    public static readonly StyledProperty<IBrush?> ProfessionalSelectedBrushProperty =
        AvaloniaProperty.Register<NativeToolStripSplitButton, IBrush?>(nameof(ProfessionalSelectedBrush));

    public static readonly StyledProperty<IBrush?> ProfessionalPressedBrushProperty =
        AvaloniaProperty.Register<NativeToolStripSplitButton, IBrush?>(nameof(ProfessionalPressedBrush));

    public static readonly StyledProperty<IBrush?> ProfessionalBorderBrushProperty =
        AvaloniaProperty.Register<NativeToolStripSplitButton, IBrush?>(nameof(ProfessionalBorderBrush));

    public static readonly StyledProperty<IBrush?> ProfessionalSplitterBrushProperty =
        AvaloniaProperty.Register<NativeToolStripSplitButton, IBrush?>(nameof(ProfessionalSplitterBrush));

    public static readonly StyledProperty<IBrush?> ProfessionalOpenBrushProperty =
        AvaloniaProperty.Register<NativeToolStripSplitButton, IBrush?>(nameof(ProfessionalOpenBrush));

    public static readonly StyledProperty<IBrush?> ProfessionalOpenBorderBrushProperty =
        AvaloniaProperty.Register<NativeToolStripSplitButton, IBrush?>(nameof(ProfessionalOpenBorderBrush));

    static NativeToolStripSplitButton()
    {
        AffectsRender<NativeToolStripSplitButton>(UseSystemVisualStyleProperty, ProfessionalSelectedBrushProperty,
            ProfessionalPressedBrushProperty, ProfessionalBorderBrushProperty, ProfessionalSplitterBrushProperty,
            ProfessionalOpenBrushProperty, ProfessionalOpenBorderBrushProperty);
    }

    public NativeToolStripSplitButton()
    {
        AddHandler(PointerPressedEvent, NativePointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, NativePointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <summary>Gets or sets whether the source ToolStrip allocation and input route applies.</summary>
    public bool UseNativeToolStripLayout
    {
        get => GetValue(UseNativeToolStripLayoutProperty);
        set => SetValue(UseNativeToolStripLayoutProperty, value);
    }

    /// <summary>Gets or sets the source System versus Professional renderer boundary.</summary>
    public bool UseSystemVisualStyle
    {
        get => GetValue(UseSystemVisualStyleProperty);
        set => SetValue(UseSystemVisualStyleProperty, value);
    }

    /// <summary>Gets or sets the native ProfessionalColorTable selected fill.</summary>
    public IBrush? ProfessionalSelectedBrush
    {
        get => GetValue(ProfessionalSelectedBrushProperty);
        set => SetValue(ProfessionalSelectedBrushProperty, value);
    }

    /// <summary>Gets or sets the native ProfessionalColorTable primary-pressed fill.</summary>
    public IBrush? ProfessionalPressedBrush
    {
        get => GetValue(ProfessionalPressedBrushProperty);
        set => SetValue(ProfessionalPressedBrushProperty, value);
    }

    /// <summary>Gets or sets the native ProfessionalColorTable selected outline.</summary>
    public IBrush? ProfessionalBorderBrush
    {
        get => GetValue(ProfessionalBorderBrushProperty);
        set => SetValue(ProfessionalBorderBrushProperty, value);
    }

    /// <summary>Gets or sets the source dark Silver or light selected-outline splitter color.</summary>
    public IBrush? ProfessionalSplitterBrush
    {
        get => GetValue(ProfessionalSplitterBrushProperty);
        set => SetValue(ProfessionalSplitterBrushProperty, value);
    }

    /// <summary>Gets or sets the source menu-title vertical gradient for an open drop-down.</summary>
    public IBrush? ProfessionalOpenBrush
    {
        get => GetValue(ProfessionalOpenBrushProperty);
        set => SetValue(ProfessionalOpenBrushProperty, value);
    }

    /// <summary>Gets or sets the native menu border used while the drop-down is open.</summary>
    public IBrush? ProfessionalOpenBorderBrush
    {
        get => GetValue(ProfessionalOpenBorderBrushProperty);
        set => SetValue(ProfessionalOpenBorderBrushProperty, value);
    }

    /// <summary>Gets the source primary segment in this item's coordinate system.</summary>
    public Rect ButtonBounds => GetButtonBounds(Bounds.Size);

    /// <summary>Gets the source one-pixel splitter allocation.</summary>
    public Rect SplitterBounds => GetSplitterBounds(Bounds.Size);

    /// <summary>Gets the source eleven-pixel drop-down segment.</summary>
    public Rect DropDownButtonBounds => GetDropDownButtonBounds(Bounds.Size);

    /// <summary>Gets whether the primary segment is pushed, independently of button kind.</summary>
    public bool ButtonPressed => _buttonPressed;

    /// <summary>Gets whether the native primary is selected; its private button delegates to the owner.</summary>
    public bool ButtonSelected => IsPointerOver || IsFocused || DropDownButtonPressed;

    /// <summary>Gets whether the whole owner is selected, not merely the drop-down half.</summary>
    public bool DropDownButtonSelected => IsPointerOver || IsFocused;

    /// <summary>Gets whether the owned drop-down is open.</summary>
    public bool DropDownButtonPressed => _dropDownButtonPressed;

    protected override Type StyleKeyOverride => typeof(SplitButton);

    protected override bool BypassFlowDirectionPolicies => UseNativeToolStripLayout;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!UseNativeToolStripLayout || UseSystemVisualStyle || !IsEffectivelyEnabled
            || (!ButtonSelected && !ButtonPressed && !DropDownButtonPressed))
        {
            return;
        }

        Rect bounds = new(Bounds.Size);
        if (DropDownButtonPressed)
        {
            // An open source item uses the menu-title gradient/border, not the
            // primary button's pressed color and not an accent-colored Fluent fill.
            context.FillRectangle(ProfessionalOpenBrush ?? Brushes.Transparent, bounds);
            FillOutline(context, ProfessionalOpenBorderBrush, bounds);
        }
        else
        {
            context.FillRectangle(ProfessionalSelectedBrush ?? Brushes.Transparent, bounds);
            FillOutline(context, ProfessionalBorderBrush, bounds);
            if (ButtonPressed)
            {
                Rect primary = ButtonBounds;
                Thickness sourceDeflate = FlowDirection == FlowDirection.RightToLeft
                    ? new Thickness(0, 1, 1, 1) : new Thickness(1, 1, 0, 1);
                context.FillRectangle(ProfessionalPressedBrush ?? Brushes.Transparent, primary.Deflate(sourceDeflate));
            }

            context.FillRectangle(ProfessionalSplitterBrush ?? Brushes.Transparent, SplitterBounds);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_systemFrame is not null)
        {
            _partGrid?.Children.Remove(_systemFrame);
            _systemFrame = null;
        }

        ReleasePartValues();
        base.OnApplyTemplate(e);
        _primaryButton = e.NameScope.Find<Button>("PART_PrimaryButton");
        _secondaryButton = e.NameScope.Find<Button>("PART_SecondaryButton");
        _splitter = e.NameScope.Find<Border>("SeparatorBorder");
        _partGrid = _primaryButton?.GetVisualParent<Grid>();
        if (_partGrid is not null)
        {
            _systemFrame = new NativeToolStripSplitButtonFrame(this);
            Grid.SetColumnSpan(_systemFrame, 3);
            _partGrid.Children.Add(_systemFrame);
        }

        ApplyNativeLayout();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Size result = base.ArrangeOverride(finalSize);
        if (UseNativeToolStripLayout)
        {
            // ToolStripSplitButton.CalculateLayout owns these rectangles, including
            // RTL placement. A Grid's independent Auto column minima cannot own them.
            _primaryButton?.Arrange(GetButtonBounds(finalSize));
            _secondaryButton?.Arrange(GetDropDownButtonBounds(finalSize));
            _splitter?.Arrange(GetSplitterBounds(finalSize));
            _systemFrame?.Arrange(new Rect(finalSize));
        }

        return result;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseNativeToolStripLayoutProperty)
        {
            Classes.Set("gitextensions-native-toolstrip-split-button", UseNativeToolStripLayout);
            Classes.Set("gitextensions-native-professional-toolstrip-split-button", UseNativeToolStripLayout && !UseSystemVisualStyle);
            ApplyNativeLayout();
            ApplyFlyoutInputRoute();
            ResetNativePress();
            InvalidateMeasure();
        }
        else if (change.Property == FlyoutProperty)
        {
            ApplyFlyoutInputRoute();
        }
        else if (change.Property == UseSystemVisualStyleProperty)
        {
            Classes.Set("gitextensions-native-professional-toolstrip-split-button", UseNativeToolStripLayout && !UseSystemVisualStyle);
            _systemFrame?.InvalidateVisual();
        }
        else if (change.Property == FlowDirectionProperty)
        {
            ApplyNativeLayout();
            InvalidateArrange();
        }
        else if (change.Property == IsPointerOverProperty || change.Property == IsFocusedProperty
            || change.Property == IsEnabledProperty || change.Property == IsEffectivelyEnabledProperty)
        {
            if (!IsEffectivelyEnabled)
            {
                ResetNativePress();
            }

            UpdateNativeStates();
        }
        else if (change.Property == BorderBrushProperty)
        {
            _systemFrame?.InvalidateVisual();
        }
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ApplyFlyoutInputRoute();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ResetNativePress();
        _overlayPassThrough?.Dispose();
        _overlayPassThrough = null;
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnFlyoutOpened()
    {
        base.OnFlyoutOpened();
        _dropDownButtonPressed = true;
        UpdateNativeStates();
    }

    protected override void OnFlyoutClosed()
    {
        base.OnFlyoutClosed();
        _dropDownButtonPressed = false;
        UpdateNativeStates();
    }

    protected override void OnClickSecondary(RoutedEventArgs? e)
    {
        if (!UseNativeToolStripLayout || !_secondaryPointerSequence)
        {
            base.OnClickSecondary(e);
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (UseNativeToolStripLayout && !new Rect(Bounds.Size).Contains(e.GetPosition(this)))
        {
            // Native OnMouseLeave clears Push(true) and the opening mouse ID.
            _openingMouseId = -1;
            _buttonPressed = false;
            UpdateNativeStates();
        }
    }

    private void ApplyNativeLayout()
    {
        ReleasePartValues();
        if (!UseNativeToolStripLayout)
        {
            return;
        }

        // The native default is eleven pixels plus a one-pixel splitter, not two
        // independently padded/minimum-width Fluent buttons. Content still uses
        // the framework's original primary presenter and translation API.
        if (_primaryButton is not null)
        {
            _partValues.Add(_primaryButton.SetValue(MinWidthProperty, 0, BindingPriority.Template));
            _partValues.Add(_primaryButton.SetValue(FlowDirectionProperty, FlowDirection, BindingPriority.Template));
        }

        if (_secondaryButton is not null)
        {
            _partValues.Add(_secondaryButton.SetValue(MinWidthProperty, 0, BindingPriority.Template));
            _partValues.Add(_secondaryButton.SetValue(WidthProperty, 11, BindingPriority.Template));
            _partValues.Add(_secondaryButton.SetValue(FlowDirectionProperty, FlowDirection, BindingPriority.Template));
        }

        if (_splitter is not null)
        {
            _partValues.Add(_splitter.SetValue(WidthProperty, 1, BindingPriority.Template));
        }

        if (_partGrid is not null)
        {
            _partValues.Add(_partGrid.SetValue(FlowDirectionProperty, FlowDirection.LeftToRight, BindingPriority.Template));
        }
    }

    private Rect GetButtonBounds(Size size) => new(
        FlowDirection == FlowDirection.RightToLeft ? 12 : 0,
        0,
        Math.Max(0, Math.Max(0, size.Width - Math.Min(size.Width, 11)) - 1),
        Math.Max(0, size.Height));

    private Rect GetSplitterBounds(Size size) => new(
        FlowDirection == FlowDirection.RightToLeft ? Math.Min(size.Width, 11) : GetButtonBounds(size).Right,
        0,
        1,
        Math.Max(0, size.Height));

    private Rect GetDropDownButtonBounds(Size size) => new(
        FlowDirection == FlowDirection.RightToLeft ? 0 : GetButtonBounds(size).Right + 1,
        0,
        Math.Min(size.Width, 11),
        Math.Max(0, size.Height));

    private void ReleasePartValues()
    {
        foreach (IDisposable? value in _partValues)
        {
            value?.Dispose();
        }

        _partValues.Clear();
    }

    private void ApplyFlyoutInputRoute()
    {
        _overlayPassThrough?.Dispose();
        _overlayPassThrough = null;
        if (UseNativeToolStripLayout && Flyout is PopupFlyoutBase popup)
        {
            // Only this source owner remains an input target under its popup. Other
            // outside clicks keep the ordinary dismiss behavior and are not replayed.
            _overlayPassThrough = popup.SetValue(PopupFlyoutBase.OverlayInputPassThroughElementProperty, this, BindingPriority.Template);
        }
    }

    private void NativePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!UseNativeToolStripLayout || !IsEffectivelyEnabled)
        {
            return;
        }

        _mouseId++;
        Point point = e.GetPosition(this);
        bool dropdown = DropDownButtonBounds.Contains(point);
        _secondaryPointerSequence = dropdown;
        bool left = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        if (dropdown && left)
        {
            // WinForms opens on MouseDown and leaves the matching MouseUp alone.
            // Intercept before the framework secondary's release-click/preview close.
            e.Handled = true;
            if (!DropDownButtonPressed)
            {
                _openingMouseId = _mouseId;
                OpenFlyout();
            }
        }
        else
        {
            _buttonPressed = true;
            if (SplitterBounds.Contains(point))
            {
                e.Handled = true;
            }
        }

        UpdateNativeStates();
    }

    private void NativePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!UseNativeToolStripLayout || !IsEffectivelyEnabled)
        {
            return;
        }

        _buttonPressed = false;
        if (_secondaryPointerSequence && e.InitialPressMouseButton == MouseButton.Left)
        {
            e.Handled = true;
            if (DropDownButtonBounds.Contains(e.GetPosition(this)) && DropDownButtonPressed && _mouseId != _openingMouseId)
            {
                CloseFlyout();
            }
        }

        UpdateNativeStates();
    }

    private void ResetNativePress()
    {
        _buttonPressed = false;
        _secondaryPointerSequence = false;
        _openingMouseId = -1;
        UpdateNativeStates();
    }

    private void UpdateNativeStates()
    {
        PseudoClasses.Set(":native-selected", UseNativeToolStripLayout && IsEffectivelyEnabled && ButtonSelected);
        PseudoClasses.Set(":native-button-pressed", UseNativeToolStripLayout && IsEffectivelyEnabled && ButtonPressed);
        PseudoClasses.Set(":native-dropdown-pressed", UseNativeToolStripLayout && IsEffectivelyEnabled && DropDownButtonPressed);
        InvalidateVisual();
        _systemFrame?.InvalidateVisual();
    }

    internal static void FillOutline(DrawingContext context, IBrush? brush, Rect bounds)
    {
        if (brush is not null && bounds.Width >= 1 && bounds.Height >= 1)
        {
            // GDI DrawRectangle uses the last included integer row/column. Filled
            // edge rectangles avoid a half-pixel Pen changing this allocation.
            context.FillRectangle(brush, new Rect(bounds.X, bounds.Y, bounds.Width, 1));
            context.FillRectangle(brush, new Rect(bounds.X, bounds.Bottom - 1, bounds.Width, 1));
            context.FillRectangle(brush, new Rect(bounds.X, bounds.Y, 1, bounds.Height));
            context.FillRectangle(brush, new Rect(bounds.Right - 1, bounds.Y, 1, bounds.Height));
        }
    }
}

/// <summary>
/// Paints the existing System toolbar outline over the framework presenters without
/// making that one-pixel frame part of the source image/caption layout or hit target.
/// The native themed background asset remains a distinct paint boundary.
/// </summary>
internal sealed class NativeToolStripSplitButtonFrame : Control
{
    private readonly NativeToolStripSplitButton _owner;

    public NativeToolStripSplitButtonFrame(NativeToolStripSplitButton owner)
    {
        _owner = owner;
        IsHitTestVisible = false;
        Focusable = false;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_owner.UseNativeToolStripLayout && _owner.UseSystemVisualStyle && _owner.IsEffectivelyEnabled
            && (_owner.ButtonSelected || _owner.ButtonPressed))
        {
            NativeToolStripSplitButton.FillOutline(context, _owner.BorderBrush, new Rect(Bounds.Size));
            if (_owner.BorderBrush is not null)
            {
                context.FillRectangle(_owner.BorderBrush, _owner.SplitterBounds);
            }
        }
    }
}

/// <summary>
/// A drop-down button retaining its original string content for translation while the
/// toolbar presents only the corresponding image.
/// </summary>
public class IconDropDownButton : DropDownButton
{
    public IconDropDownButton()
    {
        Classes.Add("gitextensions-icon-drop-down-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconDropDownButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(DropDownButton);
}

/// <summary>
/// A radio button variant retaining a string Content value for the existing translation
/// adapter while presenting the original WinForms image beside it.
/// </summary>
public class IconRadioButton : RadioButton
{
    public IconRadioButton()
    {
        Classes.Add("gitextensions-icon-radio-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconRadioButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(RadioButton);
}
