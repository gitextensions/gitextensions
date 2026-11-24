using System.Diagnostics;
using Avalonia.Controls;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.Properties;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

[DebuggerDisplay("(Worktree) Path = {Worktree.Path}, Branch = {Worktree.Branch}")]
internal sealed class WorktreeNode(Tree tree, GitWorktree worktree, bool isCurrent, string displayPath) : Node(tree)
{
    public GitWorktree Worktree { get; } = worktree;
    public bool IsCurrent { get; } = isCurrent;
    private string DisplayPath { get; } = displayPath;

    public override string SearchText => Worktree.Path;

    public ObjectId? ObjectId
        => GitExtensions.Extensibility.Git.ObjectId.TryParse(Worktree.Sha1, out ObjectId objectId)
            ? objectId
            : null;

    internal override void OnSelected()
    {
        if (Tree.IgnoreSelectionChangedEvent)
        {
            return;
        }

        if (Worktree.Sha1 is not null)
        {
            GoToRevision(Worktree.Sha1);
        }
    }

    internal override void OnDoubleClick()
    {
        if (!IsCurrent && !Worktree.IsDeleted)
        {
            OpenWorktree();
        }
    }

    public void OpenWorktree()
    {
        if (Worktree.IsDeleted)
        {
            return;
        }

        UICommands.WorktreeSwitch(ParentWindow(), Worktree.Path);
    }

    public void DeleteWorktree()
    {
        if (UICommands.WorktreeDelete(ParentWindow(), Worktree.Path))
        {
            ((WorktreeTree)Tree).Refresh();
        }
    }

    protected override WinFormsShims.FontStyle GetFontStyle()
        => base.GetFontStyle() | (IsCurrent ? WinFormsShims.FontStyle.Bold : WinFormsShims.FontStyle.Regular);

    public override void ApplyStyle()
    {
        base.ApplyStyle();

        TreeViewNode.Classes.Set("worktree-deleted", Worktree.IsDeleted);

        TreeViewNode.SetImage(Images.WorkTree);
        ToolTip.SetTip(TreeViewNode, GetToolTipText());

        return;

        string GetToolTipText()
        {
            string? shortSha = Worktree.Sha1?.Length >= 7 ? Worktree.Sha1[..7] : Worktree.Sha1;

            string status = IsCurrent ? " (current)"
                : Worktree.IsDeleted ? " (deleted)"
                : "";

            string branchLine = Worktree.HeadType is GitWorktreeHeadType.Bare ? "bare"
                : Worktree.HeadType is GitWorktreeHeadType.Detached ? $"detached at {shortSha}"
                : Worktree.Branch ?? "unknown";

            return shortSha is not null
                ? $"{Worktree.Path}{status}\nBranch: {branchLine}\nHEAD: {shortSha}"
                : $"{Worktree.Path}{status}\nBranch: {branchLine}";
        }
    }

    protected override string DisplayText()
        => Worktree.GetDisplayName(DisplayPath);

    protected override string NodeName()
        => Worktree.Path;
}
