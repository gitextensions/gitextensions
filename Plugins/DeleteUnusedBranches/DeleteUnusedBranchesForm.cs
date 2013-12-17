using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using System.Text.RegularExpressions;

namespace DeleteUnusedBranches
{
    public sealed partial class DeleteUnusedBranchesForm : Form
    {
        private readonly SortableBranchesList branches = new SortableBranchesList();
        private int days;
        private string referenceBranch;
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
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            mergedIntoBranch.Text = referenceBranch;
            olderThanDays.Value = days;

            BranchesGrid.DataSource = branches;
            clearResults();
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
            return gitCommands.RunGitCmd("branch" + (IncludeRemoteBranches.Checked ? " -r" : "") + (includeUnmergedBranches.Checked ? "" : " --merged " + referenceBranch))
                .Split('\n')
                .Where(branchName => !string.IsNullOrEmpty(branchName))
                .Select(branchName => branchName.Trim('*', ' ', '\n', '\r'))
                .Where(branchName => branchName != "HEAD" &&
                                     branchName != referenceBranch &&
                                     (!IncludeRemoteBranches.Checked || branchName.StartsWith(remote.Text + "/")) &&
                                     (!useRegexFilter.Checked || Regex.IsMatch(branchName, regexFilter.Text)));
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure to delete the selected branches?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (IncludeRemoteBranches.Checked)
                {
                    if (MessageBox.Show(this, "DANGEROUS ACTION!" + Environment.NewLine + "Branches will be delete on the remote '" + remote.Text + "'. This can not be undone." + Environment.NewLine + "Are you sure you want to continue?", "Delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
                        return;
                }

                foreach (Branch branch in branches.Where(branch => branch.Delete))
                {
                    if (IncludeRemoteBranches.Checked && branch.Name.StartsWith(remote.Text + "/"))
                    {
                        branch.Result = gitCommands.RunGitCmd("push " + remote.Text + " :" + branch.Name.Substring((remote.Text + "/").Length)).Trim();
                    }
                    else
                    {
                        branch.Result = gitCommands.RunGitCmd("branch -d " + branch.Name).Trim();
                    }
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

        private void IncludeRemoteBranches_CheckedChanged(object sender, EventArgs e)
        {
            clearResults();
        }

        private void useRegexFilter_CheckedChanged(object sender, EventArgs e)
        {
            clearResults();
        }

        private void remote_TextChanged(object sender, EventArgs e)
        {
            clearResults();
        }

        private void regexFilter_TextChanged(object sender, EventArgs e)
        {
            clearResults();
        }

        private void mergedIntoBranch_TextChanged(object sender, EventArgs e)
        {
            referenceBranch = mergedIntoBranch.Text;
            clearResults();
        }

        private void includeUnmergedBranches_CheckedChanged(object sender, EventArgs e)
        {
            clearResults();

            if (includeUnmergedBranches.Checked)
                MessageBox.Show(this, "Deleting unmerged branches will result in dangling commits. Use with caution!", "Delete", MessageBoxButtons.OK);
        }

        private void olderThanDays_ValueChanged(object sender, EventArgs e)
        {
            days = (int)olderThanDays.Value;
            clearResults();
        }

        private void clearResults()
        {
            instructionLabel.Text = "Choose branches to delete. Only branches that are fully merged in '" + referenceBranch + "' will be deleted.";
            refreshHint.Text = "Press '" + Refresh.Text + "' to search for branches to delete.";
            branches.Clear();
            branches.ResetBindings();
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            refreshHint.Text = "Loading...";
            refreshHint.Update();
            branches.Clear();
            branches.AddRange(GetObsoleteBranches());
            branches.ResetBindings();
            refreshHint.Text = branches.Count(b => b.Delete).ToString() + "/" + branches.Count().ToString() + " branches selected.";
        }

    }
}
