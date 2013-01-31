using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    /// <summary>Tree-like structure for a repo's objects.</summary>
    public partial class RepoObjectsTree : GitModuleControl
    {
        /// <summary>the <see cref="GitModule"/></summary>
        GitModule git;
        GitUICommands uiCommands;

        TreeNode nodeTags;
        TreeNode nodeBranches;
        TreeNode nodeStashes;

        List<RepoObjectsTreeSet> treeSets = new List<RepoObjectsTreeSet>();

        public RepoObjectsTree()
        {
            InitializeComponent();

            treeMain.ShowNodeToolTips = true;
            DragDrops();

            nodeBranches = AddTreeNode(Strings.branches.Text);
            //nodeTags = AddTreeNode("tags");
            //nodeStashes = AddTreeNode("stashes");

            //AddTreeSet(nodeStashes, (git) => git.GetStashes(), ReloadStashes, AddStash, ApplyStashStyle);
            AddTreeSet(
                nodeBranches,
                git =>
                {
                    var branchNames = git.GetBranchNames().ToArray();
                    return BranchNode.GetBranchTree(branchNames);
                },
                ReloadBranchNodes,
                AddBranchNode,
                null // taken care of in AddBranchNode
                );
            //AddTreeSet(nodeTags, ...);

            foreach (TreeNode node in treeMain.Nodes)
            {
                ApplyTreeNodeStyle(node);
            }
        }

        void AddTreeSet<T>(
            TreeNode rootNode,
            Func<GitModule, ICollection<T>> getValues,
            Action<ICollection<T>> onReset,
            Func<TreeNodeCollection, T, TreeNode> itemToTreeNode,
            Action<TreeNode> applyStyle)
        {
            AddTreeSet(new RepoObjectsTreeSet<T>(git, rootNode, getValues, onReset, itemToTreeNode, applyStyle));
        }

        void AddTreeSet(RepoObjectsTreeSet treeSet)
        {
            treeSets.Add(treeSet);
        }

        TreeNode AddTreeNode(string node)
        {
            return treeMain.Nodes.Add(node);
        }

        /// <summary>Sets up the objects tree for a new repo, then reloads the objects tree.</summary>
        public void NewRepo(GitModule git, GitUICommands uiCommands)
        {
            this.git = git;
            this.uiCommands = uiCommands;

            foreach (RepoObjectsTreeSet treeSet in treeSets)
            {
                treeSet.NewRepo(git);
            }

            Reload();
        }

        /// <summary>Reloads the repo's objects tree.</summary>
        public void Reload()
        {
            // todo: async CancellationToken(s)
            // todo: task exception handling

            foreach (RepoObjectsTreeSet treeSet in treeSets)
            {
                treeSet.ReloadAsync();
            }

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
        static void ApplyTreeNodeStyle(TreeNode node)
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
    }
}
