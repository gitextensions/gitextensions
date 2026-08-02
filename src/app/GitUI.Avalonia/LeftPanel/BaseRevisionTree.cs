namespace GitUI.LeftPanel;

internal abstract class BaseRevisionTree : Tree
{
    protected BaseRevisionTree(RepoObjectsTree owner, RepoTreeKind kind, string caption, Avalonia.Media.IImage icon)
        : base(owner, kind, caption, icon)
    {
    }
}
