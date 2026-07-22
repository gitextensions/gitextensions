using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog;

public abstract class GroupSettingsPage : Translate, ISettingsPage
{
    protected GroupSettingsPage(string title)
    {
        Title = title;
        Translator.Translate(this, AppSettings.CurrentTranslation);
    }

    public string Title { get; }

    public string GetTitle() => Title;

    public Control? GuiControl => null;

    public void OnPageShown()
    {
    }

    public void LoadSettings()
    {
    }

    public void SaveSettings()
    {
    }

    public IEnumerable<string> GetSearchKeywords() => [];

    public bool IsInstantSavePage => false;

    public SettingsPageReference PageReference => new SettingsPageReferenceByType(GetType());
}
