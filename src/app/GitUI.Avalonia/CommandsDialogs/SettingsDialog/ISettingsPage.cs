using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog;

public interface ISettingsPage
{
    string GetTitle();

    Control? GuiControl { get; }

    void OnPageShown();

    void LoadSettings();

    void SaveSettings();

    bool IsInstantSavePage { get; }

    IEnumerable<string> GetSearchKeywords();

    SettingsPageReference PageReference { get; }
}
