using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

public interface IRevisionGridInfo
{
    ObjectId? CurrentCheckout { get; }
    GitRevision GetRevision(ObjectId objectId);
    GitRevision? GetActualRevision(ObjectId objectId);
    GitRevision GetActualRevision(GitRevision revision);
    IReadOnlyList<GitRevision> GetSelectedRevisions();
    string DescribeRevision(GitRevision revision, int maxLength = 0);
    string GetCurrentBranch();
}
