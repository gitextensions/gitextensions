using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel.Interfaces;
using Microsoft;

namespace GitUI.LeftPanel;

internal abstract class Node : NodeBase, INode
{
    protected Tree Tree
    {
        get
        {
            Validates.NotNull(Nodes.Tree);
            return Nodes.Tree;
        }
    }

    protected IGitUICommands UICommands => Tree.UICommands;

    protected Node(Tree? tree)
        : base(tree)
    {
    }

    /// <summary>
    /// The tree node representing this node.
    /// Note that it may not always be set and which <see cref="Node"/> it represents may change
    /// because <see cref="Nodes.FillTreeViewNode(TreeNode)"/> recycles <see cref="TreeNode"/>s.
    /// </summary>
    protected internal override TreeNode TreeViewNode
    {
        get
        {
            Validates.NotNull(field);
            return field;
        }
        set
        {
            Validates.NotNull(value);
            field = value;
            field.Tag = this;
            ApplyText();
            ApplyStyle();
        }
    }

    /// <summary>
    ///  Gets the tree view that shows this node, also while the subtree containing the node is still being built
    ///  and its <see cref="TreeNode"/> is not yet attached (see <see cref="Nodes.FillTreeViewNode(TreeNode)"/>).
    /// </summary>
    protected TreeView? OwnerTreeView => TreeViewNode.TreeView ?? Nodes.Tree?.TreeViewNode.TreeView;

    protected IWin32Window? ParentWindow()
    {
        return TreeViewNode.TreeView?.FindForm();
    }

    protected virtual string DisplayText()
    {
        return ToString()!;
    }

    // Override to provide a unique node name (key), otherwise DisplayText is used
    protected virtual string NodeName()
    {
        return DisplayText();
    }

    protected void ApplyText()
    {
        TreeViewNode.Name = NodeName();

        // Check before if value has changed because that's a costly operation
        string updatedText = DisplayText();
        if (updatedText != TreeViewNode.Text)
        {
            TreeViewNode.Text = updatedText;
        }
    }

    /// <summary>
    ///  Navigates the revision grid to the specified ref (commit SHA, branch name, tag, etc.)
    ///  and returns focus to the tree view.
    /// </summary>
    protected void GoToRevision(string @ref)
    {
        TreeView? treeView = TreeViewNode.TreeView;
        bool toggleSelection = Control.ModifierKeys.HasFlag(Keys.Control);

        treeView?.BeginInvoke(() =>
        {
            UICommands.BrowseRepo?.GoToRef(@ref, showNoRevisionMsg: true, toggleSelection: toggleSelection);
            treeView.Focus();
        });
    }

    internal virtual void OnSelected()
    {
    }

    internal virtual void OnClick()
    {
    }

    internal virtual void OnDoubleClick()
    {
    }

    internal virtual void OnRename()
    {
    }

    internal virtual void OnDelete()
    {
    }

    public static Node GetNode(TreeNode treeNode)
    {
        return (Node)treeNode.Tag!;
    }

    internal static T? GetNodeSafe<T>(TreeNode? treeNode) where T : class, INode
    {
        return treeNode?.Tag as T;
    }

    public static void OnNode<T>(TreeNode? treeNode, Action<T> action) where T : class, INode
    {
        T? node = GetNodeSafe<T>(treeNode);

        if (node is not null)
        {
            action(node);
        }
    }
}
