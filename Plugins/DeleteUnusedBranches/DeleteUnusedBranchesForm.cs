using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace DeleteUnusedBranches
{
    public partial class DeleteUnusedBranchesForm : Form
    {
        public DeleteUnusedBranchesForm(int days, IGitCommands gitCommands)
        {
            InitializeComponent();

            Days = days;
            GitCommands = gitCommands;
            Branches = new List<Branch>();
        }

        public int Days { get; private set; }

        public IGitCommands GitCommands { get; private set; }

        public IList<Branch> Branches { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach (string reference in GitCommands.RunGit("branch").Split('\n'))
            {
                if (string.IsNullOrEmpty(reference)) continue;

                string branchName = reference.Trim('*', ' ', '\n', '\r');

                DateTime date = new DateTime();
                foreach (string dateString in GitCommands.RunGit(string.Concat("log --pretty=%ci ", branchName, "^1..", branchName)).Split('\n'))
                {
                    DateTime singleDate;
                    if (DateTime.TryParse(dateString, out singleDate))
                        if (singleDate > date)
                            date = singleDate;

                }
                Branches.Add(new Branch(branchName, date, date < (DateTime.Now.AddDays(-Days))));
            }

            BranchesGrid.DataSource = Branches;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete the selected branches?" + Environment.NewLine + "Only branches that are not fully merged will be deleted.", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (Branch branch in Branches)
                {
                    if (branch.Delete)
                    {
                        branch.Result = GitCommands.RunGit(string.Concat("branch -d " + branch.Name)).Trim();
                        BranchesGrid.Refresh();
                    }
                }
            }
        }
    }
}
