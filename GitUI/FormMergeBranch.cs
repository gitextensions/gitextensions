using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormMergeBranch : Form
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
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Command executed \n" + GitCommands.GitCommands.MergeBranch(Branches.Text), "Merge");

            MergeConflictHandler.HandleMergeConflicts();
        }
    }
}
