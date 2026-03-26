using System.Diagnostics;
using GitUI.Properties;

namespace GitUI.LeftPanel;

[DebuggerDisplay("(Worktree) Path = {WorktreePath}, Branch = {Branch}")]
internal sealed class WorktreeNode(Tree tree, string worktreePath, string? branch, string? sha1, bool isCurrent, bool isDetached, bool isBare, bool isDeleted) : Node(tree)
{
    public string WorktreePath { get; } = worktreePath;
    public string? Branch { get; } = branch;
    public string? Sha1 { get; } = sha1;
    public bool IsCurrent { get; } = isCurrent;
    public bool IsDetached { get; } = isDetached;
    public bool IsBare { get; } = isBare;
    public bool IsDeleted { get; } = isDeleted;

    internal override void OnDoubleClick()
    {
        if (!IsCurrent && !IsDeleted)
        {
            OpenWorktree();
        }
    }

    public void OpenWorktree()
    {
        if (IsDeleted)
        {
            return;
        }

        UICommands.WorktreeSwitch(ParentWindow(), WorktreePath);
    }

    public void DeleteWorktree()
    {
        if (UICommands.WorktreeDelete(ParentWindow(), WorktreePath))
        {
            ((WorktreeTree)Tree).Refresh();
        }
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();

        if (IsDeleted)
        {
            TreeViewNode.ForeColor = SystemColors.GrayText;
        }
        else if (IsCurrent)
        {
            TreeViewNode.NodeFont = new Font(TreeViewNode.TreeView!.Font, FontStyle.Bold);
        }

        TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.WorkTree);
        TreeViewNode.ToolTipText = GetToolTipText();

        return;

        string GetToolTipText()
        {
            string shortSha = Sha1?.Length >= 7 ? Sha1[..7] : Sha1;

            string status = IsCurrent ? " (current)"
                : IsDeleted ? " (deleted)"
                : "";

            string branchLine = IsBare ? "bare"
                : IsDetached ? $"detached at {shortSha}"
                : Branch ?? "unknown";

            return shortSha is not null
                ? $"{WorktreePath}{status}\nBranch: {branchLine}\nHEAD: {shortSha}"
                : $"{WorktreePath}{status}\nBranch: {branchLine}";
        }
    }

    protected override string DisplayText()
    {
        string relativePath = GetRelativePath();

        if (IsBare)
        {
            return $"{relativePath} (bare)";
        }

        if (IsDetached)
        {
            string shortSha = Sha1?.Length >= 7 ? Sha1[..7] : Sha1 ?? "???";
            return $"{relativePath} (detached at {shortSha})";
        }

        return Branch is not null
            ? $"{relativePath} ({Branch})"
            : relativePath;

        string GetRelativePath()
        {
            string workingDir = Tree.UICommands.Module.WorkingDir.TrimEnd(Path.DirectorySeparatorChar);
            string parentDir = Path.GetDirectoryName(workingDir) ?? workingDir;

            try
            {
                string relative = Path.GetRelativePath(parentDir, WorktreePath);
                return relative;
            }
            catch
            {
                return WorktreePath;
            }
        }
    }
}
