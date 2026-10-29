using System.Globalization;
using System.Text.Json;
using Avalonia;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitCommands;
using GitUI.Avatars;
using GitUI.Compat;
using SkiaSharp;
using Font = GitExtensions.Shims.WinForms.Font;
using ShimFontStyle = GitExtensions.Shims.WinForms.FontStyle;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class InitialsAvatarRenderingTests
{
    private const float EstimationPoints = 20;
    private const string Email = "avatar-source@example.invalid";

    private static IEnumerable<TestCaseData> InitialsCases()
    {
        foreach (string family in new[] { "Segoe UI", "Arial", "Consolas" })
        {
            foreach ((string name, string initials) in new[]
            {
                ("Nikola Begovic", "NB"),
                ("I", "I"),
                ("William Gray", "WG"),
                ("Åse Øster", "ÅØ"),
            })
            {
                foreach (int imageSize in new[] { 20, 22, 80 })
                {
                    yield return new TestCaseData(family, name, initials, imageSize);
                }
            }
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(InitialsCases))]
    public async Task Initials_should_render_the_source_float_scale_and_offsets_at_the_requested_bitmap_size(
        string family, string name, string initials, int imageSize)
    {
        Font previousFont = AppSettings.Font;
        string previousPalette = AppSettings.AvatarAuthorInitialsPalette;
        try
        {
            AppSettings.Font = new Font(family, 9, ShimFontStyle.Bold | ShimFontStyle.Italic);
            AppSettings.AvatarAuthorInitialsPalette = "OrangeRed";
            AppSettings.AvatarAuthorInitialsLuminanceThreshold.Should().Be(0.5f);
            InitialsAvatarProvider provider = new();
            provider.GetInitialsAndColorIndex(Email, name).initials.Should().Be(initials);
            byte[] actual = await provider.GetAvatarAsync(Email, name, imageSize)
                ?? throw new AssertionException("The local provider must produce its owned bitmap bytes.");
            FontFamily fontFamily = new(family);
            bool native = InitialsAvatarTextMeasurer.TryMeasureNative(initials, family, EstimationPoints, imageSize, out Size nativeMeasured);
            Size measured = native ? nativeMeasured : InitialsAvatarTextMeasurer.MeasurePortable(initials, fontFamily, EstimationPoints);
            native.Should().Be(OperatingSystem.IsWindows(), "the Windows source boundary uses a bitmap context, while other platforms use actual resolved shaping");
            float estimatedWidth = (float)measured.Width;
            float estimatedHeight = (float)measured.Height;
            int squareSize = Math.Max((int)estimatedWidth, (int)estimatedHeight);
            float ratio = (float)imageSize / squareSize;
            float drawingPoints = EstimationPoints * ratio;
            float xOffset = Math.Max((imageSize - (estimatedWidth * ratio)) / 2, 0);
            float yOffset = Math.Max((imageSize - (estimatedHeight * ratio)) / 2, 0);
            IBrush brush = Brushes.Black;
            FormattedText text = new(initials, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(fontFamily), drawingPoints * 96.0 / 72.0, brush);
            using RenderTargetBitmap expected = new(new PixelSize(imageSize, imageSize), new Vector(96, 96));
            Avalonia.Point paintOrigin = new(xOffset, yOffset);
            bool nativeLayout = InitialsAvatarTextMeasurer.TryGetDrawingMetrics(initials, family, drawingPoints, imageSize,
                out InitialsAvatarTextMeasurer.DrawingMetrics layoutMetrics);
            nativeLayout.Should().Be(OperatingSystem.IsWindows());
            if (nativeLayout)
            {
                Vector sourceLeading = new(ObservedNativeLeading(family, initials, imageSize), 0);
                float fontSizeInPixels = drawingPoints * 96 / 72;
                int ascent = family switch
                {
                    "Segoe UI" => 2210,
                    "Arial" => 1854,
                    "Consolas" => 1884,
                    _ => throw new AssertionException("This source fixture has no observed ascent for the requested family."),
                };
                float sourceBaseline = fontSizeInPixels * ascent / 2048;
                layoutMetrics.Origin.Should().Be(sourceLeading,
                    "the independent original bitmap probe measured this default-minus-typographic full-character origin");
                layoutMetrics.Ascent.Should().Be(sourceBaseline,
                    "the source drawing baseline comes from its observed regular-family design-unit ascent/em metrics");
                paintOrigin += sourceLeading + new Vector(0, sourceBaseline - text.Baseline);
            }

            using (DrawingContext drawing = expected.CreateDrawingContext())
            {
                drawing.FillRectangle(Brushes.OrangeRed, new Rect(0, 0, imageSize, imageSize));
                drawing.DrawText(text, paintOrigin);
            }

            FontManager.Current.TryGetGlyphTypeface(new Typeface(fontFamily), out GlyphTypeface? resolved);
            RetainEvidence(actual, new
            {
                family,
                resolvedFamily = resolved?.FamilyName,
                name,
                initials,
                imageSize,
                estimationPoints = EstimationPoints,
                requestedStyle = AppSettings.Font.Style.ToString(),
                measurementApi = native ? "bitmapGdiPlusClearTypeGridFit" : "actualResolvedPortableTypeface",
                estimated = new { Width = estimatedWidth, Height = estimatedHeight },
                squareSize,
                ratio,
                drawingPoints,
                xOffset,
                yOffset,
                paintOriginX = paintOrigin.X,
                paintOriginY = paintOrigin.Y,
                typographicBaseline = text.Baseline,
                nativeLayout,
                nativeCharacterOriginX = layoutMetrics.Origin.X,
                nativeCharacterOriginY = layoutMetrics.Origin.Y,
                nativeAscent = layoutMetrics.Ascent,
                inkBounds = DescribeInk(actual),
                captureMethod = "actualProviderSkiaBitmap",
                scope = "source sizing formula and same-backend paint origin; not GDI+/Skia glyph-raster equivalence",
            });
            AvatarImage.GetPixelSize(actual).Should().Be(new PixelSize(imageSize, imageSize));
            actual.Should().Equal(AvatarImage.Encode(expected),
                "the real provider must draw at source-formula size/origin, not fit an avatar-size-dependent guessed font");
        }
        finally
        {
            AppSettings.Font = previousFont;
            AppSettings.AvatarAuthorInitialsPalette = previousPalette;
        }
    }

    [AvaloniaTest]
    [TestCase("Segoe UI", "NB", 45.177944f, 38.80208f)]
    [TestCase("Segoe UI", "I", 16.193573f, 38.80208f)]
    [TestCase("Segoe UI", "WG", 53.381065f, 38.80208f)]
    [TestCase("Segoe UI", "ÅØ", 47.31336f, 38.80208f)]
    [TestCase("Arial", "NB", 47.03992f, 33.124992f)]
    [TestCase("Arial", "I", 16.519094f, 33.124992f)]
    [TestCase("Arial", "WG", 56.180546f, 33.124992f)]
    [TestCase("Arial", "ÅØ", 48.57638f, 33.124992f)]
    [TestCase("Consolas", "NB", 39.097214f, 34.557285f)]
    [TestCase("Consolas", "I", 23.993052f, 34.557285f)]
    [TestCase("Consolas", "WG", 39.097214f, 34.557285f)]
    [TestCase("Consolas", "ÅØ", 39.097214f, 34.557285f)]
    public void Bitmap_measurement_should_match_original_provider_metrics_or_the_explicit_portable_substitute(
        string familyName, string initials, float nativeWidth, float nativeHeight)
    {
        FontFamily family = new(familyName);
        Size actual = InitialsAvatarTextMeasurer.Measure(initials, family, EstimationPoints, 22);
        if (OperatingSystem.IsWindows())
        {
            // Independently observed original provider Bitmap Graphics, regular20pt,
            // HighSpeed/ClearTypeGridFit; values are font measurements, not UI sizes.
            actual.Should().Be(new Size(nativeWidth, nativeHeight));
        }
        else
        {
            InitialsAvatarTextMeasurer.TryMeasureNative(initials, familyName, EstimationPoints, 22, out _).Should().BeFalse();
            FormattedText resolved = new(initials, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(family), EstimationPoints * 96.0 / 72.0, foreground: null);
            actual.Should().Be(new Size(resolved.Width, resolved.Height),
                "the substitute uses the platform's resolved typeface rather than borrowing Windows metrics");
        }
    }

    [AvaloniaTest]
    public async Task Initials_should_remeasure_a_changed_font_family_but_not_inherit_configured_size_or_emphasis()
    {
        Font previousFont = AppSettings.Font;
        string previousPalette = AppSettings.AvatarAuthorInitialsPalette;
        try
        {
            AppSettings.AvatarAuthorInitialsPalette = "OrangeRed";
            AppSettings.Font = new Font("Segoe UI", 9);
            InitialsAvatarProvider provider = new();
            byte[] initial = await provider.GetAvatarAsync(Email, "Nikola Begovic", 22)
                ?? throw new AssertionException("The initial local avatar must render.");
            AppSettings.Font = new Font("Segoe UI", 22, ShimFontStyle.Bold | ShimFontStyle.Italic);
            provider.UpdateFontsSettings();
            byte[] emphasized = await provider.GetAvatarAsync(Email, "Nikola Begovic", 22)
                ?? throw new AssertionException("The same configured family must render.");
            emphasized.Should().Equal(initial, "the source estimation font remains regular 20pt independently of configured form size/style");
            AppSettings.Font = new Font("Consolas", 11);
            provider.UpdateFontsSettings();
            byte[] changed = await provider.GetAvatarAsync(Email, "Nikola Begovic", 22)
                ?? throw new AssertionException("The updated configured family must render.");
            byte[] independent = await new InitialsAvatarProvider().GetAvatarAsync(Email, "Nikola Begovic", 22)
                ?? throw new AssertionException("The independent same-family provider must render.");
            changed.Should().Equal(independent, "updating the provider must not retain the previous family's estimation metrics");
        }
        finally
        {
            AppSettings.Font = previousFont;
            AppSettings.AvatarAuthorInitialsPalette = previousPalette;
        }
    }

    [AvaloniaTest]
    public void Missing_native_family_should_explicitly_use_the_actual_portable_substitute_without_a_screen_context()
    {
        const string UnavailableFamily = "GitExtensions.NonexistentAvatarFontFamily";
        FontFamily family = new(UnavailableFamily);
        InitialsAvatarTextMeasurer.TryMeasureNative("NB", UnavailableFamily, EstimationPoints, 22, out _).Should().BeFalse();
        InitialsAvatarTextMeasurer.Measure("NB", family, EstimationPoints, 22)
            .Should().Be(InitialsAvatarTextMeasurer.MeasurePortable("NB", family, EstimationPoints));
    }

    private static void RetainEvidence(byte[] image, object metadata)
    {
        string? directory = Environment.GetEnvironmentVariable("GITEXT_INITIALS_AVATAR_EVIDENCE");
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, Guid.NewGuid().ToString("N"));
        File.WriteAllBytes(path + ".png", image);
        File.WriteAllText(path + ".json", JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static int ObservedNativeLeading(string family, string initials, int size)
    {
        // Actual default-minus-typographic full-character bounds from Bitmap Graphics,
        // ClearTypeGridFit; these are an API oracle, not a raster-derived offset table.
        int[] leading = (family, initials) switch
        {
            ("Segoe UI", "NB") => [2, 2, 8],
            ("Segoe UI", "I") => [2, 3, 9],
            ("Segoe UI", "WG") => [2, 2, 7],
            ("Segoe UI", "ÅØ") => [2, 2, 8],
            ("Arial", "NB") => [2, 2, 8],
            ("Arial", "I") => [3, 3, 11],
            ("Arial", "WG") => [2, 2, 6],
            ("Arial", "ÅØ") => [2, 2, 7],
            ("Consolas", "I") => [3, 3, 11],
            ("Consolas", _) => [3, 3, 10],
            _ => throw new AssertionException("The original bitmap fixture has no character-origin observation for this input."),
        };
        return leading[size switch { 20 => 0, 22 => 1, 80 => 2, _ => throw new AssertionException("This source size was not observed.") }];
    }

    private static object DescribeInk(byte[] image)
    {
        using SKBitmap pixels = SKBitmap.Decode(image)
            ?? throw new AssertionException("The provider must retain its actual rendered bitmap.");
        SKRectI ink = SKRectI.Empty;
        for (int y = 0; y < pixels.Height; y++)
        {
            for (int x = 0; x < pixels.Width; x++)
            {
                if (pixels.GetPixel(x, y) != SKColors.OrangeRed)
                {
                    ink = ink.IsEmpty ? new SKRectI(x, y, x + 1, y + 1) : new SKRectI(
                        Math.Min(ink.Left, x), Math.Min(ink.Top, y), Math.Max(ink.Right, x + 1), Math.Max(ink.Bottom, y + 1));
                }
            }
        }

        return new { X = ink.Left, Y = ink.Top, ink.Width, ink.Height };
    }
}
