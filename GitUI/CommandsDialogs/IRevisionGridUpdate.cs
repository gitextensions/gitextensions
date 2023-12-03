using GitExtensions.Extensibility.Git;

namespace GitUI.CommandDialogs
{
    public interface IRevisionGridUpdate
    {
        bool SetSelectedRevision(ObjectId? objectId, bool toggleSelection = false, bool updateNavigationHistory = true);
    }
}
