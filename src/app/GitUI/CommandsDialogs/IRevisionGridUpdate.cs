using GitExtensions.Extensibility.Git;

namespace GitUI.CommandsDialogs;

public interface IRevisionGridUpdate
{
    bool SetSelectedRevision(ObjectId? commitId, bool toggleSelection = false, bool updateNavigationHistory = true);
}
