using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    internal sealed class TagTree : BaseRefTree
    {
        public TagTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
            : base(treeNode, uiCommands, refsSource, RefsFilter.Tags)
        {
        }

        protected override Nodes FillTree(IReadOnlyList<IGitRef> tags, CancellationToken token)
        {
            Nodes nodes = new(this);
            Dictionary<string, BaseRevisionNode> pathToNodes = new();

            foreach (IGitRef tag in tags)
            {
                token.ThrowIfCancellationRequested();

                bool isVisible = !IsFiltering.Value || (tag.ObjectId is not null && _refsSource.Contains(tag.ObjectId));
                TagNode tagNode = new(this, tag.ObjectId, tag.Name, isVisible);
                var parent = tagNode.CreateRootNode(pathToNodes, (tree, parentPath) => new BasePathNode(tree, parentPath));

                if (parent is not null)
                {
                    nodes.AddNode(parent);
                }
            }

            return nodes;
        }

        protected override void PostFillTreeViewNode(bool firstTime)
        {
            if (firstTime)
            {
                TreeViewNode.Collapse();
            }
        }
    }
}
