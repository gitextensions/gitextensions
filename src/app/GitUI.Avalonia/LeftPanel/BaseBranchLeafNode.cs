using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal abstract class BaseBranchLeafNode : BaseRevisionNode
{
    private readonly bool _isCurrent;
    private readonly bool _remote;
    private bool _isMerged;

    protected BaseBranchLeafNode(Tree tree, NodeBase parent, string fullPath, IGitRef gitRef, bool remote, bool isCurrent = false)
        : base(tree, parent, fullPath, gitRef, remote ? Images.BranchRemote : Images.BranchLocal, isCurrent)
    {
        _remote = remote;
        _isCurrent = isCurrent;
    }

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
            SetHeader(
                Name,
                _remote
                    ? value ? Images.BranchRemoteMerged : Images.BranchRemote
                    : value ? Images.BranchLocalMerged : Images.BranchLocal,
                isBold: _isCurrent);
        }
    }
}
