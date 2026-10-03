using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Paints a native vertical toolbar separator without changing its item's allocation.
/// </summary>
internal sealed class NativeToolStripSeparatorChrome : Control
{
    public static readonly StyledProperty<bool> UseSystemVisualStyleProperty =
        AvaloniaProperty.Register<NativeToolStripSeparatorChrome, bool>(nameof(UseSystemVisualStyle));

    public static readonly StyledProperty<IBrush?> DarkBrushProperty =
        AvaloniaProperty.Register<NativeToolStripSeparatorChrome, IBrush?>(nameof(DarkBrush));

    public static readonly StyledProperty<IBrush?> LightBrushProperty =
        AvaloniaProperty.Register<NativeToolStripSeparatorChrome, IBrush?>(nameof(LightBrush));

    private static readonly IBrush SystemShadow = Brush.Parse("#73000000");
    private static readonly IBrush SystemHighlight = Brush.Parse("#4DFFFFFF");

    static NativeToolStripSeparatorChrome()
    {
        AffectsRender<NativeToolStripSeparatorChrome>(UseSystemVisualStyleProperty, DarkBrushProperty, LightBrushProperty, FlowDirectionProperty);
    }

    /// <summary>
    ///  Initializes renderer-only chrome which cannot intercept toolbar input or focus.
    /// </summary>
    public NativeToolStripSeparatorChrome()
    {
        IsHitTestVisible = false;
        Focusable = false;
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }

    /// <summary>
    ///  Gets or sets the source ThemeFix choice between system and professional rendering.
    /// </summary>
    public bool UseSystemVisualStyle
    {
        get => GetValue(UseSystemVisualStyleProperty);
        set => SetValue(UseSystemVisualStyleProperty, value);
    }

    /// <summary>
    ///  Gets or sets ProfessionalColorTable.SeparatorDark's resolved native color.
    /// </summary>
    public IBrush? DarkBrush
    {
        get => GetValue(DarkBrushProperty);
        set => SetValue(DarkBrushProperty, value);
    }

    /// <summary>
    ///  Gets or sets ProfessionalColorTable.SeparatorLight's resolved native color.
    /// </summary>
    public IBrush? LightBrush
    {
        get => GetValue(LightBrushProperty);
        set => SetValue(LightBrushProperty, value);
    }

    // Native RTL swaps the two professional pens, not their item-relative coordinates. Avoid a
    // second geometric mirror by Avalonia after applying that source renderer rule.
    protected override bool BypassFlowDirectionPolicies => true;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        int width = (int)Bounds.Width;
        int height = (int)Bounds.Height;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        using DrawingContext.PushedState clip = context.PushClip(new Rect(Bounds.Size));

        if (UseSystemVisualStyle)
        {
            RenderSystemSeparator(context, width, height);
            return;
        }

        // ToolStripProfessionalRenderer.RenderSeparatorInternal first trims three
        // pixels from either end of a toolbar item (not a popup separator), then
        // inflates by -2 when the remaining height is at least four. GDI DrawLine
        // includes both integer endpoints; a centered one-DIP Border cannot retain
        // either those lengths or the native etched pair in a six-DIP allocation.
        int top = 3;
        int lineHeight = Math.Max(0, height - 6);
        if (lineHeight >= 4)
        {
            top += 2;
            lineHeight -= 4;
        }

        bool rightToLeft = FlowDirection == FlowDirection.RightToLeft;
        int x = width / 2;
        DrawVerticalLine(context, rightToLeft ? LightBrush : DarkBrush, x, top, top + lineHeight - 1);
        DrawVerticalLine(context, rightToLeft ? DarkBrush : LightBrush, x + 1, top + 1, top + lineHeight);
    }

    private static void RenderSystemSeparator(DrawingContext context, int width, int height)
    {
        // TOOLBAR part 5's native GetThemeBitmap is a 6x5 premultiplied image:
        // its only ink is row 2, columns 2/3, black alpha 115 / white alpha 77.
        // The native Stretch sizing margins are 4,1,2,2. Preserve the left cap
        // instead of centering the lines, and stretch only the one-row center.
        // The queried four-pixel item compresses that cap by one; source Browse
        // allocates six pixels. Narrower-than-four items have no verified contract.
        if (width < 4 || height <= 4)
        {
            return;
        }

        int left = width == 4 ? 1 : 2;
        context.FillRectangle(SystemShadow, new Rect(left, 2, 1, height - 4));
        context.FillRectangle(SystemHighlight, new Rect(left + 1, 2, 1, height - 4));
    }

    private static void DrawVerticalLine(DrawingContext context, IBrush? brush, int x, int first, int last)
    {
        // GDI+ omits a zero-length DrawLine with the source pen's flat end caps.
        // A one-pixel FillRectangle here would invent ink on an eleven-pixel item.
        if (brush is not null && first != last)
        {
            context.FillRectangle(brush, new Rect(x, Math.Min(first, last), 1, Math.Abs(last - first) + 1));
        }
    }
}
