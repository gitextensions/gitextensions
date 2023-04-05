using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel
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

                TagNode tagNode = new(this, tag.ObjectId, tag.Name, visible: true);
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
