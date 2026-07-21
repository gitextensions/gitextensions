using GitCommands;
using GitExtUtils.GitUI.Theming;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

/// <summary>
/// Controls the theme choices supported by the current Avalonia theme bridge. Custom CSS
/// theme discovery and validation join this same boundary with the full theme port.
/// </summary>
internal sealed class ColorsSettingsPageController
{
    private readonly IColorsSettingsPage _page;
    private ThemeId _loadedThemeId;
    private string[] _loadedThemeVariations = [];
    private bool _loadedUseSystemVisualStyle;
    private int _updateThemeSettingsCounter;

    public ColorsSettingsPageController(IColorsSettingsPage page)
    {
        _page = page;
    }

    public bool SettingsAreModified
        => _page.SelectedThemeId != _loadedThemeId
            || !_page.SelectedThemeVariations.SequenceEqual(_loadedThemeVariations)
            || (UsesSelectableSystemStyle(_page.SelectedThemeId)
                && _page.UseSystemVisualStyle != _loadedUseSystemVisualStyle);

    public void ShowThemeSettings()
    {
        BeginUpdateThemeSettings();

        _loadedThemeId = NormalizeThemeId(AppSettings.ThemeId);
        _loadedThemeVariations = [.. AppSettings.ThemeVariations];
        _loadedUseSystemVisualStyle = AppSettings.UseSystemVisualStyle;

        ThemeId[] supportedThemeIds =
        [
            ThemeId.WindowsAppColorModeId,
            ThemeId.DefaultLight,
            ThemeId.DefaultDark,
        ];
        _page.PopulateThemeMenu(supportedThemeIds.Contains(_loadedThemeId)
            ? supportedThemeIds
            : supportedThemeIds.Append(_loadedThemeId));
        _page.SelectedThemeId = _loadedThemeId;
        _page.SelectedThemeVariations = _loadedThemeVariations;
        _page.UseSystemVisualStyle = _loadedUseSystemVisualStyle;

        EndUpdateThemeSettings();
    }

    public void ApplyThemeSettings()
    {
        AppSettings.ThemeId = _page.SelectedThemeId;
        AppSettings.ThemeVariations = _page.SelectedThemeVariations;
        if (UsesSelectableSystemStyle(_page.SelectedThemeId))
        {
            AppSettings.UseSystemVisualStyle = _page.UseSystemVisualStyle;
        }
    }

    public void HandleSelectedThemeChanged()
    {
        if (IsThemeSettingsUpdating())
        {
            return;
        }

        BeginUpdateThemeSettings();
        if (_page.SelectedThemeId == ThemeId.DefaultLight)
        {
            _page.UseSystemVisualStyle = true;
            _page.SelectedThemeVariations = ThemeVariations.None;
        }
        else if (_page.SelectedThemeId == ThemeId.DefaultDark)
        {
            _page.UseSystemVisualStyle = false;
        }

        EndUpdateThemeSettings();
    }

    public void HandleUseSystemVisualStyleChanged() => UpdateThemeSettings();

    public void HandleUseColorblindVariationChanged() => UpdateThemeSettings();

    public void UpdateThemeSettings()
    {
        BeginUpdateThemeSettings();
        EndUpdateThemeSettings();
    }

    public void BeginUpdateThemeSettings() => _updateThemeSettingsCounter++;

    public bool IsThemeSettingsUpdating() => _updateThemeSettingsCounter > 0;

    public void EndUpdateThemeSettings()
    {
        int counter = --_updateThemeSettingsCounter;
        if (counter < 0)
        {
            throw new InvalidOperationException($"{nameof(EndUpdateThemeSettings)} must be called after {nameof(BeginUpdateThemeSettings)}");
        }

        if (counter == 0)
        {
            _page.LabelRestartIsNeededVisible = SettingsAreModified;
            _page.IsChoosingVisualStyleEnabled = UsesSelectableSystemStyle(_page.SelectedThemeId);
        }
    }

    private static bool UsesSelectableSystemStyle(ThemeId themeId)
        => themeId != ThemeId.WindowsAppColorModeId && themeId != ThemeId.DefaultLight;

    private static ThemeId NormalizeThemeId(ThemeId themeId)
        => string.IsNullOrWhiteSpace(themeId.Name) ? ThemeId.WindowsAppColorModeId : themeId;
}
