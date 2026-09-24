using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

internal abstract class BaseBranchLeafNode : BaseRevisionNode
{
    private readonly Avalonia.Media.IImage _imageKeyMerged;
    private readonly Avalonia.Media.IImage _imageKeyUnmerged;
    private readonly bool _isCurrent;
    private bool _isMerged = false;

    protected BaseBranchLeafNode(Tree tree, NodeBase parent, string fullPath, IGitRef gitRef, bool remote, bool isCurrent = false)
        : base(tree, parent, fullPath, gitRef, remote ? Images.BranchRemote : Images.BranchLocal, isCurrent)
    {
        _imageKeyUnmerged = remote ? Images.BranchRemote : Images.BranchLocal;
        _imageKeyMerged = remote ? Images.BranchRemoteMerged : Images.BranchLocalMerged;
        _isCurrent = isCurrent;
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
        ApplyStyle();
    }

    protected override string DisplayText()
        => string.IsNullOrEmpty(AheadBehind) ? Name : $"{Name} ({AheadBehind})";

    protected override WinFormsShims.FontStyle GetFontStyle()
        => base.GetFontStyle() | (_isCurrent ? WinFormsShims.FontStyle.Bold : WinFormsShims.FontStyle.Regular);

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
