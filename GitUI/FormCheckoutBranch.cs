using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormCheckoutBranch : GitExtensionsForm
    {
        public FormCheckoutBranch()
        {
            InitializeComponent();
            Translate();

            Initialize();
        }

        private void Initialize()
        {
            Branches.DisplayMember = "Name";

            if (LocalBranch.Checked)
            {
                Branches.DataSource = GitCommands.GitCommands.GetHeads(false);
            }
            else
            {
                var heads = GitCommands.GitCommands.GetHeads(true, true);
                var remoteHeads = new List<GitHead>();

                foreach (var head in heads)
                {
                    if (head.IsRemote)
                        remoteHeads.Add(head);
                }

                Branches.DataSource = remoteHeads;
            }

            Branches.Text = null;
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                //Get a localbranch name
                var remoteName = GitCommands.GitCommands.GetRemoteName(Branches.Text, GitCommands.GitCommands.GetRemotes());
                var localBranchName = Branches.Text.Substring(remoteName.Length + 1);
                
                var command = "checkout";
                if (Remotebranch.Checked)
                {
                    var result =
                        MessageBox.Show(
                            "You choose to checkout a remote branch." + Environment.NewLine + Environment.NewLine +
                            "Do you want create a local branch with the name '" + localBranchName + "'" +
                            Environment.NewLine + "that track's this remote branch?", "Checkout branch",
                            MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                        command += string.Format(" -b {0}", localBranchName);
                }

                if (Force.Checked)
                    command += " --force";
                command += " \"" + Branches.Text + "\"";
                var form = new FormProcess(command);
                form.ShowDialog();
                if (!form.ErrorOccured())
                    Close();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
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