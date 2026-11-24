using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Paints the Professional dropdown separator inside its unchanged source allocation.
/// </summary>
public sealed class NativeMenuSeparatorChrome : Control
{
    /// <summary>
    ///  Identifies ProfessionalColorTable.SeparatorDark's resolved native color.
    /// </summary>
    public static readonly StyledProperty<IBrush?> DarkBrushProperty =
        AvaloniaProperty.Register<NativeMenuSeparatorChrome, IBrush?>(nameof(DarkBrush));

    static NativeMenuSeparatorChrome()
    {
        AffectsRender<NativeMenuSeparatorChrome>(DarkBrushProperty);
    }

    /// <summary>
    ///  Initializes non-interactive renderer chrome without introducing another item.
    /// </summary>
    public NativeMenuSeparatorChrome()
    {
        IsHitTestVisible = false;
        Focusable = false;
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }

    /// <summary>
    ///  Gets or sets the native separator pen's resolved brush.
    /// </summary>
    public IBrush? DarkBrush
    {
        get => GetValue(DarkBrushProperty);
        set => SetValue(DarkBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // ToolStripProfessionalRenderer uses integer height / 2 for dropdowns and
        // omits the toolbar's second light pen. Centering a one-DIP child instead
        // places it between raster rows when the original item height is even.
        if (DarkBrush is not null && Bounds.Width > 0 && Bounds.Height >= 1)
        {
            context.FillRectangle(DarkBrush, new Rect(0, (int)Bounds.Height / 2, Bounds.Width, 1));
        }
    }
}
