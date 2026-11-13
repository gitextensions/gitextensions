using Avalonia.Controls;
using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal abstract class BaseBranchLeafNode : BaseRevisionNode
{
    private readonly IImage _imageKeyMerged;
    private readonly IImage _imageKeyUnmerged;

    private bool _isMerged = false;

    public BaseBranchLeafNode(Tree tree, in ObjectId objectId, string fullPath, bool visible, IImage imageKeyUnmerged, IImage imageKeyMerged)
        : base(tree, fullPath, visible)
    {
        ObjectId = objectId;
        _imageKeyUnmerged = imageKeyUnmerged;
        _imageKeyMerged = imageKeyMerged;
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

        TreeViewNode.SetImage(
            Visible
                ? IsMerged ? _imageKeyMerged : _imageKeyUnmerged
                : Images.EyeClosed);
        if (!Visible)
        {
            ToolTip.SetTip(TreeViewNode, string.Format(TranslatedStrings.InvisibleCommit, FullPath));
        }
        else if (_isMerged)
        {
            ToolTip.SetTip(TreeViewNode, string.Format(TranslatedStrings.ContainedInCurrentCommit, Name));
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

    protected override void SelectRevision()
    {
        string branch = RelatedBranch is null || !Owner.SelectionModifiers.HasFlag(Avalonia.Input.KeyModifiers.Alt)
            ? FullPath
            : RelatedBranch;
        GoToRevision(branch);
    }
}
