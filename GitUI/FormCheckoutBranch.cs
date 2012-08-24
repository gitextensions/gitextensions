using System;
using System.Diagnostics;
using System.Linq;
using GitCommands;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormCheckoutBranch : GitExtensionsForm
    {
        private string _containRevison;
        private bool isDirtyDir;

        internal FormCheckoutBranch()
        {
            InitializeComponent();
            Translate();
        }

        public FormCheckoutBranch(string branch, bool remote)
            : this(branch, remote, null)
        {
        }

        public FormCheckoutBranch(string branch, bool remote, string containRevison)
            : this()
        {
            _containRevison = containRevison;

            LocalBranch.Checked = !remote;
            Remotebranch.Checked = remote;

            Initialize();

            //Set current branch after initialize, because initialize will reset it
            if (!string.IsNullOrEmpty(branch))
                Branches.Text = branch;

            if (containRevison != null)
            {
                if (Branches.Items.Count == 0)
                {
                    Remotebranch.Checked = true;
                    Initialize();
                }
                if (Branches.Items.Count == 1)
                    Branches.SelectedIndex = 0;
            }

            isDirtyDir = Settings.Module.IsDirtyDir();
            localChangesGB.Visible = isDirtyDir;
            ChangesMode = Settings.CheckoutBranchAction;
            defaultActionChx.Checked = Settings.UseDefaultCheckoutBranchAction;
        }

        public DialogResult DoDefaultActionOrShow(IWin32Window owner)
        {
            if (!Branches.Text.IsNullOrWhiteSpace() && (!isDirtyDir || Settings.UseDefaultCheckoutBranchAction))
                return OkClick();
            else
                return ShowDialog(owner);
        }


        private void Initialize()
        {
            Branches.DisplayMember = "Name";

            if (_containRevison == null)
            {
                if (LocalBranch.Checked)
                {
                    Branches.DataSource = GitModule.Current.GetHeads(false).Select(a => a.Name).ToList();
                }
                else
                {
                    var heads = GitModule.Current.GetHeads(true, true);

                    var remoteHeads = new List<GitHead>();

                    foreach (var head in heads)
                    {
                        if (head.IsRemote && !head.IsTag)
                            remoteHeads.Add(head);
                    }

                    Branches.DataSource = remoteHeads.Select(a => a.Name).ToList();
                }
            }
            else
            {
                var branches = CommitInformation
                    .GetAllBranchesWhichContainGivenCommit(_containRevison, LocalBranch.Checked, !LocalBranch.Checked)
                    .Where(a => !a.Equals("(no branch)", StringComparison.OrdinalIgnoreCase));
                Branches.DataSource = branches.ToList();
            }

            Branches.Text = null;
        }

        private Settings.LocalChanges ChangesMode
        {
            get
            {
                if (rbReset.Checked)
                    return Settings.LocalChanges.Reset;
                else if (rbMerge.Checked)
                    return Settings.LocalChanges.Merge;
                else if (rbStash.Checked)
                    return Settings.LocalChanges.Merge;
                else
                    return Settings.LocalChanges.DontChange;
            }
            set
            {
                rbReset.Checked = value == Settings.LocalChanges.Reset;
                rbMerge.Checked = value == Settings.LocalChanges.Merge;
                rbStash.Checked = value == Settings.LocalChanges.Stash;
                rbDontChange.Checked = value == Settings.LocalChanges.DontChange;
            }
        }

        private void OkClick(object sender, EventArgs e)
        {
            DialogResult = OkClick();
            if (DialogResult == DialogResult.OK)
                Close();
        }

        private DialogResult OkClick()
        {
            Settings.LocalChanges changes = ChangesMode;
            Settings.CheckoutBranchAction = changes;
            Settings.UseDefaultCheckoutBranchAction = defaultActionChx.Checked;

            IWin32Window _owner = Visible ? this : Owner;

            if (changes == Settings.LocalChanges.Stash && Settings.Module.IsDirtyDir())
                GitUICommands.Instance.Stash(_owner);

            if (Remotebranch.Checked)
            {
                using (var checkoutRemote = new FormCheckoutRemoteBranch(Branches.Text, changes))
                    return checkoutRemote.ShowDialog(_owner);
            }
            else
            {
                var command = GitCommandHelpers.CheckoutCmd(Branches.Text, changes);
                var successfullyCheckedOut = FormProcess.ShowDialog(_owner, command);
                if (successfullyCheckedOut)
                    return DialogResult.OK;
                else
                    return DialogResult.None;
            }        
        }

        private void BranchTypeChanged()
        {
            Initialize();
        }

        private void LocalBranchCheckedChanged(object sender, EventArgs e)
        {
            BranchTypeChanged();
        }

        private void RemoteBranchCheckedChanged(object sender, EventArgs e)
        {
            BranchTypeChanged();
        }

        
    }
}