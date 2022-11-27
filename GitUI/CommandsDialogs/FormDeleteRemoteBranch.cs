using GitCommands;
using GitCommands.Git.Commands;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using GitUI.Infrastructure;
using GitUI.Script;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteRemoteBranch : GitExtensionsDialog
    {
        private readonly TranslationString _deleteRemoteBranchesCaption = new("Delete remote branches");
        private readonly TranslationString _confirmDeleteUnmergedRemoteBranchMessage =
            new("At least one remote branch is unmerged. Are you sure you want to delete it?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");

        private readonly string _defaultRemoteBranch;
        private readonly TaskManager _taskManager = ThreadHelper.CreateTaskManager();
        private HashSet<string> _mergedBranches;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormDeleteRemoteBranch()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormDeleteRemoteBranch(GitUICommands commands, string defaultRemoteBranch)
            : base(commands, enablePositionRestore: false)
        {
            _taskManager.RunAsyncAndForget(() => _mergedBranches = Module.GetMergedRemoteBranches().ToHashSet());

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
            RecalculateSizeConstraints();
            base.OnShown(e);
            Branches.Focus();
        }

        private void RecalculateSizeConstraints()
        {
            SuspendLayout();
            MinimumSize = MaximumSize = Size.Empty;

            int height = ControlsPanel.Height + MainPanel.Padding.Top + MainPanel.Padding.Bottom
                       + tlpnlMain.Height + tlpnlMain.Margin.Top + tlpnlMain.Margin.Bottom + DpiUtil.Scale(42);

            MinimumSize = new Size(tlpnlMain.PreferredSize.Width + DpiUtil.Scale(70), height);
            MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, height);
            Size = new Size(Width, height);
            ResumeLayout();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (!DeleteRemote.Checked)
            {
                return;
            }

            List<IGitRef> selectedBranches = Branches.GetSelectedBranches().ToList();

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

                bool success = ScriptManager.RunEventScripts(this, ScriptEvent.BeforePush);
                if (!success)
                {
                    return;
                }

                GitDeleteRemoteBranchesCmd cmd = new(remote, branches.Select(x => x.LocalName));
                using FormRemoteProcess form = new(UICommands, cmd.Arguments)
                {
                    Remote = remote
                };
                form.ShowDialog(Owner);

                if (!form.ErrorOccurred() && !Module.InTheMiddleOfAction())
                {
                    ScriptManager.RunEventScripts(this, ScriptEvent.AfterPush);
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
