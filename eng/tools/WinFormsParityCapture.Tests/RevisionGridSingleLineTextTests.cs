using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using GitExtensions.Extensibility.Extensions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Retains the unchanged revision painter's native single-line text boundary without constructing controls.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class RevisionGridSingleLineTextTests
{
    private const TextFormatFlags SourceFlags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter
        | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
    private const uint NativeSourceFlags = 0x800 | 0x4 | 0x20 | 0x8000;
    private const uint CalculateRectangle = 0x400;
    private const uint NoClip = 0x100;
    private const int CanvasPadding = 8;
    private const int WideWidth = 1000;
    private static readonly Color Backdrop = Color.FromArgb(255, 43, 45, 58);
    private static readonly Color Foreground = Color.FromArgb(255, 239, 233, 217);

    private static IEnumerable<TestCaseData> Fonts()
    {
        foreach (int points in new[] { 9, 11, 14 })
        {
            foreach (FontStyle style in new[] { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Bold | FontStyle.Italic })
            {
                yield return new TestCaseData("Segoe UI", points, style);
            }
        }

        yield return new TestCaseData("Arial", 9, FontStyle.Regular);
        yield return new TestCaseData("Consolas", 9, FontStyle.Regular);
    }

    /// <summary>
    ///  Checks the native glyph and finite-ellipsis contracts independently of Avalonia's rendering or measurement.
    /// </summary>
    [TestCaseSource(nameof(Fonts))]
    public void TextRenderer_should_retain_carriage_return_glyph_advances_and_ignore_line_feeds(
        string family, int points, FontStyle style)
    {
        using Font font = new(family, points, style, GraphicsUnit.Point);
        font.Name.Should().Be(family, "font substitution is not evidence for the requested source font");
        string directory = CreateDirectory();
        List<object> records = [];
        List<string> failures = [];
        foreach (TextCase sample in Samples())
        {
            string withoutLineFeeds = sample.Value.Replace("\n", string.Empty, StringComparison.Ordinal);
            string withoutControls = withoutLineFeeds.Replace("\r", string.Empty, StringComparison.Ordinal);
            string spaces = sample.Value.Replace('\r', ' ').Replace('\n', ' ');
            using Bitmap raw = Paint(font, sample.Value, WideWidth, PaintRoute.TextRenderer, out NativeObservation rawMetrics);
            using Bitmap native = Paint(font, sample.Value, WideWidth, PaintRoute.DrawText, out NativeObservation nativeMetrics);
            using Bitmap segmented = Paint(font, sample.Value, WideWidth, PaintRoute.SegmentedDrawText, out NativeObservation segmentedMetrics);
            using Bitmap noLf = Paint(font, withoutLineFeeds, WideWidth, PaintRoute.TextRenderer, out NativeObservation noLfMetrics);
            using Bitmap removed = Paint(font, withoutControls, WideWidth, PaintRoute.TextRenderer, out NativeObservation removedMetrics);
            using Bitmap replaced = Paint(font, spaces, WideWidth, PaintRoute.TextRenderer, out NativeObservation replacedMetrics);
            int nativeDelta = PixelDelta(raw, native);
            int segmentedDelta = PixelDelta(raw, segmented);
            int noLfDelta = PixelDelta(raw, noLf);
            string prefix = sample.Name;
            Save(raw, prefix + "-text-renderer.png");
            Save(native, prefix + "-native-draw-text.png");
            Save(segmented, prefix + "-native-segmented-draw-text.png");
            Save(removed, prefix + "-removed-controls.png");
            Save(replaced, prefix + "-spaces.png");
            AddFailure(nativeDelta == 0, sample.Name + " native DrawText pixel delta=" + nativeDelta);
            AddFailure(segmentedDelta == 0, sample.Name + " native segmented DrawText pixel delta=" + segmentedDelta);
            AddFailure(rawMetrics.Measured == segmentedMetrics.Measured, sample.Name + " native segmented DrawText advance differs");
            bool sameLineFeedHeight = withoutLineFeeds.Length == 0
                ? rawMetrics.Measured.Height > 0 && noLfMetrics.Measured.Height == 0
                : rawMetrics.Measured.Height == noLfMetrics.Measured.Height;
            AddFailure(noLfDelta == 0 && rawMetrics.Measured.Width == noLfMetrics.Measured.Width && sameLineFeedHeight,
                sample.Name + " line-feed filtering changed source pixels or advance");
            AddFailure(rawMetrics.Measured == nativeMetrics.Measured, sample.Name + " native DrawText measurement differs");
            if (sample.Value == "\r")
            {
                AddFailure(PixelDelta(raw, removed) == 0, "the actual carriage-return glyph must have blank ink");
                if (rawMetrics.GlyphIndices.Single() == 0)
                {
                    AddFailure(rawMetrics.Measured.Width == 0,
                        "a missing carriage-return glyph must not paint or advance by the default glyph");
                }
                else
                {
                    AddFailure(rawMetrics.Measured.Width == rawMetrics.GlyphExtent.Width,
                        "a mapped blank carriage-return glyph must retain its own native font advance");
                }
            }

            List<object> finite = [];
            int textWidth = rawMetrics.Measured.Width;
            foreach (int width in new[] { 1, Math.Max(1, textWidth / 2), Math.Max(1, textWidth - 1), Math.Max(1, textWidth), Math.Max(1, textWidth + 1) }.Distinct())
            {
                using Bitmap actualFinite = Paint(font, sample.Value, width, PaintRoute.TextRenderer, out NativeObservation actualFiniteMetrics);
                using Bitmap nativeFinite = Paint(font, sample.Value, width, PaintRoute.DrawText, out NativeObservation nativeFiniteMetrics);
                int difference = PixelDelta(actualFinite, nativeFinite);
                string finitePrefix = prefix + "-width-" + width;
                Save(actualFinite, finitePrefix + "-text-renderer.png");
                Save(nativeFinite, finitePrefix + "-native-draw-text.png");
                AddFailure(difference == 0, finitePrefix + " native ellipsis pixel delta=" + difference);
                AddFailure(actualFiniteMetrics.Measured == nativeFiniteMetrics.Measured,
                    finitePrefix + " native ellipsis measurement differs");
                finite.Add(new
                {
                    width,
                    actualFiniteMetrics,
                    nativeFiniteMetrics,
                    differingPixels = difference,
                    actualHash = BitmapPaintProbe.PixelHash(actualFinite),
                    nativeHash = BitmapPaintProbe.PixelHash(nativeFinite),
                });
            }

            records.Add(new
            {
                sample.Name,
                raw = sample.Value,
                withoutLineFeeds,
                withoutControls,
                spaces,
                rawMetrics,
                nativeMetrics,
                segmentedMetrics,
                noLfMetrics,
                removedMetrics,
                replacedMetrics,
                nativeDelta,
                segmentedDelta,
                noLfDelta,
                removedDelta = PixelDelta(raw, removed),
                spacesDelta = PixelDelta(raw, replaced),
                ink = BitmapPaintProbe.Ink(raw, Backdrop),
                decodedArgbHash = BitmapPaintProbe.PixelHash(raw),
                finite,
            });
        }

        BitmapPaintProbe.WriteReport(directory, new
        {
            sourceMethod = "GitUI.RevisionGridControl.DrawColumnText",
            sourceAssembly = typeof(GitUI.RevisionGridControl).Assembly.FullName,
            sourceFlags = SourceFlags.ToString(),
            nativeFlags = NativeSourceFlags,
            font = new { font.Name, font.SizeInPoints, style = font.Style.ToString(), font.Height },
            nativeFontHeight = -(int)Math.Ceiling(font.SizeInPoints * 96 / 72),
            nativeFontConstruction = "CreateFontW at -Ceiling(points * 96 / 72), matching TextRenderer's cached font rather than Font.ToHfont's fractional-size conversion",
            captureMethod = "ownedNative96BitmapTextRendererDrawTextAndSegmentedDrawText",
            segmentedReference = "Native DrawTextW shapes ordinary chunks; each CR advances only by its own DrawTextW CALCRECT width and paints no ink; each LF advances zero; one original rectangle clips the complete result",
            glyphDiagnostics = "Cmap indices and GetTextExtentPoint32W/GetTextExtentPointI extents are retained, not used as whole-word DrawText raster or shaping equivalence; missing CR maps to the default glyph but native DrawText must not paint or advance that glyph",
            finiteReference = "DrawTextW with the original finite rectangle and EndEllipsis flags, not an authored ellipsis string",
            settingsAccessed = false,
            controlsConstructed = false,
            scope = "Native source-API flags/glyph/advance/ellipsis oracle only; not invocation of the grid consumer, desktop capture, CSS/theme, Avalonia or full parity evidence.",
            records,
            failures,
        });
        failures.Should().BeEmpty("the diagnostic is retained before any native contract assertion fails");
        return;

        void AddFailure(bool condition, string failure)
        {
            if (!condition)
            {
                failures.Add(failure);
            }
        }

        void Save(Bitmap bitmap, string name) => bitmap.Save(Path.Combine(directory, name), ImageFormat.Png);
    }

    private static IEnumerable<TextCase> Samples()
    {
        yield return new("plain", "ABCD");
        yield return new("carriage-return", "AB\rCD");
        yield return new("line-feed", "AB\nCD");
        yield return new("crlf", "AB\r\nCD");
        yield return new("carriage-return-only", "\r");
        yield return new("line-feed-only", "\n");
        yield return new("crlf-only", "\r\n");
        yield return new("trailing-carriage-return-title", "subject\r");
        yield return new("literal-ampersand", "AB&\rCD");
        string formatted = UIExtensions.FormatBodyAndNotes("Referenced commit\n\nFull referenced details", "Reference note");
        yield return new("inline-notes-body", FlattenBody(formatted));
        string rawBody = "CRLF commit\r\n\r\nbody line one\r\nbody line two\r\n";
        string[] rawLines = rawBody.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        yield return new("raw-crlf-title", rawLines[0]);
        yield return new("raw-crlf-body", FlattenBody(rawBody));
        yield break;

        static string FlattenBody(string value)
            => string.Concat(value.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(line => " " + line));
    }

    private static string CreateDirectory()
    {
        string? root = Environment.GetEnvironmentVariable("GITEXT_SINGLE_LINE_SOURCE_EVIDENCE");
        string directory = Path.Combine(root ?? Path.Combine(TestContext.CurrentContext.WorkDirectory, "RevisionGridSingleLineTextProbe"),
            TestContext.CurrentContext.Test.ID + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        TestContext.Out.WriteLine("nativeSingleLineTextEvidence=" + directory);
        return directory;
    }

    private static Bitmap Paint(Font font, string value, int width, PaintRoute route, out NativeObservation observation)
    {
        int height = Math.Max(40, font.Height + 16);
        Rectangle bounds = new(CanvasPadding, CanvasPadding, width, height);
        Bitmap bitmap = BitmapPaintProbe.CreateBitmap(new Size(bounds.Right + CanvasPadding, bounds.Bottom + CanvasPadding), Backdrop);
        using Graphics graphics = Graphics.FromImage(bitmap);
        Size measured = TextRenderer.MeasureText(graphics, value, font, bounds.Size, SourceFlags);
        if (route == PaintRoute.TextRenderer)
        {
            graphics.SetClip(bounds);
            TextRenderer.DrawText(graphics, value, font, bounds, Foreground, SourceFlags);
        }

        nint context = graphics.GetHdc();
        nint handle = 0;
        nint originalFont = 0;
        try
        {
            // TextRenderer rounds the logical pixel height upward before caching
            // its font. Font.ToHfont uses a different fractional-point conversion.
            handle = CreateFont(-(int)Math.Ceiling(font.SizeInPoints * 96 / 72), 0, 0, 0, font.Bold ? 700 : 400,
                font.Italic, font.Underline, font.Strikeout, 1, 0, 0, 0, 0, font.Name);
            handle.Should().NotBe(0);
            originalFont = SelectObject(context, handle);
            originalFont.Should().NotBe(0);
            SetTextColor(context, (uint)(Foreground.R | (Foreground.G << 8) | (Foreground.B << 16)));
            SetBkMode(context, 1);
            NativeRectangle rectangle = new(bounds);
            DrawText(context, value, value.Length, ref rectangle, NativeSourceFlags | CalculateRectangle);
            Size nativeMeasured = new(rectangle.Right - rectangle.Left, rectangle.Bottom - rectangle.Top);
            string glyphText = value.Replace("\n", string.Empty, StringComparison.Ordinal);
            ushort[] indices = new ushort[glyphText.Length];
            NativeSize extent = default;
            NativeSize glyphExtent = default;
            if (glyphText.Length > 0)
            {
                GetGlyphIndices(context, glyphText, glyphText.Length, indices, 0).Should().NotBe(uint.MaxValue);
                GetTextExtentPoint32(context, glyphText, glyphText.Length, out extent).Should().BeTrue();
                GetTextExtentPointI(context, indices, indices.Length, out glyphExtent).Should().BeTrue();
            }

            List<TextSegment> segments = [];
            if (route == PaintRoute.DrawText)
            {
                rectangle = new(bounds);
                DrawText(context, value, value.Length, ref rectangle, NativeSourceFlags);
                measured = nativeMeasured;
            }
            else if (route == PaintRoute.SegmentedDrawText)
            {
                IntersectClipRect(context, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom).Should().NotBe(0);
                int advance = 0;
                int start = 0;
                for (int index = 0; index < value.Length; index++)
                {
                    char character = value[index];
                    if (character is not '\r' and not '\n')
                    {
                        continue;
                    }

                    DrawOrdinaryChunk(start, index);
                    int controlAdvance = 0;
                    if (character == '\r')
                    {
                        NativeRectangle controlBounds = new(bounds);
                        DrawText(context, "\r", 1, ref controlBounds, NativeSourceFlags | CalculateRectangle);
                        controlAdvance = controlBounds.Right - controlBounds.Left;
                    }

                    segments.Add(new TextSegment(character.ToString(), advance, controlAdvance));
                    advance += controlAdvance;
                    start = index + 1;
                }

                DrawOrdinaryChunk(start, value.Length);
                measured = new Size(advance, nativeMeasured.Height);
                observation = new NativeObservation(measured, nativeMeasured, new Size(extent.Width, extent.Height),
                    new Size(glyphExtent.Width, glyphExtent.Height), indices, segments);
                return bitmap;

                void DrawOrdinaryChunk(int first, int last)
                {
                    if (first == last)
                    {
                        return;
                    }

                    string chunk = value[first..last];
                    NativeRectangle chunkBounds = new(new Rectangle(bounds.X + advance, bounds.Y,
                        bounds.Width - advance, bounds.Height));
                    NativeRectangle measuredBounds = chunkBounds;
                    DrawText(context, chunk, chunk.Length, ref measuredBounds, NativeSourceFlags | CalculateRectangle);
                    int chunkAdvance = measuredBounds.Right - measuredBounds.Left;

                    // Keep one original clip for italic overhangs: clipping each
                    // chunk at its own left edge would invent internal cell borders.
                    DrawText(context, chunk, chunk.Length, ref chunkBounds, NativeSourceFlags | NoClip);
                    segments.Add(new TextSegment(chunk, advance, chunkAdvance));
                    advance += chunkAdvance;
                }
            }

            observation = new NativeObservation(measured, nativeMeasured, new Size(extent.Width, extent.Height),
                new Size(glyphExtent.Width, glyphExtent.Height), indices, segments);
        }
        finally
        {
            if (originalFont != 0)
            {
                SelectObject(context, originalFont);
            }

            if (handle != 0)
            {
                DeleteObject(handle);
            }

            graphics.ReleaseHdc(context);
        }

        return bitmap;
    }

    private static int PixelDelta(Bitmap actual, Bitmap expected)
    {
        int differences = 0;
        for (int y = 0; y < actual.Height; y++)
        {
            for (int x = 0; x < actual.Width; x++)
            {
                differences += actual.GetPixel(x, y).ToArgb() == expected.GetPixel(x, y).ToArgb() ? 0 : 1;
            }
        }

        return differences;
    }

    private enum PaintRoute
    {
        TextRenderer,
        DrawText,
        SegmentedDrawText,
    }

    private sealed record TextCase(string Name, string Value);

    private sealed record NativeObservation(Size Measured, Size NativeMeasured, Size TextExtent, Size GlyphExtent,
        ushort[] GlyphIndices, IReadOnlyList<TextSegment> Segments);

    private sealed record TextSegment(string Text, int Offset, int Advance);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle(Rectangle rectangle)
    {
        public int Left = rectangle.Left;
        public int Top = rectangle.Top;
        public int Right = rectangle.Right;
        public int Bottom = rectangle.Bottom;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "DrawTextW")]
    private static extern int DrawText(nint context, string text, int length, ref NativeRectangle rectangle, uint flags);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetGlyphIndicesW")]
    private static extern uint GetGlyphIndices(nint context, string text, int length, [Out] ushort[] indices, uint flags);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetTextExtentPoint32W")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTextExtentPoint32(nint context, string text, int length, out NativeSize size);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTextExtentPointI(nint context, ushort[] indices, int length, out NativeSize size);

    [DllImport("gdi32.dll")]
    private static extern int IntersectClipRect(nint context, int left, int top, int right, int bottom);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateFontW")]
    private static extern nint CreateFont(int height, int width, int escapement, int orientation, int weight,
        [MarshalAs(UnmanagedType.Bool)] bool italic, [MarshalAs(UnmanagedType.Bool)] bool underline,
        [MarshalAs(UnmanagedType.Bool)] bool strikeOut, uint charSet, uint outPrecision, uint clipPrecision,
        uint quality, uint pitchAndFamily, string faceName);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint context, nint value);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll")]
    private static extern uint SetTextColor(nint context, uint color);

    [DllImport("gdi32.dll")]
    private static extern int SetBkMode(nint context, int mode);
}
