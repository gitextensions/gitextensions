using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel.Interfaces;

namespace GitUI.LeftPanel;

internal abstract class Node : NodeBase, INode
{
    protected Tree Tree { get; }

    protected IGitUICommands UICommands => Tree.UICommands;

    protected Node(Tree tree, NodeBase parent, string caption, IImage icon, bool isBold = false, bool isItalic = false)
        : base(tree.OwnerControl, parent, caption, icon, isBold, isItalic)
    {
        Tree = tree;
    }

    protected IWin32Window ParentWindow()
        => Owner;

    protected virtual string DisplayText()
        => SearchText;

    // Override to provide a unique node name (key), otherwise DisplayText is used
    protected virtual string NodeName()
        => DisplayText();

    protected void ApplyText()
    {
        if (TreeViewNode.Header is Avalonia.Controls.StackPanel panel
            && panel.Children.OfType<Avalonia.Controls.Image>().FirstOrDefault()?.Source is IImage icon)
        {
            SetHeader(DisplayText(), icon);
        }
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

    public static Node GetNode(Avalonia.Controls.TreeViewItem treeNode)
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
