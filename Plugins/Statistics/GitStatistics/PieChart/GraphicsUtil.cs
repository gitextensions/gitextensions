using System.Drawing;

namespace GitStatistics.PieChart
{
    /// <summary>
    ///   Structure with graphics utility methods.
    /// </summary>
    public static class GraphicsUtil
    {
        /// <summary>
        ///   Checks if point is contained within <c>RectangleF</c> structure
        ///   and extends rectangle bounds if necessary to include the point.
        /// </summary>
        /// <param name = "rect">
        ///   Reference to <c>RectangleF</c> to check.
        /// </param>
        /// <param name = "pointToInclude">
        ///   <c>PointF</c> object to include.
        /// </param>
        public static void IncludePoint(ref RectangleF rect, PointF pointToInclude)
        {
            IncludePointX(ref rect, pointToInclude.X);
            IncludePointY(ref rect, pointToInclude.Y);
        }

        /// <summary>
        ///   Checks if x-coordinate is contained within the <c>RectangleF</c>
        ///   structure and extends rectangle bounds if necessary to include
        ///   the point.
        /// </summary>
        /// <param name = "rect">
        ///   <c>RectangleF</c> to check.
        /// </param>
        /// <param name = "xToInclude">
        ///   x-coordinate to include.
        /// </param>
        public static void IncludePointX(ref RectangleF rect, float xToInclude)
        {
            if (xToInclude < rect.X)
            {
                rect.Width = rect.Right - xToInclude;
                rect.X = xToInclude;
            }
            else if (xToInclude > rect.Right)
            {
                rect.Width = xToInclude - rect.X;
            }
        }

        /// <summary>
        ///   Checks if y-coordinate is contained within the <c>RectangleF</c>
        ///   structure and extends rectangle bounds if necessary to include
        ///   the point.
        /// </summary>
        /// <param name = "rect">
        ///   <c>RectangleF</c> to check.
        /// </param>
        /// <param name = "yToInclude">
        ///   y-coordinate to include.
        /// </param>
        public static void IncludePointY(ref RectangleF rect, float yToInclude)
        {
            if (yToInclude < rect.Y)
            {
                rect.Height = rect.Bottom - yToInclude;
                rect.Y = yToInclude;
            }
            else if (yToInclude > rect.Bottom)
            {
                rect.Height = yToInclude - rect.Y;
            }
        }
    }
}