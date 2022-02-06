using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GitUI.BranchTreePanel
{
    internal static class NodeExtensions
    {
        internal static IEnumerable<NodeBase> HavingChildren(this IEnumerable<NodeBase> nodes)
            => nodes.Where(node => node.HasChildren);

        /// <summary>Filters <paramref name="nodes"/> for those that are not <see cref="TreeNode.IsExpanded"/>.
        /// This only makes sense on nodes <see cref="HavingChildren(IEnumerable{NodeBase})"/>,
        /// so you'll probably want to use that filter first.</summary>
        internal static IEnumerable<NodeBase> Expandable(this IEnumerable<NodeBase> nodes)
            => nodes.Where(node => !node.TreeViewNode.IsExpanded);

        /// <summary>Filters <paramref name="nodes"/> for those that are <see cref="TreeNode.IsExpanded"/>.
        /// This only makes sense on nodes <see cref="HavingChildren(IEnumerable{NodeBase})"/>,
        /// so you'll probably want to use that filter first.</summary>
        internal static IEnumerable<NodeBase> Collapsible(this IEnumerable<NodeBase> nodes)
            => nodes.Where(node => node.TreeViewNode.IsExpanded);
    }
}
