namespace GitExtensions.Plugins.DeleteUnusedBranches;

/// <summary>
/// Custom sortable branches list, use for support user-defined sorting in <see cref="DeleteUnusedBranchesForm"/>.
/// </summary>
internal sealed class SortableBranchesList : List<Branch>
{
    public IReadOnlyList<Branch> GetSorted(string? column, bool ascending)
    {
        IEnumerable<Branch> branches = column switch
        {
            nameof(Branch.Date) => OrderBy(branch => branch.Date),
            nameof(Branch.Name) => OrderBy(branch => branch.Name, StringComparer.CurrentCulture),
            nameof(Branch.Author) => OrderBy(branch => branch.Author, StringComparer.CurrentCulture),
            _ => this,
        };
        return branches.ToArray();

        IEnumerable<Branch> OrderBy<TKey>(Func<Branch, TKey> selector, IComparer<TKey>? comparer = null)
            => ascending
                ? Enumerable.OrderBy(this, selector, comparer)
                : Enumerable.OrderByDescending(this, selector, comparer);
    }
}
