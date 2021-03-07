using System.Drawing.Drawing2D;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.UserControls.RevisionGrid.GraphDrawer
{
    internal sealed class SegmentDrawer
    {
        private readonly Graphics _g;
        private readonly Pen _pen;

        private Point? _fromPoint;
        internal SegmentDrawer(Graphics g, Pen pen)
        {
            _g = g;
            _pen = pen;
        }

        internal void DrawTo(Point toPoint)
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

            // Anti-aliasing with bezier & PixelOffsetMode.HighQuality introduces an offset of ~1/8 px - compensate it.
            g.SmoothingMode = SmoothingMode.AntiAlias;
            const float antiAliasOffset = -1f / 8f;
            PointF e0 = new(antiAliasOffset + fromPoint.X, fromPoint.Y);
            PointF e1 = new(antiAliasOffset + toPoint.X, toPoint.Y);

            // bezier curve with perpendicular ends
            float midY = 1f / 2f * (fromPoint.Y + toPoint.Y);
            PointF c0 = new(e0.X, midY);
            PointF c1 = new(e1.X, midY);
            g.DrawBezier(pen, e0, c0, c1, e1);
        }
    }
}
