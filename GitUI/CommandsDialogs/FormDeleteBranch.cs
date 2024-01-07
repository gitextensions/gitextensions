using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteBranch : GitExtensionsDialog
    {
        private readonly TranslationString _deleteBranchCaption = new("Delete Branches");
        private readonly TranslationString _cannotDeleteCurrentBranchMessage = new("Cannot delete the branch “{0}” which you are currently on.");
        private readonly TranslationString _deleteBranchConfirmTitle = new("Delete Confirmation");
        private readonly TranslationString _deleteBranchQuestion = new("The selected branch(es) have not been merged into any local branch.\r\n\r\nProceed?");
        private readonly TranslationString _useReflogHint = new("This commit is available from the reflog that you could use to restore this deleted branches");
        private readonly TranslationString _deleteBranchNotInReflogQuestion = new("The selected branch(es) have not been merged into any local branch\r\n and is(are) not in the reflog so you won't be able to recover the commit(s) easily.\r\n\r\nProceed?");
        private readonly TranslationString _useRecoverLostObjectsHint = new("This commit will only be recoverable using \"Recover lost objects\" feature but could be cleaned by git at any time!");
        private readonly TranslationString _restoreUsingReflogAvailable = new("This branch can be restored using the reflog");
        private readonly TranslationString _warningNotInReflog = new("Warning! The head of this branch is not in the reflog!\r\nCommits won't be recoverable easily!!");

        private readonly IEnumerable<string> _defaultBranches;
        private string? _currentBranch;
        private IReadOnlySet<string> _reflogHashes;
        private HashSet<string>? _mergedBranches;
        private Dictionary<ObjectId, IReadOnlyList<string>> _containedInBranch = new();
        private Dictionary<ObjectId, bool> _cacheRefInReflog = new(1);

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

            _reflogHashes = Module.GetReflogHashes();

            Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Heads).ToList();

            _currentBranch = Module.GetSelectedBranch(emptyIfDetached: true);

            _mergedBranches = [];

            if (!string.IsNullOrEmpty(_currentBranch))
            {
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

            bool areAllInReflog = AreAllRefsInReflog(selectedBranches);

            // Detect if commits will be dangling (i.e. no remaining local refs left handling commit)
            string[] deletedCandidates = selectedBranches.Select(b => b.Name).ToArray();
            bool atLeastOneHeadCommitWillBeDangling = false;

            foreach (IGitRef selectedBranch in selectedBranches)
            {
                if (_mergedBranches.Contains(selectedBranch.Name))
                {
                    continue;
                }

                atLeastOneHeadCommitWillBeDangling = !GetAllBranchesWhichContainGivenCommit(selectedBranch.ObjectId)
                    .Any(b2 => !deletedCandidates.Contains(b2));

                if (atLeastOneHeadCommitWillBeDangling)
                {
                    break;
                }
            }

            if (atLeastOneHeadCommitWillBeDangling)
            {
                if (!areAllInReflog || !AppSettings.DontConfirmDeleteUnmergedBranch)
                {
                    TaskDialogPage page = new()
                    {
                        Text = areAllInReflog ? _deleteBranchQuestion.Text : _deleteBranchNotInReflogQuestion.Text,
                        Caption = _deleteBranchConfirmTitle.Text,
                        Icon = areAllInReflog ? TaskDialogIcon.Warning : TaskDialogIcon.ShieldWarningYellowBar,
                        Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                        DefaultButton = TaskDialogButton.No,
                        Footnote = areAllInReflog ? _useReflogHint.Text : _useRecoverLostObjectsHint.Text,
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

        private IReadOnlyList<string> GetAllBranchesWhichContainGivenCommit(ObjectId commitId)
        {
            lock (_containedInBranch)
            {
                if (_containedInBranch.TryGetValue(commitId, out IReadOnlyList<string> branches))
                {
                    return branches;
                }

                branches = Module.GetAllBranchesWhichContainGivenCommit(commitId, true, false); // include also remotes?

                _containedInBranch.Add(commitId, branches);
                return branches;
            }
        }

        private void BuildContainedInBranchData(IGitRef[] selectedBranches)
        {
            if (!selectedBranches.Any())
            {
                return;
            }

            foreach (IGitRef selectedBranch in selectedBranches)
            {
                GetAllBranchesWhichContainGivenCommit(selectedBranch.ObjectId);
            }
        }

        private void Branches_SelectedValueChanged(object sender, EventArgs e)
        {
            ProcessSelectedBranches();
        }

        private void ProcessSelectedBranches()
        {
            IGitRef[] selectedBranches = Branches.GetSelectedBranches().ToArray();
            CheckSelectedBranches(selectedBranches);

            Task.Run(() =>
            {
                BuildContainedInBranchData(selectedBranches.Where(b => !_mergedBranches.Contains(b.Name)).ToArray());
            });
        }

        private bool AreAllRefsInReflog(IGitRef[] refs)
        {
            foreach (IGitRef gitRef in refs)
            {
                if (_cacheRefInReflog.TryGetValue(gitRef.ObjectId, out bool isInReflog))
                {
                    if (!isInReflog)
                    {
                        return false;
                    }

                    continue;
                }

                isInReflog = _reflogHashes.Any(gitRef.ObjectId.Equals);
                _cacheRefInReflog.Add(gitRef.ObjectId, isInReflog);
                if (!isInReflog)
                {
                    return false;
                }
            }

            return true;
        }

        private void CheckSelectedBranches(IGitRef[] selectedBranches)
        {
            if (!selectedBranches.Any())
            {
                return;
            }

            if (!AreAllRefsInReflog(selectedBranches))
            {
                labelWarning.Text = _warningNotInReflog.Text;
                labelWarning.ForeColor = Color.Orange;
            }
            else
            {
                labelWarning.Text = _restoreUsingReflogAvailable.Text;
                labelWarning.ForeColor = Color.Green;
            }
        }
    }
}
