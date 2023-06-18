using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.CommandDialogs
{
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
}
