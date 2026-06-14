using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid;

internal sealed class GitRefListsForRevision
{
    private readonly IGitRef[] _allBranches;
    private readonly IGitRef[] _localBranches;
    private readonly IGitRef[] _branchesWithNoIdenticalRemotes;
    private readonly IGitRef[] _tags;

    public GitRefListsForRevision(GitRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);

        if (revision.Refs is null)
        {
            throw new ArgumentNullException(nameof(revision));
        }

        _allBranches = [.. revision.Refs.Where(h => !h.IsTag && (h.IsHead || h.IsRemote))];
        _localBranches = Array.FindAll(_allBranches, b => !b.IsRemote);
        _branchesWithNoIdenticalRemotes = [.. _allBranches.Where(b => !b.IsRemote ||
                                                                  !_localBranches.Any(lb => lb.TrackingRemote == b.Remote && lb.MergeWith == b.LocalName))];

        _tags = [.. revision.Refs.Where(h => h.IsTag)];
    }

    public IReadOnlyList<IGitRef> LocalBranches => _localBranches;

    public IReadOnlyList<IGitRef> AllBranches => _allBranches;

    public IReadOnlyList<IGitRef> AllTags => _tags;

    public IReadOnlyList<IGitRef> BranchesWithNoIdenticalRemotes => _branchesWithNoIdenticalRemotes;

    public IReadOnlyList<string> GetAllBranchNames()
    {
        return Array.ConvertAll(_allBranches, b => b.Name);
    }

    public IReadOnlyList<string> GetAllTagNames()
    {
        return AllTags.Select(t => t.Name).ToArray();
    }

    /// <summary>
    /// Returns the collection of branches and tags which can be deleted.
    /// </summary>
    public IReadOnlyList<IGitRef> GetDeletableRefs(string currentBranch)
    {
        return _allBranches.Where(b => b.IsRemote || b.Name != currentBranch).Union(_tags).ToArray();
    }

    /// <summary>
    /// Returns the collection of local branches which can be renamed.
    /// </summary>
    public IReadOnlyList<IGitRef> GetRenameableLocalBranches()
    {
        return _localBranches.ToList();
    }
}
