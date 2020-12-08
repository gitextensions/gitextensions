using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        /// <summary>
        /// We assume tree to position indices are 0-based and sequential. In case this
        /// is no longer true, because for e.g. user has reverted to an earlier version,
        /// this function will fix the indices, attempting to maintain the existing order.
        /// </summary>
        private void FixInvalidTreeToPositionIndices()
        {
            // Sort by index, then force assign 0-based sequential indices
            var treeToIndex = GetTreeToPositionIndex();

            int i = 0;
            foreach (var kvp in treeToIndex.OrderBy(kvp => kvp.Value))
            {
                treeToIndex[kvp.Key] = i;
                ++i;
            }

            SaveTreeToPositionIndex(treeToIndex);
        }

        private Dictionary<Tree, int> GetTreeToPositionIndex()
        {
            return new Dictionary<Tree, int>
            {
                [_branchesTree] = AppSettings.RepoObjectsTreeBranchesIndex,
                [_remotesTree] = AppSettings.RepoObjectsTreeRemotesIndex,
                [_tagTree] = AppSettings.RepoObjectsTreeTagsIndex,
                [_submoduleTree] = AppSettings.RepoObjectsTreeSubmodulesIndex
            };
        }

        private void SaveTreeToPositionIndex(Dictionary<Tree, int> treeToPositionIndex)
        {
            AppSettings.RepoObjectsTreeBranchesIndex = treeToPositionIndex[_branchesTree];
            AppSettings.RepoObjectsTreeRemotesIndex = treeToPositionIndex[_remotesTree];
            AppSettings.RepoObjectsTreeTagsIndex = treeToPositionIndex[_tagTree];
            AppSettings.RepoObjectsTreeSubmodulesIndex = treeToPositionIndex[_submoduleTree];
        }

        private void ReorderTreeNode(TreeNode node, bool up)
        {
            var tree = (Tree)node.Tag;
            var treeToIndex = GetTreeToPositionIndex();
            var indexToTree = treeToIndex.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            int currIndex = treeToIndex[tree];

            // Find next visible tree to swap with, if any
            int swapIndex = currIndex;
            do
            {
                swapIndex = up ? swapIndex - 1 : swapIndex + 1;

                // If there are no visible nodes to swap with, we're done
                if (swapIndex < 0 || swapIndex >= treeToIndex.Count())
                {
                    return;
                }
            }
            while (!indexToTree[swapIndex].TreeViewNode.IsVisible);

            var swapWithTree = indexToTree[swapIndex];

            // Swap indices
            treeToIndex[tree] = treeToIndex[swapWithTree];
            treeToIndex[swapWithTree] = currIndex;

            // Save new indices
            SaveTreeToPositionIndex(treeToIndex);

            // Remove all trees, then show enabled ones at new indices
            RemoveTree(_branchesTree);
            RemoveTree(_remotesTree);
            RemoveTree(_tagTree);
            RemoveTree(_submoduleTree);
            ShowEnabledTrees();
        }

        private void ShowEnabledTrees()
        {
            if (tsbShowBranches.Checked)
            {
                AddTree(_branchesTree);
            }

            if (tsbShowRemotes.Checked)
            {
                AddTree(_remotesTree);
            }

            if (tsbShowTags.Checked)
            {
                AddTree(_tagTree);
            }

            if (tsbShowSubmodules.Checked)
            {
                AddTree(_submoduleTree);
            }
        }

        private void tsbShowBranches_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowBranches = tsbShowBranches.Checked;
            _searchResult = null;
            if (tsbShowBranches.Checked)
            {
                AddTree(_branchesTree);
                _searchResult = null;
            }
            else
            {
                RemoveTree(_branchesTree);
            }
        }

        private void tsbShowRemotes_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowRemotes = tsbShowRemotes.Checked;
            _searchResult = null;
            if (tsbShowRemotes.Checked)
            {
                AddTree(_remotesTree);
                _searchResult = null;
            }
            else
            {
                RemoveTree(_remotesTree);
            }
        }

        private void tsbShowTags_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowTags = tsbShowTags.Checked;
            _searchResult = null;
            if (tsbShowTags.Checked)
            {
                AddTree(_tagTree);
                _searchResult = null;
            }
            else
            {
                RemoveTree(_tagTree);
            }
        }

        private void tsbShowSubmodules_Click(object sender, EventArgs e)
        {
            AppSettings.RepoObjectsTreeShowSubmodules = tsbShowSubmodules.Checked;
            _searchResult = null;
            if (tsbShowSubmodules.Checked)
            {
                AddTree(_submoduleTree);
                _searchResult = null;
            }
            else
            {
                RemoveTree(_submoduleTree);
            }
        }
    }
}
