using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
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
    private IDisposable? _nativeContentTemplateValue;
    private IDataTemplate? _nativeContentTemplate;

    public IconSplitButton()
    {
        Classes.Add("gitextensions-icon-split-button");
    }

    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconSplitButton, IImage?>(nameof(Icon));

    public static readonly StyledProperty<HorizontalAlignment> ImageAlignProperty =
        AvaloniaProperty.Register<IconSplitButton, HorizontalAlignment>(nameof(ImageAlign), HorizontalAlignment.Center);

    public static readonly StyledProperty<HorizontalAlignment> TextAlignProperty =
        AvaloniaProperty.Register<IconSplitButton, HorizontalAlignment>(nameof(TextAlign), HorizontalAlignment.Center);

    static IconSplitButton()
    {
        AffectsMeasure<IconSplitButton>(IconProperty, ImageAlignProperty, TextAlignProperty);
    }

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Gets or sets the horizontal part of the source middle image alignment.</summary>
    public HorizontalAlignment ImageAlign
    {
        get => GetValue(ImageAlignProperty);
        set => SetValue(ImageAlignProperty, value);
    }

    /// <summary>Gets or sets the horizontal part of the source middle caption alignment.</summary>
    public HorizontalAlignment TextAlign
    {
        get => GetValue(TextAlignProperty);
        set => SetValue(TextAlignProperty, value);
    }

    public void ShowDropDown() => OpenFlyout();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        UpdateNativeContentTemplate(resetScope: true);
        base.OnApplyTemplate(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateNativeContentTemplate();
        Size result = base.MeasureOverride(availableSize);
        return _nativeContentTemplate is not null && ReferenceEquals(ContentTemplate, _nativeContentTemplate)
            ? NativeToolStripSplitContent.GetPreferredSize(this) + new Size(12, 0)
            : result;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseNativeToolStripLayoutProperty || change.Property == ContentProperty)
        {
            UpdateNativeContentTemplate();
        }

        if (change.Property == ImageAlignProperty || change.Property == TextAlignProperty
            || change.Property == IconProperty || change.Property == ContentProperty
            || change.Property == FontFamilyProperty || change.Property == FontStyleProperty
            || change.Property == FontWeightProperty || change.Property == FontSizeProperty)
        {
            InvalidateMeasure();
        }

        if (change.Property == FlyoutProperty && Flyout is MenuFlyout { Items.Count: 0, Items.IsReadOnly: false } menu)
        {
            // Avalonia 12.1's presenter unwraps ItemCollection to its current backing list.
            // Initialize that list before Opening populates a dynamic, initially empty menu.
            menu.Items.Clear();
        }
    }

    protected override Type StyleKeyOverride => typeof(SplitButton);

    private void UpdateNativeContentTemplate(bool resetScope = false)
    {
        bool nativeContent = UseNativeToolStripLayout && Content is null or string;
        if (nativeContent && (_nativeContentTemplateValue is null || resetScope))
        {
            // Only source-shaped string content opts into CommonLayoutOptions.
            // Class-selected templates have StyleTrigger priority, so Template priority
            // cannot replace their old four-pixel-gap presenter. A disposable trigger
            // scope still leaves authored local templates and bindings in control.
            _nativeContentTemplateValue?.Dispose();
            _nativeContentTemplate ??= new FuncDataTemplate(_ => true, (_, _) => new NativeToolStripSplitContent(this));
            _nativeContentTemplateValue = SetValue(ContentTemplateProperty, _nativeContentTemplate, BindingPriority.StyleTrigger);
        }
        else if (!nativeContent && _nativeContentTemplateValue is not null)
        {
            _nativeContentTemplateValue.Dispose();
            _nativeContentTemplateValue = null;
            _nativeContentTemplate = null;
        }
    }
}

/// <summary>
/// Applies the source horizontal ImageBeforeText CommonLayoutOptions to opted-in toolbar content.
/// </summary>
internal sealed class NativeToolStripSplitContent : Panel
{
    private readonly IconSplitButton _owner;
    private readonly Image _image = new() { Stretch = Stretch.Fill, IsHitTestVisible = false };
    private readonly TextBlock _caption;

    public NativeToolStripSplitContent(IconSplitButton owner)
    {
        _owner = owner;
        _caption = TranslationCompat.GetConvertMnemonics(owner) ? new AccessText() : new TextBlock();
        _caption.IsHitTestVisible = false;
        _caption.ClipToBounds = true;
        Children.Add(_image);
        Children.Add(_caption);
        FlowDirection = FlowDirection.LeftToRight;
        ClipToBounds = true;
    }

    protected override bool BypassFlowDirectionPolicies => true;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _owner.PropertyChanged += OwnerPropertyChanged;
        _owner.Classes.CollectionChanged += OwnerClassesChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _owner.PropertyChanged -= OwnerPropertyChanged;
        _owner.Classes.CollectionChanged -= OwnerClassesChanged;
        base.OnDetachedFromVisualTree(e);
    }

    internal static Size GetPreferredSize(IconSplitButton owner)
    {
        Size text = MeasureCaption(owner);
        int image = owner.Icon is null ? 0 : 16;

        // CommonLayoutOptions: BorderSize=2, PaddingSize=0 and TextImageInset=0.
        return new Size(image + text.Width + 4, Math.Max(image, text.Height) + 4);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _image.Source = _owner.Icon;
        _image.IsVisible = _owner.Icon is not null;
        _caption.Text = _owner.Content as string ?? string.Empty;
        _caption.IsVisible = !_owner.Classes.Contains("gitextensions-icon-only") && !string.IsNullOrEmpty(_caption.Text);
        _caption.FontFamily = _owner.FontFamily;
        _caption.FontStyle = _owner.FontStyle;
        _caption.FontWeight = _owner.FontWeight;
        _caption.FontSize = _owner.FontSize;
        _caption.Foreground = _owner.Foreground;
        _caption.FlowDirection = _owner.FlowDirection;
        _caption.TextAlignment = _owner.TextAlign switch
        {
            HorizontalAlignment.Left => TextAlignment.Left,
            HorizontalAlignment.Right => TextAlignment.Right,
            _ => TextAlignment.Center,
        };
        _caption.Padding = WinFormsTextMeasurer.GetTextRendererPadding(_caption);
        _image.Measure(new Size(16, 16));
        _caption.Measure(MeasureCaption(_owner));
        return GetPreferredSize(_owner);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // The primary button owns a separate client. Native layout intersects the
        // resulting text/image rectangles with this two-pixel-inset field.
        System.Drawing.Rectangle field = new(2, 2,
            Math.Max(0, (int)finalSize.Width - 4), Math.Max(0, (int)finalSize.Height - 4));
        System.Drawing.Size imageSize = _owner.Icon is null ? default : new(16, 16);
        Size measured = MeasureCaption(_owner);
        System.Drawing.Size textSize = new((int)measured.Width, (int)measured.Height);
        HorizontalAlignment imageAlign = Translate(_owner.ImageAlign);
        HorizontalAlignment textAlign = Translate(_owner.TextAlign);
        bool rightToLeft = _owner.FlowDirection == FlowDirection.RightToLeft;
        System.Drawing.Rectangle image;
        System.Drawing.Rectangle text;
        if (imageSize.IsEmpty || textSize.IsEmpty)
        {
            image = Align(imageSize, field, imageAlign);
            text = Align(textSize, field, textAlign);
        }
        else
        {
            int combinedWidth = imageSize.Width + textSize.Width;
            System.Drawing.Rectangle maximum = new(field.Location,
                new System.Drawing.Size(Math.Max(field.Width, combinedWidth), Math.Max(field.Height, Math.Max(imageSize.Height, textSize.Height))));
            int boundary;
            bool imageEdge = imageAlign == (rightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left);
            bool textEdge = textAlign == (rightToLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right);
            if (imageEdge)
            {
                boundary = rightToLeft ? maximum.Right - imageSize.Width : maximum.Left + imageSize.Width;
            }
            else if (textEdge)
            {
                boundary = rightToLeft ? maximum.Left + textSize.Width : maximum.Right - textSize.Width;
            }
            else
            {
                // Split the centered combination, then expand its adjacent regions
                // to the field before independently aligning image and caption.
                boundary = maximum.Left + ((maximum.Width - combinedWidth) / 2)
                    + (rightToLeft ? textSize.Width : imageSize.Width);
            }

            System.Drawing.Rectangle leading = new(maximum.Left, maximum.Top, boundary - maximum.Left, maximum.Height);
            System.Drawing.Rectangle trailing = new(boundary, maximum.Top, maximum.Right - boundary, maximum.Height);
            image = Align(imageSize, rightToLeft ? trailing : leading, imageAlign);
            text = Align(textSize, rightToLeft ? leading : trailing, textAlign);
        }

        int textBottom = Math.Min(text.Bottom, field.Bottom);
        text.Y = Math.Max(Math.Min(text.Y, field.Y + ((field.Height - text.Height) / 2)), field.Y);
        text.Height = Math.Max(0, textBottom - text.Y);
        if (!rightToLeft && image.Width != 0)
        {
            // Preserve the source ImageBeforeText squeeze, including its asymmetric
            // TextBeforeImage RTL path; do not insert an invented inter-item gap.
            image.Width = Math.Max(0, Math.Min(field.Width - text.Width, image.Width));
            text.X = image.X + image.Width;
        }

        image = System.Drawing.Rectangle.Intersect(image, field);
        text = System.Drawing.Rectangle.Intersect(text, field);
        _image.Arrange(ToRect(image));
        _caption.Arrange(ToRect(text));
        return finalSize;
    }

    private HorizontalAlignment Translate(HorizontalAlignment alignment)
        => _owner.FlowDirection != FlowDirection.RightToLeft ? alignment : alignment switch
        {
            HorizontalAlignment.Left => HorizontalAlignment.Right,
            HorizontalAlignment.Right => HorizontalAlignment.Left,
            _ => alignment,
        };

    private void OwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentControl.ContentProperty || e.Property == IconSplitButton.IconProperty
            || e.Property == IconSplitButton.ImageAlignProperty || e.Property == IconSplitButton.TextAlignProperty
            || e.Property == TemplatedControl.FontFamilyProperty || e.Property == TemplatedControl.FontSizeProperty
            || e.Property == TemplatedControl.FontStyleProperty || e.Property == TemplatedControl.FontWeightProperty
            || e.Property == TemplatedControl.ForegroundProperty || e.Property == Visual.FlowDirectionProperty)
        {
            InvalidateMeasure();
        }
    }

    private void OwnerClassesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        InvalidateMeasure();
        _owner.InvalidateMeasure();
    }

    private static Size MeasureCaption(IconSplitButton owner)
    {
        if (owner.Classes.Contains("gitextensions-icon-only") || owner.Content is not string text || text.Length == 0)
        {
            return default;
        }

        string display = TranslationCompat.GetConvertMnemonics(owner) ? AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(text) : text;
        Size measured = WinFormsTextMeasurer.MeasureTextRenderer(owner, display.Replace("&", "&&", StringComparison.Ordinal));
        return new Size(Math.Ceiling(measured.Width), Math.Ceiling(measured.Height));
    }

    private static System.Drawing.Rectangle Align(System.Drawing.Size size, System.Drawing.Rectangle region, HorizontalAlignment alignment)
    {
        int horizontalOffset = alignment switch
        {
            HorizontalAlignment.Left => 0,
            HorizontalAlignment.Right => region.Width - size.Width,
            _ => (region.Width - size.Width) / 2,
        };
        return new System.Drawing.Rectangle(region.X + horizontalOffset,
            region.Y + ((region.Height - size.Height) / 2), size.Width, size.Height);
    }

    private static Rect ToRect(System.Drawing.Rectangle rectangle)
        => new(rectangle.X, rectangle.Y, Math.Max(0, rectangle.Width), Math.Max(0, rectangle.Height));
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
            if (this is IconSplitButton)
            {
                _partValues.Add(_primaryButton.SetValue(PaddingProperty, default(Thickness), BindingPriority.Template));
                _partValues.Add(_primaryButton.SetValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch, BindingPriority.Template));
                _partValues.Add(_primaryButton.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Stretch, BindingPriority.Template));
            }
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
