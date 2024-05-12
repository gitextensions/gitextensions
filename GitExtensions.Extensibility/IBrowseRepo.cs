using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitExtensions.Extensibility;

public interface IBrowseRepo
{
    GitRevision? GetLatestSelectedRevision();
    IReadOnlyList<GitRevision> GetSelectedRevisions();
    Point GetQuickItemSelectorLocation();
    void GoToRef(string refName, bool showNoRevisionMsg, bool toggleSelection = false);
    void SetWorkingDir(string? path, ObjectId? selectedId = null, ObjectId? firstId = null);
}
