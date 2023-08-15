using GitUI.UserControls.RevisionGrid;

namespace GitUI.LeftPanel
{
    internal abstract class BaseRevisionTree : Tree
    {
        private CancellationTokenSequence _updateCancellationTokenSequence = new();

        protected readonly ICheckRefs _refsSource;

        public BaseRevisionTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
            : base(treeNode, uiCommands)
        {
            _refsSource = refsSource;
        }

        public override void Dispose()
        {
            _updateCancellationTokenSequence.Dispose();
            base.Dispose();
        }

        protected override void OnDetached()
        {
            _updateCancellationTokenSequence.CancelCurrent();
            base.OnDetached();
        }

        internal virtual void UpdateVisibility()
        {
            CancellationToken cancellationToken = _updateCancellationTokenSequence.Next();

            TreeView treeView = TreeViewNode.TreeView;

            if (treeView is null || !IsAttached)
            {
                return;
            }

            ThreadHelper.FileAndForget(async () =>
            {
                await _updateSemaphore.WaitAsync(cancellationToken);
                try
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
                        if (node.ObjectId is null)
                        {
                            continue;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                        bool isVisible = _refsSource.Contains(node.ObjectId);
                        if (node.Visible != isVisible)
                        {
                            node.Visible = isVisible;
                            node.ApplyStyle();
                        }
                    }
                }
                finally
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        _updateSemaphore.Release();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Ignore
                    }
                }
            });
        }
    }
}
