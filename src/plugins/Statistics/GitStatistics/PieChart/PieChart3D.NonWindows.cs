using Avalonia;
using Avalonia.Media;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GitExtensions.Plugins.GitStatistics.PieChart;

/// <summary>
/// Native Avalonia renderer for the plugin's three-dimensional pie chart.
/// </summary>
public sealed class PieChart3D
{
    private const double FullCircle = 360;
    private const double HalfCircle = 180;
    private const double DegreesToRadians = Math.PI / HalfCircle;

    private readonly Rect _bounds;
    private readonly Color[] _sliceColors;
    private readonly double _sliceRelativeHeight;
    private readonly decimal[] _values;
    private double _initialAngle;
    private double[] _relativeSliceDisplacements = [0];
    private IReadOnlyList<Slice> _slices = [];

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
        _values = values;
        _sliceColors = sliceColors;
        _sliceRelativeHeight = Math.Max(0, sliceRelativeHeight);
        RebuildSlices();
    }

    public EdgeColorType EdgeColorType { private get; set; } = EdgeColorType.SystemColor;

    public double EdgeLineWidth { private get; set; } = 1;

    public bool FitToBoundingRectangle { private get; set; }

    public int HighlightedIndex { private get; set; } = -1;

    public ShadowStyle ShadowStyle { private get; set; } = ShadowStyle.GradualShadow;

    internal int SliceCount => _slices.Count;

    public void SetInitialAngle(double value)
    {
        _initialAngle = value;
        RebuildSlices();
    }

    public void SetSliceRelativeDisplacements(double[] value)
    {
        _relativeSliceDisplacements = value.Length == 0 ? [0] : value;
        RebuildSlices();
    }

    public void Draw(DrawingContext drawingContext)
    {
        foreach (Slice slice in _slices)
        {
            DrawOuterSide(drawingContext, slice);
        }

        foreach (Slice slice in _slices)
        {
            DrawCutSide(drawingContext, slice, slice.StartAngle);
            DrawCutSide(drawingContext, slice, slice.EndAngle);
        }

        foreach (Slice slice in _slices)
        {
            Color color = slice.Index == HighlightedIndex
                ? AdjustLightness(slice.Color, 0.22)
                : slice.Color;
            drawingContext.DrawGeometry(
                new SolidColorBrush(color),
                CreatePen(color),
                CreateTopGeometry(slice));
        }
    }

    public int FindPieSliceUnderPoint(Point point)
    {
        for (int index = _slices.Count - 1; index >= 0; index--)
        {
            Slice slice = _slices[index];
            double x = (point.X - slice.Center.X) / slice.RadiusX;
            double y = (point.Y - slice.Center.Y) / slice.RadiusY;
            if ((x * x) + (y * y) > 1)
            {
                continue;
            }

            double angle = NormalizeAngle(Math.Atan2(y, x) / DegreesToRadians);
            if (ContainsAngle(slice.StartAngle, slice.SweepAngle, angle))
            {
                return slice.Index;
            }
        }

        return -1;
    }

    internal Point GetSliceHitPoint(int index)
    {
        Slice slice = _slices[index];
        double angle = (slice.StartAngle + (slice.SweepAngle / 2)) * DegreesToRadians;
        return new Point(
            slice.Center.X + (Math.Cos(angle) * slice.RadiusX * 0.55),
            slice.Center.Y + (Math.Sin(angle) * slice.RadiusY * 0.55));
    }

    private void RebuildSlices()
    {
        decimal positiveTotal = _values.Where(value => value > 0).Sum();
        if (positiveTotal <= 0 || _bounds.Width <= 0 || _bounds.Height <= 0)
        {
            _slices = [];
            return;
        }

        double depth = _bounds.Height * _sliceRelativeHeight / (1 + _sliceRelativeHeight);
        double ellipseHeight = _bounds.Height - depth;
        double radiusX = _bounds.Width / 2;
        double radiusY = ellipseHeight / 2;
        if (FitToBoundingRectangle)
        {
            radiusX = Math.Max(0, radiusX - 1);
            radiusY = Math.Max(0, radiusY - 1);
        }

        Point baseCenter = new(_bounds.X + (_bounds.Width / 2), _bounds.Y + radiusY);
        List<Slice> slices = [];
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
            double displacement = GetRelativeDisplacement(index) * Math.Min(radiusX, radiusY);
            Point center = new(
                baseCenter.X + (Math.Cos(middleAngle) * displacement),
                baseCenter.Y + (Math.Sin(middleAngle) * displacement));
            slices.Add(new Slice(
                index,
                startAngle,
                sweepAngle,
                center,
                radiusX,
                radiusY,
                depth,
                _sliceColors[index % _sliceColors.Length]));
            startAngle += sweepAngle;
        }

        _slices = slices;
    }

    private double GetRelativeDisplacement(int index)
        => _relativeSliceDisplacements.Length == 1
            ? _relativeSliceDisplacements[0]
            : index < _relativeSliceDisplacements.Length
                ? _relativeSliceDisplacements[index]
                : 0;

    private void DrawOuterSide(DrawingContext drawingContext, Slice slice)
    {
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
                : AdjustLightness(slice.Color, ShadowStyle == ShadowStyle.UniformShadow ? -0.24 : -0.34);
            IBrush brush = ShadowStyle == ShadowStyle.GradualShadow
                ? new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                    GradientStops =
                    [
                        new GradientStop(AdjustLightness(slice.Color, -0.15), 0),
                        new GradientStop(AdjustLightness(slice.Color, -0.45), 1),
                    ],
                }
                : new SolidColorBrush(sideColor);
            drawingContext.DrawGeometry(brush, CreatePen(sideColor), geometry);
        }
    }

    private void DrawCutSide(DrawingContext drawingContext, Slice slice, double angle)
    {
        double normalized = NormalizeAngle(angle);
        if (normalized > HalfCircle)
        {
            return;
        }

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

    private StreamGeometry CreateTopGeometry(Slice slice)
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
        Slice slice,
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

    private static Point PointOnEllipse(Slice slice, double angle, bool bottom)
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
            EdgeColorType.DarkerThanSurface => AdjustLightness(surfaceColor, -0.2),
            EdgeColorType.DarkerDarkerThanSurface => AdjustLightness(surfaceColor, -0.4),
            EdgeColorType.LighterThanSurface => AdjustLightness(surfaceColor, 0.2),
            EdgeColorType.LighterLighterThanSurface => AdjustLightness(surfaceColor, 0.4),
            EdgeColorType.FullContrast => GetLuminance(surfaceColor) > 0.5 ? Colors.Black : Colors.White,
            EdgeColorType.Contrast => GetLuminance(surfaceColor) > 0.5
                ? AdjustLightness(surfaceColor, -0.25)
                : AdjustLightness(surfaceColor, 0.25),
            EdgeColorType.EnhancedContrast => GetLuminance(surfaceColor) > 0.5
                ? AdjustLightness(surfaceColor, -0.45)
                : AdjustLightness(surfaceColor, 0.45),
            _ => GetLuminance(surfaceColor) > 0.5 ? Colors.Black : Colors.White,
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
    {
        double normalizedStart = NormalizeAngle(start);
        double normalizedEnd = normalizedStart + sweep;
        double candidate = angle < normalizedStart ? angle + FullCircle : angle;
        return candidate >= normalizedStart && candidate <= normalizedEnd + 0.001;
    }

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
            return (byte)Math.Clamp(Math.Round(value), byte.MinValue, byte.MaxValue);
        }

        return Color.FromArgb(
            color.A,
            Adjust(color.R, factor),
            Adjust(color.G, factor),
            Adjust(color.B, factor));
    }

    private static double GetLuminance(Color color)
        => ((0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B)) / byte.MaxValue;

    private sealed record Slice(
        int Index,
        double StartAngle,
        double SweepAngle,
        Point Center,
        double RadiusX,
        double RadiusY,
        double Depth,
        Color Color)
    {
        public double EndAngle => StartAngle + SweepAngle;
    }
}
