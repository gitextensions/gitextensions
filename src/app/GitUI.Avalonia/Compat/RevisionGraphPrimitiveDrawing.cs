using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;

namespace GitUI.Compat;

/// <summary>
///  Preserves the graph's explicit primitive smoothing independently of an owner's edge options.
/// </summary>
/// <remarks>
///  Skia strokes are widened to keep stroke construction independent of the hard row clip.
///  Native96 antialiased primitives are rasterized before that clip, like cached graph pixels.
///  Operations retain only source coordinates and pen values, never native resources.
/// </remarks>
internal static class RevisionGraphPrimitiveDrawing
{
    /// <summary>
    ///  Draws a graph line using the source's choice of smoothing.
    /// </summary>
    internal static void DrawLine(DrawingContext context, Pen pen, Point start, Point end, bool antialias)
    {
        if (CanSnapshot(pen, out Color color))
        {
            context.Custom(new PrimitiveOperation(start, start, end, end, cubic: false, color, pen.Thickness, pen.MiterLimit, antialias));
            return;
        }

        context.DrawLine(pen, start, end);
    }

    /// <summary>
    ///  Draws a graph curve with the original control points and antialiasing.
    /// </summary>
    internal static void DrawBezier(DrawingContext context, Pen pen, Point start, Point control0, Point control1, Point end)
    {
        if (CanSnapshot(pen, out Color color))
        {
            context.Custom(new PrimitiveOperation(start, control0, control1, end, cubic: true, color, pen.Thickness, pen.MiterLimit, antialias: true));
            return;
        }

        StreamGeometry geometry = new();
        using (StreamGeometryContext drawing = geometry.Open())
        {
            drawing.BeginFigure(start, isFilled: false);
            drawing.CubicBezierTo(control0, control1, end);
            drawing.EndFigure(isClosed: false);
        }

        context.DrawGeometry(brush: null, pen, geometry);
    }

    private static bool CanSnapshot(Pen pen, out Color color)
    {
        if (pen.Brush is ISolidColorBrush { Opacity: 1 } brush
            && pen.DashStyle is null && pen.LineCap == PenLineCap.Flat && pen.LineJoin == PenLineJoin.Miter
            && double.IsFinite(pen.Thickness) && pen.Thickness > 0 && double.IsFinite(pen.MiterLimit))
        {
            color = brush.Color;
            return true;
        }

        color = default;
        return false;
    }

    private sealed class PrimitiveOperation(
        Point start, Point control0, Point control1, Point end, bool cubic,
        Color color, double thickness, double miterLimit, bool antialias) : ICustomDrawOperation
    {
        private readonly Point _start = start;
        private readonly Point _control0 = control0;
        private readonly Point _control1 = control1;
        private readonly Point _end = end;
        private readonly bool _cubic = cubic;
        private readonly Color _color = color;
        private readonly double _thickness = thickness;
        private readonly double _miterLimit = miterLimit;
        private readonly bool _antialias = antialias;

        public Rect Bounds
        {
            get
            {
                double left = Math.Min(Math.Min(_start.X, _control0.X), Math.Min(_control1.X, _end.X));
                double top = Math.Min(Math.Min(_start.Y, _control0.Y), Math.Min(_control1.Y, _end.Y));
                double right = Math.Max(Math.Max(_start.X, _control0.X), Math.Max(_control1.X, _end.X));
                double bottom = Math.Max(Math.Max(_start.Y, _control0.Y), Math.Max(_control1.Y, _end.Y));
                return new Rect(left, top, right - left, bottom - top).Inflate((_thickness / 2) + 1);
            }
        }

        public bool HitTest(Point point) => false;

        public bool Equals(ICustomDrawOperation? other)
            => other is PrimitiveOperation operation && _start == operation._start && _control0 == operation._control0
                && _control1 == operation._control1 && _end == operation._end && _cubic == operation._cubic
                && _color == operation._color && _thickness == operation._thickness && _miterLimit == operation._miterLimit
                && _antialias == operation._antialias;

        public void Dispose()
        {
            // Immediate RenderTargetBitmap drawing does not retire retained operations via Dispose.
            // Native paths and paints are therefore owned and disposed by each Render invocation.
        }

        public void Render(ImmediateDrawingContext context)
        {
            if (context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) is ISkiaSharpApiLeaseFeature feature)
            {
                using ISkiaSharpApiLease lease = feature.Lease();
                using SKPath path = new();
                path.MoveTo(ToSkia(_start));
                if (_cubic)
                {
                    path.CubicTo(ToSkia(_control0), ToSkia(_control1), ToSkia(_end));
                }
                else
                {
                    path.LineTo(ToSkia(_end));
                }

                using SKPaint paint = new()
                {
                    Color = new SKColor(_color.R, _color.G, _color.B, (byte)(_color.A * lease.CurrentOpacity)),
                    IsAntialias = _antialias,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = (float)_thickness,
                    StrokeCap = SKStrokeCap.Butt,
                    StrokeJoin = SKStrokeJoin.Miter,
                    StrokeMiter = (float)_miterLimit,
                };
                using SKPath widened = paint.GetFillPath(path);
                paint.Style = SKPaintStyle.Fill;
                if (_antialias && IsNativePixelContext(lease))
                {
                    DrawUnclippedRaster(lease.SkCanvas, widened, paint);
                }
                else
                {
                    lease.SkCanvas.DrawPath(widened, paint);
                }

                return;
            }

            DrawFallback(context);
        }

        private void DrawFallback(ImmediateDrawingContext context)
        {
            ImmutablePen pen = new(new ImmutableSolidColorBrush(_color), _thickness, dashStyle: null,
                PenLineCap.Flat, PenLineJoin.Miter, _miterLimit);
            if (!_cubic)
            {
                context.DrawLine(pen, _start, _end);
                return;
            }

            // The public immediate API has no geometry draw method. A non-Skia backend renders
            // the ordinary vector into an owned tile; only its canvas bounds are pixel-enclosed.
            // This is a supported fallback, not a claim of native96 GDI raster correspondence.
            Rect bounds = Bounds;
            double left = Math.Floor(bounds.Left);
            double top = Math.Floor(bounds.Top);
            Rect tileBounds = new(left, top, Math.Ceiling(bounds.Right) - left, Math.Ceiling(bounds.Bottom) - top);
            using RenderTargetBitmap tile = new(new PixelSize((int)tileBounds.Width, (int)tileBounds.Height), new Vector(96, 96));
            StreamGeometry geometry = new();
            using (StreamGeometryContext drawing = geometry.Open())
            {
                drawing.BeginFigure(_start, isFilled: false);
                drawing.CubicBezierTo(_control0, _control1, _end);
                drawing.EndFigure(isClosed: false);
            }

            using (DrawingContext drawing = tile.CreateDrawingContext())
            using (drawing.PushTransform(Matrix.CreateTranslation(-left, -top)))
            {
                drawing.DrawGeometry(brush: null, pen, geometry);
            }

            context.DrawBitmap(tile, tileBounds);
        }

        private void DrawUnclippedRaster(SKCanvas canvas, SKPath path, SKPaint paint)
        {
            Rect bounds = Bounds;
            int left = (int)Math.Floor(bounds.Left);
            int top = (int)Math.Floor(bounds.Top);
            int width = (int)Math.Ceiling(bounds.Right) - left;
            int height = (int)Math.Ceiling(bounds.Bottom) - top;
            using SKBitmap bitmap = new(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
            bitmap.Erase(SKColors.Transparent);
            using (SKCanvas raster = new(bitmap))
            {
                raster.Translate(-left, -top);
                raster.DrawPath(path, paint);
            }

            using SKImage image = SKImage.FromBitmap(bitmap);
            canvas.DrawImage(image, left, top, new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None), paint: null);
        }

        private static bool IsNativePixelContext(ISkiaSharpApiLease lease)
        {
            SKMatrix matrix = lease.SkCanvas.TotalMatrix;
            return lease.CurrentOpacity == 1
                && matrix.ScaleX == 1 && matrix.ScaleY == 1 && matrix.SkewX == 0 && matrix.SkewY == 0
                && matrix.Persp0 == 0 && matrix.Persp1 == 0 && matrix.Persp2 == 1
                && float.IsFinite(matrix.TransX) && matrix.TransX == MathF.Truncate(matrix.TransX)
                && float.IsFinite(matrix.TransY) && matrix.TransY == MathF.Truncate(matrix.TransY);
        }

        private static SKPoint ToSkia(Point point) => new((float)point.X, (float)point.Y);
    }
}
