#if !WINDOWS
using Avalonia;
using Avalonia.Media;
using Point = Avalonia.Point;
using PointF = System.Drawing.PointF;

namespace GitExtensions.Plugins.GitStatistics.PieChart;

/// <summary>
///   Quadrilateral object.
/// </summary>
// Framework constraint: StreamGeometry replaces the GDI path; retained PointF coordinates use the original hit-test math.
public class Quadrilateral
{
    /// <summary>
    ///   <c>PointF</c>s describing the <c>StreamGeometry</c> points.
    /// </summary>
    private readonly PointF[] _points = [];

    public static readonly Quadrilateral Empty = new();

    /// <summary>
    ///   <c>StreamGeometry</c> representing the quadrilateral.
    /// </summary>
    private readonly StreamGeometry _path = new();

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
        _points = [point1, point2, point3, point4];
        using StreamGeometryContext context = _path.Open();
        context.BeginFigure(new Point(point1.X, point1.Y), isFilled: true);
        context.LineTo(new Point(point2.X, point2.Y));
        context.LineTo(new Point(point3.X, point3.Y));
        context.LineTo(new Point(point4.X, point4.Y));

        // isClosed: toClose
        context.EndFigure(isClosed: toClose);
    }

    /// <summary>
    ///   Draws the <c>Quadrilateral</c> with <c>DrawingContext</c> provided.
    /// </summary>
    /// <param name = "graphics">
    ///   <c>DrawingContext</c> used to draw.
    /// </param>
    /// <param name = "pen">
    ///   <c>IPen</c> used to draw outline.
    /// </param>
    /// <param name = "brush">
    ///   <c>IBrush</c> used to fill the inside.
    /// </param>
    public void Draw(DrawingContext graphics, IPen pen, IBrush brush)
    {
        graphics.DrawGeometry(brush, pen, _path);
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
        if (_points.Length == 0)
        {
            return false;
        }

        return Contains(point, _points);
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
        int intersections = 0;
        for (int i = 1; i < cornerPoints.Length; ++i)
        {
            if (DoesIntersects(point, cornerPoints[i], cornerPoints[i - 1]))
            {
                ++intersections;
            }
        }

        if (DoesIntersects(point, cornerPoints[^1], cornerPoints[0]))
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
        float x2 = point2.X;
        float y2 = point2.Y;
        float x1 = point1.X;
        float y1 = point1.Y;
        if ((x2 < point.X && x1 >= point.X) || (x2 >= point.X && x1 < point.X))
        {
            float y = ((y2 - y1) / (x2 - x1) * (point.X - x1)) + y1;
            return y > point.Y;
        }

        return false;
    }
}
#endif
