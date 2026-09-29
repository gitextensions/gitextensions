namespace GitUI.CommandsDialogs.BrowseDialog;

public sealed class UpdateCheckService : IUpdateCheckService
{
    public void SearchForUpdatesAndShow(IWin32Window ownerWindow, Version currentVersion, bool alwaysShow)
    {
        FormUpdates updateForm = new(currentVersion);
        updateForm.SearchForUpdatesAndShow(ownerWindow, alwaysShow);
    }
}
