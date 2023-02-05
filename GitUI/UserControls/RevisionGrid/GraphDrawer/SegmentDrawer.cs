using System.Drawing.Drawing2D;

namespace GitUI.UserControls.RevisionGrid.GraphDrawer
{
    internal sealed class SegmentDrawer
    {
        internal int RowHeight { get; init; }
        private readonly int _laneWidth;
        private readonly Graphics _g;
        private readonly Pen _pen;

        private Point? _fromPoint;
        private bool _fromPerpendicularly = true;

        internal SegmentDrawer(Graphics g, Pen pen, int laneWidth, int rowHeight)
        {
            _g = g;
            _pen = pen;
            _laneWidth = laneWidth;
            RowHeight = rowHeight;
        }

        internal void DrawTo(int x, int y, bool toPerpendicularly = true)
            => DrawTo(new Point(x, y), toPerpendicularly);

        internal void DrawTo(Point toPoint, bool toPerpendicularly = true)
        {
            try
            {
                if (_fromPoint is null)
                {
                    return;
                }

                DrawTo(_g, _pen, _fromPoint.Value, toPoint, _fromPerpendicularly, toPerpendicularly, _laneWidth, RowHeight);
            }
            finally
            {
                _fromPoint = toPoint;
                _fromPerpendicularly = toPerpendicularly;
            }
        }

        private static void DrawTo(Graphics g, Pen pen, Point fromPoint, Point toPoint, bool fromPerpendicularly, bool toPerpendicularly, int laneWidth, int rowHeight)
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

            int height = toPoint.Y - fromPoint.Y;
            int width = toPoint.X - fromPoint.X;
            bool singleLane = Math.Abs(width) <= laneWidth;
            laneWidth *= Math.Sign(width);

            if (!fromPerpendicularly && !toPerpendicularly && singleLane)
            {
                // direct line with anti-aliasing
                g.DrawLine(pen, e0, e1);
            }
            else
            {
                // control points for bezier curve
                PointF c0 = e0;
                PointF c1 = e1;

                if (fromPerpendicularly && toPerpendicularly)
                {
                    float midY = 1f / 2f * (fromPoint.Y + toPoint.Y);
                    c0.Y = midY;
                    c1.Y = midY;
                }
                else
                {
                    // is the end of a diagonal
                    if (singleLane)
                    {
                        float diagonalFractionStraight = height < rowHeight ? 2f / 5f : 1f / 2f;
                        float diagonalFractionCurve = 1f / 4f;
                        float perpendicularFraction = diagonalFractionCurve;
                        float perpendicularOffset = perpendicularFraction * Math.Min(height, rowHeight);

                        if (fromPerpendicularly)
                        {
                            // draw diagonally to e1
                            c1.X -= diagonalFractionStraight * laneWidth;
                            c1.Y -= diagonalFractionStraight * rowHeight;
                            g.DrawLine(pen, c1, e1);

                            // prepare remaining curve
                            e1 = c1;
                            c1.X -= diagonalFractionCurve * laneWidth;
                            c1.Y -= diagonalFractionCurve * rowHeight;
                            c0.Y += perpendicularOffset;
                        }
                        else
                        {
                            // draw diagonally from e0
                            c0.X += diagonalFractionStraight * laneWidth;
                            c0.Y += diagonalFractionStraight * rowHeight;
                            g.DrawLine(pen, e0, c0);

                            // prepare remaining curve
                            e0 = c0;
                            c0.X += diagonalFractionCurve * laneWidth;
                            c0.Y += diagonalFractionCurve * rowHeight;
                            c1.Y -= perpendicularOffset;
                        }
                    }

                    // is a multi-lane crossing
                    else
                    {
                        float diagonalFractionStraight = 1f / 6f;
                        float midY = 1f / 2f * (e0.Y + e1.Y);

                        if (fromPerpendicularly)
                        {
                            c0.Y = midY;
                        }
                        else
                        {
                            // draw diagonally from e0
                            c0.X += diagonalFractionStraight * laneWidth;
                            c0.Y += diagonalFractionStraight * rowHeight;
                            g.DrawLine(pen, e0, c0);

                            // prepare remaining curve
                            e0 = c0;
                            c0.X += diagonalFractionStraight * laneWidth;
                            c0.Y += diagonalFractionStraight * rowHeight;
                        }

                        if (toPerpendicularly)
                        {
                            c1.Y = midY;
                        }
                        else
                        {
                            // draw diagonally to e1
                            c1.X -= diagonalFractionStraight * laneWidth;
                            c1.Y -= diagonalFractionStraight * rowHeight;
                            g.DrawLine(pen, c1, e1);

                            // prepare remaining curve
                            e1 = c1;
                            c1.X -= diagonalFractionStraight * laneWidth;
                            c1.Y -= diagonalFractionStraight * rowHeight;
                        }
                    }
                }

                g.DrawBezier(pen, e0, c0, c1, e1);
            }
        }
    }
}
