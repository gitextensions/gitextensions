using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GitUI.BranchTreePanel
{
    internal static class NodeExtensions
    {
        internal static IEnumerable<RepoObjectsTree.NodeBase> GetNodesAndSelf(this RepoObjectsTree.Tree tree)
            => tree.DepthEnumerator<RepoObjectsTree.NodeBase>().Prepend(tree);

        internal static IEnumerable<RepoObjectsTree.NodeBase> GetMultiSelection(this RepoObjectsTree.Tree tree)
            => tree.GetNodesAndSelf().Where(node => node.IsSelected);

        internal static bool HasChildren(this RepoObjectsTree.NodeBase node)
            => node.Nodes.Count > 0;

        internal static IEnumerable<RepoObjectsTree.NodeBase> HavingChildren(this IEnumerable<RepoObjectsTree.NodeBase> nodes)
            => nodes.Where(node => node.HasChildren());

        /// <summary>Filters <paramref name="nodes"/> for those that are not <see cref="TreeNode.IsExpanded"/>.
        /// This only makes sense on nodes <see cref="HavingChildren(IEnumerable{RepoObjectsTree.NodeBase})"/>,
        /// so you'll probably want to use that filter first.</summary>
        internal static IEnumerable<RepoObjectsTree.NodeBase> Expandable(this IEnumerable<RepoObjectsTree.NodeBase> nodes)
            => nodes.Where(node => !node.TreeViewNode.IsExpanded);

        /// <summary>Filters <paramref name="nodes"/> for those that are <see cref="TreeNode.IsExpanded"/>.
        /// This only makes sense on nodes <see cref="HavingChildren(IEnumerable{RepoObjectsTree.NodeBase})"/>,
        /// so you'll probably want to use that filter first.</summary>
        internal static IEnumerable<RepoObjectsTree.NodeBase> Collapsible(this IEnumerable<RepoObjectsTree.NodeBase> nodes)
            => nodes.Where(node => node.TreeViewNode.IsExpanded);
    }
}
