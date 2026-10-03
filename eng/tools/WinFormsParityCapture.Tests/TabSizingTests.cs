using System.Drawing;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class TabSizingTests
{
    [Test]
    [TestCase(9)]
    [TestCase(11)]
    [TestCase(14)]
    public void Image_free_tabs_should_retain_the_native_font_based_minimum_width(int points)
    {
        using Font font = new("Segoe UI", points);
        using GitUI.CommandsDialogs.FullBleedTabControl tabs = new()
        {
            Dock = DockStyle.Fill,
            Font = font,
            Padding = new Point(8, 6),
        };
        tabs.TabPages.Add(new TabPage("i"));
        tabs.TabPages.Add(new TabPage("Diff"));
        tabs.TabPages.Add(new TabPage("A long tab caption"));
        tabs.TabPages.Add(new TabPage(string.Empty));
        using Form window = new() { ClientSize = new Size(600, 240), AutoScaleMode = AutoScaleMode.None, ShowInTaskbar = false };
        window.Controls.Add(tabs);
        window.Show();
        Application.DoEvents();
        nint context = GetDC(tabs.Handle);
        nint previous = SelectObject(context, font.ToHfont());
        try
        {
            GetTextMetrics(context, out TextMetric metrics).Should().BeTrue();
            int minimum = (6 * metrics.AverageCharacterWidth) + (2 * tabs.Padding.X);
            tabs.GetTabRect(0).Width.Should().Be(minimum);
            tabs.GetTabRect(3).Width.Should().Be(minimum);
            tabs.GetTabRect(3).Height.Should().Be(tabs.GetTabRect(0).Height);
            tabs.GetTabRect(1).Width.Should().Be(Math.Max(minimum,
                TextRenderer.MeasureText("Diff", font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width + (2 * tabs.Padding.X)));
            TestContext.Progress.WriteLine($"points={points} average={metrics.AverageCharacterWidth} minimum={minimum} allocations={tabs.GetTabRect(0)},{tabs.GetTabRect(1)}");
        }
        finally
        {
            nint current = SelectObject(context, previous);
            DeleteObject(current);
            ReleaseDC(tabs.Handle, context);
        }
    }

    [Test]
    [TestCase(9, false)]
    [TestCase(9, true)]
    [TestCase(11, false)]
    [TestCase(11, true)]
    public void GetTabRect_should_retain_allocations_when_the_selected_header_changes(int points, bool images)
    {
        using Font font = new("Segoe UI", points);
        using ImageList imageList = new() { ImageSize = new Size(16, 16) };
        using Bitmap image = new(16, 16);
        using (Graphics graphics = Graphics.FromImage(image))
        {
            graphics.Clear(Color.Magenta);
        }

        imageList.Images.Add(image);
        using GitUI.CommandsDialogs.FullBleedTabControl tabs = new()
        {
            Dock = DockStyle.Fill,
            Font = font,
            Padding = new Point(8, 6),
            ImageList = images ? imageList : null,
        };
        foreach (string caption in new[] { "Commit", "Diff", "File tree", "GPG", "Output", "Eine übersetzte Registerkarte", "A&&B_C" })
        {
            tabs.TabPages.Add(new TabPage(caption) { ImageIndex = images ? 0 : -1 });
        }

        using Form window = new() { ClientSize = new Size(1000, 240), AutoScaleMode = AutoScaleMode.None, ShowInTaskbar = false };
        window.Controls.Add(tabs);
        window.Show();
        tabs.DeviceDpi.Should().Be(96);
        Rectangle[] allocations = Enumerable.Range(0, tabs.TabCount).Select(tabs.GetTabRect).ToArray();
        for (int selected = 0; selected < tabs.TabCount; selected++)
        {
            tabs.SelectedIndex = selected;
            Application.DoEvents();
            Enumerable.Range(0, tabs.TabCount).Select(tabs.GetTabRect).Should().Equal(allocations);
        }

        foreach (int index in Enumerable.Range(0, tabs.TabCount))
        {
            string caption = tabs.TabPages[index].Text;
            Size padded = TextRenderer.MeasureText(caption, font);
            Size unpadded = TextRenderer.MeasureText(caption, font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            if (images)
            {
                allocations[index].Width.Should().Be(unpadded.Width + (tabs.Padding.X * 3) + imageList.ImageSize.Width);
            }

            TestContext.Progress.WriteLine($"points={points} images={images} caption={caption} allocation={allocations[index]} padded={padded} unpadded={unpadded} display={tabs.DisplayRectangle}");
        }

        tabs.SelectedIndex = 0;
        using Bitmap bitmap = new(tabs.Width, tabs.Height);
        tabs.DrawToBitmap(bitmap, tabs.ClientRectangle);
        if (images)
        {
            foreach (int index in Enumerable.Range(0, tabs.TabCount))
            {
                Rectangle item = allocations[index];
                Point[] pixels = Enumerable.Range(item.Top, item.Height)
                    .SelectMany(y => Enumerable.Range(item.Left, item.Width).Where(x => bitmap.GetPixel(x, y).ToArgb() == Color.Magenta.ToArgb()).Select(x => new Point(x, y)))
                    .ToArray();
                pixels.Should().NotBeEmpty();
                Rectangle ink = Rectangle.FromLTRB(pixels.Min(point => point.X), pixels.Min(point => point.Y), pixels.Max(point => point.X) + 1, pixels.Max(point => point.Y) + 1);
                ink.X.Should().Be(item.X + tabs.Padding.X);
                ink.Size.Should().Be(imageList.ImageSize);
                TestContext.Progress.WriteLine($"image index={index} item={item} ink={ink}");
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint context);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint context, nint value);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTextMetrics(nint context, out TextMetric metric);

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
}
