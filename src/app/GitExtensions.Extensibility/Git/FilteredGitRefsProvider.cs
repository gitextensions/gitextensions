using GitExtensions.Extensibility;

namespace GitExtensions.Extensibility.Git;

// TODO: This should not be in Extensibility!

public sealed class FilteredGitRefsProvider : IFilteredGitRefsProvider
{
    public FilteredGitRefsProvider(IGitModule module)
    {
        _getRefs = new(() => module.GetRefs(RefsFilter.NoFilter));
    }

    public FilteredGitRefsProvider(Lazy<IReadOnlyList<IGitRef>> getRefs)
    {
        _getRefs = getRefs;
    }

    private readonly Lazy<IReadOnlyList<IGitRef>> _getRefs;

    /// <inherit/>
    public IReadOnlyList<IGitRef> GetRefs(RefsFilter filter)
    {
        if (filter == RefsFilter.NoFilter)
        {
            return _getRefs.Value;
        }

        return _getRefs.Value
            .Where(r =>
                ((filter & RefsFilter.Tags) != 0 && r.IsTag)
                || ((filter & RefsFilter.Remotes) != 0 && r.IsRemote)
                || ((filter & RefsFilter.Heads) != 0 && r.IsHead))
            .ToList();
    }
}
