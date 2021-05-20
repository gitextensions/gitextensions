using System.IO;
using FluentAssertions;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Theming
{
    [TestFixture]
    public class CssUrlResolverTests
    {
        private const string PreinstalledThemesMockPath = "pre-installed\\themes";
        private const string UserDefinedThemesMockPath = "app-data\\themes";

        [Test]
        public void Should_resolve_to_preinstalled_themes_directory_by_default()
        {
            ThemeCssUrlResolver resolver = new(CreateMockThemePathProvider());
            var resolvedPath = resolver.ResolveCssUrl("dark.css");
            resolvedPath.Should().Be(Path.Combine(PreinstalledThemesMockPath, "dark.css"));
        }

        [Test]
        public void Should_resolve_to_user_defined_themes_directory_When_url_starts_with_macro()
        {
            ThemeCssUrlResolver resolver = new(CreateMockThemePathProvider());
            var resolvedPath = resolver.ResolveCssUrl("{UserAppData}/dark.css");
            resolvedPath.Should().Be(Path.Combine(UserDefinedThemesMockPath, "dark.css"));
        }

        private static IThemePathProvider CreateMockThemePathProvider()
        {
            var pathProvider = Substitute.For<IThemePathProvider>();

            pathProvider.ThemeExtension.Returns(".css");

            pathProvider.GetThemePath(Arg.Any<ThemeId>()).Returns(callInfo =>
                Path.Combine(
                    callInfo.Arg<ThemeId>().IsBuiltin
                        ? PreinstalledThemesMockPath
                        : UserDefinedThemesMockPath,
                    callInfo.Arg<ThemeId>().Name + ".css"));

            return pathProvider;
        }
    }
}
