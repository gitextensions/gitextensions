using GitUIPluginInterfaces;

namespace GitUI.Script
{
    public interface IScriptHostControl
    {
        GitRevision? GetLatestSelectedRevision();
        IReadOnlyList<GitRevision> GetSelectedRevisions();
        Point GetQuickItemSelectorLocation();
    }
}
