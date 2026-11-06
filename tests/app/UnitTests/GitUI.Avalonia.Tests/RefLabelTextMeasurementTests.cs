using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using GitUI.Compat;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RefLabelTextMeasurementTests
{
    // Native96 original TextRenderer.NoPadding measurements from the real renderer
    // consumer contract, not layout constants or an off-Windows font-raster claim.
    [AvaloniaTest]
    [TestCase("Segoe UI", 9, FontStyle.Normal, false, 27, 67, 15)]
    [TestCase("Segoe UI", 9, FontStyle.Normal, true, 27, 74, 15)]
    [TestCase("Segoe UI", 11, FontStyle.Normal, false, 33, 85, 20)]
    [TestCase("Segoe UI", 11, FontStyle.Normal, true, 35, 92, 20)]
    [TestCase("Arial", 9, FontStyle.Italic, false, 28, 68, 15)]
    [TestCase("Consolas", 9, FontStyle.Normal, false, 28, 91, 14)]
    public void NoPadding_route_should_match_actual_native_font_measurements(
        string family, int points, FontStyle style, bool bold, int mainWidth, int prefixWidth, int height)
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Ignore("Native GDI measurement equivalence is Windows-only; the portable prefix substitute is exercised separately.");
        }

        Label owner = new()
        {
            FontFamily = new FontFamily(family),
            FontSize = points * 96d / 72,
            FontStyle = style,
            FontWeight = bold ? FontWeight.Bold : FontWeight.Normal,
        };
        WinFormsTextMeasurer.MeasureTextRendererNoPadding(owner, "main").Should().Be(new Size(mainWidth, height));
        WinFormsTextMeasurer.MeasureTextRendererNoPadding(owner, "feature&parity").Should().Be(new Size(prefixWidth, height));
        WinFormsTextMeasurer.MeasureTextRendererNoPadding(owner, "&").Should().Be(new Size(0, height));
    }

    [AvaloniaTest]
    [TestCase("a&b", "ab")]
    [TestCase("a&&b", "a&b")]
    [TestCase("main&", "main")]
    [TestCase("feature&parity&", "featureparity")]
    public void Measurement_prefix_parsing_should_not_change_literal_paint_or_other_measurement_consumers(string value, string measured)
    {
        Label owner = new() { Content = value, FontFamily = new FontFamily("Segoe UI"), FontSize = 12 };
        Size parsed = WinFormsTextMeasurer.MeasureTextRendererNoPadding(owner, value);
        Size literal = WinFormsTextMeasurer.MeasureSize(owner, measured);
        parsed.Width.Should().Be(literal.Width);
        parsed.Height.Should().BeGreaterThan(0);
        owner.Content.Should().Be(value);
        WinFormsTextMeasurer.MeasureSize(owner, value).Width.Should().BeGreaterThan(parsed.Width,
            "existing literal measurement consumers must keep their unchanged NoPrefix route");
    }

    [AvaloniaTest]
    public void Empty_text_should_remain_empty_while_a_prefix_only_value_retains_the_native_line_height()
    {
        Label owner = new() { FontFamily = new FontFamily("Segoe UI"), FontSize = 12 };
        WinFormsTextMeasurer.MeasureTextRendererNoPadding(owner, string.Empty).Should().Be(default(Size));
        Size prefixOnly = WinFormsTextMeasurer.MeasureTextRendererNoPadding(owner, "&");
        prefixOnly.Width.Should().Be(0);
        prefixOnly.Height.Should().Be(WinFormsTextMeasurer.MeasureTextRendererNoPadding(owner, " ").Height);
    }
}
