using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Color = Avalonia.Media.Color;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Paints the source flat-button background without treating a Button as a ToolStrip item.
/// </summary>
internal sealed class NativeFlatButtonChrome : Control
{
    public static readonly StyledProperty<bool> UseDarkModeProperty =
        AvaloniaProperty.Register<NativeFlatButtonChrome, bool>(nameof(UseDarkMode));

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<NativeFlatButtonChrome, IBrush?>(nameof(Background));

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<NativeFlatButtonChrome, IBrush?>(nameof(BorderBrush));

    public static readonly StyledProperty<bool> ShowKeyboardFocusProperty =
        AvaloniaProperty.Register<NativeFlatButtonChrome, bool>(nameof(ShowKeyboardFocus));

    public static readonly StyledProperty<IBrush?> FocusBrushProperty =
        AvaloniaProperty.Register<NativeFlatButtonChrome, IBrush?>(nameof(FocusBrush));

    public static readonly StyledProperty<IBrush?> BackdropBrushProperty =
        AvaloniaProperty.Register<NativeFlatButtonChrome, IBrush?>(nameof(BackdropBrush));

    static NativeFlatButtonChrome()
    {
        AffectsRender<NativeFlatButtonChrome>(UseDarkModeProperty, BackgroundProperty, BorderBrushProperty, ShowKeyboardFocusProperty, FocusBrushProperty, BackdropBrushProperty);
    }

    /// <summary>
    ///  Initializes chrome which never participates in input or changes content layout.
    /// </summary>
    public NativeFlatButtonChrome()
    {
        Focusable = false;
        IsHitTestVisible = false;
    }

    /// <summary>
    ///  Gets or sets the source theme's WinForms dark-adapter choice, independently of OS accent.
    /// </summary>
    public bool UseDarkMode
    {
        get => GetValue(UseDarkModeProperty);
        set => SetValue(UseDarkModeProperty, value);
    }

    /// <summary>
    ///  Gets or sets the resolved flat-button fill for the current interaction state.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    ///  Gets or sets the resolved dark flat-button outline for the current interaction state.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    ///  Gets or sets whether the owning button requests its native keyboard-focus cue.
    /// </summary>
    public bool ShowKeyboardFocus
    {
        get => GetValue(ShowKeyboardFocusProperty);
        set => SetValue(ShowKeyboardFocusProperty, value);
    }

    /// <summary>
    ///  Gets or sets the source adapter's resolved focus color, rather than its caption color.
    /// </summary>
    public IBrush? FocusBrush
    {
        get => GetValue(FocusBrushProperty);
        set => SetValue(FocusBrushProperty, value);
    }

    /// <summary>
    ///  Gets or sets the owning panel's source BackColor used by dark fill-edge composition.
    /// </summary>
    public IBrush? BackdropBrush
    {
        get => GetValue(BackdropBrushProperty);
        set => SetValue(BackdropBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        Rect bounds = new(Bounds.Size);
        using DrawingContext.PushedState clip = context.PushClip(bounds);
        if (Background is { } background)
        {
            context.FillRectangle(background, bounds);
            if (UseDarkMode && background is ISolidColorBrush fill && BackdropBrush is ISolidColorBrush backdrop)
            {
                // The source AntiAlias FillRectangle covers half its top/left
                // boundary pixels and a quarter of their shared outer corner.
                // Compose the original opaque Color values explicitly so Skia's
                // eight-bit alpha quantization cannot change the rounded bytes.
                SolidColorBrush edge = new(BlendFillEdge(fill.Color, backdrop.Color, 2));
                SolidColorBrush corner = new(BlendFillEdge(fill.Color, backdrop.Color, 4));
                context.FillRectangle(edge, new Rect(0, 0, bounds.Width, 1));
                context.FillRectangle(edge, new Rect(0, 0, 1, bounds.Height));
                context.FillRectangle(corner, new Rect(0, 0, 1, 1));
            }
        }

        if (UseDarkMode && BorderBrush is { } border && bounds.Width > 3 && bounds.Height > 3)
        {
            // The source path is Inflate(client,-1), with a six-pixel corner
            // diameter. Preserve that path's width/height and map its integer
            // pixel centers to Skia; its right/bottom stroke occupies the last
            // client pixel. Rounded GDI+/Skia antialias raster differences remain
            // distinct from this source-backed geometry.
            Rect outline = new(1.5, 1.5, bounds.Width - 2, bounds.Height - 2);
            context.DrawRectangle(brush: null, new Pen(border, 1), outline, 3, 3);
        }

        if (ShowKeyboardFocus && FocusBrush is { } focus && bounds.Width > 6 && bounds.Height > 6)
        {
            DrawFocus(context, focus, bounds.Size);
        }
    }

    private static Color BlendFillEdge(Color fill, Color backdrop, int divisor) => Color.FromRgb(
        (byte)((fill.R + (backdrop.R * (divisor - 1)) + (divisor / 2)) / divisor),
        (byte)((fill.G + (backdrop.G * (divisor - 1)) + (divisor / 2)) / divisor),
        (byte)((fill.B + (backdrop.B * (divisor - 1)) + (divisor / 2)) / divisor));

    private void DrawFocus(DrawingContext context, IBrush brush, Size size)
    {
        double width = size.Width - 6;
        double height = size.Height - 6;
        double right = size.Width - 4;
        double bottom = size.Height - 4;
        if (!UseDarkMode)
        {
            // ButtonFlatAdapter.DrawFlatFocus uses a solid native GDI rectangle,
            // not the dotted focus cue used by the dark adapter.
            context.FillRectangle(brush, new Rect(3, 3, width, 1));
            context.FillRectangle(brush, new Rect(3, bottom, width, 1));
            context.FillRectangle(brush, new Rect(3, 3, 1, height));
            context.FillRectangle(brush, new Rect(right, 3, 1, height));
            return;
        }

        // ControlPaint draws a transparent/black two-by-two texture here. Its
        // foreColor argument is ignored; the fixed #A0A0A0 focus background picks
        // black. Paint native one-pixel dots so Skia dash endpoints cannot blend
        // half dots or shift the alternating phase at the rectangle's corners.
        for (int x = 0; x < width; x++)
        {
            if ((x & 1) == 0)
            {
                context.FillRectangle(brush, new Rect(3 + x, 3, 1, 1));
            }

            if (((x + ((int)height - 1)) & 1) == 0)
            {
                context.FillRectangle(brush, new Rect(3 + x, bottom, 1, 1));
            }
        }

        for (int y = 0; y < height; y++)
        {
            if ((y & 1) == 0)
            {
                context.FillRectangle(brush, new Rect(3, 3 + y, 1, 1));
            }

            if (((y + ((int)width - 1)) & 1) == 0)
            {
                context.FillRectangle(brush, new Rect(right, 3 + y, 1, 1));
            }
        }
    }
}
