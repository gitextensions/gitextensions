using GitUI.UserControls.RevisionGrid;

namespace GitUI.BranchTreePanel
{
    internal abstract class BaseRevisionTree : Tree
    {
        protected readonly ICheckRefs _refsSource;

        public BaseRevisionTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
            : base(treeNode, uiCommands)
        {
            _refsSource = refsSource;
        }

        internal void UpdateVisibility()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                foreach (Node node in Nodes)
                {
                    if (node is not BaseRevisionNode baseNode || baseNode.ObjectId is null)
                    {
                        continue;
                    }

                    bool isVisible = _refsSource.Contains(baseNode.ObjectId);
                    if (baseNode.Visible != isVisible)
                    {
                        baseNode.Visible = isVisible;
                        baseNode.ApplyStyle();
                    }
                }
            }).FileAndForget();
        }
    }
}
