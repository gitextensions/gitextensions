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
        TreeNode nodeTags;
        TreeNode nodeBranches;
        TreeNode nodeStashes;

        List<RepoObjectsTreeSet> treeSets = new List<RepoObjectsTreeSet>();

        public RepoObjectsTree()
        {
            InitializeComponent();

            treeMain.ShowNodeToolTips = true;
            treeMain.NodeMouseClick += OnNodeClick;
            treeMain.NodeMouseDoubleClick += OnNodeDoubleClick;
            DragDrops();

            nodeBranches = AddTreeNode(Strings.branches.Text);
            //nodeTags = AddTreeNode("tags");
            //nodeStashes = AddTreeNode("stashes");

            AddTreeSet(nodeStashes, () =>
            {
                var stashes = Module.GetStashes();
                return new RootNode<GitStash>(UICommands, null, stashes);
            }, ReloadStashes, AddStash, ApplyStashStyle);
            AddTreeSet(
                nodeBranches,
                () =>
                {
                    var branchNames = Module.GetBranchNames().ToArray();
                    return BaseBranchNode.GetBranchTree(UICommands, branchNames);
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
            Func<RootNode<T>> getValues,
            Action<ICollection<T>> onReset,
            Func<TreeNodeCollection, T, TreeNode> itemToTreeNode,
            Action<TreeNode> applyStyle)
        {
            treeSets.Add(new RepoObjectsTreeSet<T>(rootNode, getValues, onReset, itemToTreeNode, applyStyle));
        }

        TreeNode AddTreeNode(string node)
        {
            return treeMain.Nodes.Add(node);
        }

        /// <summary>Sets up the objects tree for a new repo, then reloads the objects tree.</summary>
        public void RepoChanged()
        {
            foreach (RepoObjectsTreeSet treeSet in treeSets)
            {
                treeSet.RepoChanged();
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
