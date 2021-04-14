using System;
using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Theming;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs.SettingsDialog.Pages
{
    [TestFixture]
    public class ColorsPageSettingsPageControllerTests
    {
        [SetUp]
        public void Setup()
        {
            AppSettings.UseSystemVisualStyle = true; // to prevent installing win32 theming hooks
        }

        [Test]
        public void When_current_theme_is_default_choosing_visual_style_or_theme_variation_should_be_disabled()
        {
            var page = new MockColorsSettingsPage();
            var themeRepository = Substitute.For<IThemeRepository>();
            var themePathProvider = Substitute.For<IThemePathProvider>();
            AppSettings.ThemeId = ThemeId.Default;
            ThemeModule.TestAccessor.ReloadThemeSettings(themeRepository);

            var controller = new ColorsSettingsPageController(page, themeRepository, themePathProvider);
            controller.ShowThemeSettings();

            page.IsChoosingThemeVariationsEnabled.Should().BeFalse();
            page.IsChoosingVisualStyleEnabled.Should().BeFalse();
        }

        [Test]
        public void When_current_theme_is_non_default_choosing_visual_style_or_theme_variation_should_be_enabled()
        {
            var page = new MockColorsSettingsPage();
            var themeRepository = Substitute.For<IThemeRepository>();
            var themePathProvider = Substitute.For<IThemePathProvider>();

            var changedThemeId = new ThemeId("non_default", isBuiltin: true);
            var theme = new Theme(new Dictionary<AppColor, Color>(), new Dictionary<KnownColor, Color>(), changedThemeId);
            themeRepository
                .GetTheme(Arg.Is(changedThemeId), Arg.Any<IReadOnlyList<string>>())
                .Returns(theme);
            AppSettings.ThemeId = changedThemeId;
            ThemeModule.TestAccessor.ReloadThemeSettings(themeRepository);

            var controller = new ColorsSettingsPageController(page, themeRepository, themePathProvider);
            controller.ShowThemeSettings();

            page.IsChoosingThemeVariationsEnabled.Should().BeTrue();
            page.IsChoosingVisualStyleEnabled.Should().BeTrue();
        }

        [Test]
        public void SettingsAreModified_should_return_false_initially()
        {
            var page = new MockColorsSettingsPage();
            var themeRepository = Substitute.For<IThemeRepository>();
            var themePathProvider = Substitute.For<IThemePathProvider>();
            AppSettings.ThemeId = ThemeId.Default;
            ThemeModule.TestAccessor.ReloadThemeSettings(themeRepository);

            var controller = new ColorsSettingsPageController(page, themeRepository, themePathProvider);
            controller.ShowThemeSettings();
            controller.SettingsAreModified.Should().BeFalse();
        }

        [Test]
        public void SettingsAreModified_should_reflect_ThemeId_change()
        {
            var page = new MockColorsSettingsPage();
            var themeRepository = Substitute.For<IThemeRepository>();
            var themePathProvider = Substitute.For<IThemePathProvider>();
            AppSettings.ThemeId = ThemeId.Default;
            ThemeModule.TestAccessor.ReloadThemeSettings(themeRepository);

            var controller = new ColorsSettingsPageController(page, themeRepository, themePathProvider);
            controller.ShowThemeSettings();
            page.SelectedThemeId = new ThemeId("non_default", isBuiltin: false);
            controller.SettingsAreModified.Should().BeTrue();
        }

        [Test]
        public void When_current_theme_is_default_SettingsAreModified_should_ignore_UseSystemVisualStyle_change()
        {
            var page = new MockColorsSettingsPage();
            var themeRepository = Substitute.For<IThemeRepository>();
            var themePathProvider = Substitute.For<IThemePathProvider>();
            AppSettings.ThemeId = ThemeId.Default;
            ThemeModule.TestAccessor.ReloadThemeSettings(themeRepository);

            var controller = new ColorsSettingsPageController(page, themeRepository, themePathProvider);
            controller.ShowThemeSettings();
            page.UseSystemVisualStyle = !page.UseSystemVisualStyle;
            controller.SettingsAreModified.Should().BeFalse();
        }

        [Test]
        public void When_current_theme_is_default_SettingsAreModified_should_ignore_ThemeVariations_change()
        {
            var page = new MockColorsSettingsPage();
            var themeRepository = Substitute.For<IThemeRepository>();
            var themePathProvider = Substitute.For<IThemePathProvider>();
            AppSettings.ThemeId = ThemeId.Default;
            AppSettings.ThemeVariations = ThemeVariations.None;
            ThemeModule.TestAccessor.ReloadThemeSettings(themeRepository);

            var controller = new ColorsSettingsPageController(page, themeRepository, themePathProvider);
            controller.ShowThemeSettings();
            page.SelectedThemeVariations = new[] { ThemeVariations.Colorblind };
            controller.SettingsAreModified.Should().BeFalse();
        }

        [Test]
        public void When_current_theme_is_non_default_SettingsAreModified_should_reflect_UseSystemVisualStyle_change()
        {
            var page = new MockColorsSettingsPage();
            var themeRepository = Substitute.For<IThemeRepository>();
            var themePathProvider = Substitute.For<IThemePathProvider>();

            var changedThemeId = new ThemeId("non_default", isBuiltin: true);
            var theme = new Theme(new Dictionary<AppColor, Color>(), new Dictionary<KnownColor, Color>(), changedThemeId);
            themeRepository
                .GetTheme(Arg.Is(changedThemeId), Arg.Any<IReadOnlyList<string>>())
                .Returns(theme);
            AppSettings.ThemeId = changedThemeId;
            ThemeModule.TestAccessor.ReloadThemeSettings(themeRepository);

            var controller = new ColorsSettingsPageController(page, themeRepository, themePathProvider);

            controller.ShowThemeSettings();
            controller.SettingsAreModified.Should().BeFalse();

            page.UseSystemVisualStyle = !page.UseSystemVisualStyle;
            controller.SettingsAreModified.Should().BeTrue();
        }

        [Test]
        public void When_current_theme_is_non_default_SettingsAreModified_should_reflect_ThemeVariations_change()
        {
            var page = new MockColorsSettingsPage();
            var themeRepository = Substitute.For<IThemeRepository>();
            var themePathProvider = Substitute.For<IThemePathProvider>();

            var changedThemeId = new ThemeId("non_default", isBuiltin: true);
            var theme = new Theme(new Dictionary<AppColor, Color>(), new Dictionary<KnownColor, Color>(), changedThemeId);
            themeRepository
                .GetTheme(Arg.Is(changedThemeId), Arg.Any<IReadOnlyList<string>>())
                .Returns(theme);
            AppSettings.ThemeId = changedThemeId;
            AppSettings.ThemeVariations = ThemeVariations.None;
            ThemeModule.TestAccessor.ReloadThemeSettings(themeRepository);

            var controller = new ColorsSettingsPageController(page, themeRepository, themePathProvider);

            controller.ShowThemeSettings();
            controller.SettingsAreModified.Should().BeFalse();

            page.SelectedThemeVariations = new[] { ThemeVariations.Colorblind };
            controller.SettingsAreModified.Should().BeTrue();
        }

        private class MockColorsSettingsPage : IColorsSettingsPage
        {
            public ThemeId SelectedThemeId { get; set; }
            public string[] SelectedThemeVariations { get; set; }
            public bool UseSystemVisualStyle { get; set; }
            public bool LabelRestartIsNeededVisible { get; set; }
            public bool IsChoosingThemeVariationsEnabled { get; set; }
            public bool IsChoosingVisualStyleEnabled { get; set; }

            public void ShowThemeLoadingErrorMessage(ThemeId themeId, string[] variations, Exception ex) =>
                throw new NotImplementedException();

            public void PopulateThemeMenu(IEnumerable<ThemeId> themeIds)
            {
            }
        }
    }
}
