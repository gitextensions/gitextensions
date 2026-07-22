using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal sealed class TagTree : BaseRefTree
{
    public TagTree(RepoObjectsTree owner, IReadOnlyList<IGitRef> tags)
        : base(owner, RepoTreeKind.Tags, TranslatedStrings.Tags, Images.TagHorizontal)
    {
        FillNested(
            tags,
            (parent, path, gitRef, _) => gitRef is null
                ? new BasePathNode(this, parent, path)
                : new TagNode(this, parent, gitRef));
        Complete(TranslatedStrings.Tags, Images.TagHorizontal, tags.Count, expanded: false);
    }
}
