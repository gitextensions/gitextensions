using System.Drawing;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class MenuSizingTests
{
    private const int NativeItemBorderWidth = 2;

    [Test]
    [TestCase(9, "&Commands")]
    [TestCase(9, "&Plugins")]
    [TestCase(9, "Ausgewählte &Änderungen übernehmen")]
    [TestCase(9, "&A&&B_C")]
    [TestCase(9, "&First\nSecond")]
    [TestCase(11, "&Commands")]
    [TestCase(11, "&Plugins")]
    [TestCase(11, "Ausgewählte &Änderungen übernehmen")]
    [TestCase(11, "&A&&B_C")]
    [TestCase(11, "&First\nSecond")]
    public void GetPreferredSize_should_use_mnemonic_text_metrics_source_padding_and_internal_borders(int points, string caption)
    {
        using Font font = new("Segoe UI", points);
        using GitUI.MenuStripEx menu = new() { Font = font };
        using ToolStripMenuItem item = new(caption);
        using Form window = new()
        {
            ClientSize = new Size(640, 120),
            AutoScaleMode = AutoScaleMode.None,
            ShowInTaskbar = false,
        };
        menu.Items.Add(item);
        window.Controls.Add(menu);
        window.Show();
        menu.PerformLayout();
        menu.DeviceDpi.Should().Be(96);
        item.Font.Should().Be(font);
        item.Padding.Should().Be(new Padding(4, 0, 4, 0));
        Size text = TextRenderer.MeasureText(caption, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        Size preferred = item.GetPreferredSize(Size.Empty);
        preferred.Should().Be(text + new Size(NativeItemBorderWidth * 2, NativeItemBorderWidth * 2) + item.Padding.Size);
        item.GetPreferredSize(new Size(30, 20)).Width.Should().Be(preferred.Width);
        item.Width.Should().Be(preferred.Width);
        TestContext.Progress.WriteLine($"font={points} caption={caption.Replace('\n', '|')} text={text} padding={item.Padding} preferred={preferred} bounds={item.Bounds}");

        item.Padding = new Padding(7, 1, 11, 2);
        menu.PerformLayout();
        item.GetPreferredSize(Size.Empty).Should().Be(text + new Size(NativeItemBorderWidth * 2, NativeItemBorderWidth * 2) + item.Padding.Size);
        item.Width.Should().Be(item.GetPreferredSize(Size.Empty).Width);
    }

    [Test]
    [TestCase(9, false)]
    [TestCase(9, true)]
    [TestCase(11, false)]
    [TestCase(11, true)]
    public void GetPreferredSize_should_include_the_scaled_or_unscaled_source_image_slot(int points, bool unscaled)
    {
        using Font font = new("Segoe UI", points);
        using Bitmap image = new(24, 16);
        using GitUI.MenuStripEx menu = new() { Font = font };
        using ToolStripMenuItem item = new("&Tools", image)
        {
            ImageScaling = unscaled ? ToolStripItemImageScaling.None : ToolStripItemImageScaling.SizeToFit,
        };
        menu.Items.Add(item);
        using Form window = new() { ClientSize = new Size(640, 120), AutoScaleMode = AutoScaleMode.None, ShowInTaskbar = false };
        window.Controls.Add(menu);
        window.Show();
        menu.PerformLayout();
        Size text = TextRenderer.MeasureText(item.Text, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        int imageWidth = unscaled ? image.Width : menu.ImageScalingSize.Width;
        Size preferred = item.GetPreferredSize(Size.Empty);
        preferred.Width.Should().Be(text.Width + imageWidth + (NativeItemBorderWidth * 2) + item.Padding.Horizontal);
        menu.ImageScalingSize.Should().Be(new Size(16, 16));
        TestContext.Progress.WriteLine($"font={points} imageScaling={item.ImageScaling} imageWidth={imageWidth} text={text} preferred={preferred}");
    }

    [Test]
    [TestCase(9)]
    [TestCase(11)]
    public void AutoSize_false_should_preserve_authored_width_after_font_and_translation_changes(int points)
    {
        using Font font = new("Segoe UI", points);
        using GitUI.MenuStripEx menu = new() { Font = font };
        using ToolStripMenuItem item = new("&Tools") { AutoSize = false, Size = new Size(47, 20) };
        menu.Items.Add(item);
        using Form window = new() { ClientSize = new Size(640, 120), AutoScaleMode = AutoScaleMode.None, ShowInTaskbar = false };
        window.Controls.Add(menu);
        window.Show();
        menu.PerformLayout();
        item.Width.Should().Be(47);
        item.Text = "Eine erheblich längere übersetzte &Beschriftung";
        menu.PerformLayout();
        item.GetPreferredSize(Size.Empty).Width.Should().BeGreaterThan(item.Width);
        item.Width.Should().Be(47);
    }
}
