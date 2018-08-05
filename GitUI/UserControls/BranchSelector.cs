using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitCommands.Git;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls
{
    public partial class BranchSelector : GitModuleControl
    {
        public event EventHandler SelectedIndexChanged;

        private readonly bool _isLoading;
        private IReadOnlyList<ObjectId> _containRevisions;
        private string[] _localBranches;
        private string[] _remoteBranches;
        public ObjectId CommitToCompare;

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

        public void Initialize(bool remote, IReadOnlyList<ObjectId> containRevisions)
        {
            lbChanges.Text = "";
            LocalBranch.Checked = !remote;
            Remotebranch.Checked = remote;

            _containRevisions = containRevisions;

            Branches.Items.Clear();
            Branches.Items.AddRange(_containRevisions != null
                ? GetContainsRevisionBranches()
                : LocalBranch.Checked
                    ? GetLocalBranches()
                    : GetRemoteBranches());

            if (_containRevisions != null && Branches.Items.Count == 1)
            {
                Branches.SelectedIndex = 0;
            }
            else
            {
                Branches.Text = null;
            }

            string[] GetLocalBranches()
            {
                if (_localBranches == null)
                {
                    _localBranches = Module.GetRefs(false).Select(b => b.Name).ToArray();
                }

                return _localBranches;
            }

            string[] GetRemoteBranches()
            {
                if (_remoteBranches == null)
                {
                    _remoteBranches = Module.GetRefs(true, true).Where(h => h.IsRemote && !h.IsTag).Select(b => b.Name).ToArray();
                }

                return _remoteBranches;
            }

            string[] GetContainsRevisionBranches()
            {
                var result = new HashSet<string>();

                if (_containRevisions.Count > 0)
                {
                    var branches = Module.GetAllBranchesWhichContainGivenCommit(_containRevisions[0], LocalBranch.Checked,
                            !LocalBranch.Checked)
                        .Where(a => !DetachedHeadParser.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                    result.UnionWith(branches);
                }

                for (int index = 1; index < _containRevisions.Count; index++)
                {
                    var containRevision = _containRevisions[index];
                    var branches =
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

            if (SelectedBranchName.IsNullOrWhiteSpace())
            {
                lbChanges.Text = "";
            }
            else
            {
                var branchName = SelectedBranchName;
                var currentCheckout = CommitToCompare ?? Module.GetCurrentCheckout();

                if (currentCheckout == null)
                {
                    lbChanges.Text = "";
                    return;
                }

                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                        var text = Module.GetCommitCountString(currentCheckout.ToString(), branchName);
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
