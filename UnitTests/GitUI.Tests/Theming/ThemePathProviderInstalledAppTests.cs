using FluentAssertions;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUITests.Theming
{
    [TestFixture]
    public class ThemePathProviderInstalledAppTests
    {
        private string _originalAppExecutablePath;
        private Lazy<string> _originalAppDataPath;

        private const string MockAppInstallPath = "c:\\GitExtensions";
        private const string MockAppDataPath = "c:\\user\\username\\appdata";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var testAccessor = AppSettings.GetTestAccessor();
            _originalAppExecutablePath = testAccessor.ApplicationExecutablePath;
            testAccessor.ApplicationExecutablePath = Path.Combine(MockAppInstallPath, "gitextensions.exe");

            _originalAppDataPath = testAccessor.ApplicationDataPath;
            testAccessor.ApplicationDataPath = new Lazy<string>(() => MockAppDataPath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var testAccessor = AppSettings.GetTestAccessor();
            testAccessor.ApplicationExecutablePath = _originalAppExecutablePath;
            testAccessor.ApplicationDataPath = _originalAppDataPath;
        }

        [Test]
        public void AppThemesDirectory_should_be_inside_app_install()
        {
            new ThemePathProvider().AppThemesDirectory
                .Should().Be(Path.Combine(MockAppInstallPath, "Themes"));
        }

        [Test]
        public void UserThemesDirectory_should_be_inside_AppData()
        {
            new ThemePathProvider().UserThemesDirectory
                .Should().Be(Path.Combine(MockAppDataPath, "Themes"));
        }

        [Test]
        public void GetThemePath_should_return_path_inside_app_install_When_theme_is_builtin()
        {
            new ThemePathProvider().GetThemePath(new ThemeId("arbitrary_name", isBuiltin: true))
                .Should().Be(Path.Combine(MockAppInstallPath, "Themes", "arbitrary_name.css"));
        }

        [Test]
        public void GetThemePath_should_return_path_inside_app_data_When_theme_is_not_builtin()
        {
            new ThemePathProvider().GetThemePath(new ThemeId("arbitrary_name", isBuiltin: false))
                .Should().Be(Path.Combine(MockAppDataPath, "Themes", "arbitrary_name.css"));
        }
    }
}
