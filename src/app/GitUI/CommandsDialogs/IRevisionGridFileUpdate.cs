using GitExtensions.Extensibility.Git;

namespace GitUI.CommandsDialogs;

public interface IRevisionGridFileUpdate
{
    bool SelectFileInRevision(ObjectId objectId, RelativePath filename);
}
