using FluentAssertions;
using GitExtUtils.GitUI.Theming;

namespace GitUITests.Theming
{
    [TestFixture]
    public class WhatSystemColorsAreThemeableTests
    {
        [Test]
        public void Themable_system_colors_should_contain_these_colors(
            [Values(
                KnownColor.ActiveBorder, // begins first range of system colors
                KnownColor.WindowText, // ends first range of system colors
                KnownColor.GradientActiveCaption, // first non-duplicate system color from second range of system colors
                KnownColor.MenuHighlight)] // ends second range of system colors
            KnownColor value)
        {
            Theme.TestAccessor.SysColorNames.Should().Contain(value);
        }

        [Test]
        public void Themable_system_colors_should_not_contain_these_colors(
            [Values(
                KnownColor.Transparent,
                KnownColor.AliceBlue, // begins the range of web colors
                KnownColor.YellowGreen, // ends the range of web colors
                KnownColor.ButtonFace, // duplicates KnownColor.Control
                KnownColor.ButtonShadow, // duplicates KnownColor.ControlDark
                KnownColor.ButtonHighlight)] // duplicates KnownColor.ControlLight
            KnownColor value)
        {
            Theme.TestAccessor.SysColorNames.Should().NotContain(value);
        }
    }
}
