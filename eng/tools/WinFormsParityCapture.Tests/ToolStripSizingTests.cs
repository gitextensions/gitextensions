using System.Drawing;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class ToolStripSizingTests
{
    [Test]
    [TestCase("0↑↓")]
    [TestCase("3↑ 2↓")]
    [TestCase("123↑ 456↓")]
    public void ToolStrip_preferred_width_should_compose_padded_text_image_and_native_border(string text)
    {
        using Font font = new("Segoe UI", 9);
        using Bitmap image = new(16, 16);
        using ToolStripButton button = new(text, image) { Font = font };
        Size textSize = TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        button.GetPreferredSize(Size.Empty).Width.Should().Be(textSize.Width + image.Width + (2 * 2));
        if (text == "0↑↓")
        {
            textSize.Width.Should().Be(25);
            button.GetPreferredSize(Size.Empty).Width.Should().Be(45);
        }
    }
}
