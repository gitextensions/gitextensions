using System.Windows.Forms;
using GitUI.Editor;

namespace GitUITests.Editor;

[Apartment(ApartmentState.STA)]
public class DifftasticWidthTests
{
    [Test]
    public void ComputeSideBySideWidth_should_return_max_when_width_exceeds_max()
    {
        DifftasticWidth.ComputeSideBySideWidth(1500, 7.2f).Should().Be(200);
    }

    [Test]
    public void ComputeSideBySideWidth_should_return_min_when_width_below_min()
    {
        DifftasticWidth.ComputeSideBySideWidth(500, 7.2f).Should().Be(88);
    }

    [Test]
    public void ComputeSideBySideWidth_should_return_even_when_computed_is_odd()
    {
        DifftasticWidth.ComputeSideBySideWidth(1499, 7.5f).Should().Be(198);
    }

    [Test]
    public void ComputeSideBySideWidth_should_fallback_to_min_when_char_width_is_zero()
    {
        DifftasticWidth.ComputeSideBySideWidth(1500, 0f).Should().Be(88);
    }

    [Test]
    public void ComputeSideBySideWidth_should_fallback_to_min_when_char_width_is_negative()
    {
        DifftasticWidth.ComputeSideBySideWidth(1500, -1f).Should().Be(88);
    }

    [Test]
    public void ComputeSideBySideWidth_should_return_computed_when_within_range()
    {
        DifftasticWidth.ComputeSideBySideWidth(1050, 7.0f).Should().Be(150);
    }

    [Test]
    public void MeasureCharWidth_should_return_zero_for_null_font()
    {
        DifftasticWidth.MeasureCharWidth(null).Should().Be(0f);
    }

    [Test]
    public void MeasureCharWidth_should_return_positive_cell_width_for_monospace_font()
    {
        using Font font = new("Courier New", 9f);

        float width = DifftasticWidth.MeasureCharWidth(font);

        width.Should().BeGreaterThan(0f).And.BeLessThan(20f);
    }
}
