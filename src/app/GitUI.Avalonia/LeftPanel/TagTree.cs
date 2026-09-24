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
        FillTree(tags, CancellationToken.None);
        Complete(TranslatedStrings.Tags, Images.TagHorizontal, expanded: false);
    }

    protected override Nodes FillTree(IReadOnlyList<IGitRef> tags, CancellationToken token)
    {
        FillNested(
            tags,
            (parent, path, gitRef, _) =>
            {
                token.ThrowIfCancellationRequested();
                return gitRef is null
                    ? new BasePathNode(this, parent, path)
                    : new TagNode(this, parent, gitRef);
            });
        return Nodes;
    }

    protected override void PostFillTreeViewNode(bool firstTime)
    {
        if (firstTime)
        {
            TreeViewNode.IsExpanded = false;
        }
    }
}
