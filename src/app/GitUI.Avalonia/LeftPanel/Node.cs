using Avalonia.Media;
using GitExtensions.Extensibility.Git;

namespace GitUI.LeftPanel;

internal abstract class Node : NodeBase
{
    protected Node(Tree tree, NodeBase parent, string caption, IImage icon, bool isBold = false, bool isItalic = false)
        : base(tree.OwnerControl, parent, caption, icon, isBold, isItalic)
    {
        Tree = tree;
    }

    protected Tree Tree { get; }

    protected IGitUICommands UICommands => Tree.UICommands;
}
