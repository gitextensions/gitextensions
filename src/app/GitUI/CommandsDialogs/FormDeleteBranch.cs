using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public sealed partial class FormDeleteBranch : GitExtensionsDialog
{
    private readonly TranslationString _deleteBranchCaption = new("Delete Branches");
    private readonly TranslationString _cannotDeleteCurrentBranchMessage = new("Cannot delete the branch \u201c{0}\u201d which you are currently on.");
    private readonly TranslationString _cannotDeleteProtectedBranchMessage = new("The following branches are protected from accidental deletion:\n{0}\n\nRight-click the branch in the branch list and select 'Remove branch deletion prevention' to unlock it first.");
    private readonly TranslationString _deleteBranchConfirmTitle = new("Delete Confirmation");
    private readonly TranslationString _deleteBranchQuestion = new("The selected branch(es) have not been merged into HEAD.\r\nProceed?");
    private readonly TranslationString _useReflogHint = new("Did you know you can use reflog to restore deleted branches?");

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
            MessageBox.Show(this, string.Format(_cannotDeleteCurrentBranchMessage.Text, _currentBranch), _deleteBranchCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        IGitRef[] protectedBranches = [.. selectedBranches.Where(branch => ProtectedBranchStore.IsProtected(Module.WorkingDirGitDir, branch.Name))];
        if (protectedBranches.Length > 0)
        {
            string names = string.Join("\n", protectedBranches.Select(b => $"  \u2022 {b.Name}"));
            MessageBox.Show(this, string.Format(_cannotDeleteProtectedBranchMessage.Text, names), _deleteBranchCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
}
