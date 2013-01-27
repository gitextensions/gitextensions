using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    // "stashes"
    public partial class RepoObjectsTree
    {
        ListWatcher<GitStash> _stashesWatcher;

        Task LoadStashes()
        {
            return _stashesWatcher.CheckUpdateAsync();
        }

        void ResetStashes(ICollection<GitStash> stashes)
        {//
            nodeStashes.Value.Nodes.Clear();
            nodeStashes.Value.Text = string.Format("stashes ({0})", stashes.Count);

            foreach (GitStash stash in stashes)
            {
                AddStash(nodeStashes.Value.Nodes, stash);
            }
        }

        void AddStash(TreeNodeCollection nodes, GitStash stash)
        {
            TreeNode treeNode = nodes.Add(stash.Index.ToString(), stash.Name);
            treeNode.Tag = stash;
            treeNode.ToolTipText = stash.Message;
            ApplyStashStyle(treeNode);
        }

        void ApplyStashStyle(TreeNode treeNode)
        {
            ApplyTreeNodeStyle(treeNode);
            // style
        }

        /// <summary>Readies stashes for a new repo.</summary>
        void NewRepoStashes()
        {
            _stashesWatcher = new ListWatcher<GitStash>(() => git.GetStashes(), ResetStashes);
        }
    }
}
