using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog;

public interface ISettingsPageHost
{
    void GotoPage(SettingsPageReference settingsPageReference);

    void SaveAll();

    void LoadAll();

    CheckSettingsLogic CheckSettingsLogic { get; }
}

public class SettingsPageHostMock(CheckSettingsLogic checkSettingsLogic) : ISettingsPageHost
{
    public CheckSettingsLogic CheckSettingsLogic { get; } = checkSettingsLogic;

    public void GotoPage(SettingsPageReference settingsPageReference)
    {
    }

    public void SaveAll()
    {
    }

    public void LoadAll()
    {
    }
}
