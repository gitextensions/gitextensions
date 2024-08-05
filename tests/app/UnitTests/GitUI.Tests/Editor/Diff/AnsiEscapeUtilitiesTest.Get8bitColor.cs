using FluentAssertions;
using GitUI.Editor.Diff;

namespace GitUITests.Editor.Diff;

[TestFixture]
public class AnsiEscapeUtilitiesTest_Get8bitColor
{
    private const int _blackId = 0;
    private const int _redId = 1;
    private readonly List<Color> _redAnsiTheme = [Color.FromArgb(212, 44, 58), Color.FromArgb(233, 149, 156), Color.FromArgb(255, 118, 118), Color.FromArgb(254, 186, 186)];

    [Test]
    public void Get8bitColor_ShouldReturnNamedColors()
    {
        List<int> offsets = [0, AnsiEscapeUtilities.TestAccessor.GetBoldOffset()];
        for (int colorId = 0; colorId < 8; ++colorId)
        {
            foreach (int offset in offsets)
            {
                foreach (bool dim in new List<bool>() { false, true })
                {
                    // Only check the result for red, the other should just get a value and set the id
                    // (No need to maintain all ANSI theme colors in parallel)
                    Color result = AnsiEscapeUtilities.TestAccessor.Get8bitColor(colorId + offset, dim, out int seenColorId);
                    colorId.Should().Be(seenColorId);
                    if (colorId != _redId)
                    {
                        continue;
                    }

                    int themeOffset = (dim ? 1 : 0) + (offset == 0 ? 0 : 2);
                    result.Should().Be(_redAnsiTheme[themeOffset]);
                }
            }
        }
    }

    [Test]
    public void Get8bitColor_ShouldReturnCorrect6bitColor()
    {
        // currentColorId is always reset, no named color
        const int currentColorId = _blackId;

        for (int i = 16; i < 232; ++i)
        {
            Color result = AnsiEscapeUtilities.TestAccessor.Get8bitColor(i, dim: false, out int colorId);
            result.Should().Be(GetExpectedColor(i));
            colorId.Should().Be(currentColorId);

            // sample colors
            if (i == 196)
            {
                result.Should().Be(Color.FromArgb(255, 0, 0));
            }
            else if (i == 231)
            {
                result.Should().Be(Color.FromArgb(255, 255, 255));
            }
        }

        return;

        static Color GetExpectedColor(int level)
        {
            int i = level - 16;
            int blue = Get8bitFrom6over3bit(i % 6);
            int green = Get8bitFrom6over3bit((i % 36) / 6);
            int red = Get8bitFrom6over3bit(i / 36);

            return Color.FromArgb(red, green, blue);

            static int Get8bitFrom6over3bit(int color)
                => color * 51;
        }
    }

    [Test]
    public void Get8bitColor_ShouldReturnCorrect4bitGrey()
    {
        // currentColorId is always rest, no named color
        const int currentColorId = _blackId;

        for (int i = 232; i <= 255; ++i)
        {
            Color result = AnsiEscapeUtilities.TestAccessor.Get8bitColor(i, dim: false, out int colorId);
            result.Should().Be(Get24StepGray(i));
            colorId.Should().Be(currentColorId);

            // border
            if (i == 232)
            {
                result.Should().Be(Color.FromArgb(0, 0, 0));
            }
            else if (i == 255)
            {
                result.Should().Be(Color.FromArgb(253, 253, 253));
            }
        }

        return;

        static Color Get24StepGray(int level)
        {
            int i = (level - 232) * 11;
            return Color.FromArgb(i, i, i);
        }
    }

    [Test]
    public void Get8bitColor_ShouldThrowException_WhenColorCodeIsOutOfRange()
    {
        int colorCode = 256;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            AnsiEscapeUtilities.TestAccessor.Get8bitColor(colorCode, dim: false, out int colorId);
        });
    }
}
