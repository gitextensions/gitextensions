using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;
using NSubstitute;
using SkiaSharp;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;

namespace GitExtensionsTests;

/// <summary>
///  Checks the source graph palette's process lifetime without replacing its cached brushes.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGraphLaneColorTests
{
    private const int CanvasDimension = 24;
    private const int NodeCenterX = 8;
    private static readonly AppColor[] BranchSlots = [.. Enum.GetValues<AppColor>()
        .Where(color => color.ToString().StartsWith(nameof(AppColor.GraphBranch1)[..^1], StringComparison.Ordinal))];

    /// <summary>
    ///  Keeps a retained lane valid when later theme inputs would shrink or recolor its palette.
    /// </summary>
    [AvaloniaTest]
    public void Retained_lane_and_rendered_row_should_keep_the_first_use_palette_after_theme_changes(
        [Values(false, true)] bool replaceSettings,
        [Values(RevisionGraphDrawStyle.Normal, RevisionGraphDrawStyle.DrawNonRelativesGray, RevisionGraphDrawStyle.HighlightSelected)] RevisionGraphDrawStyle style)
    {
        PaletteSnapshot initial = new();
        using MutableThemeScope theme = new(initial);
        PaletteSnapshot cached = new();
        RevisionGraphSegment segment = CreateHighestColorSegment(cached.Brushes.Length);
        LaneInfo retainedLane = segment.LaneInfo ?? throw new AssertionException("The actual segment must have lane information.");
        IBrush retainedBrush = RevisionGraphLaneColor.GetBrushForLane(retainedLane.Color);
        GraphControl control = new(segment, style);
        using RenderTargetBitmap before = Render(control);
        using SKBitmap beforePixels = Decode(before);
        Color expected = BrushColor(style == RevisionGraphDrawStyle.Normal ? retainedBrush : cached.NonRelativeBrush);
        Retain(before, "before", replaceSettings, style, retainedLane.Color);
        beforePixels.GetPixel(NodeCenterX, CanvasDimension / 2).Should().Be(Pixel(expected));

        ThemeSettings previousSettings = ThemeModule.Settings;
        theme.ChangePalette(replaceSettings);
        ReferenceEquals(ThemeModule.Settings, previousSettings).Should().Be(!replaceSettings);
        BranchSlots.Select(slot => slot.GetThemeColor()).Distinct().Should().HaveCount(2,
            "the changed theme deliberately has fewer distinct branch colors than the source minimum");
        AppColor.GraphNonRelativeBranch.GetThemeColor().Should().NotBe(cached.NonRelativeColor);

        cached.AssertUnchanged();
        segment.LaneInfo.Should().BeSameAs(retainedLane);
        retainedLane.Color.Should().Be(cached.Brushes.Length - 1);
        RevisionGraphLaneColor.GetBrushForLane(retainedLane.Color).Should().BeSameAs(retainedBrush);
        control.InvalidateVisual();
        using RenderTargetBitmap after = Render(control);
        using SKBitmap afterPixels = Decode(after);
        Retain(after, "after", replaceSettings, style, retainedLane.Color);
        ReadPixels(afterPixels).Should().Equal(ReadPixels(beforePixels),
            "the original draws retained graph rows with its first-use brushes until restart");
    }

    /// <summary>
    ///  Exercises worker-created lane information against the already initialized immutable palette.
    /// </summary>
    [AvaloniaTest]
    public async Task Worker_created_lane_information_should_share_the_same_cross_thread_readable_brushes()
    {
        PaletteSnapshot cached = new();
        Color[] expectedColors = [.. cached.Brushes.Select(BrushColor)];
        foreach (IBrush brush in cached.Brushes.Append(cached.NonRelativeBrush))
        {
            (brush is IImmutableBrush).Should().BeTrue();
            (brush is AvaloniaObject).Should().BeFalse();
        }

        (RevisionGraphSegment Segment, IBrush[] Brushes, IBrush NonRelativeBrush, Color[] Colors) observed = await Task.Run(() =>
        {
            RevisionGraphSegment segment = CreateHighestColorSegment(cached.Brushes.Length);
            IBrush[] brushes = [.. Enumerable.Range(0, cached.Brushes.Length).Select(RevisionGraphLaneColor.GetBrushForLane)];
            Color[] colors = [.. brushes.Select(BrushColor)];
            _ = BrushColor(RevisionGraphLaneColor.NonRelativeBrush);
            return (segment, brushes, RevisionGraphLaneColor.NonRelativeBrush, colors);
        });

        // This does not claim worker-first initialization: the palette was deliberately
        // warmed above, and the assertion covers retained immutable values across threads.
        observed.Colors.Should().Equal(expectedColors);
        observed.NonRelativeBrush.Should().BeSameAs(cached.NonRelativeBrush);
        for (int index = 0; index < cached.Brushes.Length; index++)
        {
            observed.Brushes[index].Should().BeSameAs(cached.Brushes[index]);
        }

        LaneInfo lane = observed.Segment.LaneInfo ?? throw new AssertionException("The worker must construct actual lane information.");
        lane.Color.Should().Be(cached.Brushes.Length - 1);
        BrushColor(RevisionGraphLaneColor.GetBrushForLane(lane.Color)).Should().Be(expectedColors[lane.Color]);
        cached.AssertUnchanged();
    }

    private static RevisionGraphSegment CreateHighestColorSegment(int paletteCount)
    {
        ObjectId childId = Id(1);
        RevisionGraphRevision child = new(childId, guessScore: 1)
        {
            GitRevision = new GitRevision(childId) { Refs = [Substitute.For<IGitRef>()] },
        };
        const int searchLimit = byte.MaxValue + 1;
        for (int value = 2; value < searchLimit; value++)
        {
            RevisionGraphRevision parent = new(Id(value), guessScore: 2);
            RevisionGraphSegment segment = new(parent, child);
            LaneInfo lane = new(segment, segmentToTheLeft: null);
            if (lane.Color == paletteCount - 1)
            {
                segment.LaneInfo = lane;
                return segment;
            }
        }

        throw new AssertionException("Actual lane assignment must reach the highest cached palette index.");
    }

    private static ObjectId Id(int value)
        => ObjectId.Parse(value.ToString("x2", CultureInfo.InvariantCulture) + new string('0', ObjectId.Sha1CharCount - 2));

    private static Color BrushColor(IBrush brush)
        => (brush as ISolidColorBrush)?.Color ?? throw new AssertionException("The source graph palette must contain solid brushes.");

    private static SKColor Pixel(Color color) => new(color.R, color.G, color.B, color.A);

    private static RenderTargetBitmap Render(Control control)
    {
        Size size = new(CanvasDimension, CanvasDimension);
        control.Measure(size);
        control.Arrange(new Rect(size));
        RenderTargetBitmap bitmap = new(new PixelSize(CanvasDimension, CanvasDimension), new Vector(96, 96));
        bitmap.Render(control);
        return bitmap;
    }

    private static SKBitmap Decode(Bitmap bitmap)
    {
        using MemoryStream stream = new();
        bitmap.Save(stream, PngBitmapEncoderOptions.Default);
        return SKBitmap.Decode(stream.ToArray()) ?? throw new AssertionException("The actual graph bitmap must decode.");
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

    private static void Retain(Bitmap bitmap, string stage, bool replaceSettings, RevisionGraphDrawStyle style, int retainedIndex)
    {
        string? root = Environment.GetEnvironmentVariable("GITEXT_GRAPH_RASTER_EVIDENCE");
        if (string.IsNullOrWhiteSpace(root))
        {
            return;
        }

        string directory = Path.Combine(root, "palette-lifetime", TestContext.CurrentContext.Test.ID);
        Directory.CreateDirectory(directory);
        string identity = $"{(replaceSettings ? "replacement" : "in-place")}-{style}-{stage}-{Guid.NewGuid():N}";
        using (FileStream output = File.Create(Path.Combine(directory, identity + ".png")))
        {
            bitmap.Save(output, PngBitmapEncoderOptions.Default);
        }

        File.WriteAllText(Path.Combine(directory, identity + ".json"), JsonSerializer.Serialize(new
        {
            test = TestContext.CurrentContext.Test.FullName,
            captureMethod = "actualAvaloniaGraphRendererOwnedHeadlessBitmap",
            comparisonScope = "Same-process palette lifetime invariance; not fresh-start theme initialization or a whole-matrix native pixel join.",
            stage,
            replaceSettings,
            style = style.ToString(),
            retainedIndex,
            cachedPaletteArgb = RevisionGraphLaneColor.PresetGraphBrushes.Select(brush => Argb(BrushColor(brush))).ToArray(),
            cachedNonRelativeArgb = Argb(RevisionGraphLaneColor.NonRelativeColor),
            cachedNonRelativeBrushArgb = Argb(BrushColor(RevisionGraphLaneColor.NonRelativeBrush)),
            configuredPaletteArgb = BranchSlots.Select(slot => Argb(slot.GetThemeColor())).ToArray(),
            configuredNonRelativeArgb = Argb(AppColor.GraphNonRelativeBranch.GetThemeColor()),
            bitmap.PixelSize,
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string Argb(Color color) => Argb(DrawingColor.FromArgb(color.A, color.R, color.G, color.B));

    private static string Argb(DrawingColor color) => $"#{unchecked((uint)color.ToArgb()):X8}";

    private sealed class GraphControl : Control
    {
        private readonly RevisionGraphConfig _config = new();
        private readonly RevisionGraphRow _current;
        private readonly RevisionGraphRow _next;
        private readonly RevisionGraphDrawStyle _style;

        public GraphControl(RevisionGraphSegment segment, RevisionGraphDrawStyle style)
        {
            _current = new RevisionGraphRow(segment.Child, [segment], mergeGraphLanesHavingCommonParent: false);
            _next = new RevisionGraphRow(segment.Parent, [segment], mergeGraphLanesHavingCommonParent: false);
            _style = style;
        }

        public override void Render(DrawingContext context)
            => GraphRenderer.DrawItem(_config, context, index: 0, CanvasDimension,
                index => index switch { 0 => _current, 1 => _next, _ => null }, _style, headId: Id(0));
    }

    private sealed class PaletteSnapshot
    {
        public DrawingColor NonRelativeColor { get; } = RevisionGraphLaneColor.NonRelativeColor;

        public IBrush NonRelativeBrush { get; } = RevisionGraphLaneColor.NonRelativeBrush;

        public IBrush[] Brushes { get; } = [.. RevisionGraphLaneColor.PresetGraphBrushes];

        public void AssertUnchanged()
        {
            RevisionGraphLaneColor.NonRelativeColor.Should().Be(NonRelativeColor);
            RevisionGraphLaneColor.NonRelativeBrush.Should().BeSameAs(NonRelativeBrush);
            RevisionGraphLaneColor.PresetGraphBrushes.Should().HaveCount(Brushes.Length);
            for (int index = 0; index < Brushes.Length; index++)
            {
                RevisionGraphLaneColor.GetBrushForLane(index).Should().BeSameAs(Brushes[index]);
            }
        }
    }

    private sealed class MutableThemeScope : IDisposable
    {
        private static readonly PropertyInfo ModuleSettings = typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings))
            ?? throw new InvalidOperationException("The source-shaped theme boundary must exist.");
        private readonly ThemeSettings _original = ThemeModule.Settings;
        private readonly Dictionary<AppColor, DrawingColor> _colors = [];

        public MutableThemeScope(PaletteSnapshot cached)
        {
            for (int index = 0; index < BranchSlots.Length; index++)
            {
                Color color = BrushColor(cached.Brushes[index % cached.Brushes.Length]);
                _colors[BranchSlots[index]] = DrawingColor.FromArgb(color.A, color.R, color.G, color.B);
            }

            _colors[AppColor.GraphNonRelativeBranch] = cached.NonRelativeColor;
            _colors[AppColor.PanelBackground] = AppColor.PanelBackground.GetThemeColor();
            ReplaceSettings();
        }

        public void ChangePalette(bool replaceSettings)
        {
            for (int index = 0; index < BranchSlots.Length; index++)
            {
                _colors[BranchSlots[index]] = index % 2 == 0 ? DrawingColor.Red : DrawingColor.Blue;
            }

            DrawingColor previous = _colors[AppColor.GraphNonRelativeBranch];
            _colors[AppColor.GraphNonRelativeBranch] = DrawingColor.FromArgb(previous.A, byte.MaxValue - previous.R, previous.G, previous.B);
            if (replaceSettings)
            {
                ReplaceSettings();
            }
        }

        public void Dispose() => ModuleSettings.SetValue(null, _original);

        private void ReplaceSettings()
        {
            Theme theme = new(_colors, new Dictionary<KnownColor, DrawingColor>(), new ThemeId("graph-lane-cache-lifetime"));
            ModuleSettings.SetValue(null, new ThemeSettings(theme, _original.InvariantTheme, _original.Variations, _original.UseSystemVisualStyle));
        }
    }
}
