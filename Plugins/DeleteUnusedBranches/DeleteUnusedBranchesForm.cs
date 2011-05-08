using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace DeleteUnusedBranches
{
    public sealed partial class DeleteUnusedBranchesForm : Form
    {
        private readonly SortableBranchesList branches = new SortableBranchesList();
        private readonly int days;
        private readonly IGitCommands gitCommands;

        public DeleteUnusedBranchesForm(int days, IGitCommands gitCommands)
        {
            InitializeComponent();

            this.days = days;
            this.gitCommands = gitCommands;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            branches.AddRange(GetObsoleteBranches());
            BranchesGrid.DataSource = branches;
        }

        private IEnumerable<Branch> GetObsoleteBranches()
        {
            foreach (string branchName in GetObsoleteBranchNames())
            {
                DateTime date = new DateTime();
                foreach (string dateString in gitCommands.RunGit(string.Concat("log --pretty=%ci ", branchName, "^1..", branchName)).Split('\n'))
                {
                    DateTime singleDate;
                    if (DateTime.TryParse(dateString, out singleDate))
                        if (singleDate > date)
                            date = singleDate;

                }
                yield return new Branch(branchName, date, date < DateTime.Now.AddDays(-days));
            }
        }

        private IEnumerable<string> GetObsoleteBranchNames()
        {
            return gitCommands.RunGit("branch --merged")
                .Split('\n')
                .Where(branchName => !string.IsNullOrEmpty(branchName))
                .Select(branchName => branchName.Trim('*', ' ', '\n', '\r'))
                .Where(branchName => branchName != "master");
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete the selected branches?" + Environment.NewLine + "Only branches that are fully merged will be deleted.", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (Branch branch in branches.Where(branch => branch.Delete))
                {
                    branch.Result = gitCommands.RunGit("branch -d " + branch.Name).Trim();
                }
                BranchesGrid.Refresh();
            }
        }
    }
}
