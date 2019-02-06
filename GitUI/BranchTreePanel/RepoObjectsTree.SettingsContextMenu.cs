using System;
using GitCommands;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        private void tsmiShowBranches_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowBranches = tsmiShowBranches.Checked;
            _searchResult = null;
            if (tsmiShowBranches.Checked)
            {
                AddBranches();
            }
            else
            {
                _rootNodes.Remove(_branchesTree);
                treeMain.Nodes.Remove(_branchesTreeRootNode);
            }
        }

        private void tsmiShowRemotes_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowRemotes = tsmiShowRemotes.Checked;
            _searchResult = null;
            if (tsmiShowRemotes.Checked)
            {
                AddRemotes();
            }
            else
            {
                _rootNodes.Remove(_remotesTree);
                treeMain.Nodes.Remove(_remotesTreeRootNode);
            }
        }

        private void tsmiShowTags_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowTags = tsmiShowTags.Checked;
            _searchResult = null;
            if (tsmiShowTags.Checked)
            {
                AddTags();
            }
            else
            {
                _rootNodes.Remove(_tagTree);
                treeMain.Nodes.Remove(_tagTreeRootNode);
            }
        }
    }
}