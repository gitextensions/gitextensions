using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Preserves the native Explorer image-part corner mask without changing label layout.
/// </summary>
internal sealed class NativeTreeSelectionCorners : Control
{
    public static readonly StyledProperty<bool> UseNativeCornersProperty =
        AvaloniaProperty.Register<NativeTreeSelectionCorners, bool>(nameof(UseNativeCorners));

    public static readonly StyledProperty<IBrush?> CornerBrushProperty =
        AvaloniaProperty.Register<NativeTreeSelectionCorners, IBrush?>(nameof(CornerBrush));

    public static readonly StyledProperty<IBrush?> AdjacentEdgeBrushProperty =
        AvaloniaProperty.Register<NativeTreeSelectionCorners, IBrush?>(nameof(AdjacentEdgeBrush));

    public static readonly StyledProperty<IBrush?> InnerCornerBrushProperty =
        AvaloniaProperty.Register<NativeTreeSelectionCorners, IBrush?>(nameof(InnerCornerBrush));

    static NativeTreeSelectionCorners()
    {
        AffectsRender<NativeTreeSelectionCorners>(UseNativeCornersProperty, CornerBrushProperty, AdjacentEdgeBrushProperty, InnerCornerBrushProperty);
    }

    /// <summary>
    ///  Initializes a noninteractive overlay; the ordinary Border retains all content layout.
    /// </summary>
    public NativeTreeSelectionCorners()
    {
        IsHitTestVisible = false;
        Focusable = false;
    }

    /// <summary>
    ///  Gets or sets whether this overlay paints the Explorer selection image mask.
    /// </summary>
    public bool UseNativeCorners
    {
        get => GetValue(UseNativeCornersProperty);
        set => SetValue(UseNativeCornersProperty, value);
    }

    /// <summary>
    ///  Gets or sets the resolved image-part brush at each outer corner.
    /// </summary>
    public IBrush? CornerBrush
    {
        get => GetValue(CornerBrushProperty);
        set => SetValue(CornerBrushProperty, value);
    }

    /// <summary>
    ///  Gets or sets the image-part edge brush next to each outer corner.
    /// </summary>
    public IBrush? AdjacentEdgeBrush
    {
        get => GetValue(AdjacentEdgeBrushProperty);
        set => SetValue(AdjacentEdgeBrushProperty, value);
    }

    /// <summary>
    ///  Gets or sets the image-part fill brush diagonally inside each corner.
    /// </summary>
    public IBrush? InnerCornerBrush
    {
        get => GetValue(InnerCornerBrushProperty);
        set => SetValue(InnerCornerBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!UseNativeCorners || Bounds.Width < 4 || Bounds.Height < 4)
        {
            return;
        }

        // Explorer's native image is mirrored at each corner. Its two-pixel mask is
        // not a rounded-rectangle radius: dark/light and inactive samples differ.
        for (int horizontal = 0; horizontal < 2; horizontal++)
        {
            for (int vertical = 0; vertical < 2; vertical++)
            {
                double x = horizontal == 0 ? 0 : Bounds.Width - 1;
                double y = vertical == 0 ? 0 : Bounds.Height - 1;
                double innerX = horizontal == 0 ? 1 : Bounds.Width - 2;
                double innerY = vertical == 0 ? 1 : Bounds.Height - 2;
                if (CornerBrush is { } corner)
                {
                    context.FillRectangle(corner, new Rect(x, y, 1, 1));
                }

                if (AdjacentEdgeBrush is { } edge)
                {
                    context.FillRectangle(edge, new Rect(innerX, y, 1, 1));
                    context.FillRectangle(edge, new Rect(x, innerY, 1, 1));
                }

                if (InnerCornerBrush is { } inner)
                {
                    context.FillRectangle(inner, new Rect(innerX, innerY, 1, 1));
                }
            }
        }
    }
}
