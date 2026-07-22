using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel;

internal enum RepoTreeKind
{
    Branches,
    Remotes,
    Tags,
    Submodules,
    Stashes,
}

internal abstract class Tree : NodeBase
{
    protected Tree(RepoObjectsTree owner, RepoTreeKind kind, string caption, IImage icon)
        : base(owner, parent: null, caption, icon)
    {
        Kind = kind;
    }

    public RepoTreeKind Kind { get; }

    internal RepoObjectsTree OwnerControl => Owner;

    internal IGitUICommands UICommands => Owner.UICommands;

    public int PositionIndex
    {
        get => Owner.GetTreePosition(Kind);
        set => Owner.SetTreePosition(Kind, value);
    }

    public bool IsEnabled
    {
        get => Owner.GetTreeVisibility(Kind);
        set => Owner.SetTreeVisibility(Kind, value);
    }

    protected void Complete(string caption, Avalonia.Media.IImage icon, int count, bool expanded)
    {
        SetHeader($"{caption} ({count})", icon);
        TreeViewNode.IsExpanded = expanded;
    }
}
