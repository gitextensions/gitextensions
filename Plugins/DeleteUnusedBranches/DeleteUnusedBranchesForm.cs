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
		private readonly string referenceBranch;
        private readonly IGitModule gitCommands;
        private readonly IGitUICommands _gitUICommands;
        private readonly IGitPlugin _gitPlugin;

        public DeleteUnusedBranchesForm(int days, string referenceBranch, IGitModule gitCommands, IGitUICommands gitUICommands, IGitPlugin gitPlugin)
        {
            InitializeComponent();

			this.referenceBranch = referenceBranch;
            this.days = days;
            this.gitCommands = gitCommands;
            _gitUICommands = gitUICommands;
            _gitPlugin = gitPlugin;
            instructionLabel.Text = "Choose branches to delete. Only branches that are fully merged in '" + referenceBranch + "' will be deleted.";
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
                foreach (string dateString in gitCommands.RunGitCmd(string.Concat("log --pretty=%ci ", branchName, "^1..", branchName)).Split('\n'))
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
            // TODO: skip current branch
			return gitCommands.RunGitCmd("branch --merged " + referenceBranch)
                .Split('\n')
                .Where(branchName => !string.IsNullOrEmpty(branchName))
                .Select(branchName => branchName.Trim('*', ' ', '\n', '\r'))
                .Where(branchName => branchName != "HEAD" && 
									 branchName != referenceBranch);
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure to delete the selected branches?" + Environment.NewLine + "Only branches that are fully merged in '" + referenceBranch + "' will be deleted.", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (Branch branch in branches.Where(branch => branch.Delete))
                {
                    branch.Result = gitCommands.RunGitCmd("branch -d " + branch.Name).Trim();
                }
                BranchesGrid.Refresh();
            }
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            Hide();
            Close();
            _gitUICommands.StartSettingsDialog(_gitPlugin);
        }
    }
}
