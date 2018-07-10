using System.Drawing;
using System.Drawing.Drawing2D;

namespace GitStatistics.PieChart
{
    /// <summary>
    ///   Quadrilateral object.
    /// </summary>
    public class Quadrilateral
    {
        /// <summary>
        ///   <c>PathPointType</c>s describing the <c>GraphicsPath</c> points.
        /// </summary>
        private static readonly byte[] QuadrilateralPointTypes =
            {
                    (byte)PathPointType.Start,
                    (byte)PathPointType.Line,
                    (byte)PathPointType.Line,
                    (byte)PathPointType.Line

                    // | (byte)PathPointType.CloseSubpath
                };

        public static readonly Quadrilateral Empty = new Quadrilateral();

        /// <summary>
        ///   <c>GraphicsPath</c> representing the quadrilateral.
        /// </summary>
        private readonly GraphicsPath _path = new GraphicsPath();

        /// <summary>
        ///   Creates empty <c>Quadrilateral</c> object
        /// </summary>
        protected Quadrilateral()
        {
        }

        /// <summary>
        ///   Initializes <c>Quadrilateral</c> object with given corner points.
        /// </summary>
        /// <param name = "point1">
        ///   First <c>PointF</c>.
        /// </param>
        /// <param name = "point2">
        ///   Second <c>PointF</c>.
        /// </param>
        /// <param name = "point3">
        ///   Third <c>PointF</c>.
        /// </param>
        /// <param name = "point4">
        ///   Fourth <c>PointF</c>.
        /// </param>
        /// <param name = "toClose">
        ///   Indicator should the quadrilateral be closed by the line.
        /// </param>
        public Quadrilateral(PointF point1, PointF point2, PointF point3, PointF point4, bool toClose)
        {
            var pointTypes = (byte[])QuadrilateralPointTypes.Clone();
            if (toClose)
            {
                pointTypes[3] |= (byte)PathPointType.CloseSubpath;
            }

            _path = new GraphicsPath(new[] { point1, point2, point3, point4 }, pointTypes);
        }

        /// <summary>
        ///   Draws the <c>Quadrilateral</c> with <c>Graphics</c> provided.
        /// </summary>
        /// <param name = "graphics">
        ///   <c>Graphics</c> used to draw.
        /// </param>
        /// <param name = "pen">
        ///   <c>Pen</c> used to draw outline.
        /// </param>
        /// <param name = "brush">
        ///   <c>Brush</c> used to fill the inside.
        /// </param>
        public void Draw(Graphics graphics, Pen pen, Brush brush)
        {
            graphics.FillPath(brush, _path);
            graphics.DrawPath(pen, _path);
        }

        /// <summary>
        ///   Checks if the given <c>PointF</c> is contained within the
        ///   quadrilateral.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> structure to check for.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the point is contained within the quadrilateral.
        /// </returns>
        public bool Contains(PointF point)
        {
            if (_path.PointCount == 0 || _path.PathPoints.Length == 0)
            {
                return false;
            }

            return Contains(point, _path.PathPoints);
        }

        /// <summary>
        ///   Checks if given <c>PointF</c> is contained within quadrilateral
        ///   defined by <c>cornerPoints</c> provided.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> to check.
        /// </param>
        /// <param name = "cornerPoints">
        ///   Array of <c>PointF</c> structures defining corners of the
        ///   quadrilateral.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the point is contained within the quadrilateral.
        /// </returns>
        public static bool Contains(PointF point, PointF[] cornerPoints)
        {
            var intersections = 0;
            for (var i = 1; i < cornerPoints.Length; ++i)
            {
                if (DoesIntersects(point, cornerPoints[i], cornerPoints[i - 1]))
                {
                    ++intersections;
                }
            }

            if (DoesIntersects(point, cornerPoints[cornerPoints.Length - 1], cornerPoints[0]))
            {
                ++intersections;
            }

            return intersections % 2 != 0;
        }

        /// <summary>
        ///   Checks if the line coming out of the <c>point</c> downwards
        ///   intersects with a line through <c>point1</c> and <c>point2</c>.
        /// </summary>
        /// <param name = "point">
        ///   <c>PointF</c> from which vertical line is drawn downwards.
        /// </param>
        /// <param name = "point1">
        ///   First <c>PointF</c> through which line is drawn.
        /// </param>
        /// <param name = "point2">
        ///   Second <c>PointF</c> through which line is drawn.
        /// </param>
        /// <returns>
        ///   <c>true</c> if lines intersect.
        /// </returns>
        private static bool DoesIntersects(PointF point, PointF point1, PointF point2)
        {
            var x2 = point2.X;
            var y2 = point2.Y;
            var x1 = point1.X;
            var y1 = point1.Y;
            if ((x2 < point.X && x1 >= point.X) || (x2 >= point.X && x1 < point.X))
            {
                var y = ((y2 - y1) / (x2 - x1) * (point.X - x1)) + y1;
                return y > point.Y;
            }

            return false;
        }
    }
}