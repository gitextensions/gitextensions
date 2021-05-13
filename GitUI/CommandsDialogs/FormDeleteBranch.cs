using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormDeleteBranch : GitModuleForm
    {
        private readonly TranslationString _deleteBranchCaption = new("Delete branches");
        private readonly TranslationString _deleteBranchQuestion = new(
            "Are you sure you want to delete selected branches?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");
        private readonly TranslationString _deleteUnmergedBranchForcingSuggestion =
            new("You cannot delete unmerged branch until you set “force delete” mode.");
        private readonly TranslationString _cannotDeleteCurrentBranchMessage =
            new("Cannot delete the branch “{0}” which you are currently on.");

        private readonly IEnumerable<string> _defaultBranches;
        private readonly HashSet<string> _mergedBranches = new();
        private string? _currentBranch;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormDeleteBranch()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
            Branches.BranchesToSelect = Module.GetRefs(RefsFilter.Heads).ToList();
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

            if (_defaultBranches is not null)
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

                if (_currentBranch is not null && selectedBranches.Any(branch => branch.Name == _currentBranch))
                {
                    MessageBox.Show(this, string.Format(_cannotDeleteCurrentBranchMessage.Text, _currentBranch), _deleteBranchCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // always treat branches as unmerged if there is no current branch (HEAD is detached)
                var hasUnmergedBranches = _currentBranch is null || selectedBranches.Any(branch => !_mergedBranches.Contains(branch.Name));

                // we could show yes/no dialog and set forcing checkbox automatically, but more safe way is asking user to do it himself
                if (hasUnmergedBranches && !ForceDelete.Checked)
                {
                    MessageBox.Show(this, _deleteUnmergedBranchForcingSuggestion.Text, _deleteBranchCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ask for confirmation to delete unmerged branch that may cause loosing commits
                // (actually we could check if there are another branches pointing to that commit)
                if (hasUnmergedBranches
                    && MessageBox.Show(this, _deleteBranchQuestion.Text, _deleteBranchCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                GitDeleteBranchCmd cmd = new(selectedBranches, ForceDelete.Checked);
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
