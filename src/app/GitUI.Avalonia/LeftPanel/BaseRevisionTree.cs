using Avalonia.Controls;
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;
using TreeNode = Avalonia.Controls.TreeViewItem;

namespace GitUI.LeftPanel;

internal abstract class BaseRevisionTree : Tree
{
    // The previewer constructs repository trees without the runtime ThreadHelper bootstrap.
    private readonly ExclusiveTaskRunner _updateTaskRunner = Design.IsDesignMode
        ? new ExclusiveTaskRunner(new TaskManager(new JoinableTaskContext()))
        : ThreadHelper.CreateExclusiveTaskRunner();

    protected readonly ICheckRefs _refsSource;

    public BaseRevisionTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
        : base(treeNode, uiCommands)
    {
        _refsSource = refsSource;
    }

    public override void Dispose()
    {
        _updateTaskRunner.Dispose();
        base.Dispose();
    }

    protected override void OnDetached()
    {
        _updateTaskRunner.CancelCurrent();
        base.OnDetached();
    }

    internal virtual void UpdateVisibility()
    {
        if (!IsAttached)
        {
            return;
        }

        _updateTaskRunner.RunDetached(async cancellationToken =>
        {
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await LoadingCompleted.Task;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

            await TreeViewNode.SwitchToMainThreadAsync(cancellationToken);

            // Check again after switch to main thread
            if (!IsAttached)
            {
                return;
            }

            // Batch style updates to avoid per-node TreeView invalidation on large ref sets.
            foreach (BaseRevisionNode node in Nodes.DepthEnumerator<BaseRevisionNode>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (node.ObjectId.IsZero)
                {
                    continue;
                }

                bool isVisible = _refsSource.Contains(node.ObjectId);
                if (node.Visible != isVisible)
                {
                    node.Visible = isVisible;
                    node.ApplyStyle();
                }
            }
        });
    }
}
