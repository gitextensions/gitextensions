using GitUIPluginInterfaces;

namespace GitUI.CommandDialogs
{
    public interface IRevisionGridInfo
    {
        IReadOnlyList<GitRevision> GetSelectedRevisions();
        string GetCurrentBranch();
    }
}
