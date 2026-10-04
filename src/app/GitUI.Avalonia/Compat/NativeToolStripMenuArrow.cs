using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Point = Avalonia.Point;

namespace GitUI.Compat;

/// <summary>
///  Paints ToolStripRenderer's integer-centred submenu arrow without taking input ownership.
/// </summary>
public sealed class NativeToolStripMenuArrow : Control
{
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<NativeToolStripMenuArrow, IBrush?>(nameof(Foreground));

    static NativeToolStripMenuArrow()
    {
        AffectsRender<NativeToolStripMenuArrow>(ForegroundProperty, FlowDirectionProperty);
    }

    public NativeToolStripMenuArrow()
    {
        IsHitTestVisible = false;
    }

    public IBrush? Foreground { get => GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }

    protected override bool BypassFlowDirectionPolicies => true;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        int x = (int)Bounds.Width / 2;
        int y = (int)Bounds.Height / 2;
        int direction = FlowDirection == FlowDirection.RightToLeft ? -1 : 1;
        StreamGeometry geometry = new();
        using (StreamGeometryContext path = geometry.Open())
        {
            path.BeginFigure(new Point(x - (2 * direction), y - 4), isFilled: true);
            path.LineTo(new Point(x - (2 * direction), y + 4));
            path.LineTo(new Point(x + (2 * direction), y));
            path.EndFigure(isClosed: true);
        }

        context.DrawGeometry(Foreground, null, geometry);
    }
}
