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
        List<RootNode> rootNodes = new List<RootNode>();

        public RepoObjectsTree()
        {
            InitializeComponent();

            GitUICommandsSourceSet += OnGitUICommandsSource;

            treeMain.ShowNodeToolTips = true;
            treeMain.NodeMouseClick += OnNodeClick;
            treeMain.NodeMouseDoubleClick += OnNodeDoubleClick;
        }

        void OnGitUICommandsSource(object sender, IGitUICommandsSource uicommandssource)
        {
            GitUICommandsSourceSet -= OnGitUICommandsSource;// only do this once
            DragDrops();

            //nodeTags = AddTreeNode("tags");
            //nodeStashes = AddTreeNode("stashes");

            AddTreeSet(new TreeNode(Strings.Instance.stashes.Text),
                () => Module.GetStashes().Select(stash => new StashNode(stash, UICommands)).ToList(),
                OnReloadStashes,
                OnAddStash
            );
            AddTreeSet(
                new TreeNode(Strings.branches.Text),
                () =>
                {
                    var branchNames = Module.GetBranchNames().ToArray();
                    return BranchesNode.GetBranchTree(UICommands, branchNames);
                },
                OnReloadBranches,
                OnAddBranchNode
                );
            //AddTreeSet(nodeTags, ...);

            RepoChanged();
        }

        void AddTreeSet<T>(
            TreeNode rootTreeNode,
            Func<ICollection<T>> getValues,
            Action<ICollection<T>, RootNode<T>> onReload,
            Func<TreeNodeCollection, T, TreeNode> itemToTreeNode)
            where T : Node
        {
            AddNode(new RootNode<T>(rootTreeNode, UICommands, getValues, onReload, itemToTreeNode));
        }

        void AddNode(RootNode rootNode)
        {
            treeMain.Nodes.Add(rootNode.TreeNode);
            rootNodes.Add(rootNode);
        }

        /// <summary>Sets up the objects tree for a new repo, then reloads the objects tree.</summary>
        public void RepoChanged()
        {
            foreach (RootNode rootNode in rootNodes)
            {
                rootNode.RepoChanged();
            }

            Reload();
        }

        /// <summary>Reloads the repo's objects tree.</summary>
        public void Reload()
        {
            // todo: async CancellationToken(s)
            // todo: task exception handling

            foreach (RootNode rootNode in rootNodes)
            {
                rootNode.ReloadAsync();
            }

            // update tree little by little OR after all data retrieved?

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
