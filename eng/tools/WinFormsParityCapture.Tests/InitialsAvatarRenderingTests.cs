using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI.Avatars;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// Only the original provider's owned Bitmap Graphics is exercised here. No form,
// HWND, screen DC, desktop capture, input route, repository or network is involved.
[TestFixture]
[NonParallelizable]
public sealed class InitialsAvatarRenderingTests
{
    private const float EstimationPoints = 20;
    private const string Email = "avatar-source@example.invalid";
    private const string PortableSettingName = "IsPortable";
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

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

    [TestCaseSource(nameof(InitialsCases))]
    public void Original_initials_should_use_twenty_point_bitmap_measurement_float_scaling_and_source_draw_offsets(
        string family, string name, string initials, int imageSize)
    {
        WithIsolatedSettings(() =>
        {
            using Font configured = new(family, 9, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            configured.Name.Should().Be(family, "a substituted native family is not proof of the requested family");
            AppSettings.Font = configured;
            AppSettings.AvatarAuthorInitialsPalette = "OrangeRed";
            InitialsAvatarProvider provider = new();
            try
            {
                Font estimation = ReadEstimationFont(provider);
                estimation.FontFamily.Name.Should().Be(family);
                estimation.SizeInPoints.Should().Be(EstimationPoints);
                estimation.Style.Should().Be(FontStyle.Regular,
                    "the original provider uses only the configured family, not the form font's size or emphasis");
                Task<Image?> result = provider.GetAvatarAsync(Email, name, imageSize);
                result.IsCompletedSuccessfully.Should().BeTrue("the original provider completes its local bitmap synchronously");
#pragma warning disable VSTHRD002 // The original Task.FromResult completion is asserted immediately above.
                using Image original = result.Result ?? throw new AssertionException("The original local provider must return a bitmap.");
#pragma warning restore VSTHRD002
                using Bitmap expected = new(imageSize, imageSize);
                using Graphics graphics = Graphics.FromImage(expected);
                graphics.Clear(Color.OrangeRed);
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.DpiX.Should().Be(96);
                graphics.DpiY.Should().Be(96);
                SizeF estimated = graphics.MeasureString(initials, estimation);
                int squareSize = Math.Max((int)estimated.Width, (int)estimated.Height);
                float ratio = (float)imageSize / squareSize;
                using Font drawing = new(estimation.FontFamily, EstimationPoints * ratio);
                SizeF displayed = estimated * ratio;
                float x = Math.Max((imageSize - displayed.Width) / 2, 0);
                float y = Math.Max((imageSize - displayed.Height) / 2, 0);
                using Brush foreground = new SolidBrush(Color.OrangeRed.GetContrastColor(AppSettings.AvatarAuthorInitialsLuminanceThreshold));
                graphics.DrawString(initials, drawing, foreground, x, y);
                graphics.Flush();
                Bitmap actual = original as Bitmap
                    ?? throw new AssertionException("The original provider must retain its actual bitmap pixels.");
                int differentPixels = 0;
                for (int row = 0; row < imageSize; row++)
                {
                    for (int column = 0; column < imageSize; column++)
                    {
                        if (actual.GetPixel(column, row).ToArgb() != expected.GetPixel(column, row).ToArgb())
                        {
                            differentPixels++;
                        }
                    }
                }

                object characterLayout = ReadCharacterLayout(graphics, initials, drawing);
                graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
                SizeF systemDefaultEstimate = graphics.MeasureString(initials, estimation);
                string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "InitialsAvatarProbe", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(directory);
                original.Save(Path.Combine(directory, "original-provider.png"), ImageFormat.Png);
                expected.Save(Path.Combine(directory, "independent-source-formula.png"), ImageFormat.Png);
                string json = JsonSerializer.Serialize(new
                {
                    family,
                    name,
                    initials,
                    imageSize,
                    estimationPoints = estimation.SizeInPoints,
                    estimationStyle = estimation.Style.ToString(),
                    requestedPoints = configured.SizeInPoints,
                    requestedStyle = configured.Style.ToString(),
                    estimated = new { estimated.Width, estimated.Height },
                    systemDefaultEstimate = new { systemDefaultEstimate.Width, systemDefaultEstimate.Height },
                    squareSize,
                    ratio,
                    drawingPoints = drawing.SizeInPoints,
                    displayed = new { displayed.Width, displayed.Height },
                    xOffset = x,
                    yOffset = y,
                    dpiX = graphics.DpiX,
                    dpiY = graphics.DpiY,
                    differentPixels,
                    backgroundArgb = Color.OrangeRed.ToArgb().ToString("X8"),
                    inkBounds = DescribeInk(actual, Color.OrangeRed),
                    characterLayout,
                    captureMethod = "originalProviderOwnedBitmap",
                    measurementApi = "Graphics.MeasureString; Bitmap Graphics; HighSpeed; ClearTypeGridFit; default StringFormat",
                    scope = "native bitmap provider/formula proof only; not a desktop, row state, Skia glyph or cross-platform raster claim",
                }, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Path.Combine(directory, "probe.json"), json);
                TestContext.Out.WriteLine($"initialsAvatarEvidence={directory}");
                TestContext.Out.WriteLine(json);
                differentPixels.Should().Be(0, "the independent source formula must reproduce the actual original provider, not just its declared metrics");
            }
            finally
            {
                DisposeProviderDrawingResources(provider);
            }
        });
    }

    private static Font ReadEstimationFont(InitialsAvatarProvider provider)
        => typeof(InitialsAvatarProvider).GetField("_estimationFont", PrivateInstance)?.GetValue(provider) as Font
            ?? throw new MissingFieldException(typeof(InitialsAvatarProvider).FullName, "_estimationFont");

    private static object ReadCharacterLayout(Graphics graphics, string text, Font drawing)
    {
        SizeF measured = graphics.MeasureString(text, drawing);
        RectangleF layout = new(0, 0, measured.Width * 2, measured.Height * 2);
        using StringFormat defaultFormat = StringFormat.GenericDefault;
        using StringFormat typographicFormat = StringFormat.GenericTypographic;
        RectangleF defaultBounds = MeasureCharacterBounds(graphics, text, drawing, layout, defaultFormat);
        RectangleF typographicBounds = MeasureCharacterBounds(graphics, text, drawing, layout, typographicFormat);
        int em = drawing.FontFamily.GetEmHeight(drawing.Style);
        int ascent = drawing.FontFamily.GetCellAscent(drawing.Style);
        int descent = drawing.FontFamily.GetCellDescent(drawing.Style);
        int lineSpacing = drawing.FontFamily.GetLineSpacing(drawing.Style);
        float fontSizeInPixels = drawing.SizeInPoints * graphics.DpiY / 72;
        return new
        {
            defaultBounds = new { defaultBounds.X, defaultBounds.Y, defaultBounds.Width, defaultBounds.Height },
            typographicBounds = new { typographicBounds.X, typographicBounds.Y, typographicBounds.Width, typographicBounds.Height },
            defaultFormatFlags = defaultFormat.FormatFlags.ToString(),
            typographicFormatFlags = typographicFormat.FormatFlags.ToString(),
            em,
            ascent,
            descent,
            lineSpacing,
            fontSizeInPixels,
            ascentPixels = fontSizeInPixels * ascent / em,
            descentPixels = fontSizeInPixels * descent / em,
            fontHeight = drawing.GetHeight(graphics),
            inputHint = graphics.TextRenderingHint.ToString(),
            scope = "actual full-character regions and font metrics, not glyph ink or an assumed padding-to-pixel identity",
        };
    }

    private static RectangleF MeasureCharacterBounds(Graphics graphics, string text, Font font, RectangleF layout, StringFormat format)
    {
        format.SetMeasurableCharacterRanges([new CharacterRange(0, text.Length)]);
        Region[] regions = graphics.MeasureCharacterRanges(text, font, layout, format);
        try
        {
            regions.Should().HaveCount(1);
            return regions[0].GetBounds(graphics);
        }
        finally
        {
            foreach (Region region in regions)
            {
                region.Dispose();
            }
        }
    }

    private static object DescribeInk(Bitmap image, Color background)
    {
        Rectangle ink = Rectangle.Empty;
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                if (image.GetPixel(x, y).ToArgb() != background.ToArgb())
                {
                    Rectangle pixel = new(x, y, 1, 1);
                    ink = ink.IsEmpty ? pixel : Rectangle.Union(ink, pixel);
                }
            }
        }

        return new { ink.X, ink.Y, ink.Width, ink.Height };
    }

    private static void DisposeProviderDrawingResources(InitialsAvatarProvider provider)
    {
        ReadEstimationFont(provider).Dispose();
        if (typeof(InitialsAvatarProvider).GetField("_avatarColors", PrivateInstance)?.GetValue(provider) is ValueTuple<Brush, Color>[] colors)
        {
            foreach ((Brush brush, _) in colors)
            {
                brush.Dispose();
            }
        }
    }

    private static void WithIsolatedSettings(Action action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.InitialsAvatarProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string settingsPath = Path.Combine(directory, "GitExtensions.settings");
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(directory);
        using GitExtSettingsCache cache = new(settingsPath, autoSave: false);
        DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
        AppSettings.UsingContainer(settings, action);
        TestContext.Out.WriteLine($"isolated original settings retained: {directory}");
    }

    private static void InitializeAppSettingsWithoutRealConfiguration(string directory)
    {
        Type settingsType = typeof(AppSettings).Assembly.GetType("GitCommands.Properties.Settings", throwOnError: true)
            ?? throw new TypeLoadException("GitCommands.Properties.Settings");
        ApplicationSettingsBase configuration = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ApplicationSettingsBase
            ?? throw new MissingMemberException(settingsType.FullName, "Default");
        _ = configuration[PortableSettingName];
        SettingsPropertyValue portable = configuration.PropertyValues[PortableSettingName]
            ?? throw new MissingMemberException(settingsType.FullName, PortableSettingName);
        object originalPortableValue = portable.PropertyValue;
        bool originalDirty = portable.IsDirty;
        FieldInfo frameworkPath = typeof(Application).GetField("s_executablePath", PrivateStatic)
            ?? throw new MissingFieldException(typeof(Application).FullName, "s_executablePath");
        object? originalFrameworkPath = frameworkPath.GetValue(null);
        string actualExecutablePath = Application.ExecutablePath;
        string isolatedExecutablePath = Path.Combine(directory, "GitExtensions.exe");
        try
        {
            // The original static constructor opens its first cache before the
            // public container scope exists. Alias that path without creating HWNDs.
            portable.PropertyValue = true;
            frameworkPath.SetValue(null, isolatedExecutablePath);
            _ = AppSettings.SettingsContainer;
            FieldInfo sourcePath = typeof(AppSettings).GetField("_applicationExecutablePath", PrivateStatic)
                ?? throw new MissingFieldException(typeof(AppSettings).FullName, "_applicationExecutablePath");
            if (Equals(sourcePath.GetValue(null), isolatedExecutablePath))
            {
                sourcePath.SetValue(null, actualExecutablePath);
            }
        }
        finally
        {
            frameworkPath.SetValue(null, originalFrameworkPath);
            portable.PropertyValue = originalPortableValue;
            portable.IsDirty = originalDirty;
        }
    }
}
