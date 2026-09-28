using GitExtensions.Extensibility.Git;
using GitUI.Properties;
namespace GitUI.LeftPanel;

internal abstract class BaseBranchLeafNode : BaseRevisionNode
{
    private readonly Avalonia.Media.IImage _imageKeyMerged;
    private readonly Avalonia.Media.IImage _imageKeyUnmerged;
    private bool _isMerged = false;

    public BaseBranchLeafNode(Tree tree, NodeBase parent, string fullPath, IGitRef gitRef, bool remote)
        : base(tree, parent, fullPath, gitRef, remote ? Images.BranchRemote : Images.BranchLocal)
    {
        _imageKeyUnmerged = remote ? Images.BranchRemote : Images.BranchLocal;
        _imageKeyMerged = remote ? Images.BranchRemoteMerged : Images.BranchLocalMerged;
    }

    protected string? AheadBehind { get; set; }

    protected string? RelatedBranch { get; set; }

    public bool IsMerged
    {
        get => _isMerged;
        set
        {
            if (_isMerged == value)
            {
                return;
            }

            _isMerged = value;

            ApplyStyle();
        }
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();

        if (!Visible)
        {
            Avalonia.Controls.ToolTip.SetTip(TreeViewNode, string.Format(TranslatedStrings.InvisibleCommit, FullPath));
        }
        else if (_isMerged)
        {
            Avalonia.Controls.ToolTip.SetTip(TreeViewNode, string.Format(TranslatedStrings.ContainedInCurrentCommit, Name));
        }
    }

    public void UpdateAheadBehind(string aheadBehindData, string relatedBranch)
    {
        AheadBehind = aheadBehindData;
        RelatedBranch = relatedBranch;
    }

    protected override string DisplayText()
    {
        return string.IsNullOrEmpty(AheadBehind) ? Name : $"{Name} ({AheadBehind})";
    }

    protected override Avalonia.Media.IImage GetVisibleIcon()
        => IsMerged ? _imageKeyMerged : _imageKeyUnmerged;

    protected override void SelectRevision()
    {
        string branch = RelatedBranch is null || !Owner.SelectionModifiers.HasFlag(Avalonia.Input.KeyModifiers.Alt)
            ? FullPath
            : RelatedBranch;
        GoToRevision(branch);
    }
}
