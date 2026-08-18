using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel.Interfaces;

namespace GitUI.LeftPanel;

internal abstract class Node : NodeBase, INode
{
    protected Node(Tree tree, NodeBase parent, string caption, IImage icon, bool isBold = false, bool isItalic = false)
        : base(tree.OwnerControl, parent, caption, icon, isBold, isItalic)
    {
        Tree = tree;
    }

    protected Tree Tree { get; }

    protected IGitUICommands UICommands => Tree.UICommands;

    internal virtual void OnSelected()
    {
    }

    internal virtual void OnClick()
    {
    }

    internal override void OnDoubleClick()
    {
    }

    internal virtual void OnRename()
    {
    }

    internal virtual void OnDelete()
    {
    }

    internal static Node GetNode(Avalonia.Controls.TreeViewItem treeNode)
    {
        return (Node)treeNode.Tag!;
    }

    internal static T? GetNodeSafe<T>(Avalonia.Controls.TreeViewItem? treeNode) where T : class, INode
    {
        return treeNode?.Tag as T;
    }

    public static void OnNode<T>(Avalonia.Controls.TreeViewItem? treeNode, Action<T> action) where T : class, INode
    {
        T? node = GetNodeSafe<T>(treeNode);

        if (node is not null)
        {
            action(node);
        }
    }
}
