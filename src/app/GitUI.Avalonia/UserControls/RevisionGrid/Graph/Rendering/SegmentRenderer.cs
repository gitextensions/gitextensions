using Avalonia.Media;
using AvaloniaPoint = Avalonia.Point;

namespace GitUI.UserControls.RevisionGrid.Graph.Rendering;

// Twin of GitUI/UserControls/RevisionGrid/Graph/Rendering/SegmentRenderer.cs.
// The geometry math is kept identical to upstream (System.Drawing primitives); only the
// draw calls target Avalonia's DrawingContext. GDI's SmoothingMode switches and its
// anti-alias offset compensation do not apply to Avalonia and are dropped.
internal ref struct SegmentRenderer
{
    public readonly int RowHeight => _context.CellSize.Height;

    private readonly Context _context;

    private bool _fromPerpendicularly = true;
    private Point? _fromPoint = null;

    public SegmentRenderer(in Context context)
    {
        _context = context;
    }

    public void DrawTo(in int x, in int y, in bool toPerpendicularly = true)
        => DrawTo(new Point(x, y), toPerpendicularly);

    public void DrawTo(in Point toPoint, in bool toPerpendicularly = true)
    {
        try
        {
            if (_fromPoint is null)
            {
                return;
            }

            DrawTo(_fromPoint.Value, toPoint, _fromPerpendicularly, toPerpendicularly, _context);
        }
        finally
        {
            _fromPoint = toPoint;
            _fromPerpendicularly = toPerpendicularly;
        }
    }

    private static void DrawTo(in Point fromPoint, in Point toPoint, in bool fromPerpendicularly, in bool toPerpendicularly, in Context context)
    {
        DrawingContext g = context.DrawingContext;
        Pen pen = context.Pen;

        if (fromPoint.X == toPoint.X)
        {
            // Direct line
            DrawLine(fromPoint, toPoint);
            return;
        }

        PointF e0 = new(fromPoint.X, fromPoint.Y);
        PointF e1 = new(toPoint.X, toPoint.Y);

        int height = toPoint.Y - fromPoint.Y;
        int width = toPoint.X - fromPoint.X;
        bool singleLane = Math.Abs(width) <= context.CellSize.Width;
        Size cellShift = new(Math.Sign(width) * context.CellSize.Width, context.CellSize.Height);

        if (!fromPerpendicularly && !toPerpendicularly && singleLane)
        {
            // Direct line
            DrawLine(e0, e1);
            return;
        }

        // Control points for Bezier curve
        PointF c0 = e0;
        PointF c1 = e1;

        const float diagonalFractionCurve = 1f / 4f;
        const float perpendicularFraction = diagonalFractionCurve;
        float perpendicularOffset = perpendicularFraction * cellShift.Height;

        if (fromPerpendicularly && toPerpendicularly)
        {
            if (context.Config.RenderGraphWithDiagonals && singleLane)
            {
                c0.Y += perpendicularOffset;
                c1.Y -= perpendicularOffset;

                PointF mid = new(1f / 2f * (e0.X + e1.X), 1f / 2f * (e0.Y + e1.Y));
                SizeF shift = diagonalFractionCurve * cellShift;
                DrawBezier(e0, c0, mid - shift, mid);
                DrawBezier(e1, c1, mid + shift, mid);
                return;
            }

            c0.Y = c1.Y = 1f / 2f * (fromPoint.Y + toPoint.Y);
        }
        else
        {
            // Is the end of a diagonal
            if (singleLane)
            {
                float diagonalFractionStraight = height < cellShift.Height ? 2f / 5f : 1f / 2f;

                if (fromPerpendicularly)
                {
                    MoveDrawDiagonallyFrom(ref e1, out _, -diagonalFractionStraight);

                    // Prepare remaining curve
                    c1 = e1 - (diagonalFractionCurve * cellShift);
                    c0.Y += perpendicularOffset;
                }
                else
                {
                    MoveDrawDiagonallyFrom(ref e0, out _, +diagonalFractionStraight);

                    // Prepare remaining curve
                    c0 = e0 + (diagonalFractionCurve * cellShift);
                    c1.Y -= perpendicularOffset;
                }
            }

            // Is a multi-lane crossing
            else
            {
                float diagonalFractionStraight = 1f / 6f;

                if (fromPerpendicularly)
                {
                    c0.Y += perpendicularOffset;
                }
                else
                {
                    MoveDrawDiagonallyFrom(ref e0, out c0, +diagonalFractionStraight);
                }

                if (toPerpendicularly)
                {
                    c1.Y -= perpendicularOffset;
                }
                else
                {
                    MoveDrawDiagonallyFrom(ref e1, out c1, -diagonalFractionStraight);
                }
            }
        }

        DrawBezier(e0, c0, c1, e1);

        return;

        void DrawBezier(in PointF e0, in PointF c0, in PointF c1, in PointF e1)
        {
            StreamGeometry geometry = new();
            using (StreamGeometryContext geometryContext = geometry.Open())
            {
                geometryContext.BeginFigure(ToAvalonia(e0), isFilled: false);
                geometryContext.CubicBezierTo(ToAvalonia(c0), ToAvalonia(c1), ToAvalonia(e1));
                geometryContext.EndFigure(isClosed: false);
            }

            g.DrawGeometry(brush: null, pen, geometry);
        }

        void DrawLine(in PointF from, in PointF to)
        {
            g.DrawLine(pen, ToAvalonia(from), ToAvalonia(to));
        }

        void MoveDrawDiagonallyFrom(ref PointF start, out PointF bezierCenter, in float fractionOfCell)
        {
            SizeF shift = fractionOfCell * cellShift;
            PointF end = start + shift;
            DrawLine(start, end);

            start = end;
            bezierCenter = end + shift;
        }

        static AvaloniaPoint ToAvalonia(in PointF point)
        {
            return new AvaloniaPoint(point.X, point.Y);
        }
    }
}
