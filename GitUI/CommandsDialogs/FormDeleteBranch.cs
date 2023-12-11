using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteBranch : GitExtensionsDialog
    {
        private readonly TranslationString _deleteBranchCaption = new("Delete Branches");
        private readonly TranslationString _cannotDeleteCurrentBranchMessage = new("Cannot delete the branch “{0}” which you are currently on.");
        private readonly TranslationString _deleteBranchConfirmTitle = new("Delete Confirmation");
        private readonly TranslationString _useReflogHint = new("Did you know you can use reflog to restore deleted branches?");
        private readonly TranslationString _deleteBranchQuestion = new("The selected branch(es) have not been merged into any local branch.\r\n\r\nProceed?");

        private readonly IEnumerable<string> _defaultBranches;
        private string? _currentBranch;
        private IReadOnlyList<string> _reflogHashes;

        public FormDeleteBranch(GitUICommands commands, IEnumerable<string> defaultBranches)
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
            _currentBranch = Module.GetSelectedBranch();

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

            // Detect if commits will be dangling (i.e. no remaining local refs left handling commit)
            string[] deletedCandidates = selectedBranches.Select(b => b.Name).ToArray();
            bool atLeastOneHeadCommitWillBeDangling = false;

            foreach (IGitRef selectedBranch in selectedBranches)
            {
                atLeastOneHeadCommitWillBeDangling = !Module.GetAllBranchesWhichContainGivenCommit(selectedBranch.ObjectId, true, false) // include also remotes?
                    .Any(b2 => !deletedCandidates.Contains(b2));

                if (atLeastOneHeadCommitWillBeDangling)
                {
                    break;
                }
            }

            if (atLeastOneHeadCommitWillBeDangling)
            {
                if (!AppSettings.DontConfirmDeleteUnmergedBranch)
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
