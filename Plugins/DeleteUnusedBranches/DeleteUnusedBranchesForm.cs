using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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

        public int Days { get; set; }

        public IGitCommands GitCommands { get; set; }

        public IList<Branch> Branches { get; set; }

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
