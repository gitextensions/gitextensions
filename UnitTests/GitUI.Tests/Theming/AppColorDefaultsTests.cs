using System;
using System.Drawing;
using System.IO;
using FluentAssertions;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using NUnit.Framework;

namespace GitUITests.Theming
{
    [TestFixture]
    public class AppColorDefaultsTests
    {
        private string _originalPath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var testAccessor = AppSettings.GetTestAccessor();
            _originalPath = testAccessor.ApplicationExecutablePath;
            testAccessor.ApplicationExecutablePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "gitextensions.exe");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var testAccessor = AppSettings.GetTestAccessor();
            testAccessor.ApplicationExecutablePath = _originalPath;
        }

        [Test]
        public void Default_values_are_defined_in_AppColorDefaults()
        {
            foreach (AppColor name in Enum.GetValues(typeof(AppColor)))
            {
                Color value = AppColorDefaults.GetBy(name);
                value.Should().NotBe(AppColorDefaults.FallbackColor);
            }
        }

#if SUPPORT_THEMES
        [Test]
        public void Default_values_are_specified_in_invariant_theme()
        {
            Theme invariantTheme = GetInvariantTheme();
            invariantTheme.Should().NotBeNull();
            foreach (AppColor name in Enum.GetValues(typeof(AppColor)))
            {
                Color value = invariantTheme.GetColor(name);
                value.Should().NotBe(Color.Empty);

                var defaultValue = AppColorDefaults.GetBy(name);
                value.ToArgb().Should().Be(defaultValue.ToArgb());
            }
        }

        [Test]
        public void Invariant_theme_colors_match_AppColorDefaults()
        {
            Theme invariantTheme = GetInvariantTheme();
            foreach (AppColor name in Enum.GetValues(typeof(AppColor)))
            {
                Color value = invariantTheme.GetColor(name);
                var defaultValue = AppColorDefaults.GetBy(name);
                value.ToArgb().Should().Be(defaultValue.ToArgb());
            }
        }
#endif

        private static Theme GetInvariantTheme()
        {
            ThemePathProvider themePathProvider = new();
            ThemeLoader themeLoader = new(new ThemeCssUrlResolver(themePathProvider), new ThemeFileReader());
            ThemeRepository repository = new(new ThemePersistence(themeLoader), themePathProvider);
            return repository.GetInvariantTheme();
        }
    }
}
