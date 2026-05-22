using GitUI.Editor.Diff;

namespace GitUITests.Editor.Diff;
public class AnsiEscapeUtilitiesGet8bitColorTests : AnsiEscapeUtilitiesTestBase
{
    [Test]
    public void Get8bitColor_ShouldReturnBuiltinThemeColors()
    {
        for (int colorId = 0; colorId < 8; ++colorId)
        {
            foreach (bool fore in new List<bool>() { true, false })
            {
                foreach (bool bold in new List<bool>() { true, false })
                {
                    foreach (bool dim in new List<bool>() { false, true })
                    {
                        // Only check the result for red, the other should just get a value
                        // (No need to maintain all ANSI theme colors in parallel)
                        Color result = AnsiEscapeUtilities.TestAccessor.Get8bitColor(colorId, fore, bold, dim);
                        if (colorId != RedId)
                        {
                            continue;
                        }

                        result.Should().Be(GetAnsiColor(RedId, fore, bold, dim), $"Failed for fore:{fore}, bold:{bold}, dim:{dim}");
                    }
                }
            }
        }
    }

    [Test]
    public void Get8bitColor_ShouldReturnCorrect6bitColor()
    {
        for (int i = 16; i < 232; ++i)
        {
            Color result = AnsiEscapeUtilities.TestAccessor.Get8bitColor(i, fore: true, bold: false, dim: false);
            result.Should().Be(GetExpectedColor(i));

            // sample specific colors only
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
        for (int i = 232; i <= 255; ++i)
        {
            Color result = AnsiEscapeUtilities.TestAccessor.Get8bitColor(i, fore: true, bold: false, dim: false);
            result.Should().Be(Get24StepGray(i));

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

        ((Action)(() => AnsiEscapeUtilities.TestAccessor.Get8bitColor(colorCode, fore: true, bold: false, dim: false))).Should().Throw<ArgumentOutOfRangeException>();
    }
}
