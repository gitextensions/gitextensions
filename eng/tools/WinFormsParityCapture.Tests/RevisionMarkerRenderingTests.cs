using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using AwesomeAssertions;
using GitExtUtils.GitUI;
using GitUI.Properties;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Exercises the original static revision-marker painter against owned bitmap Graphics.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class RevisionMarkerRenderingTests
{
    private const int InitialOffset = 3;
    private const int CanvasPadding = 8;
    private const int MarkerAdvance = 4;

    private static IEnumerable<TestCaseData> MarkerCases()
    {
        foreach (string asset in new[] { nameof(Images.BisectGood), nameof(Images.BisectBad), nameof(Images.FixupAndSquashMessageMarker) })
        {
            foreach (int height in new[] { 13, 14, 15, 16, 23, 24, 26 })
            {
                foreach (int remainingWidth in new[] { 16, 17, 18, 19, 20, 64 })
                {
                    foreach (bool clipToCell in new[] { false, true })
                    {
                        yield return new TestCaseData(asset, height, remainingWidth, clipToCell);
                    }
                }
            }
        }
    }

    /// <summary>
    ///  Preserves native integer placement, strict fit, inherited clipping and the source advance.
    /// </summary>
    [TestCaseSource(nameof(MarkerCases))]
    public void DrawImage_should_match_original_integer_center_strict_fit_and_caller_clip(
        string asset, int height, int remainingWidth, bool clipToCell)
    {
        // This original helper queries screen DPI caps in its static constructor.
        // Only that read-only query is permitted; rendering remains bitmap-only.
        DpiUtil.DpiX.Should().Be(96);
        DpiUtil.DpiY.Should().Be(96);
        DpiUtil.ScaleX.Should().Be(1);
        DpiUtil.ScaleY.Should().Be(1);
        using Bitmap source = BitmapPaintProbe.ReadAsset(asset);
        source.Size.Should().Be(new Size(16, 16));
        Rectangle cellBounds = new(CanvasPadding, CanvasPadding, InitialOffset + remainingWidth, height);
        Size canvasSize = new(cellBounds.Right + CanvasPadding, cellBounds.Bottom + CanvasPadding);
        Color backdrop = Color.FromArgb(255, 43, 45, 58);
        using Bitmap actual = BitmapPaintProbe.CreateBitmap(canvasSize, backdrop);
        using Bitmap expected = BitmapPaintProbe.CreateBitmap(canvasSize, backdrop);
        int actualOffset = InitialOffset;
        int expectedOffset = InitialOffset;
        int x = cellBounds.X + InitialOffset;
        int y = cellBounds.Y + ((cellBounds.Height - source.Height) / 2);
        bool fits = x + source.Width < cellBounds.Right;
        using (Graphics graphics = Graphics.FromImage(actual))
        {
            if (clipToCell)
            {
                graphics.SetClip(cellBounds);
            }

            DataGridViewCellPaintingEventArgs arguments = BitmapPaintProbe.CreateCellPaintArguments(graphics, cellBounds);
            Type provider = typeof(Images).Assembly.GetType("GitUI.UserControls.RevisionGrid.Columns.MessageColumnProvider", throwOnError: true)
                ?? throw new TypeLoadException("MessageColumnProvider");
            MethodInfo painter = provider.GetMethod("DrawImage", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMethodException(provider.FullName, "DrawImage");
            object[] parameters = [arguments, source, cellBounds, actualOffset];
            painter.Invoke(null, parameters);
            actualOffset = (int)parameters[3];
        }

        using (Graphics graphics = Graphics.FromImage(expected))
        {
            if (clipToCell)
            {
                graphics.SetClip(cellBounds);
            }

            if (fits)
            {
                graphics.DrawImage(source, x, y);
                expectedOffset += source.Width + MarkerAdvance;
            }
        }

        string directory = BitmapPaintProbe.CreateDirectory();
        source.Save(Path.Combine(directory, "source-asset.png"), ImageFormat.Png);
        actual.Save(Path.Combine(directory, "original-static-painter.png"), ImageFormat.Png);
        expected.Save(Path.Combine(directory, "independent-source-formula.png"), ImageFormat.Png);
        BitmapPaintProbe.WriteReport(directory, new
        {
            asset,
            sourceWidth = source.Width,
            sourceHeight = source.Height,
            sourcePixelHash = BitmapPaintProbe.PixelHash(source),
            sourcePixels = BitmapPaintProbe.Pixels(source),
            height,
            remainingWidth,
            clipToCell,
            cellBounds,
            x,
            y,
            relativeY = y - cellBounds.Y,
            fits,
            initialOffset = InitialOffset,
            actualOffset,
            expectedOffset,
            dpiX = DpiUtil.DpiX,
            dpiY = DpiUtil.DpiY,
            backdrop = BitmapPaintProbe.Argb(backdrop),
            actualInk = BitmapPaintProbe.Ink(actual, backdrop),
            expectedInk = BitmapPaintProbe.Ink(expected, backdrop),
            actualPixelHash = BitmapPaintProbe.PixelHash(actual),
            expectedPixelHash = BitmapPaintProbe.PixelHash(expected),
            pixels = BitmapPaintProbe.Pixels(actual),
            captureMethod = "originalPrivateStaticPainterOwnedBitmapGraphics",
            argumentAdapter = "uninitialized event args only; Graphics/CellBounds/ClipBounds private setters; no DataGridView or Control",
            dpiQuery = "original DpiUtil GetDC(NULL)/GetDeviceCaps only; no screen pixels, BitBlt or capture",
            settingsAccessed = false,
            scope = "native96 original helper/formula only; not full-cell paint, desktop capture or cross-platform raster proof",
        });
        actualOffset.Should().Be(expectedOffset);
        BitmapPaintProbe.AssertPixels(actual, expected);
    }
}
