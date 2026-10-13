using GitUI.UserControls.RevisionGrid;

namespace GitUI.LeftPanel;

internal abstract class BaseRevisionTree : Tree
{
    protected readonly ICheckRefs? _refsSource;

    protected BaseRevisionTree(
        RepoObjectsTree owner,
        RepoTreeKind kind,
        string caption,
        Avalonia.Media.IImage icon,
        ICheckRefs? refsSource)
        : base(owner, kind, caption, icon)
    {
        _refsSource = refsSource;
    }

    internal virtual void UpdateVisibility()
    {
        if (!IsAttached || _refsSource is null)
        {
            return;
        }

        foreach (BaseRevisionNode node in Nodes.DepthEnumerator<BaseRevisionNode>())
        {
            if (node.ObjectId.IsZero)
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
}
