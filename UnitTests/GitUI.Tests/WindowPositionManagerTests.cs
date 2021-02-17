using System.Drawing;
using FluentAssertions;
using GitUI;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public class WindowPositionManagerTests
    {
        [Test]
        public void IsDisplayedOn10Percent_should_return_false_if_no_screen_supplied()
        {
            WindowPositionManager.TestAccessor.IsDisplayedOn10Percent(Rectangle.Empty, new Rectangle(1, 1, 1, 1))
                .Should().BeFalse();
        }

        [Test]
        public void IsDisplayedOn10Percent_should_return_false_if_no_window_supplied()
        {
            WindowPositionManager.TestAccessor.IsDisplayedOn10Percent(new Rectangle(1, 1, 1, 1), Rectangle.Empty)
                .Should().BeFalse();
        }

        [TestCase(500, 500)] // normal window
        [TestCase(50, 50)] // less than 10% visibility
        public void IsDisplayedOn10Percent_should_return_true_if_window_fully_fit_on_screen(int width, int height)
        {
            Rectangle screen = new(0, 0, 1920, 1080);
            Rectangle window = new(0, 0, width, height);

            WindowPositionManager.TestAccessor.IsDisplayedOn10Percent(screen, window)
                .Should().BeTrue();
        }

        //// clipped on the left
        [TestCase(-500, 100, 500, 500, false)] // less than 10% visible horizontally
        [TestCase(-192, 100, 500, 500, true)] // 10% visible horizontally
        //// clipped on the top
        [TestCase(50, -50, 500, 500, false)]
        //// clipped on the right
        [TestCase(1727, 100, 500, 500, true)] // 10% visible horizontally, (1920 * 90% - 1)
        [TestCase(1728, 100, 500, 500, false)] // less than 10% visible horizontally, (1920 * 90%)
        //// clipped at the bottom
        [TestCase(100, 971, 500, 500, true)] // 10% visible vertically, (1080 * 90% - 1)
        [TestCase(100, 972, 500, 500, false)] // 10% visible vertically, (1080 * 90%)
        //// clipped on both the left and right
        [TestCase(-500, 100, 2100, 500, true)] // more than 10% visible horizontally
        [TestCase(-2700, 100, 5000, 500, false)] // unexpected case! neither left-middle-rights points are visible
        public void IsDisplayedOn10Percent_should_return_expected_if_window_partially_fit_on_screen(int x, int y, int width, int height, bool expected)
        {
            Rectangle screen = new(0, 0, 1920, 1080);
            Rectangle window = new(x, y, width, height);

            WindowPositionManager.TestAccessor.IsDisplayedOn10Percent(screen, window)
                .Should().Be(expected);
        }
    }
}
