using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteBranch : GitModuleForm
    {
        private readonly TranslationString _deleteBranchCaption = new TranslationString("Delete branches");
        private readonly TranslationString _deleteBranchQuestion = new TranslationString(
            "Are you sure you want to delete selected branches?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");
        private readonly TranslationString _deleteUnmergedBranchForcingSuggestion =
            new TranslationString("You cannot delete unmerged branch until you set “force delete” mode.");
        private readonly TranslationString _cannotDeleteCurrentBranchMessage =
            new TranslationString("Cannot delete the branch “{0}” which you are currently on.");

        private readonly IEnumerable<string> _defaultBranches;
        private readonly HashSet<string> _mergedBranches = new HashSet<string>();
        private string _currentBranch;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormDeleteBranch()
        {
            InitializeComponent();
        }

        public FormDeleteBranch(GitUICommands commands, IEnumerable<string> defaultBranches)
            : base(commands)
        {
            _defaultBranches = defaultBranches;

            InitializeComponent();
            InitializeComplete();
        }

        private void FormDeleteBranchLoad(object sender, EventArgs e)
        {
            Branches.BranchesToSelect = Module.GetRefs(true, true).Where(h => h.IsHead && !h.IsRemote).ToList();
            foreach (var branch in Module.GetMergedBranches())
            {
                if (!branch.StartsWith("* "))
                {
                    _mergedBranches.Add(branch.Trim());
                }
                else if (!branch.StartsWith("* ") || !DetachedHeadParser.IsDetachedHead(branch.Substring(2)))
                {
                    _currentBranch = branch.Trim('*', ' ');
                }
            }

            if (_defaultBranches != null)
            {
                Branches.SetSelectedText(_defaultBranches.Join(" "));
            }
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var selectedBranches = Branches.GetSelectedBranches().ToArray();

                if (!selectedBranches.Any())
                {
                    return;
                }

                if (_currentBranch != null && selectedBranches.Any(branch => branch.Name == _currentBranch))
                {
                    MessageBox.Show(this, string.Format(_cannotDeleteCurrentBranchMessage.Text, _currentBranch), _deleteBranchCaption.Text);
                    return;
                }

                // always treat branches as unmerged if there is no current branch (HEAD is detached)
                var hasUnmergedBranches = _currentBranch == null || selectedBranches.Any(branch => !_mergedBranches.Contains(branch.Name));

                // we could show yes/no dialog and set forcing checkbox automatically, but more safe way is asking user to do it himself
                if (hasUnmergedBranches && !ForceDelete.Checked)
                {
                    MessageBox.Show(this, _deleteUnmergedBranchForcingSuggestion.Text, _deleteBranchCaption.Text);
                    return;
                }

                // ask for confirmation to delete unmerged branch that may cause loosing commits
                // (actually we could check if there are another branches pointing to that commit)
                if (hasUnmergedBranches
                    && MessageBox.Show(this, _deleteBranchQuestion.Text, _deleteBranchCaption.Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }

                var cmd = new GitDeleteBranchCmd(selectedBranches, ForceDelete.Checked);
                UICommands.StartCommandLineProcessDialog(this, cmd);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            Close();
        }
    }
}