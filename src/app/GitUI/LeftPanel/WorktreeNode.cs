using System.Diagnostics;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel;

[DebuggerDisplay("(Worktree) Path = {Worktree.Path}, Branch = {Worktree.Branch}")]
internal sealed class WorktreeNode(Tree tree, GitWorktree worktree, bool isCurrent, bool isMain, string displayPath) : Node(tree)
{
    public GitWorktree Worktree { get; } = worktree;
    public bool IsCurrent { get; } = isCurrent;

    /// <summary>
    ///  Indicates whether this is the first entry reported by Git, including a bare repository.
    /// </summary>
    public bool IsMain { get; } = isMain;

    /// <summary>
    ///  Indicates whether this is an existing linked worktree other than the current worktree.
    /// </summary>
    public bool CanDelete => !IsMain && !IsCurrent && !Worktree.IsDeleted;
    private string DisplayPath { get; } = displayPath;

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
        if (!CanDelete)
        {
            return;
        }

        if (UICommands.WorktreeDelete(ParentWindow(), Worktree.Path))
        {
            ((WorktreeTree)Tree).Refresh();
        }
    }

    protected override FontStyle GetFontStyle()
        => base.GetFontStyle() | (IsCurrent ? FontStyle.Bold : FontStyle.Regular);

    public override void ApplyStyle()
    {
        base.ApplyStyle();

        if (Worktree.IsDeleted)
        {
            TreeViewNode.ForeColor = SystemColors.GrayText;
        }

        TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.WorkTree);
        TreeViewNode.ToolTipText = GetToolTipText();

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
