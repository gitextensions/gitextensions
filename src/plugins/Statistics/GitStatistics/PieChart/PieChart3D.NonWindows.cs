using Avalonia;
using Avalonia.Media;
using GitExtensions.Extensibility;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using Microsoft;
using Color = Avalonia.Media.Color;
using DrawingColor = System.Drawing.Color;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GitExtensions.Plugins.GitStatistics.PieChart;

/// <summary>
///   Object representing a pie chart.
/// </summary>
public class PieChart3D
{
    private const double FullCircle = 360;
    private const double HalfCircle = 180;
    private const double DegreesToRadians = Math.PI / HalfCircle;

    private readonly Rect _bounds;

    /// <summary>
    ///   Array of colors used for rendering.
    /// </summary>
    private readonly Color[] _sliceColors;

    /// <summary>
    ///   Slice relative height.
    /// </summary>
    private readonly double _sliceRelativeHeight;

    /// <summary>
    ///   Array of values to be presented by the chart.
    /// </summary>
    private readonly decimal[] _values;

    /// <summary>
    ///   Initial angle from which chart is drawn.
    /// </summary>
    private double _initialAngle;

    /// <summary>
    ///   Array of relative displacements from the common center.
    /// </summary>
    private double[] _relativeSliceDisplacements = [0];

    /// <summary>
    ///   List of ordered pie slices constituting the chart, starting from 270 degrees axis.
    /// </summary>
    private IReadOnlyList<PieSlice> _slices = [];

    /// <summary>
    ///   Initializes a new instance of <c>PieChart3D</c> with given bounds,
    ///   array of values and relative pie slice height.
    /// </summary>
    /// <param name = "xBoundingRect">
    ///   x-coordinate of the upper-left corner of the rectangle bounding
    ///   the chart.
    /// </param>
    /// <param name = "yBoundingRect">
    ///   y-coordinate of the upper-left corner of the rectangle bounding
    ///   the chart.
    /// </param>
    /// <param name = "widthBoundingRect">
    ///   Width of the rectangle bounding the chart.
    /// </param>
    /// <param name = "heightBoundingRect">
    ///   Height of the rectangle bounding the chart.
    /// </param>
    /// <param name = "values">
    ///   An array of <c>decimal</c> values to chart.
    /// </param>
    /// <param name = "sliceColors">
    ///   An array of colors used to render slices.
    /// </param>
    /// <param name = "sliceRelativeHeight">
    ///   Thickness of the slice to chart relative to the height of the
    ///   bounding rectangle.
    /// </param>
    public PieChart3D(
        double xBoundingRect,
        double yBoundingRect,
        double widthBoundingRect,
        double heightBoundingRect,
        decimal[] values,
        Color[] sliceColors,
        double sliceRelativeHeight)
    {
        _bounds = new Rect(xBoundingRect, yBoundingRect, widthBoundingRect, heightBoundingRect);

        // Sets values to be displayed on the chart.
        _values = values;

        // Sets colors used for individual pie slices.
        _sliceColors = sliceColors;

        // Sets slice height, relative to the top ellipse semi-axis. Must be less than or equal to 0.5.
        _sliceRelativeHeight = Math.Max(0, sliceRelativeHeight);
    }

    /// <summary>
    ///   Sets slice edge color mode. If set to <c>PenColor</c> (default),
    ///   then value set by <c>EdgeColor</c> property is used.
    /// </summary>
    public EdgeColorType EdgeColorType { private get; set; } = EdgeColorType.SystemColor;

    /// <summary>
    ///   Sets slice edge line width. If not set, default value is 1.
    /// </summary>
    public double EdgeLineWidth { private get; set; } = 1;

    /// <summary>
    ///   Sets the shadowing style used.
    /// </summary>
    public ShadowStyle ShadowStyle { private get; set; } = ShadowStyle.GradualShadow;

    /// <summary>
    ///   Sets the flag that controls if chart is fit to bounding rectangle
    ///   exactly.
    /// </summary>
    public bool FitToBoundingRectangle { private get; set; }

    public int HighlightedIndex { private get; set; } = -1;

    internal int SliceCount => _slices.Count;

    internal Color GetSliceColor(int index) => _slices.Single(slice => slice.Index == index).Color;

    internal Color GetEdgeColor(int index)
        => CreatePen(_slices.Single(slice => slice.Index == index).Color)?.Brush is ISolidColorBrush brush
            ? brush.Color
            : Colors.Transparent;

    /// <summary>
    ///   Sets the slice displacement relative to the ellipse semi-axis.
    ///   Must be less than 1.
    /// </summary>
    public void SetSliceRelativeDisplacements(double[] value)
    {
        _relativeSliceDisplacements = value.Length == 0 ? [0] : value;
    }

    /// <summary>
    ///   Sets the initial angle from which pies are placed.
    /// </summary>
    public void SetInitialAngle(double value)
    {
        _initialAngle = value;
    }

    /// <summary>
    ///   Draws the chart.
    /// </summary>
    /// <param name = "graphics">
    ///   <c>DrawingContext</c> object used for drawing.
    /// </param>
    public void Draw(DrawingContext graphics)
    {
        InitializePieSlices();
        if (_slices.Count > 0)
        {
            DrawSliceSides(graphics);
        }

        DrawTops(graphics);
    }

    /// <summary>
    ///   Searches the chart to find the index of the pie slice which
    ///   contains point given. Search order goes in the direction opposite
    ///   to drawing order.
    /// </summary>
    /// <param name = "point">
    ///   <c>PointF</c> point for which pie slice is searched for.
    /// </param>
    /// <returns>
    ///   Index of the corresponding pie slice, or -1 if none is found.
    /// </returns>
    public int FindPieSliceUnderPoint(Point point)
    {
        if (_slices.Count == 0)
        {
            return -1;
        }

        // first check tops
        for (int i = 0; i < _slices.Count; ++i)
        {
            PieSlice slice = _slices[i];
            if (slice.PieSliceContainsPoint(point))
            {
                return slice.Index;
            }
        }

        // split the backmost (at 270 degrees) pie slice
        List<PieSlice> pieSlices = [.. _slices];
        PieSlice[] splitSlices = _slices[0].Split(270F);
        pieSlices[0] = splitSlices[0];
        if (splitSlices[1].SweepAngle > 0F)
        {
            pieSlices.Add(splitSlices[1]);
        }

        int indexFound = -1;

        // if not found yet, then check for peripheries
        int incrementIndex = 0;
        int decrementIndex = pieSlices.Count - 1;
        while (incrementIndex <= decrementIndex)
        {
            PieSlice sliceLeft = pieSlices[decrementIndex];
            double angle1 = 270 - sliceLeft.StartAngle;
            PieSlice sliceRight = pieSlices[incrementIndex];
            double angle2 = (sliceRight.EndAngle + 90) % 360;
            DebugHelpers.Assert(angle2 >= 0, "angle2 >= 0");
            if (angle2 < angle1)
            {
                if (sliceRight.PeripheryContainsPoint(point))
                {
                    indexFound = incrementIndex;
                }

                ++incrementIndex;
            }
            else
            {
                if (sliceLeft.PeripheryContainsPoint(point))
                {
                    indexFound = decrementIndex;
                }

                --decrementIndex;
            }
        }

        // check for start/stop sides, starting from the foremost
        if (indexFound < 0)
        {
            int foremostPieIndex = GetForemostPieSlice(pieSlices);

            // check for start sides from the foremost slice to the left
            // side
            int i = foremostPieIndex;
            while (i < pieSlices.Count)
            {
                PieSlice sliceLeft = pieSlices[i];
                if (sliceLeft.StartSideContainsPoint(point))
                {
                    indexFound = i;
                    break;
                }

                ++i;
            }

            // if not found yet, check end sides from the foremost to the right
            // side
            if (indexFound < 0)
            {
                i = foremostPieIndex;
                while (i >= 0)
                {
                    PieSlice sliceLeft = pieSlices[i];
                    if (sliceLeft.EndSideContainsPoint(point))
                    {
                        indexFound = i;
                        break;
                    }

                    --i;
                }
            }
        }

        // finally search for bottom sides
        if (indexFound < 0)
        {
            for (int i = 0; i < _slices.Count; ++i)
            {
                PieSlice slice = _slices[i];
                if (slice.BottomSurfaceSectionContainsPoint(point))
                {
                    return slice.Index;
                }
            }
        }

        if (indexFound > -1)
        {
            return pieSlices[indexFound].Index;
        }

        return -1;
    }

    /// <summary>
    ///   Return the index of the foremost pie slice i.e. the one crossing
    ///   90 degrees boundary.
    /// </summary>
    /// <param name = "pieSlices">
    ///   Array of <c>PieSlice</c> objects to examine.
    /// </param>
    /// <returns>
    ///   Index of the foremost pie slice.
    /// </returns>
    private static int GetForemostPieSlice(IReadOnlyList<PieSlice> pieSlices)
    {
        Requires.NotNullOrEmpty(pieSlices, nameof(pieSlices));

        for (int i = 0; i < pieSlices.Count; ++i)
        {
            PieSlice pieSlice = pieSlices[i];
            if (((pieSlice.StartAngle <= 90) && ((pieSlice.StartAngle + pieSlice.SweepAngle) >= 90)) ||
                ((pieSlice.StartAngle + pieSlice.SweepAngle > 360) && (pieSlice.StartAngle <= 450) &&
                 (pieSlice.StartAngle + pieSlice.SweepAngle) >= 450))
            {
                return i;
            }
        }

        DebugHelpers.Assert(false, "Foremost pie slice not found");
        return -1;
    }

    internal Point GetSliceHitPoint(int index)
    {
        PieSlice slice = _slices.Single(slice => slice.Index == index);
        double angle = (slice.StartAngle + (slice.SweepAngle / 2)) * DegreesToRadians;
        return new Point(
            slice.Center.X + (Math.Cos(angle) * slice.RadiusX * 0.55),
            slice.Center.Y + (Math.Sin(angle) * slice.RadiusY * 0.55));
    }

    /// <summary>
    ///   Initializes pies.
    /// </summary>
    /// Creates a list of pies, starting with the pie that is crossing the
    /// 270 degrees boundary, i.e. "backmost" pie that always has to be
    /// drawn first to ensure correct surface overlapping.
    protected virtual void InitializePieSlices()
    {
        // calculates the sum of values required to evaluate sweep angles
        // for individual pies
        decimal positiveTotal = _values.Where(value => value > 0).Sum();
        if (positiveTotal <= 0 || _bounds.Width <= 0 || _bounds.Height <= 0)
        {
            _slices = [];
            return;
        }

        // some values and indices that will be used in the loop
        double largestDisplacement = _relativeSliceDisplacements.Take(_values.Length).DefaultIfEmpty(0).Max();
        double factor = 1 + largestDisplacement;
        double depth = _bounds.Height / factor * _sliceRelativeHeight;
        double ellipseHeight = _bounds.Height / factor * (1 - _sliceRelativeHeight);
        double radiusX = _bounds.Width / factor / 2;
        double radiusY = ellipseHeight / 2;
        if (FitToBoundingRectangle)
        {
            radiusX = Math.Max(0, radiusX - 1);
            radiusY = Math.Max(0, radiusY - 1);
        }

        Point baseCenter = new(_bounds.X + (_bounds.Width / 2), _bounds.Y + ((ellipseHeight * largestDisplacement) / 2) + radiusY);
        List<PieSlice> slices = [];
        int colorIndex = 0;
        int backPieIndex = -1;
        int displacementIndex = 0;
        int maxDisplacementIndex = _relativeSliceDisplacements.Length - 1;
        double startAngle = NormalizeAngle(_initialAngle);
        for (int index = 0; index < _values.Length; index++)
        {
            decimal value = _values[index];
            if (value <= 0)
            {
                continue;
            }

            double sweepAngle = (double)(value / positiveTotal) * FullCircle;
            double middleAngle = (startAngle + (sweepAngle / 2)) * DegreesToRadians;

            // displacement from the center of the ellipse
            double displacement = _relativeSliceDisplacements[displacementIndex];
            Point center = new(
                baseCenter.X + (Math.Cos(middleAngle) * radiusX * displacement),
                baseCenter.Y + (Math.Sin(middleAngle) * radiusY * displacement));

            Color sliceColor = AdaptBackColor(_sliceColors[colorIndex]);

            // Collection of reordered pie slices mapped to original order.
            PieSlice slice = new(
                index,
                startAngle,
                sweepAngle,
                center,
                radiusX,
                radiusY,
                depth,
                sliceColor);

            // the backmost pie is inserted to the front of the list for correct drawing
            if (backPieIndex > -1 || ((startAngle <= 270) && (startAngle + sweepAngle > 270)) ||
                ((startAngle >= 270) && (startAngle + sweepAngle > 630)))
            {
                ++backPieIndex;
                slices.Insert(backPieIndex, slice);
            }
            else
            {
                slices.Add(slice);
            }

            // increment displacementIndex only if there are more displacements available
            if (displacementIndex < maxDisplacementIndex)
            {
                ++displacementIndex;
            }

            ++colorIndex;

            // if all colors have been exhausted, reset color index
            if (colorIndex >= _sliceColors.Length)
            {
                colorIndex = 0;
            }

            // prepare for the next pie slice
            startAngle += sweepAngle;
            if (startAngle > 360)
            {
                startAngle -= 360;
            }
        }

        _slices = slices;
    }

    /// <summary>
    ///   Draws outer peripheries of all slices.
    /// </summary>
    /// <param name = "graphics">
    ///   <c>DrawingContext</c> used for drawing.
    /// </param>
    protected void DrawSliceSides(DrawingContext graphics)
    {
        List<PieSlice> pieSlices = [.. _slices];

        // if the first slice spreads across 180 and 360 degrees boundaries it
        // will appear on both left and right edge so its periphery has to be
        // drawn twice
        PieSlice[] splitSlices = _slices[0].Split(270F);
        pieSlices[0] = splitSlices[0];
        if (splitSlices[1].SweepAngle > 0F)
        {
            pieSlices.Add(splitSlices[1]);
        }

        // finds the leftmost slice that is crossing 180 degrees boundary
        int decrementIndex = pieSlices.Count - 1;
        for (int i = decrementIndex; i > 0; --i)
        {
            if (pieSlices[i].StartAngle >= 180)
            {
                continue;
            }

            decrementIndex = i;
            break;
        }

        // finds the rightmost slice that is crossing 0 degrees boundary
        int incrementIndex = decrementIndex;
        for (int i = 0; i < pieSlices.Count - 1; ++i)
        {
            PieSlice slice = pieSlices[i];
            if ((slice.StartAngle + slice.SweepAngle <= 360) && (slice.EndAngle >= 180 || slice.EndAngle <= 0))
            {
                continue;
            }

            incrementIndex = i;
            break;
        }

        // draws visible start sides for slices from the backmost (at 270 degrees)
        // to the leftmost one
        for (int i = pieSlices.Count - 1; i > decrementIndex; --i)
        {
            PieSlice slice = pieSlices[i];
            DrawVisibleStartSide(graphics, slice);
        }

        // draws visible end sides for slices from the backmost (at 270 degrees)
        // to the rightmost one
        for (int i = 0; i < incrementIndex; ++i)
        {
            PieSlice slice = pieSlices[i];
            DrawVisibleEndSide(graphics, slice);
        }

        // draws from leftmost and rightmost alternatively, drawing the slice
        // with larger offset from 90 degrees first
        while (incrementIndex < decrementIndex)
        {
            PieSlice sliceLeft = pieSlices[decrementIndex];
            double angle1 = 270 - sliceLeft.StartAngle;
            PieSlice sliceRight = pieSlices[incrementIndex];
            double angle2 = (sliceRight.EndAngle + 90) % 360;
            DebugHelpers.Assert(angle2 >= 0, "angle2 >= 0");
            if (angle2 < angle1)
            {
                DrawVisibleEndSide(graphics, sliceRight);
                DrawVisiblePeriphery(graphics, sliceRight);
                ++incrementIndex;
            }
            else
            {
                DrawVisibleStartSide(graphics, sliceLeft);
                DrawVisiblePeriphery(graphics, sliceLeft);
                --decrementIndex;
            }
        }

        // for the foremost slice only periphery has to be drawn
        DrawVisiblePeriphery(graphics, pieSlices[incrementIndex]);
    }

    /// <summary>
    ///   Draws top sides of all pie slices.
    /// </summary>
    /// <param name = "graphics">
    ///   <c>DrawingContext</c> used for drawing.
    /// </param>
    protected void DrawTops(DrawingContext graphics)
    {
        foreach (PieSlice slice in _slices)
        {
            Color color = slice.Index == HighlightedIndex
                ? AdjustLightness(slice.Color, 0.3)
                : slice.Color;
            graphics.DrawGeometry(new SolidColorBrush(color), CreatePen(color), CreateTopGeometry(slice));
        }
    }

    // Avalonia draws the original slice surfaces through DrawingContext instead of GDI+.
    private void DrawVisibleStartSide(DrawingContext graphics, PieSlice slice)
    {
        if (slice.Depth > 0 && slice.StartAngle is > 90 and < 270)
        {
            DrawCutSide(graphics, slice, slice.StartAngle);
        }
    }

    private void DrawVisibleEndSide(DrawingContext graphics, PieSlice slice)
    {
        if (slice.Depth > 0 && slice.EndAngle is > 270 or < 90)
        {
            DrawCutSide(graphics, slice, slice.EndAngle);
        }
    }

    private void DrawVisiblePeriphery(DrawingContext graphics, PieSlice slice)
    {
        if (slice.Depth > 0)
        {
            DrawOuterSide(graphics, slice);
        }
    }

    private void DrawOuterSide(DrawingContext drawingContext, PieSlice slice)
    {
        // if a slice spreads across 180 and 360 degrees boundaries it
        // will appear on both left and right edge so its periphery has to be
        // drawn twice
        foreach ((double start, double sweep) in GetFrontSegments(slice.StartAngle, slice.SweepAngle))
        {
            StreamGeometry geometry = new();
            using (StreamGeometryContext context = geometry.Open())
            {
                Point topStart = PointOnEllipse(slice, start, bottom: false);
                Point topEnd = PointOnEllipse(slice, start + sweep, bottom: false);
                Point bottomEnd = PointOnEllipse(slice, start + sweep, bottom: true);
                Point bottomStart = PointOnEllipse(slice, start, bottom: true);
                context.BeginFigure(topStart, isFilled: true);
                AddArc(context, slice, topEnd, sweep, SweepDirection.Clockwise);
                context.LineTo(bottomEnd);
                AddArc(context, slice, bottomStart, sweep, SweepDirection.CounterClockwise);
                context.EndFigure(isClosed: true);
            }

            Color sideColor = ShadowStyle == ShadowStyle.NoShadow
                ? slice.Color
                : AdjustLightness(slice.Color, -0.3);
            IBrush brush = ShadowStyle == ShadowStyle.GradualShadow
                ? new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                    GradientStops =
                    [
                        new GradientStop(AdjustLightness(slice.Color, -0.15), 0),
                        new GradientStop(slice.Color, 0.1),
                        new GradientStop(AdjustLightness(slice.Color, -0.3), 1),
                    ],
                }
                : new SolidColorBrush(sideColor);
            drawingContext.DrawGeometry(brush, CreatePen(sideColor), geometry);
        }
    }

    private void DrawCutSide(DrawingContext drawingContext, PieSlice slice, double angle)
    {
        Point top = PointOnEllipse(slice, angle, bottom: false);
        Point bottom = PointOnEllipse(slice, angle, bottom: true);
        Point centerBottom = new(slice.Center.X, slice.Center.Y + slice.Depth);
        StreamGeometry geometry = new();
        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(slice.Center, isFilled: true);
            context.LineTo(top);
            context.LineTo(bottom);
            context.LineTo(centerBottom);
            context.EndFigure(isClosed: true);
        }

        Color sideColor = ShadowStyle == ShadowStyle.NoShadow
            ? slice.Color
            : AdjustLightness(slice.Color, -0.3);
        drawingContext.DrawGeometry(new SolidColorBrush(sideColor), CreatePen(sideColor), geometry);
    }

    private StreamGeometry CreateTopGeometry(PieSlice slice)
    {
        StreamGeometry geometry = new();
        using StreamGeometryContext context = geometry.Open();
        if (slice.SweepAngle >= FullCircle - 0.001)
        {
            Point right = PointOnEllipse(slice, 0, bottom: false);
            Point left = PointOnEllipse(slice, HalfCircle, bottom: false);
            context.BeginFigure(right, isFilled: true);
            AddArc(context, slice, left, HalfCircle, SweepDirection.Clockwise);
            AddArc(context, slice, right, HalfCircle, SweepDirection.Clockwise);
            context.EndFigure(isClosed: true);
            return geometry;
        }

        Point start = PointOnEllipse(slice, slice.StartAngle, bottom: false);
        Point end = PointOnEllipse(slice, slice.EndAngle, bottom: false);
        context.BeginFigure(slice.Center, isFilled: true);
        context.LineTo(start);
        AddArc(context, slice, end, slice.SweepAngle, SweepDirection.Clockwise);
        context.LineTo(slice.Center);
        context.EndFigure(isClosed: true);
        return geometry;
    }

    private static void AddArc(
        StreamGeometryContext context,
        PieSlice slice,
        Point end,
        double sweep,
        SweepDirection direction)
    {
        context.ArcTo(
            end,
            new Size(slice.RadiusX, slice.RadiusY),
            rotationAngle: 0,
            isLargeArc: sweep > HalfCircle,
            direction);
    }

    private static Point PointOnEllipse(PieSlice slice, double angle, bool bottom)
    {
        double radians = angle * DegreesToRadians;
        return new Point(
            slice.Center.X + (Math.Cos(radians) * slice.RadiusX),
            slice.Center.Y + (Math.Sin(radians) * slice.RadiusY) + (bottom ? slice.Depth : 0));
    }

    private Pen? CreatePen(Color surfaceColor)
    {
        if (EdgeColorType == EdgeColorType.NoEdge || EdgeLineWidth <= 0)
        {
            return null;
        }

        Color edgeColor = EdgeColorType switch
        {
            EdgeColorType.SurfaceColor => surfaceColor,
            EdgeColorType.DarkerThanSurface => AdjustLightness(surfaceColor, -0.3),
            EdgeColorType.DarkerDarkerThanSurface => AdjustLightness(surfaceColor, -0.5),
            EdgeColorType.LighterThanSurface => AdjustLightness(surfaceColor, 0.3),
            EdgeColorType.LighterLighterThanSurface => AdjustLightness(surfaceColor, 0.5),
            EdgeColorType.FullContrast => GetBrightness(surfaceColor) > 0.4 ? Colors.Black : Colors.White,
            EdgeColorType.Contrast => GetBrightness(surfaceColor) > 0.4
                ? AdjustLightness(surfaceColor, -0.3)
                : AdjustLightness(surfaceColor, 0.3),
            EdgeColorType.EnhancedContrast => GetBrightness(surfaceColor) > 0.4
                ? AdjustLightness(surfaceColor, -0.5)
                : AdjustLightness(surfaceColor, 0.5),
            _ => GetSystemWindowTextColor(),
        };
        return new Pen(new SolidColorBrush(edgeColor), EdgeLineWidth);
    }

    private static IEnumerable<(double Start, double Sweep)> GetFrontSegments(double start, double sweep)
    {
        double end = start + sweep;
        int firstTurn = (int)Math.Floor((start - HalfCircle) / FullCircle);
        int lastTurn = (int)Math.Ceiling(end / FullCircle);
        for (int turn = firstTurn; turn <= lastTurn; turn++)
        {
            double frontStart = turn * FullCircle;
            double frontEnd = frontStart + HalfCircle;
            double segmentStart = Math.Max(start, frontStart);
            double segmentEnd = Math.Min(end, frontEnd);
            if (segmentEnd - segmentStart > 0.001)
            {
                yield return (segmentStart, segmentEnd - segmentStart);
            }
        }
    }

    private static bool ContainsAngle(double start, double sweep, double angle)
        => (angle >= start && angle <= start + sweep)
            || (start + sweep > FullCircle && angle + FullCircle <= start + sweep);

    private static double NormalizeAngle(double angle)
    {
        double normalized = angle % FullCircle;
        return normalized < 0 ? normalized + FullCircle : normalized;
    }

    private static Color AdjustLightness(Color color, double factor)
    {
        static byte Adjust(byte channel, double factor)
        {
            double value = factor >= 0
                ? channel + ((byte.MaxValue - channel) * factor)
                : channel * (1 + factor);
            return (byte)Math.Clamp((int)value, byte.MinValue, byte.MaxValue);
        }

        return Color.FromRgb(
            Adjust(color.R, factor),
            Adjust(color.G, factor),
            Adjust(color.B, factor));
    }

    private static double GetBrightness(Color color)
    {
        byte max = Math.Max(color.R, Math.Max(color.G, color.B));
        byte min = Math.Min(color.R, Math.Min(color.G, color.B));
        return (max + min) / (2d * byte.MaxValue);
    }

    private static Color AdaptBackColor(Color color)
    {
        DrawingColor original = DrawingColor.FromArgb(color.A, color.R, color.G, color.B);
        DrawingColor adapted = original.AdaptBackColor();
        return Color.FromArgb(adapted.A, adapted.R, adapted.G, adapted.B);
    }

    private static Color GetSystemWindowTextColor()
    {
        DrawingColor color = ThemeModule.Settings.Theme.GetNonEmptyColor(System.Drawing.KnownColor.WindowText);
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    private sealed record PieSlice(
        int Index,
        double StartAngle,
        double SweepAngle,
        Point Center,
        double RadiusX,
        double RadiusY,
        double Depth,
        Color Color)
    {
        public double EndAngle => NormalizeAngle(StartAngle + SweepAngle);

        internal PieSlice[] Split(double splitAngle)
            =>
            [
                this with { StartAngle = splitAngle, SweepAngle = (StartAngle + SweepAngle - splitAngle + FullCircle) % FullCircle },
                this with { SweepAngle = (splitAngle - StartAngle + FullCircle) % FullCircle },
            ];

        internal bool PieSliceContainsPoint(Point point)
        {
            double x = (point.X - Center.X) / RadiusX;
            double y = (point.Y - Center.Y) / RadiusY;
            double angle = NormalizeAngle(Math.Atan2(y, x) / DegreesToRadians);
            return (x * x) + (y * y) < 1 && ContainsAngle(StartAngle, SweepAngle, angle);
        }

        internal bool PeripheryContainsPoint(Point point)
        {
            foreach ((double start, double sweep) in GetFrontSegments(StartAngle, SweepAngle))
            {
                if (CylinderSurfaceSectionContainsPoint(point, PointOnEllipse(this, start, bottom: false), PointOnEllipse(this, start + sweep, bottom: false)))
                {
                    return true;
                }
            }

            return false;
        }

        internal bool StartSideContainsPoint(Point point)
            => Depth > 0 && StartAngle is > 90 and < 270
                && ContainsQuadrilateral(point, Center, PointOnEllipse(this, StartAngle, bottom: false), PointOnEllipse(this, StartAngle, bottom: true), new Point(Center.X, Center.Y + Depth));

        internal bool EndSideContainsPoint(Point point)
            => Depth > 0 && EndAngle is > 270 or < 90
                && ContainsQuadrilateral(point, Center, PointOnEllipse(this, EndAngle, bottom: false), PointOnEllipse(this, EndAngle, bottom: true), new Point(Center.X, Center.Y + Depth));

        internal bool BottomSurfaceSectionContainsPoint(Point point)
            => Depth > 0 && PieSliceContainsPoint(new Point(point.X, point.Y - Depth));

        private bool CylinderSurfaceSectionContainsPoint(Point point, Point point1, Point point2)
            => Depth > 0 && ContainsQuadrilateral(point, point1, new Point(point1.X, point1.Y + Depth), new Point(point2.X, point2.Y + Depth), point2);

        // Framework constraint: preserve the original quadrilateral intersection math at the native Point boundary.
        private static bool ContainsQuadrilateral(Point point, params Point[] corners)
            => Quadrilateral.Contains(new System.Drawing.PointF((float)point.X, (float)point.Y),
                [.. corners.Select(corner => new System.Drawing.PointF((float)corner.X, (float)corner.Y))]);
    }
}
