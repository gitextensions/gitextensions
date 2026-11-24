using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using AwesomeAssertions;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUI.Theming;
using NSubstitute;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Exercises framework menu-image and ownerless separator painting on owned bitmaps only.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class NativeMenuPaintTests
{
    private const string SyntheticAsset = "synthetic-alpha-rgb";
    private const string DisabledMatrixField = "s_disabledImageColorMatrix";
    private const float RedWeight = 0.2125f;
    private const float GreenWeight = 0.2577f;
    private const float BlueWeight = 0.0361f;
    private const float LightTranslation = 0.38f;
    private const float DarkTranslation = -0.1f;
    private const float LightAlpha = 0.7f;
    private const float DarkAlpha = 0.8f;

    private static IEnumerable<TestCaseData> ImageCases()
    {
        foreach (string theme in new[] { "light", "dark", "custom" })
        {
            foreach (string asset in new[] { nameof(Images.BranchDelete), nameof(Images.BisectGood), nameof(Images.BisectBad), nameof(Images.FixupAndSquashMessageMarker), SyntheticAsset })
            {
                yield return new TestCaseData(theme, asset);
            }
        }
    }

    /// <summary>
    ///  Measures both original APIs and their distinct alpha contracts, including transparent source pixels.
    /// </summary>
    [TestCaseSource(nameof(ImageCases))]
    public void CreateDisabledImage_should_match_source_matrix_and_preserve_its_actual_alpha_pipeline(string themeName, string asset)
    {
        Theme theme = LoadToolTheme(themeName);
        SystemColorMode previousMode = Application.ColorMode;
        FieldInfo matrixField = ReadMatrixField();
        object? previousMatrix = matrixField.GetValue(null);
        string directory = BitmapPaintProbe.CreateDirectory();
        try
        {
            Application.SetColorMode(theme.SystemColorMode);
            bool dark = Application.IsDarkModeEnabled;
            dark.Should().Be(theme.SystemColorMode == SystemColorMode.Dark);

            // The original caches this matrix for the process. Reset only the
            // owned test host to model an independent fresh startup per theme.
            matrixField.SetValue(null, null);
            using Bitmap source = ReadSource(asset);
            using Bitmap actual = (Bitmap)ToolStripRenderer.CreateDisabledImage(source);
            using Bitmap expected = DrawMatrix(source, CreateSourceMatrix(dark, toolStrip: true), Color.Transparent);
            using Bitmap controlPaint = BitmapPaintProbe.CreateBitmap(source.Size, Color.Transparent);
            using (Graphics graphics = Graphics.FromImage(controlPaint))
            {
                ControlPaint.DrawImageDisabled(graphics, source, 0, 0, Color.Magenta);
            }

            using Bitmap controlPaintExpected = DrawMatrix(source, CreateSourceMatrix(dark: false, toolStrip: false), Color.Transparent);
            source.Save(Path.Combine(directory, "source-asset.png"), ImageFormat.Png);
            actual.Save(Path.Combine(directory, "original-toolstrip-disabled.png"), ImageFormat.Png);
            expected.Save(Path.Combine(directory, "independent-source-matrix.png"), ImageFormat.Png);
            controlPaint.Save(Path.Combine(directory, "original-controlpaint-disabled.png"), ImageFormat.Png);
            controlPaintExpected.Save(Path.Combine(directory, "independent-controlpaint-matrix.png"), ImageFormat.Png);
            List<object> backdrops = [];
            foreach (Color backdrop in new[] { Color.Black, Color.White, theme.GetColor(AppColor.PanelBackground) })
            {
                using Bitmap composed = Composite(actual, backdrop);
                using Bitmap expectedComposed = Composite(expected, backdrop);
                string file = $"toolstrip-over-{backdrop.ToArgb():X8}.png";
                composed.Save(Path.Combine(directory, file), ImageFormat.Png);
                backdrops.Add(new
                {
                    backdrop = BitmapPaintProbe.Argb(backdrop),
                    file,
                    pixels = BitmapPaintProbe.Pixels(composed),
                });
                BitmapPaintProbe.AssertPixels(composed, expectedComposed);
            }

            ColorMatrix nativeMatrix = matrixField.GetValue(null) as ColorMatrix
                ?? throw new InvalidOperationException("The original API must retain its actual first-use color matrix.");
            BitmapPaintProbe.WriteReport(directory, new
            {
                themeName,
                asset,
                requestedMode = theme.SystemColorMode.ToString(),
                actualMode = Application.ColorMode.ToString(),
                dark,
                runtime = FileVersionInfo.GetVersionInfo(typeof(ToolStripRenderer).Assembly.Location).FileVersion,
                nativeMatrix = MatrixRows(nativeMatrix),
                sourcePixelHash = BitmapPaintProbe.PixelHash(source),
                sourcePixels = BitmapPaintProbe.Pixels(source),
                actualPixelHash = BitmapPaintProbe.PixelHash(actual),
                expectedPixelHash = BitmapPaintProbe.PixelHash(expected),
                toolStripPixels = BitmapPaintProbe.Pixels(actual),
                controlPaintPixels = BitmapPaintProbe.Pixels(controlPaint),
                backdrops,
                captureMethod = "originalFrameworkApisOwnedBitmapGraphics",
                settingsAccessed = false,
                themeSource = "consumer-owned installed Themes directory only; no user theme/config reads or writes",
                sourceReference = "dotnet/winforms v10.0.14 ToolStripRenderer.DisabledImageColorMatrix/CreateDisabledImage; ControlPaint.DrawImageDisabled",
                scope = "native96 bitmap transformation/compositing only; not an opened menu, desktop capture or portable raster claim",
            });
            MatrixRows(nativeMatrix).Should().BeEquivalentTo(MatrixRows(CreateSourceMatrix(dark, toolStrip: true)), options => options.WithStrictOrdering());
            BitmapPaintProbe.AssertPixels(actual, expected);
            BitmapPaintProbe.AssertPixels(controlPaint, controlPaintExpected);
        }
        finally
        {
            matrixField.SetValue(null, previousMatrix);
            Application.SetColorMode(previousMode);
        }
    }

    /// <summary>
    ///  Preserves the original's first-draw mode cache rather than silently improving live mode switches.
    /// </summary>
    [TestCase(false)]
    [TestCase(true)]
    public void CreateDisabledImage_should_retain_first_draw_matrix_after_native_mode_switch(bool firstDark)
    {
        SystemColorMode previousMode = Application.ColorMode;
        FieldInfo matrixField = ReadMatrixField();
        object? previousMatrix = matrixField.GetValue(null);
        try
        {
            Application.SetColorMode(firstDark ? SystemColorMode.Dark : SystemColorMode.Classic);
            Application.IsDarkModeEnabled.Should().Be(firstDark);
            matrixField.SetValue(null, null);
            using Bitmap source = BitmapPaintProbe.ReadAsset(nameof(Images.BranchDelete));
            using Bitmap first = (Bitmap)ToolStripRenderer.CreateDisabledImage(source);
            object? firstMatrix = matrixField.GetValue(null);
            Application.SetColorMode(firstDark ? SystemColorMode.Classic : SystemColorMode.Dark);
            Application.IsDarkModeEnabled.Should().Be(!firstDark);
            using Bitmap afterSwitch = (Bitmap)ToolStripRenderer.CreateDisabledImage(source);
            using Bitmap expected = DrawMatrix(source, CreateSourceMatrix(firstDark, toolStrip: true), Color.Transparent);
            string directory = BitmapPaintProbe.CreateDirectory();
            source.Save(Path.Combine(directory, "source-asset.png"), ImageFormat.Png);
            first.Save(Path.Combine(directory, "original-first-mode.png"), ImageFormat.Png);
            afterSwitch.Save(Path.Combine(directory, "original-after-mode-switch.png"), ImageFormat.Png);
            expected.Save(Path.Combine(directory, "independent-first-mode-matrix.png"), ImageFormat.Png);
            BitmapPaintProbe.WriteReport(directory, new
            {
                firstDark,
                actualDarkAfterSwitch = Application.IsDarkModeEnabled,
                sourcePixelHash = BitmapPaintProbe.PixelHash(source),
                firstPixelHash = BitmapPaintProbe.PixelHash(first),
                switchedPixelHash = BitmapPaintProbe.PixelHash(afterSwitch),
                pixels = BitmapPaintProbe.Pixels(afterSwitch),
                settingsAccessed = false,
                captureMethod = "originalFrameworkFirstUseMatrixCacheOwnedBitmaps",
            });
            matrixField.GetValue(null).Should().BeSameAs(firstMatrix);
            BitmapPaintProbe.AssertPixels(first, afterSwitch);
            BitmapPaintProbe.AssertPixels(first, expected);
        }
        finally
        {
            matrixField.SetValue(null, previousMatrix);
            Application.SetColorMode(previousMode);
        }
    }

    /// <summary>
    ///  Measures the honest ownerless renderer branch without constructing a prohibited dropdown Control.
    /// </summary>
    [TestCase("light", 5)]
    [TestCase("light", 6)]
    [TestCase("light", 7)]
    [TestCase("dark", 5)]
    [TestCase("dark", 6)]
    [TestCase("dark", 7)]
    [TestCase("custom", 5)]
    [TestCase("custom", 6)]
    [TestCase("custom", 7)]
    public void DrawSeparator_should_match_ownerless_source_lines_and_record_dropdown_branch_limitation(string themeName, int height)
    {
        Theme theme = LoadToolTheme(themeName);
        SystemColorMode previousMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(theme.SystemColorMode);
            using ToolStripSeparator separator = new() { AutoSize = false, Size = new Size(64, height), Margin = Padding.Empty };
            separator.Owner.Should().BeNull();
            separator.GetCurrentParent().Should().BeNull();
            SystemInformation.HighContrast.Should().BeFalse();
            ToolStripProfessionalRenderer renderer = new();
            Color backdrop = theme.GetColor(AppColor.PanelBackground);
            using Bitmap actual = BitmapPaintProbe.CreateBitmap(separator.Size, backdrop);
            using Bitmap expected = BitmapPaintProbe.CreateBitmap(separator.Size, backdrop);
            using (Graphics graphics = Graphics.FromImage(actual))
            {
                renderer.DrawSeparator(new ToolStripSeparatorRenderEventArgs(graphics, separator, vertical: false));
            }

            int y = height / 2;
            Rectangle sourceBounds = new(Point.Empty, separator.Size);
            sourceBounds.Inflate(-2, 0);
            using (Graphics graphics = Graphics.FromImage(expected))
            using (Pen dark = new(renderer.ColorTable.SeparatorDark))
            using (Pen light = new(renderer.ColorTable.SeparatorLight))
            {
                graphics.DrawLine(dark, sourceBounds.Left, y, sourceBounds.Right - 1, y);
                graphics.DrawLine(light, sourceBounds.Left + 1, y + 1, sourceBounds.Right - 1, y + 1);
            }

            string directory = BitmapPaintProbe.CreateDirectory();
            actual.Save(Path.Combine(directory, "original-ownerless-renderer.png"), ImageFormat.Png);
            expected.Save(Path.Combine(directory, "independent-ownerless-source-lines.png"), ImageFormat.Png);
            BitmapPaintProbe.WriteReport(directory, new
            {
                themeName,
                height,
                y,
                actualDark = Application.IsDarkModeEnabled,
                backdrop = BitmapPaintProbe.Argb(backdrop),
                separatorDark = BitmapPaintProbe.Argb(renderer.ColorTable.SeparatorDark),
                separatorLight = BitmapPaintProbe.Argb(renderer.ColorTable.SeparatorLight),
                actualInk = BitmapPaintProbe.Ink(actual, backdrop),
                pixels = BitmapPaintProbe.Pixels(actual),
                captureMethod = "originalOwnerlessProfessionalRendererBitmapGraphics",
                settingsAccessed = false,
                sourceReference = "dotnet/winforms v10.0.14 ToolstripProfessionalRenderer.RenderSeparatorInternal 1334-1413",
                limitation = "ownerless branch paints two rows; actual ToolStripDropDownMenu branch suppresses highlight, but requires a Control and is not invoked here",
                dropdownEvidence = "one-row popup evidence must come from retained real-native captures and inspected source, not this ownerless bitmap",
            });
            BitmapPaintProbe.AssertPixels(actual, expected);
        }
        finally
        {
            Application.SetColorMode(previousMode);
        }
    }

    private static Bitmap Composite(Bitmap image, Color backdrop)
    {
        Bitmap result = BitmapPaintProbe.CreateBitmap(image.Size, backdrop);
        using Graphics graphics = Graphics.FromImage(result);
        graphics.DrawImage(image, 0, 0);
        return result;
    }

    private static ColorMatrix CreateSourceMatrix(bool dark, bool toolStrip)
    {
        float translation = dark ? DarkTranslation : LightTranslation;
        float alpha = toolStrip ? dark ? DarkAlpha : LightAlpha : 1;
        return new ColorMatrix(
        [
            [RedWeight, RedWeight, RedWeight, 0, 0],
            [GreenWeight, GreenWeight, GreenWeight, 0, 0],
            [BlueWeight, BlueWeight, BlueWeight, 0, 0],
            [0, 0, 0, alpha, 0],
            [translation, translation, translation, 0, toolStrip ? 0 : 1],
        ]);
    }

    private static Bitmap DrawMatrix(Bitmap source, ColorMatrix matrix, Color backdrop)
    {
        Bitmap result = BitmapPaintProbe.CreateBitmap(source.Size, backdrop);
        using Graphics graphics = Graphics.FromImage(result);
        using ImageAttributes attributes = new();
        attributes.ClearColorKey();
        attributes.SetColorMatrix(matrix);
        graphics.DrawImage(source, new Rectangle(Point.Empty, source.Size), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
        return result;
    }

    private static Theme LoadToolTheme(string name)
    {
        if (name == "light")
        {
            return Theme.CreateDefaultTheme();
        }

        string themeRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "Themes");
        IThemeCssUrlResolver resolver = Substitute.For<IThemeCssUrlResolver>();
        resolver.ResolveCssUrl(Arg.Any<string>()).Returns(call => Path.Combine(themeRoot, call.Arg<string>()));
        ThemeLoader loader = new(resolver, new ThemeFileReader());
        string file = name == "custom" ? "parity-custom.css" : "dark.css";
        return loader.LoadTheme(Path.Combine(themeRoot, file), new ThemeId(name), []);
    }

    private static float[][] MatrixRows(ColorMatrix matrix) => Enumerable.Range(0, 5)
        .Select(row => Enumerable.Range(0, 5).Select(column => matrix[row, column]).ToArray()).ToArray();

    private static FieldInfo ReadMatrixField() => typeof(ToolStripRenderer).GetField(DisabledMatrixField, BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingFieldException(typeof(ToolStripRenderer).FullName, DisabledMatrixField);

    private static Bitmap ReadSource(string asset)
    {
        if (asset != SyntheticAsset)
        {
            return BitmapPaintProbe.ReadAsset(asset);
        }

        Bitmap source = BitmapPaintProbe.CreateBitmap(new Size(16, 16), Color.Transparent);
        int[] alphaValues = [0, 1, 2, 32, 64, 127, 128, 192, 254, 255];
        Color[] rgbValues = [Color.Black, Color.White, Color.Red, Color.Lime, Color.Blue, Color.FromArgb(225, 29, 29), Color.FromArgb(31, 97, 173), Color.FromArgb(113, 143, 217)];
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                source.SetPixel(x, y, Color.FromArgb(alphaValues[(x + y) % alphaValues.Length], rgbValues[x % rgbValues.Length]));
            }
        }

        return source;
    }
}
