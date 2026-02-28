using FluentAssertions;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Theming;
using NSubstitute;

namespace GitUITests.CommandsDialogs.SettingsDialog.Pages;

[TestFixture]
public class ColorsPageSettingsPageControllerTests
{
    private ColorSettingsPageTestContext _context = null!;

    [SetUp]
    public void Setup()
    {
        MockColorsSettingsPage page = new();
        IThemeRepository themeRepository = Substitute.For<IThemeRepository>();
        IThemePathProvider themePathProvider = Substitute.For<IThemePathProvider>();
        themeRepository
            .GetTheme(Arg.Any<ThemeId>(), Arg.Any<IReadOnlyList<string>>())
            .Returns(callInfo => new Theme(new Dictionary<AppColor, Color>(), new Dictionary<KnownColor, Color>(), callInfo.Arg<ThemeId>()));
        ColorsSettingsPageController controller = new(page, themeRepository, themePathProvider);

        _context = new ColorSettingsPageTestContext(page, controller, themeRepository);
    }

    [Test]
    public void When_current_theme_is_default_choosing_visual_style_should_be_disabled()
    {
        AppSettings.ThemeId = ThemeId.DefaultLight;
        ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);

        _context.Controller.ShowThemeSettings();
        _context.Page.IsChoosingVisualStyleEnabled.Should().BeFalse();
    }

    [Test]
    public void When_current_theme_is_non_default_choosing_visual_style_should_be_enabled()
    {
        AppSettings.ThemeId = new ThemeId("non_default", isBuiltin: true);
        ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);

        _context.Controller.ShowThemeSettings();
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

        _context.Page.SelectedThemeId = ThemeId.DefaultLight;
        _context.Controller.HandleSelectedThemeChanged();
        _context.Page.UseSystemVisualStyle.Should().BeTrue();
    }

    [Test]
    public void When_user_switches_to_non_default_theme_UseSystemVisualStyle_should_be_unchecked()
    {
        AppSettings.ThemeId = ThemeId.DefaultLight;
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
        AppSettings.ThemeVariations = [ThemeVariations.Colorblind];
        ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);
        _context.Controller.ShowThemeSettings();
        _context.Page.SelectedThemeVariations.Should().BeEquivalentTo(ThemeVariations.Colorblind);

        _context.Page.SelectedThemeId = ThemeId.DefaultLight;
        _context.Controller.HandleSelectedThemeChanged();
        _context.Page.SelectedThemeVariations.Should().BeEmpty();
    }

    [TestCaseSource(nameof(CasesThemeSettings))]
    public void SettingsAreModified_should_reflect_ThemeId_change(
        ThemeId themeId, string[] themeVariations, bool? useSystemVisualStyle)
    {
        AppSettings.ThemeId = themeId;
        AppSettings.ThemeVariations = themeVariations;
        AppSettings.UseSystemVisualStyle = useSystemVisualStyle is true;
        ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);
        _context.Controller.ShowThemeSettings();

        _context.Page.SelectedThemeId = new ThemeId("another_theme", isBuiltin: false);
        _context.Controller.SettingsAreModified.Should().BeTrue();
    }

    [TestCaseSource(nameof(CasesThemeSettings))]
    public void SettingsAreModified_should_reflect_UseSystemVisualStyle_change(
        ThemeId themeId, string[] themeVariations, bool? useSystemVisualStyle)
    {
        AppSettings.ThemeId = themeId;
        AppSettings.ThemeVariations = themeVariations;
        AppSettings.UseSystemVisualStyle = useSystemVisualStyle is true;
        ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);

        _context.Controller.ShowThemeSettings();
        _context.Controller.SettingsAreModified.Should().BeFalse();

        _context.Page.UseSystemVisualStyle = !_context.Page.UseSystemVisualStyle;
        if (themeId == ThemeId.WindowsAppColorModeId || themeId == ThemeId.DefaultLight)
        {
            _context.Controller.SettingsAreModified.Should().BeFalse();
        }
        else if (useSystemVisualStyle is not null)
        {
            _context.Controller.SettingsAreModified.Should().BeTrue();
        }
    }

    [TestCaseSource(nameof(CasesThemeSettings))]
    public void SettingsAreModified_should_reflect_ThemeVariations_change(
        ThemeId themeId, string[] themeVariations, bool? useSystemVisualStyle)
    {
        AppSettings.ThemeId = themeId;
        AppSettings.ThemeVariations = themeVariations;
        AppSettings.UseSystemVisualStyle = useSystemVisualStyle is true;
        ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);

        _context.Controller.ShowThemeSettings();
        _context.Controller.SettingsAreModified.Should().BeFalse();

        _context.Page.SelectedThemeVariations = themeVariations.SequenceEqual(ThemeVariations.None)
            ? [ThemeVariations.Colorblind]
            : ThemeVariations.None;
        _context.Controller.SettingsAreModified.Should().BeTrue();
    }

    [TestCaseSource(nameof(CasesThemeSettings))]
    public void Theme_UseSystemVisualStyle_defaults_from_theme(
        ThemeId themeId, string[] themeVariations, bool? useSystemVisualStyle)
    {
        _context.Page.SelectedThemeId = themeId;
        _context.Controller.HandleSelectedThemeChanged();
        if (themeId == ThemeId.WindowsAppColorModeId)
        {
            _context.Page.UseSystemVisualStyle.Should().Be(Application.SystemColorMode == SystemColorMode.Classic);
        }
        else if (useSystemVisualStyle is not null)
        {
            _context.Page.UseSystemVisualStyle.Should().Be(useSystemVisualStyle.Value);
        }
    }

    [Test]
    public void Theme_WindowsAppColorModeId_should_set_SystemColorMode()
    {
        AppSettings.ThemeId = ThemeId.WindowsAppColorModeId;
        AppSettings.UseSystemVisualStyle = true;
        ThemeModule.TestAccessor.ReloadThemeSettings(_context.ThemeRepository);

        _context.Controller.ShowThemeSettings();
        _context.Page.SelectedThemeId.Should().Be(ThemeId.WindowsAppColorModeId);

        // The ThemeId is set from current Windows settings
        ThemeId id = Application.SystemColorMode == SystemColorMode.Dark
            ? ThemeId.DefaultDark
            : ThemeId.DefaultLight;
        ThemeModule.Settings.Theme.Id.Should().Be(id);
    }

    private static IEnumerable<object[]> CasesThemeSettings()
    {
        yield return new object[]
        {
            ThemeId.DefaultLight,
            ThemeVariations.None,
            true // useSystemVisualStyle
        };

        yield return new object[]
        {
            ThemeId.DefaultDark,
            new[] { ThemeVariations.Colorblind },
            false // useSystemVisualStyle
        };

        yield return new object[]
        {
            ThemeId.WindowsAppColorModeId,
            new[] { ThemeVariations.Colorblind },
            null // useSystemVisualStyle, depends on Application.SystemColorMode
        };

        yield return new object[]
        {
            new ThemeId("non_default", isBuiltin: true),
            new[] { ThemeVariations.Colorblind },
            null // useSystemVisualStyle, unknown
        };
    }

    private class MockColorsSettingsPage : IColorsSettingsPage
    {
        public ThemeId SelectedThemeId { get; set; }
        public string[] SelectedThemeVariations { get; set; }
        public bool UseSystemVisualStyle { get; set; }
        public bool LabelRestartIsNeededVisible { get; set; }
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
