using GitCommands;

namespace GitUI.CommandsDialogs.BrowseDialog;

public sealed class UpdateCheckService : IUpdateCheckService
{
    public void SearchForUpdatesAndShow(IWin32Window ownerWindow, bool alwaysShow)
    {
        FormUpdates updateForm = new(AppSettings.AppVersion);
        updateForm.SearchForUpdatesAndShow(ownerWindow, alwaysShow);
    }
}
