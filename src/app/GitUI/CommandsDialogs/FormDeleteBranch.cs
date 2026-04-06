using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public sealed partial class FormDeleteBranch : GitExtensionsDialog
{
    private readonly TranslationString _deleteBranchCaption = new("Delete Branches");
    private readonly TranslationString _cannotDeleteCurrentBranchMessage = new("Cannot delete the branch “{0}” which you are currently on.");
    private readonly TranslationString _deleteBranchConfirmTitle = new("Delete Confirmation");
    private readonly TranslationString _deleteBranchQuestion = new("The selected branch(es) have not been merged into HEAD.\r\nProceed?");
    private readonly TranslationString _useReflogHint = new("Did you know you can use reflog to restore deleted branches?");
    private readonly TranslationString _branchUsedByWorktreeQuestion = new("The following branches are checked out in worktrees and cannot be deleted directly:\n\n{0}\n\nDo you want to delete the worktrees and branches together?");
    private readonly TranslationString _cannotDeleteBranchInMainWorktree = new("The branch \u201c{0}\u201d cannot be deleted because it is checked out in the main worktree at:\n{1}");

    private readonly IEnumerable<string> _defaultBranches;
    private string? _currentBranch;
    private HashSet<string>? _mergedBranches;

    public FormDeleteBranch(IGitUICommands commands, IEnumerable<string> defaultBranches)
        : base(commands, enablePositionRestore: false)
    {
        _defaultBranches = defaultBranches;

        InitializeComponent();

        MinimumSize = new Size(Width, PreferredMinimumHeight);

        InitializeComplete();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Heads).ToList();
        if (AppSettings.DontConfirmDeleteUnmergedBranch)
        {
            // no need to fill _mergedBranches
            _currentBranch = Module.GetSelectedBranch();
        }
        else
        {
            _mergedBranches = [];
            foreach (string branch in Module.GetMergedBranches())
            {
                if (branch.StartsWith("* "))
                {
                    _currentBranch = branch.Trim('*', ' ');
                }
                else
                {
                    _mergedBranches.Add(branch.Trim());
                }
            }
        }

        if (_defaultBranches is not null)
        {
            Branches.SetSelectedText(_defaultBranches.Join(" "));
        }

        Branches.Focus();
    }

    private void Delete_Click(object sender, EventArgs e)
    {
        IGitRef[] selectedBranches = [.. Branches.GetSelectedBranches()];
        if (selectedBranches.Length == 0)
        {
            return;
        }

        if (_currentBranch is not null && selectedBranches.Any(branch => branch.Name == _currentBranch))
        {
            MessageBoxes.Show(this, string.Format(_cannotDeleteCurrentBranchMessage.Text, _currentBranch), _deleteBranchCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        selectedBranches = HandleWorktreeBranches(selectedBranches);
        if (selectedBranches.Length == 0)
        {
            return;
        }

        if (!AppSettings.DontConfirmDeleteUnmergedBranch)
        {
            Validates.NotNull(_mergedBranches);

            // always treat branches as unmerged if there is no current branch (HEAD is detached)
            bool hasUnmergedBranches = _currentBranch is null || DetachedHeadParser.IsDetachedHead(_currentBranch)
                || selectedBranches.Any(branch => !_mergedBranches.Contains(branch.Name));
            if (hasUnmergedBranches)
            {
                TaskDialogPage page = new()
                {
                    Text = _deleteBranchQuestion.Text,
                    Caption = _deleteBranchConfirmTitle.Text,
                    Icon = TaskDialogIcon.Warning,
                    Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                    DefaultButton = TaskDialogButton.No,
                    Footnote = _useReflogHint.Text,
                    SizeToContent = true,
                };

                bool isConfirmed = TaskDialog.ShowDialog(Handle, page) == TaskDialogButton.Yes;
                if (!isConfirmed)
                {
                    return;
                }
            }
        }

        IGitCommand cmd = Commands.DeleteBranch(selectedBranches, force: true);
        bool success = UICommands.StartCommandLineProcessDialog(Owner, cmd);
        if (success)
        {
            Close();
        }
    }

    /// <summary>
    ///  Checks whether any of <paramref name="selectedBranches"/> are checked out in worktrees
    ///  other than the current one. For linked worktrees, offers to delete the worktree and branch
    ///  together. For the main worktree, shows an error. Returns the branches that should proceed
    ///  to deletion.
    /// </summary>
    private IGitRef[] HandleWorktreeBranches(IGitRef[] selectedBranches)
    {
        IReadOnlyList<GitWorktree> worktrees = Module.GetWorktrees();
        if (worktrees.Count <= 1)
        {
            return selectedBranches;
        }

        string currentDir = Path.GetFullPath(Module.WorkingDir).TrimEnd(Path.DirectorySeparatorChar);

        List<(IGitRef Branch, GitWorktree Worktree)> mainWorktreeBranches = [];
        List<(IGitRef Branch, GitWorktree Worktree)> linkedWorktreeBranches = [];

        for (int i = 0; i < worktrees.Count; i++)
        {
            GitWorktree wt = worktrees[i];
            if (wt.Branch is null || wt.IsDeleted)
            {
                continue;
            }

            string worktreeDir = Path.GetFullPath(wt.Path).TrimEnd(Path.DirectorySeparatorChar);
            if (string.Equals(worktreeDir, currentDir, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (IGitRef branch in selectedBranches)
            {
                if (branch.Name == wt.Branch)
                {
                    if (i == 0)
                    {
                        mainWorktreeBranches.Add((branch, wt));
                    }
                    else
                    {
                        linkedWorktreeBranches.Add((branch, wt));
                    }

                    break;
                }
            }
        }

        if (mainWorktreeBranches.Count == 0 && linkedWorktreeBranches.Count == 0)
        {
            return selectedBranches;
        }

        HashSet<string> excludedBranches = [];

        if (mainWorktreeBranches.Count > 0)
        {
            (IGitRef branch, GitWorktree worktree) = mainWorktreeBranches[0];
            MessageBoxes.Show(
                this,
                string.Format(_cannotDeleteBranchInMainWorktree.Text, branch.Name, worktree.Path),
                _deleteBranchCaption.Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            excludedBranches.Add(branch.Name);
        }

        if (linkedWorktreeBranches.Count > 0)
        {
            string branchList = string.Join(
                "\n",
                linkedWorktreeBranches.Select(x => $"\u2022 {x.Branch.Name}  \u2192  {x.Worktree.Path}"));

            TaskDialogPage page = new()
            {
                Text = string.Format(_branchUsedByWorktreeQuestion.Text, branchList),
                Caption = _deleteBranchCaption.Text,
                Heading = TranslatedStrings.CannotBeUndone,
                Icon = TaskDialogIcon.Warning,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                DefaultButton = TaskDialogButton.No,
                SizeToContent = true,
            };

            if (TaskDialog.ShowDialog(Handle, page) == TaskDialogButton.Yes)
            {
                bool anyDeleted = false;
                foreach ((IGitRef branch, GitWorktree worktree) in linkedWorktreeBranches)
                {
                    if (worktree.Path.TryDeleteDirectory(out string? errorMessage))
                    {
                        anyDeleted = true;
                    }
                    else
                    {
                        MessageBoxes.Show(
                            this,
                            $"{string.Format(TranslatedStrings.DeleteWorktreeFailed, worktree.Path)}\n{errorMessage}",
                            TranslatedStrings.Error,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        excludedBranches.Add(branch.Name);
                    }
                }

                if (anyDeleted)
                {
                    UICommands.StartCommandLineProcessDialog(Owner, command: null, "worktree prune");
                }
            }
            else
            {
                foreach ((IGitRef branch, _) in linkedWorktreeBranches)
                {
                    excludedBranches.Add(branch.Name);
                }
            }
        }

        return excludedBranches.Count > 0
            ? selectedBranches.Where(b => !excludedBranches.Contains(b.Name)).ToArray()
            : selectedBranches;
    }
}
