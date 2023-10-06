using GitUIPluginInterfaces;

namespace GitUI.ScriptsEngine
{
    public interface IScriptHostControl
    {
        GitRevision? GetLatestSelectedRevision();
        IReadOnlyList<GitRevision> GetSelectedRevisions();
        Point GetQuickItemSelectorLocation();
    }
}
