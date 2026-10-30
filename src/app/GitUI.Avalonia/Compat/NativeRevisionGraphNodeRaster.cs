using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using GitUI.Theming;
using SkiaSharp;
using Color = Avalonia.Media.Color;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;
using Point = Avalonia.Point;
using Rect = Avalonia.Rect;
using SystemColorMode = GitExtensions.Shims.WinForms.SystemColorMode;

namespace GitUI.Compat;

/// <summary>
///  Preserves the original GDI+ coverage of a native96 revision circle without changing its layout.
/// </summary>
/// <remarks>
///  The fixed coverage is the original AA/HQ raster of the ten-pixel node and its twelve-pixel,
///  two-pixel-wide HEAD outline. Every fill alpha and RGB channel was measured against original
///  DrawItem in light, dark and custom themes. Only solid, unit-opacity nodes in an integral
///  native96 Skia context use it; other contexts retain ordinary antialiased vector rendering.
/// </remarks>
internal static class NativeRevisionGraphNodeRaster
{
    private const int NativeNodeDimension = 10;
    private const int OutlineInflation = 1;
    private const int OutlineThickness = 2;
    private const int OutlineOutset = OutlineInflation + (OutlineThickness / 2);
    private const int AntialiasAllowance = 1;
    private const int ChannelMaximum = byte.MaxValue;
    private const int ChannelRounding = ChannelMaximum / 2;
    private const int CoverageFractionBits = 8;
    private const int CoverageUnit = 1 << CoverageFractionBits;
    private const int CoverageRounding = CoverageUnit / 2;
    private const string NativeSourceOverProgram = """
        half4 main(half4 source, half4 destination)
        {
            const int channelMaximum = 255;
            const int sourceOverRounding = 127;
            int4 sourceBytes = int4(floor(float4(source) * float(channelMaximum) + 0.5));
            int4 destinationBytes = int4(floor(float4(destination) * float(channelMaximum) + 0.5));
            int4 result = sourceBytes + (destinationBytes * (channelMaximum - sourceBytes.a)
                + sourceOverRounding) / channelMaximum;
            return half4(clamp(float4(result), 0.0, float(channelMaximum)) / float(channelMaximum));
        }
        """;

    // One immutable, color-independent program lives with the process; no bitmap, brush,
    // color key or draw operation enters a cache. Each render owns its blender and images.
    private static readonly Lazy<SKRuntimeEffect?> NativeSourceOver = new(CreateNativeSourceOver);

    private static ReadOnlySpan<ushort> FillCoverage =>
    [
        0, 0, 24, 144, 192, 192, 152, 40, 0, 0,
        0, 80, 240, 256, 256, 256, 256, 248, 104, 0,
        32, 240, 256, 256, 256, 256, 256, 256, 256, 56,
        160, 256, 256, 256, 256, 256, 256, 256, 256, 192,
        208, 256, 256, 256, 256, 256, 256, 256, 256, 240,
        224, 256, 256, 256, 256, 256, 256, 256, 256, 248,
        176, 256, 256, 256, 256, 256, 256, 256, 256, 208,
        64, 256, 256, 256, 256, 256, 256, 256, 256, 104,
        0, 136, 256, 256, 256, 256, 256, 256, 184, 0,
        0, 0, 72, 208, 256, 256, 216, 120, 0, 0,
    ];

    private static ReadOnlySpan<ushort> OutlineCoverage =>
    [
        0, 0, 0, 0, 80, 136, 192, 192, 144, 96, 8, 0, 0, 0, 0,
        0, 0, 24, 184, 256, 256, 256, 256, 256, 256, 216, 56, 0, 0, 0,
        0, 32, 240, 256, 240, 136, 72, 64, 128, 216, 256, 256, 64, 0, 0,
        0, 192, 256, 192, 24, 0, 0, 0, 0, 8, 152, 256, 224, 8, 0,
        88, 256, 232, 24, 0, 0, 0, 0, 0, 0, 0, 208, 256, 120, 0,
        152, 256, 128, 0, 0, 0, 0, 0, 0, 0, 0, 72, 256, 192, 0,
        208, 256, 64, 0, 0, 0, 0, 0, 0, 0, 0, 16, 256, 248, 8,
        224, 256, 48, 0, 0, 0, 0, 0, 0, 0, 0, 8, 248, 256, 16,
        168, 256, 104, 0, 0, 0, 0, 0, 0, 0, 0, 48, 256, 208, 0,
        112, 256, 200, 0, 0, 0, 0, 0, 0, 0, 0, 160, 256, 144, 0,
        16, 232, 256, 128, 0, 0, 0, 0, 0, 0, 72, 256, 248, 32, 0,
        0, 72, 256, 256, 184, 56, 0, 0, 40, 152, 256, 256, 120, 0, 0,
        0, 0, 96, 240, 256, 256, 256, 248, 256, 256, 248, 120, 0, 0, 0,
        0, 0, 0, 16, 144, 208, 256, 256, 216, 160, 40, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 16, 24, 0, 0, 0, 0, 0, 0, 0,
    ];

    /// <summary>
    ///  Gets raw native WindowText, which the original HEAD pen uses independently of CSS overrides.
    /// </summary>
    internal static DrawingColor GetOutlineColor()
        => AvaloniaThemeResources.ResolveNativeSystemColor(ThemeModule.Settings.Theme.SystemColorMode == SystemColorMode.Dark, KnownColor.WindowText);

    /// <summary>
    ///  Records a node snapshot when its brush and source dimensions support the native raster.
    /// </summary>
    /// <returns>Whether the operation owns drawing this node, including its vector fallback.</returns>
    internal static bool TryDraw(DrawingContext context, IBrush brush, Rect bounds, bool hasOutline)
    {
        if (brush is not ISolidColorBrush { Opacity: 1 } solid
            || bounds.Width != NativeNodeDimension || bounds.Height != NativeNodeDimension)
        {
            return false;
        }

        Color outline = AvaloniaThemeResources.ToMediaColor(GetOutlineColor());
        using (context.PushRenderOptions(new RenderOptions { EdgeMode = EdgeMode.Antialias }))
        {
            context.Custom(new NodeOperation(bounds, solid.Color, outline, hasOutline));
        }

        return true;
    }

    private static SKRuntimeEffect? CreateNativeSourceOver()
    {
        SKRuntimeEffect? effect = SKRuntimeEffect.CreateBlender(NativeSourceOverProgram, out string? errors);
        if (effect is null)
        {
            Trace.WriteLine($"Native revision graph blending is unavailable; using antialiased vectors: {errors}");
        }

        return effect;
    }

    private static SKBitmap CreateBitmap(Color color, bool outline)
    {
        int dimension = outline ? NativeNodeDimension + (OutlineOutset * 2) + AntialiasAllowance : NativeNodeDimension;
        SKBitmap bitmap = new(new SKImageInfo(dimension, dimension, SKColorType.Bgra8888, SKAlphaType.Premul));
        try
        {
            byte[] pixels = new byte[bitmap.RowBytes * dimension];
            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    int weight = outline ? OutlineCoverage[(y * dimension) + x] : FillCoverage[(y * dimension) + x];
                    int offset = (y * bitmap.RowBytes) + (x * sizeof(int));
                    pixels[offset] = ApplyCoverage(color.B, color.A, weight);
                    pixels[offset + 1] = ApplyCoverage(color.G, color.A, weight);
                    pixels[offset + 2] = ApplyCoverage(color.R, color.A, weight);
                    pixels[offset + 3] = ApplyCoverage(byte.MaxValue, color.A, weight);
                }
            }

            Marshal.Copy(pixels, 0, bitmap.GetPixels(), pixels.Length);
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    private static byte ApplyCoverage(byte channel, byte alpha, int weight)
    {
        int premultiplied = ((channel * alpha) + ChannelRounding) / ChannelMaximum;
        return (byte)(((premultiplied * weight) + CoverageRounding) >> CoverageFractionBits);
    }

    private sealed class NodeOperation(Rect nodeBounds, Color fill, Color outline, bool hasOutline) : ICustomDrawOperation
    {
        private readonly Rect _nodeBounds = nodeBounds;
        private readonly Color _fill = fill;
        private readonly Color _outline = outline;
        private readonly bool _hasOutline = hasOutline;

        public Rect Bounds => _nodeBounds.Inflate(_hasOutline ? OutlineOutset + AntialiasAllowance : AntialiasAllowance);

        public bool HitTest(Point point) => false;

        public bool Equals(ICustomDrawOperation? other)
            => other is NodeOperation operation && _nodeBounds == operation._nodeBounds
                && _fill == operation._fill && _outline == operation._outline && _hasOutline == operation._hasOutline;

        public void Dispose()
        {
            // A retained operation owns only value snapshots, never a bitmap or a shared brush.
        }

        public void Render(ImmediateDrawingContext context)
        {
            if (TryRenderNative(context))
            {
                return;
            }

            if (TryRenderVector(context))
            {
                return;
            }

            context.DrawEllipse(new ImmutableSolidColorBrush(_fill), pen: null, _nodeBounds.Center, _nodeBounds.Width / 2, _nodeBounds.Height / 2);
            if (_hasOutline)
            {
                Rect bounds = _nodeBounds.Inflate(OutlineInflation);
                context.DrawEllipse(brush: null, new ImmutablePen(new ImmutableSolidColorBrush(_outline), OutlineThickness), bounds.Center, bounds.Width / 2, bounds.Height / 2);
            }
        }

        private bool TryRenderNative(ImmediateDrawingContext context)
        {
            if (context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) is not ISkiaSharpApiLeaseFeature feature)
            {
                return false;
            }

            using ISkiaSharpApiLease lease = feature.Lease();

            // Avalonia's public transform excludes its hidden DPI transform. Check the actual
            // canvas on every repaint so a retained operation also survives monitor/opacity changes.
            SKMatrix matrix = lease.SkCanvas.TotalMatrix;
            bool native = lease.CurrentOpacity == 1
                && matrix.ScaleX == 1 && matrix.ScaleY == 1 && matrix.SkewX == 0 && matrix.SkewY == 0
                && matrix.Persp0 == 0 && matrix.Persp1 == 0 && matrix.Persp2 == 1
                && IsInteger(matrix.TransX + _nodeBounds.X) && IsInteger(matrix.TransY + _nodeBounds.Y);
            if (!native || NativeSourceOver.Value is not { } effect)
            {
                return false;
            }

            using SKBlender? blender = effect.ToBlender();
            if (blender is null)
            {
                return false;
            }

            using SKPaint paint = new() { IsAntialias = false, Blender = blender };
            DrawStage(_fill, outline: false, _nodeBounds.X, _nodeBounds.Y);
            if (_hasOutline)
            {
                // Preserve the original stages over existing lanes. GDI rounds inverse alpha
                // over 255; Skia's ordinary image source-over truncates it over 256 instead.
                DrawStage(_outline, outline: true, _nodeBounds.X - OutlineOutset, _nodeBounds.Y - OutlineOutset);
            }

            return true;

            void DrawStage(Color color, bool outline, double x, double y)
            {
                // Images are consumed during this lease, not retained on the operation.
                using SKBitmap bitmap = CreateBitmap(color, outline);
                using SKImage image = SKImage.FromBitmap(bitmap);
                lease.SkCanvas.DrawImage(image, (float)x, (float)y,
                    new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None), paint);
            }
        }

        private bool TryRenderVector(ImmediateDrawingContext context)
        {
            if (context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) is not ISkiaSharpApiLeaseFeature feature)
            {
                return false;
            }

            using ISkiaSharpApiLease lease = feature.Lease();
            using SKPaint paint = new()
            {
                Color = WithOpacity(_fill, lease.CurrentOpacity),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            };
            lease.SkCanvas.DrawOval(ToSkia(_nodeBounds), paint);
            if (_hasOutline)
            {
                paint.Color = WithOpacity(_outline, lease.CurrentOpacity);
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = OutlineThickness;
                lease.SkCanvas.DrawOval(ToSkia(_nodeBounds.Inflate(OutlineInflation)), paint);
            }

            return true;
        }

        private static SKColor WithOpacity(Color color, double opacity)
            => new(color.R, color.G, color.B, (byte)(color.A * opacity));

        private static SKRect ToSkia(Rect bounds)
            => new((float)bounds.Left, (float)bounds.Top, (float)bounds.Right, (float)bounds.Bottom);

        private static bool IsInteger(double value) => double.IsFinite(value) && value == Math.Truncate(value);
    }
}
