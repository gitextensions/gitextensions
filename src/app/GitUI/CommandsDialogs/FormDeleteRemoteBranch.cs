using System.Text;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;
using GitUI.Infrastructure;
using GitUI.ScriptsEngine;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteRemoteBranch : GitExtensionsDialog
    {
        private readonly TranslationString _deleteRemoteBranchesCaption = new("Delete remote branches");
        private readonly TranslationString _confirmDeleteUnmergedRemoteBranchMessage =
            new("At least one remote branch is unmerged. Are you sure you want to delete it?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");
        private readonly TranslationString _toDeleteCandidates = new("Local tracking branche(s) candidate to deletion:");
        private readonly TranslationString _andMore = new("and {0} more...");

        private readonly string _defaultRemoteBranch;
        private readonly TaskManager _taskManager = ThreadHelper.CreateTaskManager();
        private HashSet<string> _mergedBranches;

        public FormDeleteRemoteBranch(IGitUICommands commands, string defaultRemoteBranch)
            : base(commands, enablePositionRestore: false)
        {
            _taskManager.FileAndForget(() => _mergedBranches = Module.GetMergedRemoteBranches().ToHashSet());

            _defaultRemoteBranch = defaultRemoteBranch;

            InitializeComponent();

            InitializeComplete();
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Remotes).ToList();

            if (_defaultRemoteBranch is not null)
            {
                Branches.SetSelectedText(_defaultRemoteBranch);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            CheckDeleteTrackingAllowed();
            base.OnShown(e);
            Branches.Focus();
        }

        private List<IGitRef> GetSelectedRemotRefs() => Branches.GetSelectedBranches().ToList();
        private void CheckDeleteTrackingAllowed()
        {
            string[] localTracking = GetTrackingReferenceOfRemoteRefs(GetSelectedRemotRefs());
            bool localTrackingBranchesExists = localTracking.Any();
            const int maxDisplayed = 8;

            if (!localTrackingBranchesExists)
            {
                DeleteLocalTrackingBranch.Checked = false;
                DeleteLocalTrackingBranch.Enabled = false;
                _NO_TRANSLATE_labelLocalTrackingBranches.Text = string.Empty;
            }
            else
            {
                DeleteLocalTrackingBranch.Enabled = true;

                StringBuilder branchesToDelete = new();
                branchesToDelete.AppendLine(_toDeleteCandidates.Text);
                foreach (string branch in localTracking.Take(maxDisplayed))
                {
                    branchesToDelete.Append(" - ").AppendLine(branch);
                }

                if (localTracking.Length > maxDisplayed)
                {
                    branchesToDelete
                        .AppendLine()
                        .AppendFormat(_andMore.Text, localTracking.Length - maxDisplayed);
                }

                _NO_TRANSLATE_labelLocalTrackingBranches.Text = branchesToDelete.ToString();
            }
        }

        private string[] GetTrackingReferenceOfRemoteRefs(List<IGitRef> remoteRefs)
            => Module.GetRefs(RefsFilter.Heads)
                     .Where(b => remoteRefs.Any(r => b.IsTrackingRemote(r)))
                     .Select(r => r.LocalName)
                     .ToArray();

        private void Branches_SelectedValueChanged(object? sender, EventArgs e)
            => CheckDeleteTrackingAllowed();

        private void Delete_Click(object sender, EventArgs e)
        {
            if (!DeleteRemote.Checked)
            {
                return;
            }

            List<IGitRef> selectedBranches = GetSelectedRemotRefs();

            // wait for _mergedBranches to be filled
            _taskManager.JoinPendingOperations();

            bool hasUnmergedBranches = selectedBranches.Any(branch => !_mergedBranches.Contains(branch.CompleteName));
            if (hasUnmergedBranches)
            {
                if (MessageBox.Show(this,
                                    _confirmDeleteUnmergedRemoteBranchMessage.Text,
                                    _deleteRemoteBranchesCaption.Text,
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    return;
                }
            }

            foreach ((string remote, IEnumerable<IGitRef> branches) in selectedBranches.GroupBy(b => b.Remote))
            {
                EnsurePageant(remote);

                bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforePush, this);
                if (!success)
                {
                    return;
                }

                IGitCommand cmd = Commands.DeleteRemoteBranches(remote, branches.Select(x => x.LocalName));
                using FormRemoteProcess form = new(UICommands, cmd.Arguments)
                {
                    Remote = remote
                };
                form.ShowDialog(Owner);

                if (!form.ErrorOccurred() && !Module.InTheMiddleOfAction())
                {
                    ScriptsRunner.RunEventScripts(ScriptEvent.AfterPush, this);
                    if (DeleteLocalTrackingBranch.Checked)
                    {
                        UICommands.StartDeleteBranchDialog(this, GetTrackingReferenceOfRemoteRefs(selectedBranches));
                    }
                }
            }

            UICommands.RepoChangedNotifier.Notify();
            Close();
        }

        private void DeleteRemote_CheckedChanged(object sender, EventArgs e)
        {
            Delete.Enabled = DeleteRemote.Checked;
        }

        private void EnsurePageant(string remote)
        {
            if (GitSshHelpers.IsPlink)
            {
                PuttyHelpers.StartPageantIfConfigured(() => Module.GetPuttyKeyFileForRemote(remote));
            }
        }
    }
}
