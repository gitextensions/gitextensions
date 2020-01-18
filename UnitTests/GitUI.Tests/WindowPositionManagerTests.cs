using System;
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
        public void FindWindowScreen_should_return_empty_rect_if_no_screen_supplied()
        {
            WindowPositionManager.FindWindowScreen(new Point(10, 10), Array.Empty<Rectangle>()).Should().Be(Rectangle.Empty);
        }

        [Test]
        public void FindWindowScreen_should_not_crash_if_point_in_dead_middle_between_monitors()
        {
            // imagine 3 monitors 1920x1080 positioned side-by-side, the middle monitor is the main one (0,0)
            // the other two are positioned on the same level (Y=0)
            // Place the point at the dead center
            var point = new Point(960, 540);
            var screens = new[]
            {
                new Rectangle(-1920, 0, 1920, 1080),
                new Rectangle(1920, 0, 1920, 1080),
                new Rectangle(0, 0, 1920, 1080)
            };

            WindowPositionManager.FindWindowScreen(point, screens).Should().Be(null);
        }
    }
}