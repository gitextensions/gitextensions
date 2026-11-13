using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;

namespace GitUI.LeftPanel;

internal sealed class TagTree : BaseRefTree
{
    public TagTree(RepoObjectsTree owner, IReadOnlyList<IGitRef> tags, ICheckRefs? refsSource = null)
        : base(owner, RepoTreeKind.Tags, TranslatedStrings.Tags, Images.TagHorizontal, refsSource, RefsFilter.Tags)
    {
        Nodes.AddNodes(FillTree(tags, CancellationToken.None));
        Complete(TranslatedStrings.Tags, Images.TagHorizontal, expanded: false);
    }

    protected override Nodes FillTree(IReadOnlyList<IGitRef> tags, CancellationToken token)
    {
        Nodes nodes = new(this);
        Dictionary<string, BaseRevisionNode> pathToNodes = [];

        foreach (IGitRef tag in tags)
        {
            token.ThrowIfCancellationRequested();

            TagNode tagNode = new(this, tag.ObjectId, tag.Name, visible: true) { GitRef = tag };
            BaseRevisionNode? parent = tagNode.CreateRootNode(pathToNodes, (tree, parentPath) => new BasePathNode(tree, parentPath));

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
            TreeViewNode.IsExpanded = false;
        }
    }
}
