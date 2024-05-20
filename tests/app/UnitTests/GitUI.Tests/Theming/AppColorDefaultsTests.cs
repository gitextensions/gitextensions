using FluentAssertions;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUITests.Theming
{
    [TestFixture]
    public class AppColorDefaultsTests
    {
        private string _originalPath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            AppSettings.TestAccessor testAccessor = AppSettings.GetTestAccessor();
            _originalPath = testAccessor.ApplicationExecutablePath;
            testAccessor.ApplicationExecutablePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "gitextensions.exe");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.TestAccessor testAccessor = AppSettings.GetTestAccessor();
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
        public void Default_values_are_used_when_not_overridden_in_settings([Values]AppColor name)
        {
            // This only applies when migrating colors from AppSettings to a theme.
            // Fixes https://github.com/gitextensions/gitextensions/issues/11629

            MemorySettings emptySettings = new();

            Color defaultColor = AppColorDefaults.GetBy(name);
            Color settingsColor = emptySettings.GetColor(name.ToString(), defaultColor);

            settingsColor.Should().Be(defaultColor);
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

                Color defaultValue = AppColorDefaults.GetBy(name);
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
                Color defaultValue = AppColorDefaults.GetBy(name);
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
