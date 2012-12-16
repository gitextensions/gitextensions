using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    /// <summary>Tree-like structure for a repo's objects.</summary>
    public partial class RepoObjectsTree : UserControl
    {
        GitModule git;
        GitUICommands uiCommands;

        Lazy<TreeNode> nodeTags;
        Lazy<TreeNode> nodeBranches;

        TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public RepoObjectsTree()
        {
            InitializeComponent();

            nodeBranches = GetNodeLazy("branches");
            nodeTags = GetNodeLazy("tags");
        }

        Lazy<TreeNode> GetNodeLazy(string node)
        {
            return new Lazy<TreeNode>(() => treeMain.Nodes.Find(node, false)[0]);
        }

        /// <summary>Reloads the repo's objects tree.</summary>
        public void Reload(GitModule git, GitUICommands uiCommands)
        {
            this.git = git;
            this.uiCommands = uiCommands;

            #region Branches ----------------------

            // todo: async CancellationToken(s)
            // todo: task exception handling
            var taskBranches = Task.Factory.
                            StartNew(() => git.GetBranchNames()).
                            ContinueWith(getBranchNames => GetBranchTree(getBranchNames.Result)).
                            ContinueWith(
                                getBranchesTree => treeMain.Update(() => ResetBranchNodes(getBranchesTree.Result)),
                                uiScheduler
                            );

            #endregion Branches ----------------------

            // update tree little by little OR when all data retrieved?

            //Task.Factory.ContinueWhenAll(
            //    new[] { taskBranches },
            //    tasks => treeMain.EndUpdate(),
            //    new CancellationToken(),
            //    TaskContinuationOptions.NotOnCanceled,
            //    uiScheduler);
        }

        /// <summary>Applies the style to the specified <see cref="TreeNode"/>.
        /// <remarks>Should be invoked from a more specific style.</remarks></summary>
        void ApplyTreeNodeStyle(TreeNode node)
        {
            node.NodeFont = Settings.Font;
            // ...
        }

        void ExpandAll_Click(object sender, EventArgs e)
        {
            treeMain.ExpandAll();
        }

        void CollapseAll_Click(object sender, EventArgs e)
        {
            treeMain.CollapseAll();
        }

        void OnNodeSelected(object sender, TreeViewEventArgs e)
        {

        }

        /// <summary>Performed on a <see cref="TreeNode"/> double-click.
        /// <remarks>Expand/Collapse still executes for any node with children.</remarks></summary>
        void OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.IsAncestorOf(nodeBranches.Value))
            {// branches/
                if (node.HasNoChildren())
                {// no children -> branch
                    // needs to go into Settings, but would probably like an option to:
                    // stash; checkout;
                    uiCommands.StartCheckoutBranchDialog(base.ParentForm, node.Text, false);
                }

            }
        }


    }
}
