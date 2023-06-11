using GitUI.UserControls.RevisionGrid;

namespace GitUI.LeftPanel
{
    internal abstract class BaseRevisionTree : Tree
    {
        protected readonly ICheckRefs _refsSource;

        public BaseRevisionTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
            : base(treeNode, uiCommands)
        {
            _refsSource = refsSource;
        }

        internal virtual void UpdateVisibility()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await _updateSemaphore.WaitAsync();
                try
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    foreach (BaseRevisionNode node in Nodes.DepthEnumerator<BaseRevisionNode>())
                    {
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
                }
                finally
                {
                    _updateSemaphore.Release();
                }
            }).FileAndForget();
        }
    }
}
