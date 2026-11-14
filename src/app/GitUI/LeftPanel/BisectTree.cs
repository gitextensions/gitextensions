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
    ///  Gets the bisect refs in display order: the bad ref first, then the good refs, then any other refs (i.e. skipped).
    /// </summary>
    internal static IReadOnlyList<IGitRef> GetBisectRefs(IEnumerable<IGitRef> refs)
        => [.. refs
            .Where(r => r.IsBisect && !r.ObjectId.IsZero)
            .OrderBy(r => r.IsBisectBad ? 0 : r.IsBisectGood ? 1 : 2)];

    /// <summary>
    ///  Gets whether a bisect is in progress, i.e. whether any bisect ref exists.
    /// </summary>
    internal static bool IsBisecting(IEnumerable<IGitRef> refs)
        => refs.Any(r => r.IsBisect && !r.ObjectId.IsZero);

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
        public void FillTree(IReadOnlyList<IGitRef> refs)
        {
            tree.Nodes.Clear();
            tree.Nodes.AddNodes(tree.FillTree(refs, CancellationToken.None));
            tree.Nodes.FillTreeViewNode(tree.TreeViewNode);
        }
    }
}
