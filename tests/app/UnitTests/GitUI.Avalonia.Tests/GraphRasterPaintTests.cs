using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.SceneGraph;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;
using NSubstitute;
using SkiaSharp;
using Color = Avalonia.Media.Color;
using ColorHelper = GitExtUtils.GitUI.Theming.ColorHelper;
using DrawingColor = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using KnownColor = System.Drawing.KnownColor;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class GraphRasterPaintTests
{
    private const int Width = 80;
    private static readonly ObjectId NodeId = ObjectId.Parse(new string('1', ObjectId.Sha1CharCount));
    private static readonly ObjectId OtherId = ObjectId.Parse(new string('2', ObjectId.Sha1CharCount));

    [AvaloniaTest]
    public void DrawItem_should_choose_primitive_smoothing_independently_of_the_callers_edge_mode(
        [Values(false, true)] bool square,
        [Values(false, true)] bool head,
        [Values(13, 23, 24, 26)] int rowHeight)
    {
        using ThemeScope theme = new("light");
        NodeControl normal = new(CreateRow(square, lane: 0), rowHeight, head);
        NodeControl aliasedOwner = new(CreateRow(square, lane: 0), rowHeight, head);
        RenderOptions.SetEdgeMode(aliasedOwner, EdgeMode.Aliased);
        using RenderTargetBitmap expected = Render(normal, new PixelSize(Width, rowHeight));
        using RenderTargetBitmap actual = Render(aliasedOwner, new PixelSize(Width, rowHeight));
        using SKBitmap expectedPixels = Decode(expected);
        using SKBitmap actualPixels = Decode(actual);
        ReadPixels(actualPixels).Should().Equal(ReadPixels(expectedPixels),
            "the source explicitly chooses None for squares and AntiAlias for circles, rather than inheriting owner smoothing");
        Retain(actual, new { square, head, rowHeight, ownerEdgeMode = "Aliased", nodeLane = 0 });
    }

    [AvaloniaTest]
    public void DrawItem_should_leave_the_following_painters_edge_mode_unchanged(
        [Values(false, true)] bool square,
        [Values(false, true)] bool aliasedOwner)
    {
        using ThemeScope theme = new("light");
        NodeControl control = new(CreateRow(square, lane: 0), rowHeight: 26, head: true)
        {
            PaintFollowingPrimitive = true,
        };
        TailControl baseline = new();
        if (aliasedOwner)
        {
            RenderOptions.SetEdgeMode(control, EdgeMode.Aliased);
            RenderOptions.SetEdgeMode(baseline, EdgeMode.Aliased);
        }

        using RenderTargetBitmap expected = Render(baseline, new PixelSize(Width, 26));
        using RenderTargetBitmap actual = Render(control, new PixelSize(Width, 26));
        using SKBitmap expectedPixels = Decode(expected);
        using SKBitmap actualPixels = Decode(actual);
        for (int y = 0; y < actualPixels.Height; y++)
        {
            for (int x = 40; x < actualPixels.Width; x++)
            {
                actualPixels.GetPixel(x, y).Should().Be(expectedPixels.GetPixel(x, y),
                    "primitive-specific smoothing must not leak into another painter at ({0},{1})", x, y);
            }
        }

        Retain(actual, new { square, head = true, rowHeight = 26, aliasedOwner, followingPrimitive = true });
    }

    [AvaloniaTest]
    public void SegmentRenderer_should_choose_vertical_or_curved_smoothing_independently_of_the_callers_edge_mode(
        [Values(false, true)] bool vertical,
        [Values(false, true)] bool diagonals,
        [Values(13, 23, 24, 26)] int rowHeight)
    {
        WithGraphConfig(diagonals, config =>
        {
            SegmentControl normal = new(config, rowHeight, vertical);
            SegmentControl aliasedOwner = new(config, rowHeight, vertical);
            RenderOptions.SetEdgeMode(aliasedOwner, EdgeMode.Aliased);
            using RenderTargetBitmap expected = Render(normal, new PixelSize(Width, rowHeight));
            using RenderTargetBitmap actual = Render(aliasedOwner, new PixelSize(Width, rowHeight));
            using SKBitmap expectedPixels = Decode(expected);
            using SKBitmap actualPixels = Decode(actual);
            ReadPixels(actualPixels).Should().Equal(ReadPixels(expectedPixels),
                "the source explicitly disables smoothing for a vertical segment and enables it for a curve");
            Retain(actual, new { vertical, diagonals, rowHeight, ownerEdgeMode = "Aliased" });
        });
    }

    [AvaloniaTest]
    public void DrawItem_should_repaint_its_actual_row_with_live_theme_colors_without_changing_the_lane_or_clip()
    {
        RevisionGraphRow row = CreateRow(square: true, lane: 2);
        NodeControl control = new(row, rowHeight: 26, head: true);
        List<Color> fills = [];
        List<Color> outlines = [];
        foreach (string name in new[] { "light", "dark", "custom" })
        {
            using ThemeScope theme = new(name);
            control.InvalidateVisual();
            using RenderTargetBitmap frame = Render(control, new PixelSize(Width, 26));
            using SKBitmap pixels = Decode(frame);
            Color fill = AvaloniaThemeResources.ToMediaColor(RevisionGraphLaneColor.NonRelativeColor);
            Color outline = AvaloniaThemeResources.ToMediaColor(NativeRevisionGraphNodeRaster.GetOutlineColor());
            int centerX = (2 * GraphRenderer.LaneWidth) + (GraphRenderer.LaneWidth / 2);
            pixels.GetPixel(centerX, 13).Should().Be(Pixel(fill));
            pixels.GetPixel(centerX - (GraphRenderer.NodeDimension / 2) - 2, 13).Should().Be(Pixel(outline));
            row.GetCurrentRevisionLane().Should().Be(2);
            fills.Add(fill);
            outlines.Add(outline);
            Retain(frame, new { theme = name, square = true, head = true, nodeLane = 2, rowHeight = 26, fill = fill.ToString(), outline = outline.ToString() });
        }

        fills.Should().OnlyHaveUniqueItems();
        outlines.Should().Equal(Colors.Black, Color.FromRgb(240, 240, 240), Color.FromRgb(240, 240, 240));
    }

    [AvaloniaTest]
    public void DrawItem_should_keep_native_head_outline_through_cold_and_live_css_changes(
        [Values("light", "dark", "custom")] string name, [Values(false, true)] bool square)
    {
        using ThemeScope theme = new(name);
        NodeControl original = new(CreateRow(square, lane: 0), rowHeight: 26, head: true);
        using RenderTargetBitmap baseline = Render(original, new PixelSize(Width, 26));
        using SKBitmap baselinePixels = Decode(baseline);
        DrawingColor raw = NativeRevisionGraphNodeRaster.GetOutlineColor();
        raw.ToArgb().Should().Be(name == "light" ? DrawingColor.Black.ToArgb() : DrawingColor.FromArgb(240, 240, 240).ToArgb());
        foreach ((DrawingColor color, bool cold) in new[]
        {
            (DrawingColor.FromArgb(181, 17, 203), true),
            (DrawingColor.FromArgb(13, 197, 83), false),
        })
        {
            using ThemeScope changed = new(name, color);
            ThemeModule.Settings.Theme.GetColor(KnownColor.WindowText).ToArgb().Should().Be(color.ToArgb());
            NativeRevisionGraphNodeRaster.GetOutlineColor().Should().Be(raw);
            NodeControl control = cold ? new(CreateRow(square, lane: 0), rowHeight: 26, head: true) : original;
            control.InvalidateVisual();
            using RenderTargetBitmap frame = Render(control, new PixelSize(Width, 26));
            using SKBitmap pixels = Decode(frame);
            ReadPixels(pixels).Should().Equal(ReadPixels(baselinePixels),
                "the source uses raw native WindowText rather than introducing a configurable CSS HEAD pen");
            Retain(frame, new { name, square, cold, cssWindowText = color.ToArgb(), rawWindowText = raw.ToArgb() });
        }
    }

    [AvaloniaTest]
    [TestCase(120, 1, 0, 0, 1, false)]
    [TestCase(120, 1, 0, 0, 1, true)]
    [TestCase(144, 1, 0, 0, 1, false)]
    [TestCase(144, 1, 0, 0, 1, true)]
    [TestCase(192, 1, 0, 0, 1, false)]
    [TestCase(192, 1, 0, 0, 1, true)]
    [TestCase(96, 1.25, 0, 0, 1, false)]
    [TestCase(96, 1.25, 0, 0, 1, true)]
    [TestCase(96, 1, 0.25, 0, 1, false)]
    [TestCase(96, 1, 0.25, 0, 1, true)]
    [TestCase(96, 1, 0, 0.25, 1, false)]
    [TestCase(96, 1, 0, 0.25, 1, true)]
    [TestCase(96, 1, 0, 0, 0.5, false)]
    [TestCase(96, 1, 0, 0, 0.5, true)]
    public void Native_node_operation_should_use_vector_fallback_outside_its_actual_native_pixel_context(
        int dpi, double scale, double translateX, double translateY, double opacity, bool ownerAliased)
    {
        using ThemeScope theme = new("dark");
        Rect bounds = new(3, 7, 10, 10);
        Matrix transform = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(translateX, translateY);
        RasterNodeControl actual = new(Brushes.Red, bounds, head: true, useAdapter: true, transform, opacity);
        RasterNodeControl expected = new(Brushes.Red, bounds, head: true, useAdapter: false, transform, opacity);
        if (ownerAliased)
        {
            RenderOptions.SetEdgeMode(actual, EdgeMode.Aliased);
        }

        PixelSize size = new(dpi / 2, dpi / 2);
        using RenderTargetBitmap actualFrame = Render(actual, size, dpi);
        using RenderTargetBitmap expectedFrame = Render(expected, size, dpi);
        using SKBitmap actualPixels = Decode(actualFrame);
        using SKBitmap expectedPixels = Decode(expectedFrame);
        actual.UsedNativeOperation.Should().BeTrue("support for the physical context is evaluated during each actual render, not at record time");
        ReadPixels(actualPixels).Should().Equal(ReadPixels(expectedPixels),
            "higher DPI, nonunit transforms, fractional pixel positions and opacity must retain ordinary antialiased vectors");
        Retain(actualFrame, new { dpi, scale, translateX, translateY, opacity, ownerAliased, vectorFallback = true });
    }

    [AvaloniaTest]
    [TestCase("gradient")]
    [TestCase("brush-opacity")]
    [TestCase("dimension")]
    public void Native_node_adapter_should_decline_brushes_and_dimensions_that_the_native_mask_does_not_describe(string reason)
    {
        using ThemeScope theme = new("light");
        IBrush fill = reason == "gradient"
            ? new LinearGradientBrush { GradientStops = [new GradientStop(Colors.Red, 0), new GradientStop(Colors.Blue, 1)] }
            : new SolidColorBrush(Colors.Red) { Opacity = reason == "brush-opacity" ? 0.5 : 1 };
        Rect bounds = new(3, 7, reason == "dimension" ? 12 : 10, 10);
        RasterNodeControl actual = new(fill, bounds, head: true, useAdapter: true);
        RasterNodeControl expected = new(fill, bounds, head: true, useAdapter: false);
        using RenderTargetBitmap actualFrame = Render(actual, new PixelSize(48, 48));
        using RenderTargetBitmap expectedFrame = Render(expected, new PixelSize(48, 48));
        using SKBitmap actualPixels = Decode(actualFrame);
        using SKBitmap expectedPixels = Decode(expectedFrame);
        actual.UsedNativeOperation.Should().BeFalse();
        ReadPixels(actualPixels).Should().Equal(ReadPixels(expectedPixels));
    }

    [AvaloniaTest]
    public void Native_node_operation_should_compare_value_snapshots_and_retire_without_disposing_shared_resources()
    {
        using ThemeScope theme = new("light");
        Rect bounds = new(3, 7, 10, 10);
        Color fill = Colors.Red;
        Color outline = Colors.Black;
        using ICustomDrawOperation operation = CreateNodeOperation(bounds, fill, outline, head: true);
        using ICustomDrawOperation same = CreateNodeOperation(bounds, fill, outline, head: true);
        operation.Equals(same).Should().BeTrue();
        foreach (ICustomDrawOperation changed in new[]
        {
            CreateNodeOperation(bounds.Translate(new Vector(1, 0)), fill, outline, head: true),
            CreateNodeOperation(bounds, Colors.Blue, outline, head: true),
            CreateNodeOperation(bounds, fill, Colors.White, head: true),
            CreateNodeOperation(bounds, fill, outline, head: false),
        })
        {
            using (changed)
            {
                operation.Equals(changed).Should().BeFalse();
            }
        }

        operation.HitTest(bounds.Center).Should().BeFalse();
        operation.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Should().NotContain(field => typeof(IDisposable).IsAssignableFrom(field.FieldType),
                "an immediate RenderTargetBitmap does not retire custom operations, so they must retain values rather than owned bitmaps or leases");
        using RenderTargetBitmap before = Render(new OperationControl(operation), new PixelSize(24, 24));
        operation.Dispose();
        operation.Dispose();
        using RenderTargetBitmap after = Render(new OperationControl(operation), new PixelSize(24, 24));
        using SKBitmap beforePixels = Decode(before);
        using SKBitmap afterPixels = Decode(after);
        ReadPixels(afterPixels).Should().Equal(ReadPixels(beforePixels));
    }

    [AvaloniaTest]
    public void Native_node_immediate_rendering_should_release_two_thousand_owned_frames_and_operation_owners()
    {
        using ThemeScope theme = new("dark");
        List<WeakReference> owners = [];
        for (int iteration = 0; iteration < 2000; iteration++)
        {
            owners.AddRange(RenderTransientNode());
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        owners.Should().OnlyContain(reference => !reference.IsAlive,
            "the raster adapter must not introduce a process-lifetime image cache or retain disposed frame/control owners");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] RenderTransientNode()
    {
        RasterNodeControl control = new(Brushes.Red, new Rect(3, 7, 10, 10), head: true, useAdapter: true);
        using RenderTargetBitmap frame = Render(control, new PixelSize(24, 24));
        return [new WeakReference(control), new WeakReference(frame)];
    }

    private static ICustomDrawOperation CreateNodeOperation(Rect bounds, Color fill, Color outline, bool head)
    {
        Type operation = typeof(NativeRevisionGraphNodeRaster).GetNestedType("NodeOperation", BindingFlags.NonPublic)
            ?? throw new AssertionException("The bounded node operation is missing.");
        return Activator.CreateInstance(operation, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null, [bounds, fill, outline, head], culture: null) as ICustomDrawOperation
            ?? throw new AssertionException("The actual node operation must be constructible from value snapshots.");
    }

    private sealed class OperationControl(ICustomDrawOperation operation, Rect? clip = null) : Control
    {
        public override void Render(DrawingContext context)
        {
            using (context.PushClip(clip ?? new Rect(Bounds.Size)))
            {
                context.Custom(operation);
            }
        }
    }

    private sealed class RasterNodeControl(IBrush fill, Rect bounds, bool head, bool useAdapter,
        Matrix? transform = null, double opacity = 1) : Control
    {
        public bool UsedNativeOperation { get; private set; }

        public override void Render(DrawingContext context)
        {
            using (context.PushTransform(transform ?? Matrix.Identity))
            using (context.PushOpacity(opacity))
            using (context.PushRenderOptions(new RenderOptions { EdgeMode = EdgeMode.Antialias }))
            {
                UsedNativeOperation = useAdapter && NativeRevisionGraphNodeRaster.TryDraw(context, fill, bounds, head);
                if (!UsedNativeOperation)
                {
                    context.DrawEllipse(fill, pen: null, bounds);
                    if (head)
                    {
                        context.DrawEllipse(brush: null,
                            new Pen(new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(NativeRevisionGraphNodeRaster.GetOutlineColor())), 2),
                            bounds.Inflate(1));
                    }
                }
            }
        }
    }

    [AvaloniaTest]
    public void Primitive_operation_should_compare_value_snapshots_and_retire_without_owning_native_resources()
    {
        object[] values = [new Point(4, 2), new Point(4, 10), new Point(24, 14), new Point(24, 22), true, Colors.Red, 2d, 10d, true];
        using ICustomDrawOperation operation = CreatePrimitiveOperation(values);
        using ICustomDrawOperation same = CreatePrimitiveOperation(values);
        operation.Equals(same).Should().BeTrue();
        for (int index = 0; index < values.Length; index++)
        {
            object[] altered = (object[])values.Clone();
            altered[index] = altered[index] switch
            {
                Point point => point + new Vector(1, 0),
                bool flag => !flag,
                Color => Colors.Blue,
                double number => number + 1,
                _ => throw new AssertionException("Every graph operation field must be a recognized value snapshot."),
            };
            using ICustomDrawOperation changed = CreatePrimitiveOperation(altered);
            operation.Equals(changed).Should().BeFalse("a changed source field at index{0} must invalidate retained paint", index);
        }

        operation.HitTest(new Point(8, 12)).Should().BeFalse();
        operation.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Should().NotContain(field => typeof(IDisposable).IsAssignableFrom(field.FieldType));
        using RenderTargetBitmap before = Render(new OperationControl(operation), new PixelSize(32, 26));
        operation.Dispose();
        operation.Dispose();
        using RenderTargetBitmap after = Render(new OperationControl(operation), new PixelSize(32, 26));
        using SKBitmap beforePixels = Decode(before);
        using SKBitmap afterPixels = Decode(after);
        ReadPixels(beforePixels).Should().Contain(value => value != 0);
        ReadPixels(afterPixels).Should().Equal(ReadPixels(beforePixels));
    }

    [AvaloniaTest]
    public void Primitive_drawing_should_keep_unsupported_pens_on_the_ordinary_vector_path(
        [Values(false, true)] bool cubic, [Values("gradient", "dashed")] string kind)
    {
        IBrush brush = kind == "gradient"
            ? new LinearGradientBrush { GradientStops = [new GradientStop(Colors.Red, 0), new GradientStop(Colors.Blue, 1)] }
            : Brushes.Red;
        Pen pen = new(brush, 2);
        if (kind == "dashed")
        {
            pen.DashStyle = new DashStyle([2, 1], offset: 0);
        }

        PrimitiveControl actual = new(pen, cubic, useAdapter: true);
        PrimitiveControl expected = new(pen, cubic, useAdapter: false);
        using RenderTargetBitmap actualFrame = Render(actual, new PixelSize(32, 26));
        using RenderTargetBitmap expectedFrame = Render(expected, new PixelSize(32, 26));
        using SKBitmap actualPixels = Decode(actualFrame);
        using SKBitmap expectedPixels = Decode(expectedFrame);
        ReadPixels(actualPixels).Should().Contain(value => value != 0);
        ReadPixels(actualPixels).Should().Equal(ReadPixels(expectedPixels),
            "unsupported pens must keep their real gradient/dash behavior instead of being snapshotted as a solid stroke");
    }

    [AvaloniaTest]
    [TestCase(23)]
    [TestCase(26)]
    public void Primitive_antialiasing_should_keep_identical_pixels_when_its_cache_viewport_is_clipped(int rowHeight)
    {
        int center = rowHeight / 2;
        double middle = ((center * 2) + rowHeight) / 2d;
        object[] values = [new Point(8, center), new Point(8, middle), new Point(24, middle), new Point(24, center + rowHeight),
            true, Colors.Red, 2d, 10d, true];
        using ICustomDrawOperation operation = CreatePrimitiveOperation(values);
        PixelRect clip = new(5, 2, 25, rowHeight - 4);
        using RenderTargetBitmap fullFrame = Render(new OperationControl(operation), new PixelSize(48, rowHeight));
        using RenderTargetBitmap clippedFrame = Render(new OperationControl(operation,
            new Rect(clip.X, clip.Y, clip.Width, clip.Height)), new PixelSize(48, rowHeight));
        using SKBitmap fullPixels = Decode(fullFrame);
        using SKBitmap clippedPixels = Decode(clippedFrame);
        Retain(clippedFrame, new { rowHeight, clip, cubic = true, native96ClipInvariant = true });
        for (int y = 0; y < rowHeight; y++)
        {
            for (int x = 0; x < fullPixels.Width; x++)
            {
                SKColor expected = clip.ContainsExclusive(new PixelPoint(x, y)) ? fullPixels.GetPixel(x, y) : default;
                clippedPixels.GetPixel(x, y).Should().Be(expected,
                    "the native96 AA primitive must be rasterized before the hard cache viewport clip at ({0},{1})", x, y);
            }
        }
    }

    [AvaloniaTest]
    [TestCase(1, 0)]
    [TestCase(1.25, 0.25)]
    public void Primitive_drawing_should_preserve_backend_opacity_truncation_under_its_actual_transform(double scale, double translate)
    {
        Matrix transform = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(translate, translate);
        Pen pen = new(Brushes.Red, 2);
        PrimitiveControl actual = new(pen, cubic: false, useAdapter: true, transform, opacity: 0.5);
        PrimitiveControl expected = new(pen, cubic: false, useAdapter: false, transform, opacity: 0.5);
        using RenderTargetBitmap actualFrame = Render(actual, new PixelSize(48, 48));
        using RenderTargetBitmap expectedFrame = Render(expected, new PixelSize(48, 48));
        using SKBitmap actualPixels = Decode(actualFrame);
        using SKBitmap expectedPixels = Decode(expectedFrame);
        Point center = transform.Transform(new Point(14, 12));
        int x = (int)Math.Floor(center.X);
        int y = (int)Math.Floor(center.Y);
        expectedPixels.GetPixel(x, y).Alpha.Should().Be(127, "the existing backend truncates 255 times0.5 rather than rounding it to128");
        actualPixels.GetPixel(x, y).Should().Be(expectedPixels.GetPixel(x, y));
    }

    [AvaloniaTest]
    public void Primitive_immediate_rendering_should_release_owned_frames_and_value_operations()
    {
        List<WeakReference> owners = [];
        for (int iteration = 0; iteration < 128; iteration++)
        {
            owners.AddRange(RenderTransientPrimitive());
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        owners.Should().OnlyContain(reference => !reference.IsAlive);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] RenderTransientPrimitive()
    {
        using ICustomDrawOperation operation = CreatePrimitiveOperation();
        OperationControl control = new(operation);
        using RenderTargetBitmap frame = Render(control, new PixelSize(32, 26));
        return [new WeakReference(operation), new WeakReference(control), new WeakReference(frame)];
    }

    private static ICustomDrawOperation CreatePrimitiveOperation(object[]? values = null)
    {
        values ??= [new Point(4, 2), new Point(4, 10), new Point(24, 14), new Point(24, 22), true, Colors.Red, 2d, 10d, true];
        Type type = typeof(RevisionGraphPrimitiveDrawing).GetNestedType("PrimitiveOperation", BindingFlags.NonPublic)
            ?? throw new AssertionException("The graph primitive operation is missing.");
        return Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null, values, culture: null) as ICustomDrawOperation
            ?? throw new AssertionException("The actual graph operation must be constructible from value snapshots.");
    }

    private sealed class PrimitiveControl(Pen pen, bool cubic, bool useAdapter, Matrix? transform = null, double opacity = 1) : Control
    {
        public override void Render(DrawingContext context)
        {
            using DrawingContext.PushedState pushedTransform = context.PushTransform(transform ?? Matrix.Identity);
            using DrawingContext.PushedState pushedOpacity = context.PushOpacity(opacity);
            Point start = new(4, 2);
            Point control0 = new(4, 10);
            Point control1 = new(24, 14);
            Point end = new(24, 22);
            if (useAdapter)
            {
                if (cubic)
                {
                    RevisionGraphPrimitiveDrawing.DrawBezier(context, pen, start, control0, control1, end);
                }
                else
                {
                    RevisionGraphPrimitiveDrawing.DrawLine(context, pen, start, end, antialias: true);
                }

                return;
            }

            if (!cubic)
            {
                context.DrawLine(pen, start, end);
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
    }

    private static RevisionGraphRow CreateRow(bool square, int lane)
    {
        GitRevision revision = new(NodeId)
        {
            Refs = square ? [new GitRef(Substitute.For<IGitModule>(), NodeId, "refs/heads/main")] : [],
        };
        RevisionGraphRevision node = new(NodeId, guessScore: 0) { GitRevision = revision };
        RevisionGraphRow row = new(node, [], mergeGraphLanesHavingCommonParent: false);
        row.GetCurrentRevisionLane().Should().Be(0);
        row.MoveLanesRight(fromLane: 0, by: lane);
        return row;
    }

    private static RenderTargetBitmap Render(Control control, PixelSize size, int dpi = 96)
    {
        Size logicalSize = new(size.Width * 96d / dpi, size.Height * 96d / dpi);
        control.Width = logicalSize.Width;
        control.Height = logicalSize.Height;
        control.Measure(logicalSize);
        control.Arrange(new Rect(logicalSize));
        RenderTargetBitmap frame = new(size, new Vector(dpi, dpi));
        frame.Render(control);
        return frame;
    }

    private static void WithGraphConfig(bool diagonals, Action<RevisionGraphConfig> action)
    {
        string root = Path.Combine(Path.GetTempPath(), "GitExtensions.GraphRasterPaint", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using GitExtSettingsCache cache = new(Path.Combine(root, "capture.settings"), autoSave: false);
            DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
            AppSettings.UsingContainer(settings, () =>
            {
                AppSettings.RenderGraphWithDiagonals.Value = diagonals;
                action(new RevisionGraphConfig());
            });
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static SKBitmap Decode(Bitmap bitmap)
    {
        using MemoryStream stream = new();
        bitmap.Save(stream, PngBitmapEncoderOptions.Default);
        return SKBitmap.Decode(stream.ToArray()) ?? throw new AssertionException("The actual graph frame must decode.");
    }

    private static IEnumerable<uint> ReadPixels(SKBitmap pixels)
    {
        for (int y = 0; y < pixels.Height; y++)
        {
            for (int x = 0; x < pixels.Width; x++)
            {
                yield return (uint)pixels.GetPixel(x, y);
            }
        }
    }

    private static SKColor Pixel(Color color) => new(color.R, color.G, color.B, color.A);

    private static void Retain(Bitmap bitmap, object details)
    {
        string? root = Environment.GetEnvironmentVariable("GITEXT_GRAPH_RASTER_EVIDENCE");
        if (string.IsNullOrWhiteSpace(root))
        {
            return;
        }

        string directory = Path.Combine(root, TestContext.CurrentContext.Test.ID);
        Directory.CreateDirectory(directory);
        string identity = Guid.NewGuid().ToString("N");
        using (FileStream output = File.Create(Path.Combine(directory, identity + ".png")))
        {
            bitmap.Save(output, PngBitmapEncoderOptions.Default);
        }

        File.WriteAllText(Path.Combine(directory, identity + ".json"), JsonSerializer.Serialize(new
        {
            test = TestContext.CurrentContext.Test.FullName,
            actualRenderer = "Avalonia headless Skia GraphRenderer/SegmentRenderer",
            comparisonScope = "Primitive smoothing and source allocation; antialiased GDI/Skia equality remains separately measured.",
            bitmap.PixelSize,
            details,
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed class NodeControl(RevisionGraphRow row, int rowHeight, bool head) : Control
    {
        public bool PaintFollowingPrimitive { get; init; }

        public override void Render(DrawingContext context)
        {
            using (context.PushClip(new Rect(0, 0, Bounds.Width, rowHeight)))
            {
                GraphRenderer.DrawItem(new RevisionGraphConfig(), context, index: 0, rowHeight,
                    index => index == 0 ? row : null, RevisionGraphDrawStyle.Normal, head ? NodeId : OtherId);
            }

            if (PaintFollowingPrimitive)
            {
                TailControl.Paint(context);
            }
        }
    }

    private sealed class SegmentControl(RevisionGraphConfig config, int rowHeight, bool vertical) : Control
    {
        public override void Render(DrawingContext context)
        {
            Pen pen = new(Brushes.Red, GraphRenderer.LaneLineWidth);
            SegmentRenderer renderer = new(new Context(config, context, pen, GraphRenderer.LaneWidth, rowHeight));
            renderer.DrawTo(new DrawingPoint(8, 0));
            renderer.DrawTo(new DrawingPoint(vertical ? 8 : 24, rowHeight));
        }
    }

    private sealed class TailControl : Control
    {
        public static void Paint(DrawingContext context)
            => context.DrawEllipse(Brushes.Red, pen: null, new Rect(48, 3, 15, 17));

        public override void Render(DrawingContext context) => Paint(context);
    }

    private sealed class ThemeScope : IDisposable
    {
        private static readonly PropertyInfo ThemeSettingsProperty = typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings))
            ?? throw new InvalidOperationException("The source-shaped theme boundary is missing.");
        private static readonly PropertyInfo ColorHelperSettingsProperty = typeof(ColorHelper).GetProperty(nameof(ColorHelper.ThemeSettings))
            ?? throw new InvalidOperationException("The shared color boundary is missing.");
        private readonly ThemeSettings _original = ThemeModule.Settings;
        private readonly ThemeSettings _originalHelper = ColorHelperSettingsProperty.GetValue(null) as ThemeSettings
            ?? throw new InvalidOperationException("The shared color boundary has no current settings.");

        public ThemeScope(string name, DrawingColor? windowTextOverride = null)
        {
            bool dark = name != "light";
            DrawingColor fill = name switch
            {
                "light" => DrawingColor.FromArgb(211, 211, 211),
                "dark" => DrawingColor.FromArgb(101, 103, 107),
                "custom" => DrawingColor.FromArgb(23, 137, 211),
                _ => throw new ArgumentOutOfRangeException(nameof(name)),
            };
            DrawingColor outline = windowTextOverride ?? name switch
            {
                "light" => DrawingColor.Black,
                "dark" => DrawingColor.White,
                _ => DrawingColor.FromArgb(211, 53, 101),
            };
            Theme theme = new(new Dictionary<AppColor, DrawingColor>
            {
                [AppColor.GraphNonRelativeBranch] = fill,
                [AppColor.GraphBranch1] = DrawingColor.Red,
                [AppColor.GraphBranch2] = DrawingColor.Green,
                [AppColor.GraphBranch3] = DrawingColor.Blue,
                [AppColor.GraphBranch4] = DrawingColor.Yellow,
                [AppColor.PanelBackground] = dark ? DrawingColor.FromArgb(32, 32, 32) : DrawingColor.White,
            }, new Dictionary<KnownColor, DrawingColor> { [KnownColor.WindowText] = outline }, new ThemeId("graph-raster-" + name));
            ThemeSettings settings = new(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: !dark);
            ThemeSettingsProperty.SetValue(null, settings);
            ColorHelper.ThemeSettings = settings;
        }

        public void Dispose()
        {
            ThemeSettingsProperty.SetValue(null, _original);
            ColorHelper.ThemeSettings = _originalHelper;
        }
    }
}
