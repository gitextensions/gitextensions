using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Compat;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using SkiaSharp;
using Color = Avalonia.Media.Color;
using Font = GitExtensions.Shims.WinForms.Font;
using FontStyle = Avalonia.Media.FontStyle;
using Geometry = Avalonia.Media.Geometry;
using Point = Avalonia.Point;
using ShimFontStyle = GitExtensions.Shims.WinForms.FontStyle;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridRefPaintTests
{
    private const BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const string BranchBrushKey = "GitExtensionsBranchRefBrush";
    private const string RemoteBrushKey = "GitExtensionsRemoteBranchRefBrush";
    private const string WindowBrushKey = "GitExtensionsKnownColorWindowBrush";
    private const int SourceHorizontalPadding = 4;
    private const int SourceEmptyHorizontalPadding = 1;
    private const int SourceVerticalPadding = 2;
    private const int SourceRightMargin = 5;
    private const int SourceArcDiameter = 5;

    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(LabelAllocations))]
    public void Retained_capsule_should_use_source_NoPadding_measurement_integer_shape_allowances_and_empty_space_height(
        string shapeName, string caption, string family, float points, bool bold, bool italic)
    {
        using PaintScope scope = new();
        Font font = new(family, points, (bold ? ShimFontStyle.Bold : ShimFontStyle.Regular)
            | (italic ? ShimFontStyle.Italic : ShimFontStyle.Regular));
        AppSettings.Font = font;
        RefLabelShape shape = Enum.Parse<RefLabelShape>(shapeName);
        RevisionGridRefRenderer.RefLabelControl label = new(null, caption, BranchBrushKey,
            RefLabelIcon.None, shape, fill: false, dashed: false);
        SetFont(label, font);
        label.HorizontalAlignment = HorizontalAlignment.Left;
        label.VerticalAlignment = VerticalAlignment.Top;
        Window window = new() { Width = 400, Height = 120, Content = label };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Size text = WinFormsTextMeasurer.MeasureTextRendererNoPadding(label,
                caption.Length == 0 ? " " : caption);
            int textWidth = caption.Length == 0 ? 0 : (int)Math.Ceiling(text.Width);
            int textHeight = (int)Math.Ceiling(text.Height);
            int backgroundHeight = textHeight + (SourceVerticalPadding * 2) - 1;
            int pointWidth = backgroundHeight / 2;
            int extraWidth = shape switch
            {
                RefLabelShape.NotchLeft or RefLabelShape.NotchRight => pointWidth,
                RefLabelShape.PointLeft or RefLabelShape.PointRight => pointWidth / 2,
                _ => 0,
            };
            int padding = caption.Length == 0 ? SourceEmptyHorizontalPadding : SourceHorizontalPadding;
            int capsuleWidth = textWidth + (padding * 2) + extraWidth - 1;
            int rowHeight = (int)RevisionGridControl.GetRowHeight(label);

            label.Label.Should().Be(caption, "the source measures prefixes but paints the literal NoPrefix caption");
            label.DesiredSize.Should().Be(new Size(capsuleWidth + SourceRightMargin, rowHeight));
            label.CapsuleBounds.Should().Be(new Rect(0, (rowHeight - backgroundHeight) / 2,
                capsuleWidth, backgroundHeight));
            label.PointWidth.Should().Be(pointWidth);
            RevisionGridRefRenderer.GetPointWidth(label).Should().Be(
                ((int)Math.Ceiling(WinFormsTextMeasurer.MeasureTextRendererNoPadding(label, " ").Height)
                    + (SourceVerticalPadding * 2) - 1) / 2);
            if (caption.Length == 0)
            {
                textHeight.Should().BeGreaterThan(0);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(PathAllocations))]
    public void Capsule_paths_should_keep_unshrunk_source_bounds_circular_controls_and_integer_tip_order(
        string shapeName, int height)
    {
        RefLabelShape shape = Enum.Parse<RefLabelShape>(shapeName);
        Rect bounds = new(12, 14, 47, height);
        int pointWidth = height / 2;
        MethodInfo factory = typeof(RevisionGridRefRenderer.RefLabelControl).GetMethod("CreateGeometry",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingMethodException("The retained capsule path factory is missing.");
        StreamGeometry geometry = (StreamGeometry)factory.Invoke(null, [bounds, shape, pointWidth])!;
        geometry.Bounds.Should().Be(bounds, "source paths use the painted rectangle, not a half-pixel inset or shrunken outline");
        object platform = typeof(Geometry).GetProperty("PlatformImpl", AllInstance)!.GetValue(geometry)!;
        SKPath path = (SKPath)(platform.GetType().GetProperty("FillPath", AllInstance)?.GetValue(platform)
            ?? throw new MissingMemberException("The headless Skia geometry does not expose its fill path."));
        List<SKPoint> actual = [.. path.Points];
        // Native CloseFigure and Avalonia's explicit final closing line represent
        // the same edge. Ignore a repeated starting vertex, not any curve controls.
        if (actual.Count > 1 && actual[0] == actual[^1])
        {
            actual.RemoveAt(actual.Count - 1);
        }

        Point[] expected = SourcePathPoints(bounds, shape, pointWidth);
        actual.Should().HaveCount(expected.Length);
        for (int index = 0; index < expected.Length; index++)
        {
            ((double)actual[index].X).Should().BeApproximately(expected[index].X, 0.00002);
            ((double)actual[index].Y).Should().BeApproximately(expected[index].Y, 0.00002);
        }

        Point roundedCorner = shape is RefLabelShape.NotchLeft or RefLabelShape.PointLeft
            ? new Point(bounds.Right - 0.7, bounds.Top + 0.7)
            : new Point(bounds.Left + 0.7, bounds.Top + 0.7);
        Point insideCorner = shape is RefLabelShape.NotchLeft or RefLabelShape.PointLeft
            ? new Point(bounds.Right - 0.8, bounds.Top + 0.8)
            : new Point(bounds.Left + 0.8, bounds.Top + 0.8);
        geometry.FillContains(roundedCorner).Should().BeFalse(
            "the source AddArc quarter-circle is not a quadratic corner through the rectangle corner");
        geometry.FillContains(insideCorner).Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Live_nestled_highlight_should_match_both_capsules_then_both_deferred_frames_at_the_shared_boundary(
        bool remoteFirst, bool highlightSecond)
    {
        using PairFixture fixture = new(remoteFirst);
        using SKBitmap baseline = fixture.Capture("baseline");
        (highlightSecond ? fixture.Second : fixture.First).IsHighlighted = true;
        using SKBitmap highlighted = fixture.Capture("highlighted");
        using SKBitmap deferred = fixture.CaptureOracle(deferred: true);
        Rect boundary = fixture.SharedBoundary;
        fixture.SaveEvidence($"equal-font-{remoteFirst}-{highlightSecond}",
            ("baseline", baseline), ("highlighted", highlighted), ("deferred", deferred));
        CountDifferentPixels(baseline, highlighted, boundary).Should().BeGreaterThan(0);
        CountDifferentPixels(highlighted, deferred, boundary).Should().Be(0,
            "the source draws both capsules before either highlight frame");
    }

    [AvaloniaTest]
    public void Synthetic_mixed_font_pair_should_make_premature_first_highlight_overpaint_observable()
    {
        // Source DrawRefEx accepts an independent font per call. This diagnostic
        // allocation makes paint order observable; actual nestled product labels
        // normally use same-size Normal/Bold fonts, not this mixed-size pair.
        using PairFixture fixture = new(remoteFirst: false, contrastingFonts: true);
        using SKBitmap baseline = fixture.Capture("baseline");
        fixture.First.IsHighlighted = true;
        using SKBitmap highlighted = fixture.Capture("highlighted");
        using SKBitmap deferred = fixture.CaptureOracle(deferred: true);
        using SKBitmap premature = fixture.CaptureOracle(deferred: false);
        Rect boundary = fixture.SharedBoundary;
        fixture.SaveEvidence("mixed-font-negative-control", ("baseline", baseline),
            ("highlighted", highlighted), ("deferred", deferred), ("premature", premature));

        CountDifferentPixels(baseline, highlighted, boundary).Should().BeGreaterThan(0);
        CountDifferentPixels(highlighted, deferred, boundary).Should().Be(0,
            "the live overlay must match both capsules followed by both deferred frames");
        CountDifferentPixels(highlighted, premature, boundary).Should().BeGreaterThan(0,
            "the source-order negative control must expose the second capsule overwriting the first highlight");
    }

    [AvaloniaTest]
    public void Retained_pair_palette_replacement_should_repaint_its_live_highlight_without_replacing_labels()
    {
        using PairFixture fixture = new(remoteFirst: false);
        fixture.First.IsHighlighted = true;
        using SKBitmap before = fixture.Capture("before-palette");
        RevisionGridRefRenderer.RefLabelControl first = fixture.First;
        RevisionGridRefRenderer.RefLabelControl second = fixture.Second;
        Color replacement = Color.FromRgb(210, 20, 170);

        Application.Current!.Resources[BranchBrushKey] = new SolidColorBrush(replacement);
        using SKBitmap after = fixture.Capture("after-palette");
        using SKBitmap oracle = fixture.CaptureOracle(deferred: true);
        fixture.SaveEvidence("palette-change", ("before", before), ("after", after), ("oracle", oracle));

        fixture.First.Should().BeSameAs(first);
        fixture.Second.Should().BeSameAs(second);
        ((ISolidColorBrush)first.RefBrush).Color.Should().Be(replacement);
        CountDifferentPixels(before, after, fixture.SharedBoundary).Should().BeGreaterThan(0);
        CountDifferentPixels(after, oracle, fixture.SharedBoundary).Should().Be(0);
    }

    [AvaloniaTest]
    public void Retained_pair_font_change_should_remeasure_capsules_and_repaint_the_deferred_highlight_at_its_new_boundary()
    {
        using PairFixture fixture = new(remoteFirst: false);
        fixture.First.IsHighlighted = true;
        Rect originalBounds = fixture.First.CapsuleBounds;
        using SKBitmap before = fixture.Capture("before-font");
        Font font = new("Consolas", 18, ShimFontStyle.Bold | ShimFontStyle.Italic);
        AppSettings.Font = font;
        SetFont(fixture.First, font);
        SetFont(fixture.Second, font);
        using SKBitmap after = fixture.Capture("after-font");
        using SKBitmap oracle = fixture.CaptureOracle(deferred: true);
        fixture.SaveEvidence("font-change", ("before", before), ("after", after), ("oracle", oracle));

        fixture.First.CapsuleBounds.Height.Should().BeGreaterThan(originalBounds.Height);
        fixture.First.HitPointWidth.Should().Be(RevisionGridRefRenderer.GetPointWidth(fixture.First));
        fixture.Second.HitPointWidth.Should().Be(fixture.First.HitPointWidth);
        fixture.Second.Bounds.X.Should().Be(Math.Max(0,
            fixture.First.CapsuleBounds.Right - fixture.First.HitPointWidth + 1));
        CountDifferentPixels(before, after, fixture.SharedBoundary).Should().BeGreaterThan(0);
        CountDifferentPixels(after, oracle, fixture.SharedBoundary).Should().Be(0);
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(LiteralTrimmingAllocations))]
    public void Literal_trimming_should_change_at_the_source_width_boundary_without_expanding_the_source_text_allocation(
        string caption, string family, float points, bool bold, bool italic)
    {
        Font font = new(family, points, (bold ? ShimFontStyle.Bold : ShimFontStyle.Regular)
            | (italic ? ShimFontStyle.Italic : ShimFontStyle.Regular));
        using LabelFixture fixture = new(caption, font);
        RevisionGridRefRenderer.RefLabelControl label = fixture.Label;
        Size literal = MeasureLiteral(label);
        int fittingWidth = (int)Math.Ceiling(literal.Width);
        Size sourceTextSize = fixture.CachedTextSize;
        Rect sourceTextBounds = fixture.SourceTextBounds;
        Rect capsuleBounds = label.CapsuleBounds;
        Rect controlBounds = label.Bounds;
        Size desiredSize = label.DesiredSize;

        literal.Width.Should().BeGreaterThan(0);
        label.GetTextTrimming(fittingWidth - 1).Should().Be(TextTrimming.CharacterEllipsis);
        label.GetTextTrimming(fittingWidth).Should().Be(TextTrimming.None);
        label.GetTextTrimming(fittingWidth + 1).Should().Be(TextTrimming.None);
        sourceTextSize.Width.Should().Be(Math.Ceiling(
            WinFormsTextMeasurer.MeasureTextRendererNoPadding(label, caption).Width));
        sourceTextBounds.Width.Should().Be(sourceTextSize.Width);
        label.GetTextTrimming((int)sourceTextBounds.Width).Should().Be(TextTrimming.None,
            "a source-fitting literal caption must not be abbreviated by a different portable glyph advance");

        if (!OperatingSystem.IsWindows())
        {
            // This verifies the defined portable metric substitute, not native
            // glyph/raster equivalence or the availability of the named fonts.
            FormattedText fallback = new(caption, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface(label.FontFamily, label.FontStyle, label.FontWeight),
                label.FontSize, foreground: null);
            literal.Should().Be(new Size(fallback.WidthIncludingTrailingWhitespace, fallback.Height));
        }

        label.Label.Should().Be(caption);
        fixture.CachedTextSize.Should().Be(sourceTextSize);
        fixture.SourceTextBounds.Should().Be(sourceTextBounds);
        label.CapsuleBounds.Should().Be(capsuleBounds);
        label.Bounds.Should().Be(controlBounds);
        label.DesiredSize.Should().Be(desiredSize);
        fixture.SaveEvidence($"fitting-{family}-{points}-{bold}-{italic}-{caption.Replace('/', '-')}");
    }

    [AvaloniaTest]
    [TestCase("feature&parity")]
    [TestCase("main&")]
    [TestCase("&&")]
    [TestCase("&")]
    public void Prefix_parse_underallocation_should_ellipsis_the_literal_caption_without_replacing_its_source_bounds(string caption)
    {
        using LabelFixture fixture = new(caption, new Font("Consolas", 9));
        RevisionGridRefRenderer.RefLabelControl label = fixture.Label;
        Size parsed = WinFormsTextMeasurer.MeasureTextRendererNoPadding(label, caption);
        Size literal = MeasureLiteral(label);
        Size sourceTextSize = fixture.CachedTextSize;
        Rect sourceTextBounds = fixture.SourceTextBounds;
        Rect capsuleBounds = label.CapsuleBounds;
        int allocatedWidth = (int)sourceTextBounds.Width;

        sourceTextSize.Should().Be(new Size(Math.Ceiling(parsed.Width), Math.Ceiling(parsed.Height)));
        allocatedWidth.Should().Be((int)sourceTextSize.Width);
        literal.Width.Should().BeGreaterThan(allocatedWidth,
            "the source parses ampersands for allocation but paints every literal ampersand with NoPrefix");
        capsuleBounds.Width.Should().Be(sourceTextSize.Width + (SourceHorizontalPadding * 2) - 1);
        label.GetTextTrimming(allocatedWidth).Should().Be(TextTrimming.CharacterEllipsis);
        label.GetTextTrimming((int)Math.Ceiling(literal.Width)).Should().Be(TextTrimming.None);
        label.Label.Should().Be(caption);
        fixture.CachedTextSize.Should().Be(sourceTextSize);
        fixture.SourceTextBounds.Should().Be(sourceTextBounds);
        label.CapsuleBounds.Should().Be(capsuleBounds);
        fixture.SaveEvidence($"prefix-{caption.Replace('&', '_')}");
    }

    [AvaloniaTest]
    [TestCase("Consolas", 9F, false)]
    [TestCase("Segoe UI", 11F, true)]
    public void Finite_retained_label_should_keep_the_source_clip_and_ellipsis_only_its_insufficient_text_allocation(
        string family, float points, bool bold)
    {
        using LabelFixture fixture = new("origin/main", new Font(family, points,
            bold ? ShimFontStyle.Bold : ShimFontStyle.Regular));
        RevisionGridRefRenderer.RefLabelControl label = fixture.Label;
        Size literal = MeasureLiteral(label);
        Size sourceTextSize = fixture.CachedTextSize;
        int finiteWidth = (int)Math.Ceiling(literal.Width / 2) + (SourceHorizontalPadding * 2);
        label.Width = finiteWidth;
        fixture.Settle();
        Rect sourceTextBounds = fixture.SourceTextBounds;
        Rect capsuleBounds = label.CapsuleBounds;

        label.Bounds.Width.Should().Be(finiteWidth);
        capsuleBounds.Width.Should().Be(finiteWidth);
        sourceTextBounds.Width.Should().Be(finiteWidth - (SourceHorizontalPadding * 2));
        sourceTextBounds.Right.Should().BeLessThanOrEqualTo(label.Bounds.Width);
        sourceTextBounds.Width.Should().BeLessThan(literal.Width);
        label.GetTextTrimming((int)sourceTextBounds.Width).Should().Be(TextTrimming.CharacterEllipsis);
        label.GetTextTrimming((int)Math.Ceiling(literal.Width)).Should().Be(TextTrimming.None);
        fixture.CachedTextSize.Should().Be(sourceTextSize, "clipping must not replace the source preferred text measurement");
        fixture.SourceTextBounds.Should().Be(sourceTextBounds);
        label.CapsuleBounds.Should().Be(capsuleBounds);
        fixture.SaveEvidence($"clipped-{family}-{points}-{bold}");
    }

    [AvaloniaTest]
    public void Empty_literal_should_fit_zero_width_without_discarding_the_source_space_height()
    {
        using LabelFixture fixture = new(string.Empty, new Font("Consolas", 9));
        Size sourceTextSize = fixture.CachedTextSize;
        Rect sourceTextBounds = fixture.SourceTextBounds;

        MeasureLiteral(fixture.Label).Should().Be(default(Size));
        sourceTextSize.Width.Should().Be(0);
        sourceTextSize.Height.Should().BeGreaterThan(0);
        fixture.Label.GetTextTrimming(0).Should().Be(TextTrimming.None);
        fixture.Label.GetTextTrimming(1).Should().Be(TextTrimming.None);
        fixture.Label.GetTextTrimming(-1).Should().Be(TextTrimming.CharacterEllipsis,
            "the decision compares the supplied integer directly; the source paint allocation supplies its own zero clamp");
        fixture.CachedTextSize.Should().Be(sourceTextSize);
        fixture.SourceTextBounds.Should().Be(sourceTextBounds);
    }

    [AvaloniaTest]
    public void Live_highlight_caption_should_refresh_literal_trimming_metrics_and_restore_the_normal_caption_cache()
    {
        using LabelFixture fixture = new("origin", new Font("Consolas", 9), highlightedCaption: "origin/main");
        RevisionGridRefRenderer.RefLabelControl label = fixture.Label;
        Size originalTextSize = fixture.CachedTextSize;
        Rect originalCapsule = label.CapsuleBounds;
        int normalWidth = (int)Math.Ceiling(MeasureLiteral(label).Width);
        label.GetTextTrimming(normalWidth).Should().Be(TextTrimming.None);

        label.IsHighlighted = true;
        fixture.Settle();
        label.Label.Should().Be("origin/main");
        MeasureLiteral(label).Width.Should().BeGreaterThan(normalWidth);
        label.GetTextTrimming(normalWidth).Should().Be(TextTrimming.CharacterEllipsis);
        label.GetTextTrimming((int)fixture.SourceTextBounds.Width).Should().Be(TextTrimming.None);
        fixture.CachedTextSize.Width.Should().BeGreaterThan(originalTextSize.Width);
        fixture.SaveEvidence("highlight-full-caption");

        label.IsHighlighted = false;
        fixture.Settle();
        label.Label.Should().Be("origin");
        label.GetTextTrimming(normalWidth).Should().Be(TextTrimming.None);
        fixture.CachedTextSize.Should().Be(originalTextSize);
        label.CapsuleBounds.Should().Be(originalCapsule);
    }

    [AvaloniaTest]
    [TestCase("main")]
    [TestCase("origin")]
    [TestCase("origin/main")]
    public void Live_font_change_should_refresh_the_literal_width_boundary_without_using_stale_caption_metrics(string caption)
    {
        using LabelFixture fixture = new(caption, new Font("Consolas", 9));
        RevisionGridRefRenderer.RefLabelControl label = fixture.Label;
        int previousFittingWidth = (int)Math.Ceiling(MeasureLiteral(label).Width);
        label.GetTextTrimming(previousFittingWidth).Should().Be(TextTrimming.None);
        fixture.SaveEvidence("before-font-" + caption.Replace('/', '-'));

        Font font = new("Consolas", 18, ShimFontStyle.Bold | ShimFontStyle.Italic);
        AppSettings.Font = font;
        SetFont(label, font);
        fixture.Settle();
        Size literal = MeasureLiteral(label);
        int fittingWidth = (int)Math.Ceiling(literal.Width);

        literal.Width.Should().BeGreaterThan(previousFittingWidth);
        label.GetTextTrimming(previousFittingWidth).Should().Be(TextTrimming.CharacterEllipsis);
        label.GetTextTrimming(fittingWidth - 1).Should().Be(TextTrimming.CharacterEllipsis);
        label.GetTextTrimming(fittingWidth).Should().Be(TextTrimming.None);
        label.GetTextTrimming((int)fixture.SourceTextBounds.Width).Should().Be(TextTrimming.None);
        fixture.CachedTextSize.Width.Should().Be(Math.Ceiling(
            WinFormsTextMeasurer.MeasureTextRendererNoPadding(label, caption).Width));
        label.Label.Should().Be(caption);
        fixture.SaveEvidence("after-font-" + caption.Replace('/', '-'));
    }

    [AvaloniaTest]
    public void Multiline_pair_caption_should_not_replace_the_first_font_space_hit_slant_and_font_change_should_refresh_it()
    {
        using PairFixture fixture = new(remoteFirst: false);
        int originalHitPointWidth = fixture.First.HitPointWidth;
        int originalCapsuleHeight = (int)fixture.First.CapsuleBounds.Height;
        fixture.First.AppendLabel("\nfeature");
        using SKBitmap multiline = fixture.Capture("multiline-caption");

        fixture.First.CapsuleBounds.Height.Should().BeGreaterThan(originalCapsuleHeight);
        fixture.First.PointWidth.Should().BeGreaterThan(originalHitPointWidth);
        fixture.First.HitPointWidth.Should().Be(originalHitPointWidth);
        fixture.First.HitPointWidth.Should().Be(RevisionGridRefRenderer.GetPointWidth(fixture.First));
        fixture.Second.HitPointWidth.Should().Be(fixture.First.HitPointWidth);
        fixture.Second.Bounds.X.Should().Be(Math.Max(0,
            fixture.First.CapsuleBounds.Right - fixture.First.HitPointWidth + 1));
        fixture.SaveEvidence("multiline-caption", ("multiline", multiline));

        Font font = new("Consolas", 18, ShimFontStyle.Bold | ShimFontStyle.Italic);
        AppSettings.Font = font;
        SetFont(fixture.First, font);
        SetFont(fixture.Second, font);
        using SKBitmap changed = fixture.Capture("multiline-font-change");

        fixture.First.HitPointWidth.Should().BeGreaterThan(originalHitPointWidth);
        fixture.First.HitPointWidth.Should().Be(RevisionGridRefRenderer.GetPointWidth(fixture.First));
        fixture.First.PointWidth.Should().BeGreaterThan(fixture.First.HitPointWidth);
        fixture.Second.HitPointWidth.Should().Be(fixture.First.HitPointWidth);
        fixture.Second.Bounds.X.Should().Be(Math.Max(0,
            fixture.First.CapsuleBounds.Right - fixture.First.HitPointWidth + 1));
        fixture.SaveEvidence("multiline-font-change", ("changed", changed));
    }

    private static IEnumerable<TestCaseData> LabelAllocations()
    {
        foreach (RefLabelShape shape in Enum.GetValues<RefLabelShape>())
        {
            foreach (string caption in new[] { string.Empty, "main", "feature&parity" })
            {
                yield return new TestCaseData(shape.ToString(), caption, "Segoe UI", 9F, false, false);
                yield return new TestCaseData(shape.ToString(), caption, "Segoe UI", 11F, true, false);
                yield return new TestCaseData(shape.ToString(), caption, "Arial", 9F, false, true);
            }
        }
    }

    private static IEnumerable<TestCaseData> PathAllocations()
    {
        foreach (RefLabelShape shape in Enum.GetValues<RefLabelShape>())
        {
            yield return new TestCaseData(shape.ToString(), 17);
            yield return new TestCaseData(shape.ToString(), 18);
        }
    }

    private static IEnumerable<TestCaseData> LiteralTrimmingAllocations()
    {
        foreach (string caption in new[] { "main", "origin", "origin/main" })
        {
            yield return new TestCaseData(caption, "Segoe UI", 9F, false, false);
            yield return new TestCaseData(caption, "Segoe UI", 9F, true, false);
            yield return new TestCaseData(caption, "Segoe UI", 11F, false, false);
            yield return new TestCaseData(caption, "Segoe UI", 11F, true, false);
            yield return new TestCaseData(caption, "Arial", 9F, false, true);
            yield return new TestCaseData(caption, "Consolas", 9F, false, false);
            yield return new TestCaseData(caption, "Consolas", 18F, true, true);
        }
    }

    private static Size MeasureLiteral(RevisionGridRefRenderer.RefLabelControl label)
        => WinFormsTextMeasurer.MeasureSize(label.FontFamily, label.FontStyle, label.FontWeight,
            label.FontSize, label.Label, singleLine: false, useTextRendererPadding: false);

    private static Point[] SourcePathPoints(Rect bounds, RefLabelShape shape, int pointWidth)
    {
        double left = bounds.Left;
        double top = bounds.Top;
        double right = bounds.Right;
        double bottom = bounds.Bottom;
        double middle = top + ((int)bounds.Height / 2);
        double radius = SourceArcDiameter / 2d;
        // The native AddArc control points describe a circular quarter arc. The
        // native probe's redundant zero-length wrap arc is not a distinct edge.
        double control = radius * (4d / 3d) * (Math.Sqrt(2) - 1);
        Point[] topLeft = [new(left, top + radius), new(left, top + radius - control),
            new(left + radius - control, top), new(left + radius, top)];
        Point[] topRight = [new(right - radius, top), new(right - radius + control, top),
            new(right, top + radius - control), new(right, top + radius)];
        Point[] bottomRight = [new(right, bottom - radius), new(right, bottom - radius + control),
            new(right - radius + control, bottom), new(right - radius, bottom)];
        Point[] bottomLeft = [new(left + radius, bottom), new(left + radius - control, bottom),
            new(left, bottom - radius + control), new(left, bottom - radius)];
        return shape switch
        {
            RefLabelShape.Rect => [.. topLeft, .. topRight, .. bottomRight, .. bottomLeft],
            RefLabelShape.PointRight => [.. topLeft, new(right - pointWidth, top), new(right, middle),
                new(right - pointWidth, bottom), .. bottomLeft],
            RefLabelShape.NotchRight => [.. topLeft, new(right, top), new(right - pointWidth, middle),
                new(right, bottom), .. bottomLeft],
            RefLabelShape.PointLeft => [new(left, middle), new(left + pointWidth, top),
                .. topRight, .. bottomRight, new(left + pointWidth, bottom)],
            RefLabelShape.NotchLeft => [new(left, top), new(left + pointWidth, middle), new(left, bottom),
                new(right - radius, bottom), new(right - radius + control, bottom),
                new(right, bottom - radius + control), new(right, bottom - radius),
                new(right, top + radius), new(right, top + radius - control),
                new(right - radius + control, top), new(right - radius, top)],
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
    }

    private static void SetFont(RevisionGridRefRenderer.RefLabelControl label, Font font)
    {
        label.FontFamily = new FontFamily(font.Name);
        label.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(font.Size);
        label.FontWeight = font.Bold ? FontWeight.Bold : FontWeight.Normal;
        label.FontStyle = font.Italic ? FontStyle.Italic : FontStyle.Normal;
    }

    private static int CountDifferentPixels(SKBitmap left, SKBitmap right, Rect region)
    {
        left.Width.Should().Be(right.Width);
        left.Height.Should().Be(right.Height);
        int differences = 0;
        for (int y = Math.Max(0, (int)Math.Floor(region.Top)); y < Math.Min(left.Height, Math.Ceiling(region.Bottom)); y++)
        {
            for (int x = Math.Max(0, (int)Math.Floor(region.Left)); x < Math.Min(left.Width, Math.Ceiling(region.Right)); x++)
            {
                if (left.GetPixel(x, y) != right.GetPixel(x, y))
                {
                    differences++;
                }
            }
        }

        return differences;
    }

    private static object DescribeLabel(RevisionGridRefRenderer.RefLabelControl label, Point? windowOrigin)
        => new
        {
            label.Label,
            Shape = label.Shape.ToString(),
            Icon = label.Icon.ToString(),
            label.Bounds,
            label.DesiredSize,
            label.CapsuleBounds,
            label.PointWidth,
            label.HitPointWidth,
            WindowOrigin = windowOrigin,
            FontFamily = label.FontFamily.ToString(),
            label.FontSize,
            FontStyle = label.FontStyle.ToString(),
            FontWeight = label.FontWeight.ToString(),
            FontStretch = label.FontStretch.ToString(),
            Theme = label.ActualThemeVariant?.ToString() ?? "detached",
            label.ClipToBounds,
            Clip = label.Clip?.Bounds,
            label.Opacity,
            label.IsHighlighted,
            label.IsRowSelected,
            label.IsMeasureValid,
            label.IsArrangeValid,
            label.IsInitialized,
            label.IsLoaded,
        };

    private sealed class PaintScope : IDisposable
    {
        private readonly Font _font = AppSettings.Font;
        private readonly Dictionary<string, (bool Present, object? Value)> _originalResources = [];

        public PaintScope()
        {
            AppSettings.Font = new Font("Segoe UI", 9);
            foreach (string key in new[] { BranchBrushKey, RemoteBrushKey, WindowBrushKey })
            {
                bool present = Application.Current!.Resources.ContainsKey(key);
                _originalResources[key] = (present, present ? Application.Current.Resources[key] : null);
            }

            Application.Current!.Resources[BranchBrushKey] = new SolidColorBrush(Color.FromRgb(0, 122, 0));
            Application.Current.Resources[RemoteBrushKey] = new SolidColorBrush(Color.FromRgb(0, 92, 180));
            Application.Current.Resources[WindowBrushKey] = Brushes.White;
        }

        public void Dispose()
        {
            foreach ((string key, (bool present, object? value)) in _originalResources)
            {
                if (present)
                {
                    Application.Current!.Resources[key] = value;
                }
                else
                {
                    Application.Current!.Resources.Remove(key);
                }
            }

            AppSettings.Font = _font;
        }
    }

    private sealed class LabelFixture : IDisposable
    {
        private readonly PaintScope _scope = new();

        public LabelFixture(string caption, Font font, string? highlightedCaption = null)
        {
            AppSettings.Font = font;
            Label = new(null, caption, BranchBrushKey, RefLabelIcon.None, RefLabelShape.Rect,
                fill: true, dashed: false, highlightedLabel: highlightedCaption)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            SetFont(Label, font);
            Window = new Window
            {
                Width = 420,
                Height = 140,
                Background = Brushes.White,
                RequestedThemeVariant = ThemeVariant.Light,
                Content = new Border { Padding = new Thickness(8), Child = Label },
            };
            Window.Show();
            Settle();
        }

        public RevisionGridRefRenderer.RefLabelControl Label { get; }

        public Window Window { get; }

        public Size CachedTextSize
            => (Size)(typeof(RevisionGridRefRenderer.RefLabelControl).GetField("_textSize", AllInstance)?.GetValue(Label)
                ?? throw new MissingFieldException("The retained source text allocation is missing."));

        public Rect SourceTextBounds
        {
            get
            {
                // This fixture deliberately uses source Rect/no-icon labels. Read
                // the retained prefix-parsed cache, then apply DrawRefEx's integer
                // text clip; the literal trimming metric must not replace either.
                Size text = CachedTextSize;
                int width = Math.Max(0, (int)Label.Bounds.Width);
                int padding = Label.Label.Length == 0 ? SourceEmptyHorizontalPadding : SourceHorizontalPadding;
                int textWidth = Math.Clamp(Math.Min(width - padding - padding, (int)text.Width),
                    0, Math.Max(0, width - padding));
                return new Rect(padding, Label.CapsuleBounds.Y + SourceVerticalPadding - 1,
                    textWidth, text.Height);
            }
        }

        public void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public void SaveEvidence(string stage)
        {
            if (Environment.GetEnvironmentVariable("GITEXT_REF_PAINT_EVIDENCE") is not { Length: > 0 } root)
            {
                return;
            }

            Settle();
            string directory = Path.Combine(root, "trimming", stage);
            Directory.CreateDirectory(directory);
            using WriteableBitmap frame = Window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("The live headless trimming label did not render a frame.");
            using (FileStream output = File.Create(Path.Combine(directory, "live.png")))
            {
                frame.Save(output, PngBitmapEncoderOptions.Default);
            }

            File.WriteAllText(Path.Combine(directory, "geometry.json"), JsonSerializer.Serialize(new
            {
                Note = "Portable retained source-metric trimming/clip diagnostic; no native glyph or raster equivalence claim.",
                Label = DescribeLabel(Label, Label.TranslatePoint(default, Window)),
                CachedTextSize,
                LiteralTextSize = MeasureLiteral(Label),
                SourceTextBounds,
                Trimming = Label.GetTextTrimming((int)SourceTextBounds.Width).ToString(),
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"revision-ref trimming diagnostics: {directory}");
        }

        public void Dispose()
        {
            Window.Close();
            _scope.Dispose();
        }
    }

    private sealed class PairFixture : IDisposable
    {
        private readonly PaintScope _scope = new();
        private readonly Border _host;
        private readonly Dictionary<string, object> _geometrySnapshots = [];

        public PairFixture(bool remoteFirst, bool contrastingFonts = false)
        {
            IGitRef local = Substitute.For<IGitRef>();
            local.IsHead.Returns(true);
            IGitRef remote = Substitute.For<IGitRef>();
            remote.IsRemote.Returns(true);
            First = RevisionGridRefRenderer.CreateLabel(remoteFirst ? remote : local, "main",
                remoteFirst ? RefLabelShape.NotchRight : RefLabelShape.PointRight,
                fill: true, showHeadIndicator: false);
            Second = RevisionGridRefRenderer.CreateLabel(remoteFirst ? local : remote, "origin",
                remoteFirst ? RefLabelShape.PointLeft : RefLabelShape.NotchLeft,
                fill: true, showHeadIndicator: false);
            if (contrastingFonts)
            {
                AppSettings.Font = new Font("Segoe UI", 11);
                SetFont(First, AppSettings.Font);
                SetFont(Second, new Font("Segoe UI", 9));
            }
            else
            {
                SetFont(First, AppSettings.Font);
                SetFont(Second, AppSettings.Font);
            }

            RevisionGridRefRenderer.NestledRefLabelPanel pair = new(First, Second)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            _host = new Border { Padding = new Thickness(8), Child = pair };
            Window = new Window
            {
                Width = 360,
                Height = 100,
                Background = Brushes.White,
                RequestedThemeVariant = ThemeVariant.Light,
                Content = _host,
            };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        public RevisionGridRefRenderer.RefLabelControl First { get; }

        public RevisionGridRefRenderer.RefLabelControl Second { get; }

        public Window Window { get; }

        public Rect SharedBoundary
        {
            get
            {
                Point first = First.TranslatePoint(default, Window)!.Value;
                Point second = Second.TranslatePoint(default, Window)!.Value;
                return First.CapsuleBounds.Translate(new Vector(first.X, first.Y))
                    .Intersect(Second.CapsuleBounds.Translate(new Vector(second.X, second.Y))).Inflate(2);
            }
        }

        public SKBitmap Capture(string stage = "live")
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            // Bounds/font invalidation during layout queues visual work after the
            // first drain. Capture the settled live frame, not a prior compositor frame.
            Dispatcher.UIThread.RunJobs();
            _geometrySnapshots[stage] = new
            {
                Window.Bounds,
                Window.RenderScaling,
                Theme = Window.ActualThemeVariant.ToString(),
                AppFont = new { AppSettings.Font.Name, AppSettings.Font.Size, AppSettings.Font.Bold, AppSettings.Font.Italic },
                First = DescribeLabel(First, First.TranslatePoint(default, Window)),
                Second = DescribeLabel(Second, Second.TranslatePoint(default, Window)),
            };
            // Render the actual attached visual tree afresh on both sides. This
            // isolates the ordering contract from compositor damage/cache history.
            using RenderTargetBitmap frame = new(PixelSize.FromSize(Window.Bounds.Size, 1), new Vector(96, 96));
            frame.Render(Window);
            using MemoryStream encoded = new();
            frame.Save(encoded, PngBitmapEncoderOptions.Default);
            return SKBitmap.Decode(encoded.ToArray());
        }

        public SKBitmap CaptureOracle(bool deferred)
        {
            using PairPaintOracle oracle = new(First, Second, deferred);
            string stage = deferred ? "deferred-oracle" : "premature-oracle";
            SKBitmap frame = Capture(stage);
            _geometrySnapshots[stage + "-order"] = oracle.Describe(Window);
            return frame;
        }

        public void SaveEvidence(string label, params (string Name, SKBitmap Frame)[] frames)
        {
            if (Environment.GetEnvironmentVariable("GITEXT_REF_PAINT_EVIDENCE") is not { Length: > 0 } root)
            {
                return;
            }

            string directory = Path.Combine(root, label);
            Directory.CreateDirectory(directory);
            foreach ((string name, SKBitmap frame) in frames)
            {
                using SKImage image = SKImage.FromBitmap(frame);
                using SKData png = image.Encode(SKEncodedImageFormat.Png, 100)
                    ?? throw new InvalidOperationException("The headless Skia frame could not be encoded.");
                using FileStream output = File.Create(Path.Combine(directory, name + ".png"));
                png.SaveTo(output);
            }

            File.WriteAllText(Path.Combine(directory, "geometry.json"), JsonSerializer.Serialize(new
            {
                Note = "Portable retained boundary/order diagnostic; no native mixed-font pair, glyph or raster equivalence claim.",
                SharedBoundary,
                Snapshots = _geometrySnapshots,
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"revision-ref paint diagnostics: {directory}");
        }

        public void Dispose()
        {
            Window.Close();
            _scope.Dispose();
        }
    }

    private sealed class PairPaintOracle : IDisposable
    {
        private readonly RevisionGridRefRenderer.NestledRefLabelPanel _pair;
        private readonly RevisionGridRefRenderer.RefLabelControl[] _labels;
        private readonly HighlightPaintVisual _highlight;
        private readonly IList<Visual> _visuals;
        private readonly Control _productionOverlay;
        private readonly int _overlayIndex;

        public PairPaintOracle(RevisionGridRefRenderer.RefLabelControl first,
            RevisionGridRefRenderer.RefLabelControl second, bool deferred)
        {
            _labels = [first, second];
            _pair = first.Parent as RevisionGridRefRenderer.NestledRefLabelPanel
                ?? throw new InvalidOperationException("The order oracle requires the actual live pair.");
            second.Parent.Should().BeSameAs(_pair);
            _visuals = (IList<Visual>)(typeof(Visual).GetProperty("VisualChildren", AllInstance)!
                .GetValue(_pair) ?? throw new MissingMemberException("The pair's visual collection is missing."));
            // This oracle tests ordering, not fresh-scene raster equivalence. Retain
            // the live bodies and their compositor caches while replacing only the
            // production overlay with independently ordered source highlight strokes.
            // Fresh reconstructed scenes differed by one channel at one boundary
            // pixel on Linux; the numerical cause has not been established.
            _productionOverlay = _visuals.OfType<Control>().Single(control =>
                control.GetType().Name == "RefLabelHighlightOverlay");
            _overlayIndex = _visuals.IndexOf(_productionOverlay);
            _visuals.Remove(_productionOverlay);
            _highlight = new HighlightPaintVisual(_labels) { IsHitTestVisible = false, Focusable = false };
            _visuals.Insert(deferred ? _visuals.Count : 1, _highlight);
            _highlight.Measure(_pair.Bounds.Size);
            _highlight.Arrange(new Rect(_pair.Bounds.Size));
        }

        public object Describe(Window window)
        {
            return new
            {
                PairBounds = _pair.Bounds,
                BodyVisuals = _labels.Select(label => new { label.Bounds, label.ClipToBounds }),
                HighlightBounds = _highlight.Bounds,
                First = DescribeLabel(_labels[0], _labels[0].TranslatePoint(default, window)),
                Second = DescribeLabel(_labels[1], _labels[1].TranslatePoint(default, window)),
            };
        }

        public void Dispose()
        {
            _visuals.Remove(_highlight);
            _visuals.Insert(_overlayIndex, _productionOverlay);
            _productionOverlay.InvalidateVisual();
        }

        private sealed class HighlightPaintVisual(RevisionGridRefRenderer.RefLabelControl[] labels) : Control
        {
            public override void Render(DrawingContext context)
            {
                // This independently ordered final sibling implements the source
                // deferred frames; the production overlay is not part of the oracle.
                foreach (RevisionGridRefRenderer.RefLabelControl label in labels)
                {
                    using (context.PushTransform(Matrix.CreateTranslation(label.Bounds.X, label.Bounds.Y)))
                    {
                        label.DrawHighlight(context);
                    }
                }
            }
        }
    }
}
