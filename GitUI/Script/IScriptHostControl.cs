using GitUIPluginInterfaces;

namespace GitUI.Script
{
    public interface IScriptHostControl
    {
        GitRevision GetCurrentRevision();
        GitRevision? GetLatestSelectedRevision();
        IReadOnlyList<GitRevision> GetSelectedRevisions();
        Point GetQuickItemSelectorLocation();
        string GetCurrentBranch();
    }
}
