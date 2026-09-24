using Avalonia.Media;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations.Xliff;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel;

internal enum RepoTreeKind
{
    Branches,
    Remotes,
    Worktrees,
    Tags,
    Submodules,
    Stashes,
}

[LocalizableProperties]
internal abstract class Tree : NodeBase, IDisposable
{
    private bool _firstReloadNodesSinceModuleChanged = true;

    protected Tree(RepoObjectsTree owner, RepoTreeKind kind, string caption, IImage icon)
        : base(owner, parent: null, caption, icon)
    {
        Kind = kind;
        Attached();
    }

    public RepoTreeKind Kind { get; }

    internal RepoObjectsTree OwnerControl => Owner;

    public IGitUICommands UICommands => Owner.UICommands;

    /// <summary>
    /// A flag to indicate that node SelectionChanged event is not user-originated and
    /// must not trigger the event handling sequence.
    /// </summary>
    public bool IgnoreSelectionChangedEvent { get; set; }

    protected IGitModule Module => UICommands.Module;

    /// <summary>
    /// Flag if this tree is enabled or invisible.
    /// </summary>
    protected bool IsAttached { get; private set; }

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

    public virtual void Dispose()
        => Detached();

    public void Attached()
    {
        IsAttached = true;
        OnAttached();
    }

    protected virtual void OnAttached()
    {
    }

    public void Detached()
    {
        IsAttached = false;
        OnDetached();
    }

    protected virtual void OnDetached()
    {
    }

    public void ClearTree()
    {
        Nodes.Clear();
        TreeViewNode.Items.Clear();
    }

    public IEnumerable<TNode> DepthEnumerator<TNode>() where TNode : NodeBase
        => Nodes.DepthEnumerator<TNode>();

    internal IEnumerable<NodeBase> GetNodesAndSelf()
        => DepthEnumerator<NodeBase>().Prepend(this);

    internal IEnumerable<NodeBase> GetSelectedNodes()
        => GetNodesAndSelf().Where(node => node.IsSelected);

    protected void Complete(string caption, Avalonia.Media.IImage icon, bool expanded)
    {
        SetHeader(caption, icon);
        TreeViewNode.IsExpanded = expanded;
        Nodes.FillTreeViewNode(TreeViewNode);
        PostFillTreeViewNode(_firstReloadNodesSinceModuleChanged);
        _firstReloadNodesSinceModuleChanged = false;
    }

    // Called after the TreeView has been populated from Nodes. A good place to update properties
    // of the TreeViewNode, such as its name (TreeViewNode.Text), Expand/Collapse state, and
    // to set selected node (TreeViewNode.TreeView.SelectedNode).
    protected virtual void PostFillTreeViewNode(bool firstTime)
    {
    }
}
