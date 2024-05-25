using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteBranch : GitExtensionsDialog
    {
        private readonly TranslationString _deleteBranchCaption = new("Delete Branches");
        private readonly TranslationString _cannotDeleteCurrentBranchMessage = new("Cannot delete the branch “{0}” which you are currently on.");
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
            IGitRef[] selectedBranches = Branches.GetSelectedBranches().ToArray();
            if (!selectedBranches.Any())
            {
                return;
            }

            if (_currentBranch is not null && selectedBranches.Any(branch => branch.Name == _currentBranch))
            {
                MessageBox.Show(this, string.Format(_cannotDeleteCurrentBranchMessage.Text, _currentBranch), _deleteBranchCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
}
