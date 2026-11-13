using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Windows.Forms.VisualStyles;
using AwesomeAssertions;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Measures the actual dropdown separator and border renderer independently of item layout.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class WorkingDirectoryPopupSeparatorNativeTests
{
    private const int HorizontalSeparatorPart = 6;

    [Test]
    [TestCase(false, false, false, true, 80)]
    [TestCase(false, false, true, true, 137)]
    [TestCase(false, false, false, false, 137)]
    [TestCase(false, false, true, false, 80)]
    [TestCase(true, true, false, true, 80)]
    [TestCase(true, true, true, true, 137)]
    [TestCase(true, true, false, false, 137)]
    [TestCase(true, true, true, false, 80)]
    [TestCase(true, false, false, true, 137)]
    public void Popup_separator_should_follow_the_source_menu_padding_or_native_theme_image(
        bool professional, bool dark, bool rightToLeft, bool enabled, int width)
    {
        SystemColorMode previousMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
            Application.DoEvents();
            using ToolStripDropDownMenu popup = new()
            {
                AutoSize = false,
                Size = new Size(width + 4, 60),
                RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No,
                Renderer = professional ? new ToolStripProfessionalRenderer() : new ToolStripExSystemRenderer(),
            };
            using ToolStripSeparator separator = new()
            {
                AutoSize = false,
                Size = new Size(width, 6),
                Enabled = enabled,
            };
            popup.Items.Add(separator);
            _ = popup.Handle;
            popup.PerformLayout();
            separator.Size = new Size(width, 6);
            popup.DeviceDpi.Should().Be(96);
            popup.DropShadowEnabled.Should().BeTrue();
            SystemInformation.HighContrast.Should().BeFalse();
            separator.GetCurrentParent().Should().BeSameAs(popup);
            separator.RightToLeft.Should().Be(popup.RightToLeft);
            string directory = Path.Combine(TestContext.CurrentContext.TestDirectory,
                "WorkingDirectoryPopupSeparatorProbe", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            Color backdrop = dark ? Color.FromArgb(47, 47, 47) : SystemColors.Menu;
            using Bitmap actual = ToolStripSeparatorPaintTests.RenderSeparator(popup.Renderer, separator, backdrop, vertical: false);
            using Bitmap black = ToolStripSeparatorPaintTests.RenderSeparator(popup.Renderer, separator, Color.Black, vertical: false);
            using Bitmap white = ToolStripSeparatorPaintTests.RenderSeparator(popup.Renderer, separator, Color.White, vertical: false);
            using Bitmap expected = ToolStripSeparatorPaintTests.NewBitmap(separator.Size, backdrop);
            using (Graphics graphics = Graphics.FromImage(expected))
            {
                if (popup.Renderer is ToolStripProfessionalRenderer renderer)
                {
                    Rectangle bounds = new(Point.Empty, separator.Size);
                    if (rightToLeft)
                    {
                        bounds.X += 2;
                        bounds.Width = popup.Width - bounds.X - popup.Padding.Right;
                    }
                    else
                    {
                        bounds.X += popup.Padding.Left - 2;
                        bounds.Width = popup.Width - bounds.X;
                    }

                    using Pen pen = new(renderer.ColorTable.SeparatorDark);
                    graphics.DrawLine(pen, bounds.Left, bounds.Height / 2, bounds.Right - 1, bounds.Height / 2);
                }
                else
                {
                    VisualStyleRenderer part = new("TOOLBAR", HorizontalSeparatorPart, enabled ? 1 : 4);
                    ToolStripSeparatorPaintTests.PaintOnInitializedDeviceContext(graphics, separator.Size, backdrop, deviceContext =>
                    {
                        using Graphics nativeGraphics = Graphics.FromHdc(deviceContext);
                        part.DrawBackground(nativeGraphics, new Rectangle(Point.Empty, separator.Size));
                    });
                }
            }

            using Bitmap border = ToolStripSeparatorPaintTests.NewBitmap(popup.Size, backdrop);
            using Bitmap expectedBorder = ToolStripSeparatorPaintTests.NewBitmap(popup.Size, backdrop);
            Color edge = popup.Renderer is ToolStripProfessionalRenderer professionalRenderer
                ? professionalRenderer.ColorTable.MenuBorder : SystemColors.ControlDark;
            using (Graphics graphics = Graphics.FromImage(border))
            {
                popup.Renderer.DrawToolStripBorder(new ToolStripRenderEventArgs(graphics, popup));
            }

            using (Graphics graphics = Graphics.FromImage(expectedBorder))
            using (Pen pen = new(edge))
            {
                graphics.DrawRectangle(pen, 0, 0, popup.Width - 1, popup.Height - 1);
            }

            actual.Save(Path.Combine(directory, "separator.png"), ImageFormat.Png);
            expected.Save(Path.Combine(directory, "source-contract.png"), ImageFormat.Png);
            black.Save(Path.Combine(directory, "over-black.png"), ImageFormat.Png);
            white.Save(Path.Combine(directory, "over-white.png"), ImageFormat.Png);
            border.Save(Path.Combine(directory, "border.png"), ImageFormat.Png);
            File.WriteAllText(Path.Combine(directory, "probe.json"), JsonSerializer.Serialize(new
            {
                scope = "actual ToolStripDropDownMenu renderer, source item-local rectangle, native96 unscaled GDI diagnostics; not a window capture",
                professional,
                dark,
                rightToLeft,
                enabled,
                renderer = popup.Renderer.GetType().FullName,
                popup.Size,
                popup.Padding,
                separatorBounds = separator.Bounds,
                separatorSize = separator.Size,
                backdrop = $"#{backdrop.ToArgb():X8}",
                border = $"#{edge.ToArgb():X8}",
                nativeTheme = ToolStripSeparatorPaintTests.QueryNativeTheme(popup, separator.Size, enabled ? 1 : 4, HorizontalSeparatorPart),
                pixelsOverBlack = Pixels(black),
                pixelsOverWhite = Pixels(white),
                pixels = Pixels(actual),
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"workingDirectoryPopupSeparatorEvidence={directory}");
            ToolStripSeparatorPaintTests.AssertPixels(actual, expected);
            ToolStripSeparatorPaintTests.AssertPixels(border, expectedBorder);
        }
        finally
        {
            Application.SetColorMode(previousMode);
            Application.DoEvents();
        }
    }

    private static string[][] Pixels(Bitmap bitmap) => Enumerable.Range(0, bitmap.Height)
        .Select(y => Enumerable.Range(0, bitmap.Width).Select(x => $"#{bitmap.GetPixel(x, y).ToArgb():X8}").ToArray()).ToArray();
}
