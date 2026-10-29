using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Paints ToolStripRenderer's integer-centred submenu arrow without taking input ownership.
/// </summary>
public sealed class NativeToolStripMenuArrow : Control
{
    private const int ArrowHalfWidth = 2;
    private const int ArrowHalfHeight = 4;

    /// <summary>
    ///  Identifies the renderer's resolved arrow brush.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<NativeToolStripMenuArrow, IBrush?>(nameof(Foreground));

    static NativeToolStripMenuArrow()
    {
        AffectsRender<NativeToolStripMenuArrow>(ForegroundProperty, FlowDirectionProperty);
    }

    /// <summary>
    ///  Initializes paint-only chrome without introducing an input or focus target.
    /// </summary>
    public NativeToolStripMenuArrow()
    {
        IsHitTestVisible = false;
        Focusable = false;
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }

    /// <summary>
    ///  Gets or sets the original renderer's arrow color independently of caption colors.
    /// </summary>
    public IBrush? Foreground { get => GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }

    protected override bool BypassFlowDirectionPolicies => true;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Foreground is null)
        {
            return;
        }

        int x = (int)Bounds.Width / 2;
        int y = (int)Bounds.Height / 2;

        // GDI FillPolygon excludes the two zero-width vertex rows and the right
        // edge. Its integer row runs differ from Skia's antialiased triangle,
        // including for odd rectangles and the mirrored left-facing arrow.
        for (int row = 1 - ArrowHalfHeight; row < ArrowHalfHeight; row++)
        {
            int width = ArrowHalfHeight - Math.Abs(row);
            int left = FlowDirection == FlowDirection.RightToLeft ? x + ArrowHalfWidth - width : x - ArrowHalfWidth;
            context.FillRectangle(Foreground, new Rect(left, y + row, width, 1));
        }
    }
}
