using System;
using System.Linq;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
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

        public bool SettingsAreModified
        {
            get
            {
                if (_page.SelectedThemeId != ThemeModule.Settings.Theme.Id)
                {
                    return true;
                }

                if (_page.SelectedThemeId == ThemeId.Default)
                {
                    // UseSystemVisualStyle and ThemeVariations settings are only applicable with non-default theme.
                    // However, in order to preserve user preference, we do not reset these when
                    // user chooses the default theme from the menu, we only disable the checkboxes.

                    // This is why, when the default theme is selected, we should ignore any difference in
                    // UseSystemVisualStyle or ThemeVariations checkboxes from the actual theme settings.
                    // Their value is not applied and only kept to be applied when user chooses non-default theme
                    // again.
                    return false;
                }

                return _page.UseSystemVisualStyle != ThemeModule.Settings.UseSystemVisualStyle ||
                    !_page.SelectedThemeVariations.SequenceEqual(AppSettings.ThemeVariations);
            }
        }

        public void ShowThemeSettings()
        {
            BeginUpdateThemeSettings();
            _page.PopulateThemeMenu(Enumerable.Repeat(ThemeId.Default, 1).Concat(_themeRepository.GetThemeIds()));
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
            _page.UseSystemVisualStyle = _page.SelectedThemeId == ThemeId.Default;
            if (_page.SelectedThemeId == ThemeId.Default)
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

            if (counter == 0)
            {
                _page.LabelRestartIsNeededVisible = SettingsAreModified;
                _page.IsChoosingThemeVariationsEnabled =
                    _page.IsChoosingVisualStyleEnabled =
                        _page.SelectedThemeId != ThemeId.Default;
            }

            if (_page.SelectedThemeId != ThemeId.Default)
            {
                try
                {
                    Theme unused = _themeRepository.GetTheme(_page.SelectedThemeId, _page.SelectedThemeVariations);
                }
                catch (Exception ex)
                {
                    _page.ShowThemeLoadingErrorMessage(_page.SelectedThemeId, _page.SelectedThemeVariations, ex);
                }
            }
        }

        public void ShowAppThemesDirectory() =>
            OsShellUtil.SelectPathInFileExplorer(_themePathProvider.AppThemesDirectory);

        public void ShowUserThemesDirectory()
        {
            if (_themePathProvider.UserThemesDirectory is not null)
            {
                OsShellUtil.SelectPathInFileExplorer(_themePathProvider.UserThemesDirectory);
            }
        }
    }
}
