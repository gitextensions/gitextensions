using Avalonia.Controls;
using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal abstract class BaseRevisionNode : Node
{
    protected const char PathSeparator = '/';
    private readonly IImage _visibleIcon;

    protected BaseRevisionNode(Tree tree, NodeBase parent, string fullPath, IGitRef? gitRef, IImage icon, bool isBold = false)
        : base(tree, parent, GetName(fullPath), icon, isBold)
    {
        FullPath = fullPath;
        GitRef = gitRef;
        ObjectId = gitRef?.ObjectId ?? default;
        _visibleIcon = icon;
    }

    /// <summary>
        /// Short name of the branch/branch path. <example>"issue1344"</example>.
        /// </summary>
    public string Name => GetName(FullPath);

    protected string ParentPath
    {
        get
        {
            int separator = FullPath.LastIndexOf('/');
            return separator < 0 ? string.Empty : FullPath[..separator];
        }
    }

    public IGitRef? GitRef { get; }

    /// <summary>
        /// Full path of the branch. <example>"issues/issue1344"</example>.
        /// </summary>
    public string FullPath { get; }

    public override string SearchText => FullPath;

    /// <summary>
        /// ObjectId for nodes with a revision.
        /// </summary>
    public ObjectId ObjectId { get; protected init; }

    public override void ApplyStyle()
    {
        SetHeader(DisplayText(), Visible ? GetVisibleIcon() : Images.EyeClosed);
        base.ApplyStyle();
        if (!Visible)
        {
            ToolTip.SetTip(TreeViewNode, string.Format(TranslatedStrings.InvisibleCommit, FullPath));
        }
    }

    public override int GetHashCode()
        => FullPath.GetHashCode();

    public override bool Equals(object? obj)
        => obj is BaseRevisionNode other
            && (ReferenceEquals(other, this) || string.Equals(FullPath, other.FullPath, StringComparison.Ordinal));

    public bool Rebase()
        => UICommands.StartRebaseDialog(Owner, FullPath);

    public bool Reset()
        => UICommands.StartResetCurrentBranchDialog(Owner, FullPath);

    protected override string DisplayText()
        => Name;

    protected virtual IImage GetVisibleIcon()
        => _visibleIcon;

    protected virtual void SelectRevision()
    {
        if (!ObjectId.IsZero)
        {
            GoToRevision(ObjectId.ToString());
        }
    }

    private static string GetName(string fullPath)
    {
        int separator = fullPath.LastIndexOf('/');
        return separator < 0 ? fullPath : fullPath[(separator + 1)..];
    }
}
