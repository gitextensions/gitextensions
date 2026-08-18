using System.Diagnostics;
using Avalonia.Controls;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel;

[DebuggerDisplay("(Worktree) Path = {Worktree.Path}, Branch = {Worktree.Branch}")]
internal sealed class WorktreeNode : NodeBase
{
    private readonly WorktreeTree _tree;

    public WorktreeNode(WorktreeTree tree, GitWorktree worktree, bool isCurrent, string displayPath)
        : base(tree.OwnerControl, tree, worktree.GetDisplayName(displayPath), Images.WorkTree, isBold: isCurrent)
    {
        _tree = tree;
        Worktree = worktree;
        IsCurrent = isCurrent;
        DisplayPath = displayPath;
        ToolTip.SetTip(TreeViewNode, GetToolTipText());
        if (worktree.IsDeleted)
        {
            TreeViewNode.Classes.Add("worktree-deleted");
        }
    }

    public GitWorktree Worktree { get; }

    public bool IsCurrent { get; }

    private string DisplayPath { get; }

    public override string SearchText => Worktree.Path;

    public ObjectId? ObjectId
        => GitExtensions.Extensibility.Git.ObjectId.TryParse(Worktree.Sha1, out ObjectId objectId)
            ? objectId
            : null;

    internal override void OnDoubleClick()
    {
        if (!IsCurrent && !Worktree.IsDeleted)
        {
            OpenWorktree();
        }
    }

    public void OpenWorktree()
    {
        if (!Worktree.IsDeleted)
        {
            _tree.UICommands.WorktreeSwitch(_tree.OwnerControl, Worktree.Path);
        }
    }

    public void DeleteWorktree()
        => _tree.UICommands.WorktreeDelete(_tree.OwnerControl, Worktree.Path);

    private string GetToolTipText()
    {
        string? shortSha = Worktree.Sha1?.Length >= 7 ? Worktree.Sha1[..7] : Worktree.Sha1;
        string status = IsCurrent ? " (current)"
            : Worktree.IsDeleted ? " (deleted)"
            : string.Empty;
        string branchLine = Worktree.HeadType is GitWorktreeHeadType.Bare ? "bare"
            : Worktree.HeadType is GitWorktreeHeadType.Detached ? $"detached at {shortSha}"
            : Worktree.Branch ?? "unknown";

        return shortSha is not null
            ? $"{Worktree.Path}{status}\nBranch: {branchLine}\nHEAD: {shortSha}"
            : $"{Worktree.Path}{status}\nBranch: {branchLine}";
    }
}
