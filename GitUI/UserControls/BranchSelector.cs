using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitCommands;

namespace GitUI.UserControls
{
    public partial class BranchSelector : GitModuleControl
    {
        private string[] _containRevisons;
        private readonly bool _isLoading;
        private List<string> _localBranches;
        private List<string> _remoteBranches;
        public event EventHandler SelectedIndexChanged;
        public BranchSelector()
        {
            _isLoading = true;
            try
            {
                InitializeComponent();
                Branches.SelectedIndexChanged += Branches_SelectedIndexChanged;
                Translate();
            }
            finally
            {
                _isLoading = false;
            }
        }

        public string CommitToCompare;

        public bool IsRemoteBranchChecked
        {
            get { return Remotebranch.Checked; }
        }

        public string SelectedBranchName
        {
            get { return Branches.Text; }
        }

        public override string Text
        {
            get { return Branches.Text; }
        }

        public void Initialize(bool remote, string[] containRevisons)
        {
            lbChanges.Text = "";
            LocalBranch.Checked = !remote;
            Remotebranch.Checked = remote;

            _containRevisons = containRevisons;
            Branches.Items.Clear();

            if (_containRevisons == null)
            {
                if (LocalBranch.Checked)
                {
                    Branches.Items.AddRange(GetLocalBranches().ToArray());
                }
                else
                {
                    Branches.Items.AddRange(GetRemoteBranches().ToArray());
                }
            }
            else
            {
                Branches.Items.AddRange(GetContainsRevisionBranches().ToArray());
            }

            if (_containRevisons != null && Branches.Items.Count == 1)
                Branches.SelectedIndex = 0;
            else
                Branches.Text = null;
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
                var currentCheckout = CommitToCompare??Module.GetCurrentCheckout();
                Task.Factory.StartNew(() => this.Module.GetCommitCountString(currentCheckout, branchName))
                    .ContinueWith(t => lbChanges.Text = t.Result, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public IList<string> GetLocalBranches()
        {
            if (_localBranches == null)
                _localBranches = Module.GetRefs(false).Select(b => b.Name).ToList();

            return _localBranches;
        }

        private IList<string> GetRemoteBranches()
        {
            if (_remoteBranches == null)
                _remoteBranches = Module.GetRefs(true, true).Where(h => h.IsRemote && !h.IsTag).Select(b => b.Name).ToList();

            return _remoteBranches;
        }

        private IList<string> GetContainsRevisionBranches()
        {
            var result = new HashSet<string>();
            if (_containRevisons.Length > 0)
            {
                var branches = Module.GetAllBranchesWhichContainGivenCommit(_containRevisons[0], LocalBranch.Checked,
                        !LocalBranch.Checked)
                        .Where(a => !GitModule.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                result.UnionWith(branches);

            }
            for (int index = 1; index < _containRevisons.Length; index++)
            {
                var containRevison = _containRevisons[index];
                var branches =
                    Module.GetAllBranchesWhichContainGivenCommit(containRevison, LocalBranch.Checked,
                        !LocalBranch.Checked)
                        .Where(a => !GitModule.IsDetachedHead(a) &&
                                    !a.EndsWith("/HEAD"));
                result.IntersectWith(branches);
            }
            return result.ToList();
        }

        public void SetCurrentBranch(string branch, bool remote)
        {
            //Set current branch after initialize, because initialize will reset it
            if (!string.IsNullOrEmpty(branch))
            {
                Branches.Items.Add(branch);
                Branches.SelectedItem = branch;
            }

            if (_containRevisons != null)
            {
                if (Branches.Items.Count == 0)
                {
                    Initialize(remote, _containRevisons);
                }
            }
        }

        private void LocalBranch_CheckedChanged(object sender, EventArgs e)
        {
            Branches.Focus();
            //We only need to refresh the dialog once -> RemoteBranchCheckedChanged will trigger this
            //BranchTypeChanged();
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
            if (SelectedIndexChanged != null)
            {
                SelectedIndexChanged(sender, e);
            }
        }

        public new void Focus()
        {
            Branches.Focus();
        }
    }
}
