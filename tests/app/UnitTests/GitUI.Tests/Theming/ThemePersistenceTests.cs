using GitUI.Theming;

namespace GitUITests.Theming
{
    [TestFixture]
    public class ThemePersistenceTests
    {
        private static IEnumerable<TestCaseData> ColorFormatCases()
        {
            yield return new TestCaseData(Color.FromArgb(red: 255, green: 0, blue: 0))
            {
                ExpectedResult = "#ff0000"
            };

            yield return new TestCaseData(Color.FromArgb(red: 0, green: 255, blue: 0))
            {
                ExpectedResult = "#00ff00"
            };

            yield return new TestCaseData(Color.FromArgb(red: 0, green: 0, blue: 255))
            {
                ExpectedResult = "#0000ff"
            };
        }

        [Test, TestCaseSource(nameof(ColorFormatCases))]
        public string Should_format_color_in_css_rgb_hex_format(Color color)
        {
            return ThemePersistence.TestAccessor.FormatColor(color);
        }
    }
}
