using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using System.Collections.Generic;

namespace GitUI
{
    public partial class FormCheckoutBranch : GitExtensionsForm
    {
        private readonly TranslationString trackRemoteBranch = new TranslationString("You choose to checkout a remote branch." + Environment.NewLine + Environment.NewLine + "Do you want create a local branch with the name '{0}'" + Environment.NewLine + "that track's this remote branch?");
        private readonly TranslationString trackRemoteBranchCaption = new TranslationString("Checkout branch");

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
                Branches.DataSource = GitCommandHelpers.GetHeads(false);
            }
            else
            {
                var heads = GitCommandHelpers.GetHeads(true, true);

                var remoteHeads = new List<GitHead>();

                foreach (var head in heads)
                {
                    if (head.IsRemote && !head.IsTag)
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
                var command = "checkout";
                if (Remotebranch.Checked)
                {
                    //Get a localbranch name
                    var remoteName = GitCommandHelpers.GetRemoteName(Branches.Text, GitCommandHelpers.GetRemotes());
                    var localBranchName = Branches.Text.Substring(remoteName.Length + 1);

                    MessageBoxIcon icon = MessageBoxIcon.Question;

                    //try to determine the 'best' name for a local branch, check if the local
                    //name for the remote branch is already used
                    if (LocalBranchExists(localBranchName))
                    {
                        localBranchName = string.Concat(remoteName, "_", localBranchName);
                        icon = MessageBoxIcon.Exclamation;
                    }

                    var result = MessageBox.Show(string.Format(trackRemoteBranch.Text, localBranchName), trackRemoteBranchCaption.Text, MessageBoxButtons.YesNoCancel, icon);

                    if (result == DialogResult.Cancel)
                        return;

                    if (result == DialogResult.Yes)
                        command += string.Format(" -b {0}", localBranchName);
                }

                if (Force.Checked)
                    command += " --force";
                command += " \"" + Branches.Text + "\"";
                var form = new FormProcess(command);
                form.ShowDialog();
                if (!form.ErrorOccurred())
                    Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private static bool LocalBranchExists(string name)
        {
            foreach (GitHead head in GitCommandHelpers.GetHeads(false))
            {
                if (head.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
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