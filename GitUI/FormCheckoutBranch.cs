using System;
using System.Diagnostics;
using System.Linq;
using GitCommands;
using System.Collections.Generic;

namespace GitUI
{
    public partial class FormCheckoutBranch : GitExtensionsForm
    {
        private string _containRevison;

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

            Branches.Text = branch;

            LocalBranch.Checked = !remote;
            Remotebranch.Checked = remote;

            Initialize();

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
        }

        private void Initialize()
        {
            Branches.DisplayMember = "Name";

            if (_containRevison == null)
            {
                if (LocalBranch.Checked)
                {
                    Branches.DataSource = Settings.Module.GetHeads(false).Select(a => a.Name).ToList();
                }
                else
                {
                    var heads = Settings.Module.GetHeads(true, true);

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

        private void OkClick(object sender, EventArgs e)
        {
            LocalChanges changes;
            if (rbReset.Checked)
                changes = LocalChanges.Reset;
            else if (rbMerge.Checked)
                changes = LocalChanges.Merge;
            else
                changes = LocalChanges.DontChange;
            Settings.CheckoutBranchAction = (int)changes;
            if (Remotebranch.Checked)
            {
                using (var checkoutRemote = new FormCheckoutRemoteBranch(Branches.Text, changes))
                    checkoutRemote.ShowDialog(this);
            }
            else
            {
                try
                {
                    var command = GitCommandHelpers.CheckoutCmd(Branches.Text, changes);
                    var successfullyCheckedOut = FormProcess.ShowDialog(this, command);
                    if (successfullyCheckedOut)
                        Close();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
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

        private void lnkSettings_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            localChangesGB.Show();
            lnkSettings.Hide();
            Height += (localChangesGB.Height - lnkSettings.Height) / 2;
        }
    }
}