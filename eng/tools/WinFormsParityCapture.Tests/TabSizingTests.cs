using System.Drawing;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class TabSizingTests
{
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
}
