using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private ColorSettingsPageTestContext _context = null!;

        [SetUp]
        public void Setup()
        {
            ThemeModule.TestAccessor.SuppressWin32Hooks = true;

            MockColorsSettingsPage page = new();
            var themeRepository = Substitute.For<IThemeRepository>();
            var themePathProvider = Substitute.For<IThemePathProvider>();
            themeRepository
                .GetTheme(Arg.Any<ThemeId>(), Arg.Any<IReadOnlyList<string>>())
                .Returns(callInfo => new Theme(new Dictionary<AppColor, Color>(), new Dictionary<KnownColor, Color>(), callInfo.Arg<ThemeId>()));
            ColorsSettingsPageController controller = new(page, themeRepository, themePathProvider);

            _context = new ColorSettingsPageTestContext(page, controller, themeRepository);
        }

        [Test]
        public void When_current_theme_is_default_choosing_visual_style_or_theme_variation_should_be_disabled()
        {
            AppSettings.ThemeId = ThemeId.Default;
            ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);

            _context.Controller.ShowThemeSettings();

            _context.Page.IsChoosingThemeVariationsEnabled.Should().BeFalse();
            _context.Page.IsChoosingVisualStyleEnabled.Should().BeFalse();
        }

        [Test]
        public void When_current_theme_is_non_default_choosing_visual_style_or_theme_variation_should_be_enabled()
        {
            AppSettings.ThemeId = new ThemeId("non_default", isBuiltin: true);
            ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);
            _context.Controller.ShowThemeSettings();

            _context.Page.IsChoosingThemeVariationsEnabled.Should().BeTrue();
            _context.Page.IsChoosingVisualStyleEnabled.Should().BeTrue();
        }

        [Test]
        public void When_user_switches_to_default_theme_UseSystemVisualStyle_should_be_checked()
        {
            AppSettings.ThemeId = new ThemeId("non_default", isBuiltin: true);
            AppSettings.UseSystemVisualStyle = false;
            ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);
            _context.Controller.ShowThemeSettings();
            _context.Page.UseSystemVisualStyle.Should().BeFalse();

            _context.Page.SelectedThemeId = ThemeId.Default;
            _context.Controller.HandleSelectedThemeChanged();
            _context.Page.UseSystemVisualStyle.Should().BeTrue();
        }

        [Test]
        public void When_user_switches_to_non_default_theme_UseSystemVisualStyle_should_be_unchecked()
        {
            AppSettings.ThemeId = ThemeId.Default;
            AppSettings.UseSystemVisualStyle = true;
            ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);
            _context.Controller.ShowThemeSettings();
            _context.Page.UseSystemVisualStyle.Should().BeTrue();

            _context.Page.SelectedThemeId = new ThemeId("non_default", isBuiltin: true);
            _context.Controller.HandleSelectedThemeChanged();
            _context.Page.UseSystemVisualStyle.Should().BeFalse();
        }

        [Test]
        public void When_user_switches_to_default_theme_UseColorblindVariation_should_be_unchecked()
        {
            AppSettings.ThemeId = new ThemeId("non_default", isBuiltin: true);
            AppSettings.ThemeVariations = new[] { ThemeVariations.Colorblind };
            ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);
            _context.Controller.ShowThemeSettings();
            _context.Page.SelectedThemeVariations.Should().BeEquivalentTo(ThemeVariations.Colorblind);

            _context.Page.SelectedThemeId = ThemeId.Default;
            _context.Controller.HandleSelectedThemeChanged();
            _context.Page.SelectedThemeVariations.Should().BeEmpty();
        }

        [TestCaseSource(nameof(CasesThemeSettings))]
        public void SettingsAreModified_should_reflect_ThemeId_change(
            ThemeId themeId, string[] themeVariations, bool useSystemVisualStyle)
        {
            AppSettings.ThemeId = themeId;
            AppSettings.ThemeVariations = themeVariations;
            AppSettings.UseSystemVisualStyle = useSystemVisualStyle;
            ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);
            _context.Controller.ShowThemeSettings();

            _context.Page.SelectedThemeId = new ThemeId("another_theme", isBuiltin: false);
            _context.Controller.SettingsAreModified.Should().BeTrue();
        }

        [TestCaseSource(nameof(CasesThemeSettings))]
        public void SettingsAreModified_should_reflect_UseSystemVisualStyle_change(
            ThemeId themeId, string[] themeVariations, bool useSystemVisualStyle)
        {
            AppSettings.ThemeId = themeId;
            AppSettings.ThemeVariations = themeVariations;
            AppSettings.UseSystemVisualStyle = useSystemVisualStyle;
            ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);

            _context.Controller.ShowThemeSettings();
            _context.Controller.SettingsAreModified.Should().BeFalse();

            _context.Page.UseSystemVisualStyle = !_context.Page.UseSystemVisualStyle;
            _context.Controller.SettingsAreModified.Should().BeTrue();
        }

        [TestCaseSource(nameof(CasesThemeSettings))]
        public void SettingsAreModified_should_reflect_ThemeVariations_change(
            ThemeId themeId, string[] themeVariations, bool useSystemVisualStyle)
        {
            AppSettings.ThemeId = themeId;
            AppSettings.ThemeVariations = themeVariations;
            AppSettings.UseSystemVisualStyle = useSystemVisualStyle;
            ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);

            _context.Controller.ShowThemeSettings();
            _context.Controller.SettingsAreModified.Should().BeFalse();

            _context.Page.SelectedThemeVariations = themeVariations.SequenceEqual(ThemeVariations.None)
                ? new[] { ThemeVariations.Colorblind }
                : ThemeVariations.None;
            _context.Controller.SettingsAreModified.Should().BeTrue();
        }

        private static IEnumerable<object[]> CasesThemeSettings()
        {
            yield return new object[]
            {
                ThemeId.Default,
                ThemeVariations.None,
                true // useSystemVisualStyle
            };

            yield return new object[]
            {
                new ThemeId("non_default", isBuiltin: true),
                new[] { ThemeVariations.Colorblind },
                false // useSystemVisualStyle
            };
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

        private class ColorSettingsPageTestContext
        {
            public ColorSettingsPageTestContext(
                IColorsSettingsPage page,
                ColorsSettingsPageController controller,
                IThemeRepository themeRepository)
            {
                Page = page;
                Controller = controller;
                ThemeRepository = themeRepository;
            }

            public IColorsSettingsPage Page { get; }

            public ColorsSettingsPageController Controller { get; }

            public IThemeRepository ThemeRepository { get; }
        }
    }
}
