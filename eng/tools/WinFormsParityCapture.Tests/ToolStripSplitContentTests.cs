using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolStripSplitContentTests
{
    private static IEnumerable<TestCaseData> ContentCases()
    {
        foreach (int points in new[] { 9, 11, 18, 22 })
        {
            foreach (bool rightToLeft in new[] { false, true })
            {
                foreach (string role in new[] { "iconOnly", "branch", "workingDirectory" })
                {
                    yield return new TestCaseData(points, "Segoe UI", FontStyle.Regular, rightToLeft, role, false, 0);
                }
            }
        }

        foreach (bool rightToLeft in new[] { false, true })
        {
            yield return new TestCaseData(9, "Segoe UI", FontStyle.Bold, rightToLeft, "branch", false, 0);
            yield return new TestCaseData(11, "Segoe UI", FontStyle.Italic, rightToLeft, "workingDirectory", false, 0);
            yield return new TestCaseData(18, "Consolas", FontStyle.Regular, rightToLeft, "branch", false, 0);
            yield return new TestCaseData(9, "Segoe UI", FontStyle.Regular, rightToLeft, "branch", true, 0);
            yield return new TestCaseData(22, "Segoe UI", FontStyle.Regular, rightToLeft, "branch", true, 0);
            foreach (int points in new[] { 9, 22 })
            {
                foreach (string role in new[] { "branch", "workingDirectory" })
                {
                    foreach (int fixedWidth in new[] { 60, 180 })
                    {
                        yield return new TestCaseData(points, "Segoe UI", FontStyle.Regular, rightToLeft, role, false, fixedWidth);
                    }
                }
            }
        }
    }

    [TestCaseSource(nameof(ContentCases))]
    public void Source_preferred_size_and_content_rectangles_should_record_actual_font_and_alignment_inputs(
        int points, string family, FontStyle style, bool rightToLeft, string role, bool longCaption, int fixedWidth)
    {
        using Font font = new(family, points, style, GraphicsUnit.Point);
        using Form host = new()
        {
            AutoScaleMode = AutoScaleMode.None,
            ClientSize = new Size(1800, 240),
            ShowInTaskbar = false,
        };
        using ToolStripEx strip = new()
        {
            Dock = DockStyle.None,
            AutoSize = true,
            Size = new Size(1700, 25),
            Font = font,
            ForeColor = Color.Black,
            BackColor = Color.White,
            GripStyle = ToolStripGripStyle.Hidden,
            Padding = Padding.Empty,
            ImageScalingSize = new Size(16, 16),
            LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow,
        };
        host.Controls.Add(strip);
        _ = host.Handle;
        _ = strip.Handle;
        strip.DeviceDpi.Should().Be(96, "this is a native baseline font/layout probe, not a synthesized DPI image");

        using Bitmap image = new(16, 16);
        using (Graphics graphics = Graphics.FromImage(image))
        {
            graphics.Clear(Color.Magenta);
        }

        using ToolStripSplitButton item = CreateSourceItem(role, image);
        item.AutoSize = fixedWidth == 0;
        if (fixedWidth > 0)
        {
            item.Width = fixedWidth;
        }

        item.Margin = new Padding(0, 1, 0, 2);
        item.RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No;
        if (longCaption)
        {
            item.Text = "A considerably longer translated branch caption";
        }

        strip.Items.Add(item);
        strip.PerformLayout();
        Rectangle imageRectangle = Rectangle.Empty;
        Rectangle textRectangle = Rectangle.Empty;
        TextFormatFlags? textFlags = null;
        object? paintFont = null;
        strip.Renderer.RenderItemImage += (_, e) =>
        {
            if (ReferenceEquals(e.Item, item))
            {
                imageRectangle = e.ImageRectangle;
            }
        };
        strip.Renderer.RenderItemText += (_, e) =>
        {
            if (ReferenceEquals(e.Item, item))
            {
                textRectangle = e.TextRectangle;
                textFlags = e.TextFormat;
                paintFont = DescribeFont(e.TextFont ?? throw new InvalidOperationException("The native renderer must supply its actual text font."));
            }
        };

        using Bitmap frame = new(strip.Width, strip.Height, PixelFormat.Format32bppArgb);
        frame.SetResolution(96, 96);
        strip.DrawToBitmap(frame, strip.ClientRectangle);
        item.Font.Name.Should().Be(font.Name);
        item.Font.SizeInPoints.Should().Be(font.SizeInPoints);
        item.Font.Style.Should().Be(font.Style);
        item.ButtonBounds.Width.Should().Be(Math.Max(0, item.Width - item.DropDownButtonWidth - item.SplitterBounds.Width));
        item.DropDownButtonWidth.Should().Be(11);
        if (fixedWidth == 0)
        {
            imageRectangle.Size.Should().Be(new Size(16, 16));
        }

        item.ButtonBounds.Contains(imageRectangle).Should().BeTrue();
        if (role == "workingDirectory")
        {
            item.ImageAlign.Should().Be(ContentAlignment.MiddleLeft);
            item.TextAlign.Should().Be(ContentAlignment.MiddleLeft);
        }

        if (role != "iconOnly")
        {
            textFlags.Should().NotBeNull();
            textRectangle.IsEmpty.Should().BeFalse();
            item.ButtonBounds.Contains(textRectangle).Should().BeTrue();
            paintFont.Should().NotBeNull();
        }

        object unconstrained = DescribeSize(item.GetPreferredSize(Size.Empty));
        object[] constrained = new[] { 32, 60, 83, 180 }
            .Select(width => (object)new { constraint = DescribeSize(new Size(width, 0)), preferred = DescribeSize(item.GetPreferredSize(new Size(width, 0))) })
            .ToArray();
        using Graphics sourceGraphics = host.CreateGraphics();
        float graphicsCaptionWidth = sourceGraphics.MeasureString(item.Text, item.Font).Width;
        object metadata = new
        {
            inputMode = "sourceConsumerPreferredSizeAndRenderer",
            captureMethod = "drawToBitmapClientOnly",
            dpiMode = "nativeMonitor",
            deviceDpi = strip.DeviceDpi,
            applicationColorMode = Application.ColorMode.ToString(),
            sourceLayout = "ToolStripItem.ToolStripItemInternalLayout.CommonLayoutOptions; SplitButtonButtonLayout offsets primary content into the item",
            sourceLayoutInputs = new { borderSize = 2, paddingSize = 0, textImageInset = 0 },
            requestedFont = new { family, points, style = style.ToString() },
            sourceFont = DescribeFont(item.Font),
            paintFont,
            ownerHandleFont = ReadOwnerFontMetrics(strip),
            role,
            fixedWidth,
            sourceClass = item.GetType().FullName,
            longCaption,
            rightToLeft,
            text = item.Text,
            imageSize = item.Image is null ? null : DescribeSize(item.Image.Size),
            displayStyle = item.DisplayStyle.ToString(),
            imageAlign = item.ImageAlign.ToString(),
            textAlign = item.TextAlign.ToString(),
            textImageRelation = item.TextImageRelation.ToString(),
            item.AutoSize,
            size = DescribeSize(item.Size),
            bounds = DescribeRect(item.Bounds),
            primary = DescribeRect(item.ButtonBounds),
            splitter = DescribeRect(item.SplitterBounds),
            dropdown = DescribeRect(item.DropDownButtonBounds),
            image = DescribeRect(imageRectangle),
            caption = DescribeRect(textRectangle),
            textFlags = textFlags?.ToString(),
            unconstrained,
            constrained,
            nominalMeasureText = role == "iconOnly" ? null : DescribeSize(TextRenderer.MeasureText(item.Text, item.Font)),
            sourceRendererMeasureText = textFlags is TextFormatFlags flags
                ? DescribeSize(TextRenderer.MeasureText(item.Text, item.Font, Size.Empty, flags)) : null,
            graphicsCaptionWidth,
            workingDirectoryFixedWidthBeforeConfiguredMinimum = role == "workingDirectory"
                ? (int?)(int)(graphicsCaptionWidth + item.DropDownButtonWidth + 5) : null,
            note = "Owner WM_GETFONT metrics and nominal TextRenderer measurements are separate inputs, not proof of cached paint quality or raster identity.",
        };
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "SplitContentProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        frame.Save(Path.Combine(directory, "source-client.png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(directory, "probe.json"), JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        TestContext.Out.WriteLine($"splitContentProbeEvidence={directory}");
        TestContext.Out.WriteLine(JsonSerializer.Serialize(metadata));
    }

    private static ToolStripSplitButton CreateSourceItem(string role, Image image)
    {
        if (role == "workingDirectory")
        {
            // Consume the unchanged original constructor: its image and explicit
            // MiddleLeft alignments are not a re-created approximation of the class.
            Type type = typeof(ToolStripEx).Assembly.GetType("GitUI.CommandsDialogs.Menus.WorkingDirectoryToolStripSplitButton", throwOnError: true)!;
            ToolStripSplitButton item = (ToolStripSplitButton)(Activator.CreateInstance(type)
                ?? throw new InvalidOperationException("The source working-directory item must be constructible."));
            item.Text = "WorkingDir";
            return item;
        }

        return new ToolStripSplitButton
        {
            Name = role == "iconOnly" ? "toolStripButtonLevelUp" : "branchSelect",
            Image = image,
            DisplayStyle = role == "iconOnly" ? ToolStripItemDisplayStyle.Image : ToolStripItemDisplayStyle.ImageAndText,
            Text = role == "iconOnly" ? string.Empty : "Branch",
        };
    }

    private static object DescribeFont(Font font)
        => new { font.Name, font.OriginalFontName, family = font.FontFamily.Name, font.SizeInPoints, unit = font.Unit.ToString(), style = font.Style.ToString(), font.Height };

    private static object DescribeSize(Size size) => new { width = size.Width, height = size.Height };

    private static object DescribeRect(Rectangle rectangle)
        => new { x = rectangle.X, y = rectangle.Y, width = rectangle.Width, height = rectangle.Height };

    private static object ReadOwnerFontMetrics(ToolStrip strip)
    {
        nint deviceContext = GetDC(strip.Handle);
        deviceContext.Should().NotBe(0);
        nint font = SendMessageW(strip.Handle, 0x0031, 0, 0);
        if (font == 0)
        {
            ReleaseDC(strip.Handle, deviceContext);
            return new
            {
                context = "actualToolStripHwndWM_GETFONT",
                status = "unsupported",
                reason = "The owner-drawn ToolStrip HWND does not expose a font through WM_GETFONT; item.Font and the actual RenderItemText font remain separately recorded.",
            };
        }

        nint previous = SelectObject(deviceContext, font);
        try
        {
            GetTextMetricsW(deviceContext, out NativeTextMetrics metrics).Should().BeTrue();
            StringBuilder family = new(256);
            GetTextFaceW(deviceContext, family.Capacity, family).Should().BeGreaterThan(0);
            return new
            {
                context = "actualToolStripHwndWM_GETFONT",
                status = "available",
                family = family.ToString(),
                metrics.Height,
                metrics.Ascent,
                metrics.Descent,
                metrics.InternalLeading,
                metrics.ExternalLeading,
                metrics.AverageCharacterWidth,
                metrics.MaximumCharacterWidth,
                metrics.Weight,
                metrics.Italic,
            };
        }
        finally
        {
            SelectObject(deviceContext, previous);
            ReleaseDC(strip.Handle, deviceContext);
        }
    }

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessageW(nint window, uint message, nint firstParameter, nint secondParameter);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint deviceContext, nint handle);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetTextMetricsW(nint deviceContext, out NativeTextMetrics metrics);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetTextFaceW(nint deviceContext, int count, StringBuilder text);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeTextMetrics
    {
        public int Height;
        public int Ascent;
        public int Descent;
        public int InternalLeading;
        public int ExternalLeading;
        public int AverageCharacterWidth;
        public int MaximumCharacterWidth;
        public int Weight;
        public int Overhang;
        public int DigitizedAspectX;
        public int DigitizedAspectY;
        public char FirstCharacter;
        public char LastCharacter;
        public char DefaultCharacter;
        public char BreakCharacter;
        public byte Italic;
        public byte Underlined;
        public byte StrikeOut;
        public byte PitchAndFamily;
        public byte CharacterSet;
    }
}
