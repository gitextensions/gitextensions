using GitUI.UserControls.RevisionGrid;

namespace GitUI.LeftPanel
{
    internal abstract class BaseRevisionTree : Tree
    {
        private readonly ExclusiveTaskRunner _updateTaskRunner = ThreadHelper.CreateExclusiveTaskRunner();

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
            TreeView treeView = TreeViewNode.TreeView;

            if (treeView is null || !IsAttached)
            {
                return;
            }

            _updateTaskRunner.RunDetached(async cancellationToken =>
            {
                await treeView.SwitchToMainThreadAsync(cancellationToken);

                // Check again after switch to main thread
                treeView = TreeViewNode.TreeView;

                if (treeView is null || !IsAttached)
                {
                    return;
                }

                foreach (BaseRevisionNode node in Nodes.DepthEnumerator<BaseRevisionNode>())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (node.ObjectId is null)
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
}
