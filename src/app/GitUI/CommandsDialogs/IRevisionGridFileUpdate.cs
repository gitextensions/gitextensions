using GitExtensions.Extensibility.Git;

namespace GitUI.CommandsDialogs;

public interface IRevisionGridFileUpdate
{
    /// <summary>
    ///  Tries to select the revision having <paramref name="commitId"/> in the <see cref="RevisionGridControl"/>
    ///  and stores the <paramref name="filename"/> for selection in the <see cref="FileStatusList"/> after asynchronous loading of the revision.
    /// </summary>
    /// <returns><c>true</c> if the revision was found.</returns>
    bool SelectFileInRevision(ObjectId commitId, RelativePath filename);
}
