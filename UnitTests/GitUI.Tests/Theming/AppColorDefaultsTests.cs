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

        [Test]
        public void Default_values_are_specified_in_invariant_theme()
        {
            var repository = new ThemeRepository(new ThemePersistence());
            var invariantTheme = repository.GetInvariantTheme();
            invariantTheme.Should().NotBeNull();
            foreach (AppColor name in Enum.GetValues(typeof(AppColor)))
            {
                Color value = invariantTheme.GetColor(name);
                value.Should().NotBe(Color.Empty);
            }
        }
    }
}
