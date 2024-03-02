﻿using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;

namespace GitUI.UserControls
{
    public partial class BranchSelector : GitModuleControl
    {
        public event EventHandler? SelectedIndexChanged;

        private readonly bool _isLoading;
        private IReadOnlyList<ObjectId>? _containRevisions;
        private string[]? _localBranches;
        private string[]? _remoteBranches;
        public ObjectId? CommitToCompare;

        public BranchSelector()
        {
            _isLoading = true;
            try
            {
                InitializeComponent();
                Branches.SelectedIndexChanged += Branches_SelectedIndexChanged;
                InitializeComplete();
            }
            finally
            {
                _isLoading = false;
            }
        }

        public bool IsRemoteBranchChecked => Remotebranch.Checked;
        public string SelectedBranchName => Branches.Text;
        public override string Text => Branches.Text;

        public void Initialize(bool remote, IReadOnlyList<ObjectId>? containRevisions)
        {
            lbChanges.Text = "";
            LocalBranch.Checked = !remote;
            Remotebranch.Checked = remote;

            _containRevisions = containRevisions;

            Branches.Items.Clear();
            Branches.Items.AddRange(_containRevisions is not null
                ? GetContainsRevisionBranches()
                : LocalBranch.Checked
                    ? GetLocalBranches()
                    : GetRemoteBranches());

            Branches.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);

            if (_containRevisions is not null && Branches.Items.Count == 1)
            {
                Branches.SelectedIndex = 0;
            }
            else
            {
                Branches.Text = null;
            }

            string[] GetLocalBranches()
            {
                return _localBranches ??= Module.GetRefs(RefsFilter.Heads).Select(b => b.Name).ToArray();
            }

            string[] GetRemoteBranches()
            {
                return _remoteBranches ??= Module.GetRefs(RefsFilter.Remotes).Select(b => b.Name).ToArray();
            }

            string[] GetContainsRevisionBranches()
            {
                HashSet<string> result = [];

                if (_containRevisions.Count > 0)
                {
                    IEnumerable<string> branches = Module.GetAllBranchesWhichContainGivenCommit(_containRevisions[0], LocalBranch.Checked,
                            !LocalBranch.Checked)
                        .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                    result.UnionWith(branches);
                }

                for (int index = 1; index < _containRevisions.Count; index++)
                {
                    ObjectId containRevision = _containRevisions[index];
                    IEnumerable<string> branches =
                        Module.GetAllBranchesWhichContainGivenCommit(containRevision, LocalBranch.Checked,
                                !LocalBranch.Checked)
                            .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                        !a.EndsWith("/HEAD"));
                    result.IntersectWith(branches);
                }

                return result.ToArray();
            }
        }

        private void Branches_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbChanges.Text = "";
            FireSelectionChangedEvent(sender, e);

            if (string.IsNullOrWhiteSpace(SelectedBranchName))
            {
                lbChanges.Text = "";
            }
            else
            {
                string branchName = SelectedBranchName;
                ObjectId currentCheckout = CommitToCompare ?? Module.GetCurrentCheckout();

                if (currentCheckout is null)
                {
                    lbChanges.Text = "";
                    return;
                }

                ThreadHelper.FileAndForget(async () =>
                    {
                        string text = Module.GetCommitCountString(currentCheckout, branchName);
                        await this.SwitchToMainThreadAsync();
                        lbChanges.Text = text;
                    });
            }
        }

        private void LocalBranch_CheckedChanged(object sender, EventArgs e)
        {
            Branches.Focus();

            // We only need to refresh the dialog once -> RemoteBranchCheckedChanged will trigger this
            ////BranchTypeChanged();
        }

        private void Remotebranch_CheckedChanged(object sender, EventArgs e)
        {
            Branches.Focus();
            if (!_isLoading)
            {
                Initialize(IsRemoteBranchChecked, null);
            }

            FireSelectionChangedEvent(sender, e);
        }

        private void FireSelectionChangedEvent(object sender, EventArgs e)
        {
            SelectedIndexChanged?.Invoke(sender, e);
        }

        public new void Focus()
        {
            Branches.Focus();
        }
    }
}
