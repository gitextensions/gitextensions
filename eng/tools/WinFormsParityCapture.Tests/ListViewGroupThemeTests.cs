using System.Drawing;
using System.Windows.Forms.VisualStyles;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ListViewGroupThemeTests
{
    [Test]
    [TestCase("Explorer::TEXTSTYLE", 1, 0x003399)]
    [TestCase("Explorer::TEXTSTYLE", 6, 0x0066CC)]
    [TestCase("DarkMode_Explorer::TEXTSTYLE", 1, 0x003399)]
    [TestCase("DarkMode_Explorer::TEXTSTYLE", 6, 0x0066CC)]
    public void Repository_group_text_should_use_native_text_styles_not_system_HotTrack(string className, int part, int rgb)
    {
        // NativeListView selects Explorer at handle creation. Its group caption and task
        // use TEXT_MAININSTRUCTION and TEXT_HYPERLINKTEXT, not CSS/SystemColors.HotTrack.
        VisualStyleElement element = VisualStyleElement.CreateElement(className, part, 1);
        VisualStyleRenderer.IsElementDefined(element).Should().BeTrue();
        VisualStyleRenderer renderer = new(element);
        Color color = renderer.GetColor(ColorProperty.TextColor);
        renderer.LastHResult.Should().Be(0);
        color.ToArgb().Should().Be(unchecked((int)0xFF000000) | rgb);
        TestContext.Progress.WriteLine($"class={className} part={part} ARGB=#{color.ToArgb():X8}");
    }
}
