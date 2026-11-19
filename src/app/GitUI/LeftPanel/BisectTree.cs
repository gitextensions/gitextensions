using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;

namespace GitUI.LeftPanel;

/// <summary>
///  Lists the refs of a bisect in progress (<c>refs/bisect/bad</c>, <c>refs/bisect/good-*</c> and <c>refs/bisect/skip-*</c>).
///  The tree is only shown while a bisect is in progress, always as the first root node,
///  and it takes no part in the reordering and the show/hide settings of the other trees.
/// </summary>
internal sealed class BisectTree : BaseRefTree
{
    public BisectTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
        : base(treeNode, uiCommands, refsSource, RefsFilter.NoFilter)
    {
    }

    /// <summary>
    ///  Gets whether a bisect is in progress, i.e. whether any bisect ref exists.
    /// </summary>
    public static bool IsBisecting(IEnumerable<IGitRef> refs)
        => refs.Any(IsBisectRef);

    /// <summary>
    ///  Requests to refresh the tree with already fetched refs, to avoid querying the refs a second time.
    /// </summary>
    public void Refresh(IReadOnlyList<IGitRef> refs)
        => Refresh(_ => refs);

    /// <summary>
    ///  Gets the bisect refs in display order: the bad ref first, then the good refs, then any other refs (i.e. skipped).
    /// </summary>
    private static IReadOnlyList<IGitRef> GetBisectRefs(IEnumerable<IGitRef> refs)
        => [.. refs
            .Where(IsBisectRef)
            .OrderBy(r => r.IsBisectBad ? 0 : r.IsBisectGood ? 1 : 2)];

    /// <summary>
    ///  Gets whether the ref is a bisect ref that can be shown. A ref without a commit id (<see cref="ObjectId.IsZero"/>)
    ///  cannot be navigated to in the revision grid. Git does not report such a ref below <c>refs/bisect/</c>;
    ///  this is a defensive guard, consistent with the other ref trees, which reject refs without an id.
    /// </summary>
    private static bool IsBisectRef(IGitRef gitRef)
        => gitRef.IsBisect && !gitRef.ObjectId.IsZero;

    protected override Nodes FillTree(IReadOnlyList<IGitRef> refs, CancellationToken token)
    {
        Nodes nodes = new(this);

        foreach (IGitRef gitRef in GetBisectRefs(refs))
        {
            token.ThrowIfCancellationRequested();

            nodes.AddNode(new BisectNode(this, gitRef, visible: true));
        }

        return nodes;
    }

    protected override void PostFillTreeViewNode(bool firstTime)
    {
        if (firstTime)
        {
            TreeViewNode.Expand();
        }
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(BisectTree tree)
    {
        public static IReadOnlyList<IGitRef> GetBisectRefs(IEnumerable<IGitRef> refs)
            => BisectTree.GetBisectRefs(refs);

        public void FillTree(IReadOnlyList<IGitRef> refs)
        {
            tree.Nodes.Clear();
            tree.Nodes.AddNodes(tree.FillTree(refs, CancellationToken.None));
            tree.Nodes.FillTreeViewNode(tree.TreeViewNode);
        }
    }
}
