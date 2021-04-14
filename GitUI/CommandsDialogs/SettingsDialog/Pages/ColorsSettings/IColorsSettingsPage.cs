using System;
using System.Collections.Generic;
using GitExtUtils.GitUI.Theming;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public interface IColorsSettingsPage
    {
        ThemeId SelectedThemeId { get; set; }

        string[] SelectedThemeVariations { get; set; }

        bool UseSystemVisualStyle { get; set; }

        bool LabelRestartIsNeededVisible { get; set; }

        bool IsChoosingThemeVariationsEnabled { get; set; }

        bool IsChoosingVisualStyleEnabled { get; set; }

        void ShowThemeLoadingErrorMessage(ThemeId themeId, string[] variations, Exception ex);

        void PopulateThemeMenu(IEnumerable<ThemeId> themeIds);
    }
}
