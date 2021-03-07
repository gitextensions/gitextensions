using System.Drawing.Drawing2D;

namespace GitUI.UserControls.RevisionGrid.Graph.Rendering
{
    internal ref struct SegmentRenderer
    {
        private readonly Graphics _g;
        private readonly Pen _pen;

        private Point? _fromPoint;

        public SegmentRenderer(Graphics g, Pen pen)
        {
            _g = g;
            _pen = pen;
            _fromPoint = null;
        }

        public void DrawTo(Point toPoint)
        {
            try
            {
                if (_fromPoint is null)
                {
                    return;
                }

                DrawTo(_g, _pen, _fromPoint.Value, toPoint);
            }
            finally
            {
                _fromPoint = toPoint;
            }
        }

        private static void DrawTo(Graphics g, Pen pen, Point fromPoint, Point toPoint)
        {
            if (fromPoint.X == toPoint.X)
            {
                // direct line without anti-aliasing
                g.SmoothingMode = SmoothingMode.None;
                g.DrawLine(pen, fromPoint, toPoint);
                return;
            }

            // Anti-aliasing with PixelOffsetMode.HighQuality introduces an offset of ~1/8 px - compensate it.
            g.SmoothingMode = SmoothingMode.AntiAlias;
            const float antiAliasOffset = -1f / 8f;
            PointF e0 = new(antiAliasOffset + fromPoint.X, fromPoint.Y);
            PointF e1 = new(antiAliasOffset + toPoint.X, toPoint.Y);

            // Bezier curve with perpendicular ends
            float midY = 1f / 2f * (fromPoint.Y + toPoint.Y);
            PointF c0 = new(e0.X, midY);
            PointF c1 = new(e1.X, midY);
            g.DrawBezier(pen, e0, c0, c1, e1);
        }
    }
}
