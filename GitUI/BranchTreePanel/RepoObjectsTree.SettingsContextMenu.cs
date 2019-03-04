using System;
using GitCommands;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        private void ShowEnabledTrees()
        {
            if (tsmiShowBranches.Checked)
            {
                AddTree(_branchesTree, 0);
            }

            if (tsmiShowRemotes.Checked)
            {
                AddTree(_remotesTree, 1);
            }

            if (tsmiShowTags.Checked)
            {
                AddTree(_tagTree, 2);
            }

            if (tsmiShowSubmodules.Checked)
            {
                AddTree(_submoduleTree, 3);
            }
        }

        private void tsmiShowBranches_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowBranches = tsmiShowBranches.Checked;
            _searchResult = null;
            if (tsmiShowBranches.Checked)
            {
                AddTree(_branchesTree, 0);
                _searchResult = null;
            }
            else
            {
                RemoveTree(_branchesTree);
            }
        }

        private void tsmiShowRemotes_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowRemotes = tsmiShowRemotes.Checked;
            _searchResult = null;
            if (tsmiShowRemotes.Checked)
            {
                AddTree(_remotesTree, 1);
                _searchResult = null;
            }
            else
            {
                RemoveTree(_remotesTree);
            }
        }

        private void tsmiShowTags_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowTags = tsmiShowTags.Checked;
            _searchResult = null;
            if (tsmiShowTags.Checked)
            {
                AddTree(_tagTree, 2);
                _searchResult = null;
            }
            else
            {
                RemoveTree(_tagTree);
            }
        }

        private void tsmiShowSubmodules_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowSubmodules = tsmiShowSubmodules.Checked;
            _searchResult = null;
            if (tsmiShowSubmodules.Checked)
            {
                AddTree(_submoduleTree, 3);
                _searchResult = null;
            }
            else
            {
                RemoveTree(_submoduleTree);
            }
        }
    }
}