using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormCheckoutBranch : GitExtensionsForm
    {
        public FormCheckoutBranch()
        {
            InitializeComponent();

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
                List<GitHead> heads = GitCommands.GitCommands.GetHeads(true, true);

                List<GitHead> remoteHeads = new List<GitHead>();

                foreach (GitHead head in heads)
                {
                    if (head.IsRemote)
                        remoteHeads.Add(head);
                }

                Branches.DataSource = remoteHeads;
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                FormProcess form;

                //Get a localbranch name
                int index = Branches.Text.LastIndexOfAny(new char[] { '\\', '/' });
                string localBranchName = Branches.Text;
                if (index > 0 && index + 1 < Branches.Text.Length)
                    localBranchName = localBranchName.Substring(index + 1);

                if (Remotebranch.Checked &&
                    MessageBox.Show("You choose to checkout a remote branch." + Environment.NewLine + Environment.NewLine + "Do you want create a local branch with the name '" + localBranchName + "'" + Environment.NewLine + "that track's this remote branch?", "Checkout branch", MessageBoxButtons.YesNo) == DialogResult.Yes
                    )
                {
                    //git checkout --track -b localbranch origin/remotebranch 
                    form = new FormProcess("checkout --track -b \"" + localBranchName + "\" \"" + Branches.Text + "\"");
                }
                else
                {
                    form = new FormProcess("checkout \"" + Branches.Text + "\"");
                }

                if (!form.ErrorOccured())
                    Close();
            }
            catch
            {
            }
        }

        private void FormCheckoutBranck_Load(object sender, EventArgs e)
        {

        }

        private void Branches_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void BranchTypeChanged()
        {
            Initialize();
        }

        private void Remotebranch_CheckedChanged(object sender, EventArgs e)
        {
            BranchTypeChanged();
        }

        private void LocalBranch_CheckedChanged(object sender, EventArgs e)
        {
            BranchTypeChanged();
        }
    }
}
