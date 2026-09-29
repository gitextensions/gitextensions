using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations.Xliff;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

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
    private IGitUICommandsSource? _uiCommandsSource;
    private readonly ExclusiveTaskRunner _reloadTaskRunner = ThreadHelper.CreateExclusiveTaskRunner();
    private bool _firstReloadNodesSinceModuleChanged = true;
    protected TaskCompletionSource LoadingCompleted = new();

    protected Tree(RepoObjectsTree owner, RepoTreeKind kind, string caption, IImage icon)
        : base(owner, parent: null, caption, icon)
    {
        Kind = kind;

        // When GitModule has changed, clear selected node
        // Certain operations need to happen the first time after we change modules. For example,
        // we don't want to use the expanded/collapsed state of existing nodes in the tree, but at
        // the same time, we don't want to remove them from the tree as this is visible to the user,
        // as well as less efficient.
        SetUICommandsSource(owner.CommandsSource);
        Attached();
    }

    public RepoTreeKind Kind { get; }

    internal RepoObjectsTree OwnerControl => Owner;

    public IGitUICommands UICommands => _uiCommandsSource?.UICommands ?? Owner.UICommands;

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
    {
        Detached();
        _reloadTaskRunner.Dispose();
    }

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
        _reloadTaskRunner.CancelCurrent();
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

    internal void OnModuleChanged()
    {
        // When GitModule has changed, clear selected node.
        _firstReloadNodesSinceModuleChanged = true;
    }

    internal void SetUICommandsSource(IGitUICommandsSource? uiCommands)
    {
        if (ReferenceEquals(_uiCommandsSource, uiCommands))
        {
            return;
        }

        _uiCommandsSource = uiCommands;
        if (uiCommands is not null)
        {
            uiCommands.UICommandsChanged += (_, _) => OnModuleChanged();
        }
    }

    // Invoke from child class to reload nodes for the current Tree. Clears Nodes, invokes
    // input async function that should populate Nodes, then fills the tree view with its contents,
    // making sure to disable/enable the control.
    protected JoinableTask ReloadNodesDetached(
        Func<Func<RefsFilter, IReadOnlyList<IGitRef>>, CancellationToken, Task<Nodes>> loadNodesTask,
        Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        => _reloadTaskRunner.RunDetached(async cancellationToken =>
        {
            if (!IsAttached)
            {
                return;
            }

            try
            {
                LoadingCompleted = new TaskCompletionSource();

                // Module is invalid in Dashboard
                Nodes newNodes = Module.IsValidGitWorkingDir()
                    ? await loadNodesTask(getRefs, cancellationToken)
                    : new Nodes(tree: null);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Check again after switch to main thread
                    if (!IsAttached)
                    {
                        return;
                    }

                    // remember multi-selected nodes
                    HashSet<string> selected = OwnerControl.CaptureSelectedNodeIdentities(this);
                    Nodes.Clear();
                    Nodes.AddNodes(newNodes);
                    FillTreeViewNode(originalSelectedNodeFullNamePath: null, _firstReloadNodesSinceModuleChanged);

                    // re-apply multi-selection
                    OwnerControl.RestoreSelectedNodes(this, selected);
                    ExpandPathToSelectedNode();
                    _firstReloadNodesSinceModuleChanged = false;
                });
            }
            finally
            {
                LoadingCompleted.TrySetResult();
            }
        });

    private void FillTreeViewNode(string? originalSelectedNodeFullNamePath, bool firstTime)
    {
        _ = originalSelectedNodeFullNamePath;
        Nodes.FillTreeViewNode(TreeViewNode);
        PostFillTreeViewNode(firstTime);
    }

    protected void Complete(string caption, Avalonia.Media.IImage icon, bool expanded)
    {
        SetHeader(caption, icon);
        TreeViewNode.IsExpanded = expanded;
        foreach (NodeBase node in DescendantsAndSelf().Skip(1))
        {
            node.ApplyStyle();
        }

        FillTreeViewNode(originalSelectedNodeFullNamePath: null, _firstReloadNodesSinceModuleChanged);
        ExpandPathToSelectedNode();
        if (Nodes.Count > 0)
        {
            _firstReloadNodesSinceModuleChanged = false;
        }
    }

    // Called after the TreeView has been populated from Nodes. A good place to update properties
    // of the TreeViewNode, such as it's name (TreeViewNode.Text), Expand/Collapse state, and
    // to set selected node (TreeViewNode.TreeView.SelectedNode).
    protected virtual void PostFillTreeViewNode(bool firstTime)
    {
    }

    private void ExpandPathToSelectedNode()
    {
        if (TreeViewNode.Items.Count == 0)
        {
            return;
        }

        // If no selected node, just make sure that the first node is visible
        TreeViewItem node = GetSelectedNodes().FirstOrDefault()?.TreeViewNode
            ?? TreeViewNode.Items.OfType<TreeViewItem>().First();
        node.BringIntoView();
    }
}
