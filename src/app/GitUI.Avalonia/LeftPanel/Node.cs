using Avalonia.Controls;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.LeftPanel.Interfaces;
using Microsoft;
using TreeNode = Avalonia.Controls.TreeViewItem;

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
    protected TreeView? OwnerTreeView => Owner.FindControl<TreeView>("treeMain");

    protected IWin32Window? ParentWindow()
        => Owner;

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
        // Check before if value has changed because that's a costly operation
        string updatedText = DisplayText();
        if (updatedText != TreeViewNode.GetText())
        {
            TreeViewNode.SetText(updatedText);
        }

        // Framework constraint: an Avalonia item cannot be renamed once styled, so NodeName() is not stored on it.
    }

    /// <summary>
    /// Navigates the revision grid to the specified ref (commit SHA, branch name, tag, etc.)
    /// and returns focus to the tree view.
    /// </summary>
    protected void GoToRevision(string @ref)
    {
        if (!Owner.TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            return;
        }

        bool toggleSelection = Owner.SelectionModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control)
            || Owner.SelectionModifiers.HasFlag(Avalonia.Input.KeyModifiers.Meta);
        commands.BrowseRepo?.GoToRef(@ref, showNoRevisionMsg: true, toggleSelection);
        Owner.FocusTree();
    }

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
