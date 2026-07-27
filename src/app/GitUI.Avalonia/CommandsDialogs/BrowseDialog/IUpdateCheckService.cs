namespace GitUI.CommandsDialogs.BrowseDialog;

public interface IUpdateCheckService
{
    void SearchForUpdatesAndShow(IWin32Window ownerWindow, bool alwaysShow);
}
