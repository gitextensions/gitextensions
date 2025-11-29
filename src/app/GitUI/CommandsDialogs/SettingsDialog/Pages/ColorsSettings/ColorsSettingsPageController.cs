using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

internal class ColorsSettingsPageController
{
    private readonly IColorsSettingsPage _page;
    private readonly IThemeRepository _themeRepository;
    private readonly IThemePathProvider _themePathProvider;
    private int _updateThemeSettingsCounter;

    public ColorsSettingsPageController(
        IColorsSettingsPage page, IThemeRepository themeRepository,
        IThemePathProvider themePathProvider)
    {
        _page = page;
        _themeRepository = themeRepository;
        _themePathProvider = themePathProvider;
    }

    public bool SettingsAreModified =>
        _page.SelectedThemeId != ThemeModule.Settings.Theme.Id
        || _page.UseSystemVisualStyle != ThemeModule.Settings.UseSystemVisualStyle
        || !_page.SelectedThemeVariations.SequenceEqual(AppSettings.ThemeVariations);

    public void ShowThemeSettings()
    {
        BeginUpdateThemeSettings();
        _page.PopulateThemeMenu(Enumerable.Repeat(ThemeId.WindowsAppColorModeId, 1).Concat(_themeRepository.GetThemeIds()));
        _page.SelectedThemeId = AppSettings.ThemeId;
        _page.SelectedThemeVariations = AppSettings.ThemeVariations;
        _page.UseSystemVisualStyle = AppSettings.UseSystemVisualStyle;
        EndUpdateThemeSettings();
    }

    public void ApplyThemeSettings()
    {
        AppSettings.UseSystemVisualStyle = _page.UseSystemVisualStyle;
        AppSettings.ThemeId = _page.SelectedThemeId;
        AppSettings.ThemeVariations = _page.SelectedThemeVariations;
    }

    public void HandleSelectedThemeChanged()
    {
        if (IsThemeSettingsUpdating())
        {
            return;
        }

        BeginUpdateThemeSettings();
        _page.UseSystemVisualStyle = _page.SelectedThemeId == ThemeId.DefaultLight;
        if (_page.SelectedThemeId == ThemeId.DefaultLight)
        {
            _page.SelectedThemeVariations = ThemeVariations.None;
        }

        EndUpdateThemeSettings();
    }

    public void HandleUseSystemVisualStyleChanged() =>
        UpdateThemeSettings();

    public void HandleUseColorblindVariationChanged() =>
        UpdateThemeSettings();

    public void UpdateThemeSettings()
    {
        BeginUpdateThemeSettings();
        EndUpdateThemeSettings();
    }

    public void BeginUpdateThemeSettings() =>
        _updateThemeSettingsCounter++;

    public bool IsThemeSettingsUpdating() =>
        _updateThemeSettingsCounter > 0;

    public void EndUpdateThemeSettings()
    {
        int counter = --_updateThemeSettingsCounter;
        if (counter < 0)
        {
            throw new InvalidOperationException($"{nameof(EndUpdateThemeSettings)} must be called after {nameof(BeginUpdateThemeSettings)}");
        }

        ThemeId themeId = _page.SelectedThemeId;
        if (themeId == ThemeId.WindowsAppColorModeId)
        {
            themeId = ThemeId.ColorModeThemeId;
        }

        if (counter == 0)
        {
            _page.LabelRestartIsNeededVisible = SettingsAreModified;
            _page.IsChoosingVisualStyleEnabled = themeId != ThemeId.DefaultLight;
        }

        if (themeId == ThemeId.DefaultLight)
        {
            // invariant, already loaded
            return;
        }

        try
        {
            _ = _themeRepository.GetTheme(themeId, _page.SelectedThemeVariations);
        }
        catch (Exception ex)
        {
            _page.ShowThemeLoadingErrorMessage(themeId, _page.SelectedThemeVariations, ex);
        }
    }

    public void ShowAppThemesDirectory() =>
        OsShellUtil.SelectPathInFileExplorer(_themePathProvider.AppThemesDirectory);

    public void ShowUserThemesDirectory()
    {
        if (_themePathProvider.UserThemesDirectory is not null)
        {
            // Make sure the directory exists before we try to open it
            Directory.CreateDirectory(_themePathProvider.UserThemesDirectory);

            OsShellUtil.SelectPathInFileExplorer(_themePathProvider.UserThemesDirectory);
        }
    }
}
