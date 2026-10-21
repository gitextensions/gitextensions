using System.Drawing;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class LinkLabelSizingTests
{
    [Test]
    [TestCase(9)]
    [TestCase(14)]
    [TestCase(20)]
    public void Dashboard_link_should_size_from_native_text_plus_padding_and_inset_its_image(int sizeInPoints)
    {
        using Font font = new("Segoe UI", sizeInPoints);
        using Bitmap icon = new(16, 16);
        using MeasuredLinkLabel link = new()
        {
            Font = font,
            AutoSize = true,
            AutoEllipsis = true,

            // The application bootstrap disables compatible (GDI+) text rendering.
            UseCompatibleTextRendering = false,
            Padding = new Padding(24, 3, 3, 3),
            Image = icon,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleLeft,
            LinkBehavior = LinkBehavior.NeverUnderline,
            Text = "Clone repository",
        };
        Size text = TextRenderer.MeasureText(link.Text, font);
        link.PreferredSize.Should().Be(new Size(text.Width + 27, text.Height + 6));
        link.ImageBounds().X.Should().Be(2);
        link.ActiveLinkColor.ToArgb().Should().Be(Color.Red.ToArgb());
    }

    private sealed class MeasuredLinkLabel : LinkLabel
    {
        internal Rectangle ImageBounds() => CalcImageRenderBounds(Image!, ClientRectangle, ContentAlignment.MiddleLeft);
    }
}
