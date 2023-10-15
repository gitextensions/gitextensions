using GitUIPluginInterfaces;

namespace GitUI.ScriptsEngine
{
    public interface IScriptHostControl
    {
        GitRevision? GetLatestSelectedRevision();
        IReadOnlyList<GitRevision> GetSelectedRevisions();
        Point GetQuickItemSelectorLocation();

        void GoToRef(string? refName, bool showNoRevisionMsg, bool toggleSelection = false);
        void Refresh();
    }
}
