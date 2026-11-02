using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using GitUI.Compat;
using GitUI.UserControls.RevisionGrid.Columns;

namespace GitExtensionsTests;

/// <summary>
///  Exercises source SingleLine control characters without replacing the stored caption.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class WinFormsSingleLineTextBlockTests
{
    private static IEnumerable<TestCaseData> CaptionCases()
    {
        foreach (string family in new[] { "Segoe UI", "Arial", "Consolas" })
        {
            foreach (int points in new[] { 9, 11, 14 })
            {
                foreach (bool bold in new[] { false, true })
                {
                    foreach (string control in new[] { "\r", "\n", "\r\n", "\r\r\n" })
                    {
                        yield return new TestCaseData(family, points, bold, control)
                            .SetName($"SingleLine_should_preserve_literal_text_and_control_advances({family},{points},{bold},{ControlName(control)})");
                    }
                }
            }
        }

        yield break;

        static string ControlName(string control) => control.Replace("\r", "CR").Replace("\n", "LF");
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(CaptionCases))]
    public void SingleLine_should_preserve_literal_text_and_control_advances(string family, int points, bool bold, string control)
    {
        string raw = "AB" + control + "CD";
        WinFormsSingleLineTextBlock text = Create(family, points, bold, raw);
        TextBlock prefix = Create(family, points, bold, "AB");
        TextBlock suffix = Create(family, points, bold, "CD");
        text.Measure(Size.Infinity);
        prefix.Measure(Size.Infinity);
        suffix.Measure(Size.Infinity);
        double carriageReturnAdvance = GetCarriageReturnAdvance(text);
        double expected = prefix.TextLayout.WidthIncludingTrailingWhitespace
            + (control.Count(character => character == '\r') * carriageReturnAdvance)
            + suffix.TextLayout.WidthIncludingTrailingWhitespace;

        text.Text.Should().Be(raw);
        text.TextLayout.TextLines.Should().ContainSingle();
        text.TextLayout.TextLines[0].TextRuns.OfType<ShapedTextRun>().Sum(run => run.Length).Should().Be(raw.Length,
            "every character after a control must reach the same line, excluding only the framework's synthetic paragraph terminator");
        text.TextLayout.WidthIncludingTrailingWhitespace.Should().BeApproximately(expected, 0.000001);
        text.DesiredSize.Height.Should().Be(prefix.DesiredSize.Height);

        foreach (int width in new[] { 1, 20, 35, 300 })
        {
            text.TextTrimming = TextTrimming.CharacterEllipsis;
            text.Measure(new Size(width, 60));
            text.Arrange(new Rect(0, 0, width, 60));
            text.TextLayout.TextLines.Should().ContainSingle();
            text.TextLayout.TextLines[0].Length.Should().BeInRange(raw.Length, raw.Length + 1);
            text.Text.Should().Be(raw);
        }
    }

    [AvaloniaTest]
    [TestCase("AB\rCD\nEF\r\nGH")]
    [TestCase("\rAB\r")]
    [TestCase("\nAB\n")]
    [TestCase("\r\n")]
    [TestCase("עברית\r English\n العربية")]
    [TestCase("日本語\r\n世界")]
    [TestCase("👩‍💻\r\ncommit café")]
    public void SingleLine_should_retain_all_characters_fallback_and_terminal_controls(string raw)
    {
        WinFormsSingleLineTextBlock text = Create("Segoe UI", 11, bold: false, raw);
        text.Measure(Size.Infinity);

        AssertCompleteLine();

        // A control-only caption can naturally measure zero width. A real cell
        // supplies a finite viewport; zero allocation intentionally skips shaping.
        text.Arrange(new Rect(0, 0, 300, text.DesiredSize.Height));
        AssertCompleteLine();

        void AssertCompleteLine()
        {
            text.TextLayout.TextLines.Should().ContainSingle();
            text.TextLayout.TextLines[0].Length.Should().BeInRange(raw.Length, raw.Length + 1);
            text.Text.Should().Be(raw);
            text.TextLayout.TextLines[0].TextRuns.OfType<ShapedTextRun>()
                .Sum(run => run.Length).Should().Be(raw.Length);
        }
    }

    [AvaloniaTest]
    public void SingleLine_should_preserve_explicit_inline_fonts_and_raw_run_text()
    {
        WinFormsSingleLineTextBlock text = Create("Segoe UI", 9, bold: false, string.Empty);
        Run first = new("AB\r") { FontWeight = FontWeight.Bold };
        Run second = new("CD\n") { FontStyle = FontStyle.Italic };
        Run third = new("EF");
        text.Inlines!.AddRange([first, second, third]);
        text.Measure(Size.Infinity);
        text.Arrange(new Rect(text.DesiredSize));

        text.TextLayout.TextLines.Should().ContainSingle();
        text.TextLayout.TextLines[0].TextRuns.OfType<ShapedTextRun>().Sum(run => run.Length).Should().Be(8);
        first.Text.Should().Be("AB\r");
        second.Text.Should().Be("CD\n");
        third.Text.Should().Be("EF");
        text.TextLayout.TextLines[0].TextRuns.OfType<ShapedTextRun>()
            .Select(run => run.Properties.Typeface.Weight).Should().Contain(FontWeight.Bold);
        text.TextLayout.TextLines[0].TextRuns.OfType<ShapedTextRun>()
            .Select(run => run.Properties.Typeface.Style).Should().Contain(FontStyle.Italic);
    }

    [AvaloniaTest]
    [TestCase("\r")]
    [TestCase("\n")]
    [TestCase("\r\n")]
    public void SingleLine_should_paint_the_complete_suffix_at_the_font_derived_advance(string control)
    {
        WinFormsSingleLineTextBlock text = Create("Segoe UI", 9, bold: false, "AB" + control + "CD");
        text.Foreground = Brushes.Black;
        TextBlock prefix = Create("Segoe UI", 9, bold: false, "AB");
        TextBlock suffix = Create("Segoe UI", 9, bold: false, "CD");
        prefix.Foreground = suffix.Foreground = Brushes.Black;
        prefix.Measure(Size.Infinity);
        suffix.Measure(Size.Infinity);
        SegmentedTextPainter expected = new(prefix, suffix, prefix.TextLayout.WidthIncludingTrailingWhitespace
            + (control.Count(character => character == '\r') * GetCarriageReturnAdvance(text)));
        Window window = new() { Width = 300, Height = 70, Background = Brushes.White, Content = text };
        window.Show();
        try
        {
            using WriteableBitmap actual = Capture(window);
            window.Content = expected;
            using WriteableBitmap reference = Capture(window);
            byte[] actualPixels = Pixels(actual);
            byte[] expectedPixels = Pixels(reference);
            Record(window, control, actual, reference, actualPixels.SequenceEqual(expectedPixels));
            actualPixels.Should().Equal(expectedPixels,
                "the entire suffix must paint on the original line after the nonprinting control, with no guessed gap");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void SingleLine_should_cover_the_shared_revision_column_factory_and_build_status()
    {
        System.Reflection.MethodInfo factory = typeof(ColumnProvider).GetMethod("CreateTextBlock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new AssertionException("The source-shaped cell factory must exist.");
        TextBlock ordinary = (TextBlock)(factory.Invoke(null, [0d, 1d])
            ?? throw new AssertionException("The shared cell painter must exist."));
        TextBlock build = new BuildStatusColumnProvider.BuildStatusTextBlock();
        foreach (TextBlock text in new[] { ordinary, build })
        {
            text.Should().BeAssignableTo<WinFormsSingleLineTextBlock>();
            text.Text = "first\r\nlast";
            text.Measure(Size.Infinity);
            text.TextLayout.TextLines.Should().ContainSingle();
            text.TextLayout.TextLines[0].TextRuns.OfType<ShapedTextRun>().Sum(run => run.Length).Should().Be(text.Text.Length);
        }
    }

    private sealed class SegmentedTextPainter(TextBlock prefix, TextBlock suffix, double advance) : Control
    {
        public override void Render(DrawingContext context)
        {
            // Paint in the same scene coordinate system: child layout rounding or
            // a compositor transform is not a glyph-run advance inside one line.
            prefix.TextLayout.Draw(context, default);
            suffix.TextLayout.Draw(context, new Point(advance, 0));
        }
    }

    private static WinFormsSingleLineTextBlock Create(string family, int points, bool bold, string text) => new()
    {
        FontFamily = new FontFamily(family),
        FontSize = points * 96d / 72,
        FontWeight = bold ? FontWeight.Bold : FontWeight.Normal,
        Text = text,
    };

    private static double GetCarriageReturnAdvance(TextBlock owner)
    {
        if (OperatingSystem.IsWindows())
        {
            return WinFormsTextMeasurer.MeasureSize(owner, "\r").Width;
        }

        GlyphTypeface glyphTypeface = new Typeface(owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontStretch).GlyphTypeface;
        return glyphTypeface.CharacterToGlyphMap.TryGetGlyph('\r', out ushort glyph)
            && glyphTypeface.TryGetHorizontalGlyphAdvance(glyph, out ushort advance)
                ? advance * owner.FontSize / glyphTypeface.Metrics.DesignEmHeight
                : 0;
    }

    private static WriteableBitmap Capture(Window window)
    {
        window.UpdateLayout();
        return window.CaptureRenderedFrame() ?? throw new AssertionException("The actual single-line scene must render.");
    }

    private static byte[] Pixels(WriteableBitmap bitmap)
    {
        using Avalonia.Platform.ILockedFramebuffer framebuffer = bitmap.Lock();
        byte[] pixels = new byte[framebuffer.Size.Width * framebuffer.Size.Height * 4];
        for (int row = 0; row < framebuffer.Size.Height; row++)
        {
            System.Runtime.InteropServices.Marshal.Copy(framebuffer.Address + (row * framebuffer.RowBytes), pixels,
                row * framebuffer.Size.Width * 4, framebuffer.Size.Width * 4);
        }

        return pixels;
    }

    private static void Record(Window window, string control, WriteableBitmap actual, WriteableBitmap expected, bool equal)
    {
        string? evidence = Environment.GetEnvironmentVariable("GITEXT_SINGLE_LINE_EVIDENCE");
        if (string.IsNullOrWhiteSpace(evidence))
        {
            return;
        }

        string directory = Path.Combine(evidence, TestContext.CurrentContext.Test.ID);
        Directory.CreateDirectory(directory);
        actual.Save(Path.Combine(directory, "actual.png"), PngBitmapEncoderOptions.Default);
        expected.Save(Path.Combine(directory, "segmented-reference.png"), PngBitmapEncoderOptions.Default);
        File.WriteAllText(Path.Combine(directory, "paint.json"), JsonSerializer.Serialize(new
        {
            control,
            equal,
            actual.PixelSize,
            window.RenderScaling,
            captureMethod = "actualHeadlessSingleLineAndIndependentSegmentedTextScene",
            scope = "Framework glyph paint/advance regression; not native GDI glyph raster identity or desktop evidence.",
        }));
    }
}
