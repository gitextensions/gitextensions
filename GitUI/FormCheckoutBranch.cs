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
            : this(branch, remote, null, false)
        {
        }

        public FormCheckoutBranch(string branch, bool remote, string containRevison, bool showOptions)
            : this()
        {
            _containRevison = containRevison;

            LocalBranch.Checked = !remote;
            Remotebranch.Checked = remote;

            if (showOptions)
                lnkSettings_LinkClicked(null, null);

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
            Settings.LocalChanges changes = ChangesMode;
            Settings.CheckoutBranchAction = changes;
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