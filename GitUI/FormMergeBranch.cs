using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormMergeBranch : GitExtensionsForm
    {
        public FormMergeBranch()
        {
            InitializeComponent();
        }

        private void FormMergeBranch_Load(object sender, EventArgs e)
        {
            string selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            Currentbranch.Text = "Current branch: " + selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(true, true);
            Branches.Select();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            FormProcess process = new FormProcess(GitCommands.GitCommands.MergeBranchCmd(Branches.Text));

            MergeConflictHandler.HandleMergeConflicts();

            if (!process.ErrorOccured())
                Close();
        }
    }
}
