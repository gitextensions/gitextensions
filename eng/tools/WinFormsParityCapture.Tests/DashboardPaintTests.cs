using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class DashboardPaintTests
{
    [Test]
    [TestCase(9)]
    [TestCase(14)]
    [TestCase(20)]
    public void Dashboard_heading_should_paint_with_the_same_TextRenderer_font_as_its_preferred_size(int applicationPoints)
    {
        using Font font = new("Segoe UI", applicationPoints + 5.5f);
        using Label heading = new()
        {
            AutoSize = true,
            Font = font,
            Text = "Contribute",
            UseCompatibleTextRendering = false,
            BackColor = Color.White,
            ForeColor = Color.Black,
        };
        heading.CreateControl();
        heading.Size = heading.PreferredSize;
        using Bitmap actual = new(heading.Width, heading.Height);
        actual.SetResolution(96, 96);
        heading.DrawToBitmap(actual, heading.ClientRectangle);
        using Bitmap expected = new(heading.Width, heading.Height);
        expected.SetResolution(96, 96);
        using (Graphics graphics = Graphics.FromImage(expected))
        {
            graphics.Clear(Color.White);
            MethodInfo flagsMethod = typeof(Label).GetMethod("CreateTextFormatFlags", BindingFlags.Instance | BindingFlags.NonPublic,
                Type.EmptyTypes) ?? throw new InvalidOperationException("The source label text flags must exist.");
            TextFormatFlags flags = (TextFormatFlags)flagsMethod.Invoke(heading, null)!;
            TextRenderer.DrawText(graphics, heading.Text, font, heading.ClientRectangle, heading.ForeColor, flags);
        }

        Rectangle actualInk = FindInk(actual, Color.White);
        Rectangle expectedInk = FindInk(expected, Color.White);
        actualInk.Should().Be(expectedInk);
        actualInk.IsEmpty.Should().BeFalse();
        heading.PreferredSize.Should().Be(TextRenderer.MeasureText(heading.Text, font));
        (TextMetric metrics, Size extent) = ReadTextRendererFont(font, heading.Text);
        metrics.Height.Should().Be(heading.PreferredSize.Height);
        float overhang = metrics.Height / 6f;
        extent.Width.Should().Be(heading.PreferredSize.Width - (int)Math.Ceiling(overhang) - (int)Math.Ceiling(overhang * 1.5f),
            "the cached integral-em HFONT must match the real Label's TextRenderer advance");
        TestContext.Progress.WriteLine($"heading requested={font.SizeInPoints} preferred={heading.PreferredSize} ink={actualInk} expectedInk={expectedInk} fontHeight={font.Height} cacheEm={Math.Ceiling(font.SizeInPoints * 96 / 72)} cacheAscent={metrics.Ascent} cacheHeight={metrics.Height} advance={extent.Width}");
    }

    [Test]
    [TestCase(9, 12, 6)]
    [TestCase(14, 20, 9)]
    [TestCase(20, 30, 13)]
    public void Dashboard_link_should_keep_native_caption_hit_focus_and_image_regions_separate(int points, int nativeAscent, int nativeInkTop)
    {
        using Font font = new("Segoe UI", points);
        using Bitmap icon = new(16, 16);
        using MeasuredLinkLabel link = new()
        {
            Font = font,
            Text = "Clone repository",
            AutoSize = true,
            AutoEllipsis = true,
            UseCompatibleTextRendering = false,
            Padding = new Padding(24, 3, 3, 3),
            Image = icon,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleLeft,
            LinkBehavior = LinkBehavior.NeverUnderline,
            BackColor = Color.White,
            LinkColor = Color.Black,
        };
        link.CreateControl();
        link.Size = link.PreferredSize;
        using Bitmap actual = new(link.Width, link.Height);
        actual.SetResolution(96, 96);
        link.DrawToBitmap(actual, link.ClientRectangle);
        Rectangle captionInk = FindInk(actual, Color.White);
        captionInk.Top.Should().Be(nativeInkTop,
            "the real native LinkLabel paint origin must remain independent of its hit/focus run");
        (TextMetric metrics, Size _) = ReadTextRendererFont(font, link.Text);
        metrics.Ascent.Should().Be(nativeAscent);
        using Bitmap expectedCaption = new(link.Width, link.Height);
        expectedCaption.SetResolution(96, 96);
        using (Graphics expectedGraphics = Graphics.FromImage(expectedCaption))
        {
            expectedGraphics.Clear(Color.White);
            MethodInfo flagsMethod = typeof(Label).GetMethod("CreateTextFormatFlags", BindingFlags.Instance | BindingFlags.NonPublic,
                Type.EmptyTypes) ?? throw new InvalidOperationException("The source label text flags must exist.");
            TextFormatFlags flags = (TextFormatFlags)flagsMethod.Invoke(link, null)!;
            Rectangle paddedClient = new(link.Padding.Left, link.Padding.Top,
                link.ClientSize.Width - link.Padding.Horizontal, link.ClientSize.Height - link.Padding.Vertical);
            TextRenderer.DrawText(expectedGraphics, link.Text, font, paddedClient, Color.Black, flags);
        }

        captionInk.Should().Be(FindInk(expectedCaption, Color.White),
            "the real native LinkLabel must use the source padded TextRenderer paint rectangle");
        using Graphics graphics = Graphics.FromImage(actual);
        FieldInfo regionField = typeof(LinkLabel).GetField("_textRegion", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The native LinkLabel run region must exist.");
        Region region = (Region)(regionField.GetValue(link)
            ?? throw new InvalidOperationException("Painting must establish the native caption region."));
        Rectangle regionBounds = Rectangle.Round(region.GetBounds(graphics));
        Size measured = TextRenderer.MeasureText(link.Text, font);
        Rectangle expectedFocus = new(link.Padding.Left, 0, measured.Width, measured.Height);
        link.HasLinkAt(8, 8).Should().BeFalse("the image is not part of LinkLabel's hyperlink run");
        link.HasLinkAt(regionBounds.Left + (regionBounds.Width / 2), regionBounds.Top + (regionBounds.Height / 2)).Should().BeTrue();
        regionBounds.Left.Should().Be(link.Padding.Left);
        regionBounds.Top.Should().Be(0, "LinkLabel's middle-alignment helper centers within the deflated client height without adding its Y origin");
        regionBounds.Height.Should().Be(measured.Height);
        Rectangle image = link.ImageBounds();
        image.X.Should().Be(2);
        image.Y.Should().Be((link.Height - icon.Height) / 2);
        link.MovePointer(new Point(8, 8));
        link.PressPointer(new Point(8, 8));
        link.DrawToBitmap(actual, link.ClientRectangle);
        CountColor(actual, Color.Red).Should().Be(0, "pressing the image does not activate the native link run");
        link.ReleasePointer(new Point(8, 8));
        Point caption = new(regionBounds.Left + (regionBounds.Width / 2), regionBounds.Top + (regionBounds.Height / 2));
        link.MovePointer(caption);
        link.PressPointer(caption);
        link.DrawToBitmap(actual, link.ClientRectangle);
        CountColor(actual, Color.Red).Should().BeGreaterThan(0, "the native caption run uses ActiveLinkColor while pressed");
        link.ReleasePointer(caption);
        TestContext.Progress.WriteLine($"link points={points} preferred={link.PreferredSize} region={regionBounds} focus={expectedFocus} image={image} ink={captionInk} cacheAscent={metrics.Ascent} cacheHeight={metrics.Height}");
    }

    private static int CountColor(Bitmap bitmap, Color color)
    {
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).ToArgb() == color.ToArgb())
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static Rectangle FindInk(Bitmap bitmap, Color background)
    {
        Rectangle ink = Rectangle.Empty;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).ToArgb() != background.ToArgb())
                {
                    Rectangle pixel = new(x, y, 1, 1);
                    ink = ink.IsEmpty ? pixel : Rectangle.Union(ink, pixel);
                }
            }
        }

        return ink;
    }

    private static (TextMetric Metrics, Size Extent) ReadTextRendererFont(Font requested, string text)
    {
        nint context = GetDC(0);
        context.Should().NotBe(0);
        nint font = CreateFont(-(int)Math.Ceiling(requested.SizeInPoints * 96 / 72), 0, 0, 0, 400,
            false, false, false, 1, 0, 0, 0, 0, requested.Name);
        font.Should().NotBe(0);
        nint previous = SelectObject(context, font);
        try
        {
            GetTextMetrics(context, out TextMetric metrics).Should().BeTrue();
            GetTextExtentPoint(context, text, text.Length, out Size extent).Should().BeTrue();
            return (metrics, extent);
        }
        finally
        {
            SelectObject(context, previous);
            DeleteObject(font);
            ReleaseDC(0, context);
        }
    }

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint context);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateFont(int height, int width, int escapement, int orientation, int weight,
        bool italic, bool underline, bool strikeOut, int charSet, int outPrecision, int clipPrecision, int quality, int pitchAndFamily, string name);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint context, nint value);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetTextMetrics(nint context, out TextMetric metrics);

    [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W", CharSet = CharSet.Unicode)]
    private static extern bool GetTextExtentPoint(nint context, string text, int length, out Size extent);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct TextMetric
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
        public byte StruckOut;
        public byte PitchAndFamily;
        public byte CharacterSet;
    }

    private sealed class MeasuredLinkLabel : LinkLabel
    {
        internal bool HasLinkAt(int x, int y) => PointInLink(x, y) is not null;

        internal Rectangle ImageBounds() => CalcImageRenderBounds(Image!, ClientRectangle, ImageAlign);

        internal void MovePointer(Point point) => OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0));

        internal void PressPointer(Point point) => OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0));

        internal void ReleasePointer(Point point) => OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0));
    }
}
