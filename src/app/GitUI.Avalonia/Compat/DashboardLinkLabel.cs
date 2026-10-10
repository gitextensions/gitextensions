using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Adapts Dashboard's native LinkLabel caption regions without changing its button command route.
/// </summary>
internal sealed class DashboardLinkLabel : WinFormsControls.LinkLabel
{
    private const uint GetMouseHoverWidth = 0x0062;
    private const uint GetMouseHoverHeight = 0x0064;
    private const uint GetMouseHoverTime = 0x0066;
    private static readonly DashStyle DottedLine = new([1, 1], 0);
    private static readonly Cursor ArrowCursor = new(StandardCursorType.Arrow);
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);
    private readonly DispatcherTimer _hoverTimer;
    private readonly Size _hoverSize;
    private Point _hoverOrigin;
    private bool _captionPressed;
    private bool _hoverRaised;

    public DashboardLinkLabel()
    {
        // WinForms MouseHover uses the OS tracking rectangle/time, not immediate pointerover.
        // Avalonia has no hover event; retain that timing with the native default off Windows.
        _hoverSize = new Size(GetHoverSetting(GetMouseHoverWidth, 4), GetHoverSetting(GetMouseHoverHeight, 4));
        _hoverTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(GetHoverSetting(GetMouseHoverTime, 400)),
        };
        _hoverTimer.Tick += (_, _) =>
        {
            _hoverTimer.Stop();
            if (IsPointerOver && IsEffectivelyEnabled)
            {
                _hoverRaised = true;
                PseudoClasses.Set(":native-mousehover", true);
            }
        };
        FocusAdorner = null;
        ClipToBounds = true;
    }

    internal TimeSpan HoverDelay => _hoverTimer.Interval;

    internal Rect CaptionHitBounds => GetCaptionBounds(includeOverhang: false);

    internal Rect CaptionFocusBounds => GetCaptionBounds(includeOverhang: true);

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _hoverOrigin = e.GetPosition(this);
        _hoverRaised = false;
        _hoverTimer.Start();
        UpdateCaptionCursor(_hoverOrigin);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        Point position = e.GetPosition(this);
        if (e.Pointer.Captured == this && !new Rect(Bounds.Size).Contains(position))
        {
            ResetPointerState();
            return;
        }

        UpdateCaptionCursor(position);
        if (!_hoverRaised
            && (Math.Abs(position.X - _hoverOrigin.X) >= _hoverSize.Width / 2
                || Math.Abs(position.Y - _hoverOrigin.Y) >= _hoverSize.Height / 2))
        {
            _hoverOrigin = position;
            _hoverTimer.Stop();
            _hoverTimer.Start();
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        // Avalonia clears pointer-over when a captured parent differs from its hit-tested
        // caption/image child. Native MouseLeave means leaving the whole label's HWND.
        if (e.Pointer.Captured == this && new Rect(Bounds.Size).Contains(e.GetPosition(this)))
        {
            return;
        }

        ResetPointerState();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(this).Properties;
        _captionPressed = IsEffectivelyEnabled && e.ClickCount <= 1
            && (properties.IsLeftButtonPressed || properties.IsRightButtonPressed || properties.IsMiddleButtonPressed)
            && CaptionHitBounds.Contains(e.GetPosition(this));
        base.OnPointerPressed(e);
        if (_captionPressed)
        {
            // Native LinkLabel activates/focuses its caption for any mouse button.
            // Its Dashboard Click handler still follows Button's left-button route.
            Focus(NavigationMethod.Pointer);
            e.Pointer.Capture(this);
        }

        PseudoClasses.Set(":native-link-active", _captionPressed);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _captionPressed = false;
        PseudoClasses.Set(":native-link-active", false);
        if (e.Pointer.Captured == this)
        {
            e.Pointer.Capture(null);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _captionPressed = false;
        PseudoClasses.Set(":native-link-active", false);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ResetPointerState();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsEffectivelyEnabledProperty && !IsEffectivelyEnabled)
        {
            ResetPointerState();
        }

        if (change.Property == IsFocusedProperty)
        {
            InvalidateVisual();
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Size arranged = base.ArrangeOverride(finalSize);
        if (Content is Grid grid && grid.TranslatePoint(default, this) is { } contentOrigin)
        {
            if (grid.Children.OfType<Image>().SingleOrDefault() is { } image)
            {
                // Label.CalcImageRenderBounds uses the whole client and integer middle alignment,
                // independently of the caption's Padding. Grid centering would add half a pixel.
                image.Arrange(new Rect(2 - contentOrigin.X,
                    Math.Floor((finalSize.Height - image.Height) / 2) - contentOrigin.Y,
                    image.Width, image.Height));
            }

            if (grid.Children.OfType<TextBlock>().SingleOrDefault() is { } caption)
            {
                // Label.OnPaint centers TextRenderer's integral run inside the padded client.
                // Centering Skia's fractional DesiredSize instead shifts the native paint origin.
                Size text = WinFormsTextMeasurer.MeasureTextRenderer(caption, caption.Text ?? string.Empty);
                double clientHeight = Math.Max(0, finalSize.Height - Padding.Top - Padding.Bottom);
                caption.Arrange(new Rect(Padding.Left - contentOrigin.X,
                    Padding.Top + Math.Max(0, Math.Floor((clientHeight - text.Height) / 2)) - contentOrigin.Y,
                    Math.Max(0, finalSize.Width - Padding.Left - Padding.Right), text.Height));
            }
        }

        return arranged;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!IsFocused || !Classes.Contains(":focus-visible"))
        {
            return;
        }

        // Native draws focus around the padded text run, excluding the image and outer label.
        Rect bounds = CaptionFocusBounds;
        if (bounds.Width > 1 && bounds.Height > 1
            && this.TryFindResource("GitExtensionsKnownColorControlTextBrush", out object? value)
            && value is IBrush stroke)
        {
            context.DrawRectangle(null, new Pen(stroke, 1, DottedLine),
                new Rect(bounds.X + 0.5, bounds.Y + 0.5, bounds.Width - 1, bounds.Height - 1));
        }
    }

    private static uint GetHoverSetting(uint setting, uint fallback)
        => OperatingSystem.IsWindows() && SystemParametersInfo(setting, 0, out uint value, 0) && value > 0
            ? value : fallback;

    private Rect GetCaptionBounds(bool includeOverhang)
    {
        if (Content is not Grid grid || grid.Children.OfType<TextBlock>().SingleOrDefault() is not { } caption)
        {
            return default;
        }

        Size text = WinFormsTextMeasurer.MeasureTextRenderer(caption, caption.Text ?? string.Empty);
        Thickness overhang = WinFormsTextMeasurer.GetTextRendererPadding(caption);
        double width = includeOverhang ? text.Width : Math.Max(0, text.Width - overhang.Left - overhang.Right);
        double clientWidth = Math.Max(0, Bounds.Width - Padding.Left - Padding.Right);
        double clientHeight = Math.Max(0, Bounds.Height - Padding.Top - Padding.Bottom);

        // LinkLabel.CalcTextRenderBounds centers within the deflated client height without
        // adding its Y origin. This is its actual hit/focus region, not the text paint origin.
        double y = text.Height > clientHeight ? Padding.Top : Math.Floor((clientHeight - text.Height) / 2);
        return new Rect(Padding.Left, y, Math.Min(width, clientWidth), Math.Min(text.Height, clientHeight));
    }

    private void ResetPointerState()
    {
        _hoverTimer.Stop();
        _hoverRaised = false;
        _captionPressed = false;
        PseudoClasses.Set(":native-mousehover", false);
        PseudoClasses.Set(":native-link-active", false);
        Cursor = ArrowCursor;
    }

    private void UpdateCaptionCursor(Point point)
        => Cursor = IsEffectivelyEnabled && CaptionHitBounds.Contains(point) ? HandCursor : ArrowCursor;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint action, uint parameter, out uint value, uint flags);
}
