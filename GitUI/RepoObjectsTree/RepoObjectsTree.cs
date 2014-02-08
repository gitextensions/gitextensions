using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.Notifications;
using GitUI.Properties;

namespace GitUI.UserControls
{
    /// <summary>Tree-like structure for a repo's objects.</summary>
    public partial class RepoObjectsTree : GitModuleControl
    {
        List<Tree> rootNodes = new List<Tree>();
        /// <summary>Image key for a head branch.</summary>
        static readonly string headBranchKey = Guid.NewGuid().ToString();
        public RevisionGrid RevisionGrid { private get; set; }

        public RepoObjectsTree()
        {
            InitializeComponent();

            Translate();

            RegisterContextActions();

            treeMain.ShowNodeToolTips = true;
            treeMain.HideSelection = false;
            treeMain.NodeMouseClick += OnNodeClick;
            treeMain.NodeMouseDoubleClick += OnNodeDoubleClick;
        }

        protected override void OnUICommandsSourceChanged(object sender, IGitUICommandsSource newSource)
        {
            base.OnUICommandsSourceChanged(sender, newSource);

            DragDrops();

            AddTree(new BranchTree(new TreeNode(Strings.branches.Text), newSource));

            /*            AddTreeSet(new TreeNode(Strings.stashes.Text)
                            {
                                ContextMenuStrip = menuStashes,
                                ImageKey = stashesKey
                            },
                           () => Module.GetStashes().Select(stash => new StashNode(stash, UICommands)).ToList(),
                           OnReloadStashes,
                           OnAddStash
                        );*/
            /*
            AddTreeSet(new TreeNode(Strings.remotes.Text)
                {
                    ContextMenuStrip = menuRemotes,
                    ImageKey = remotesKey
                },
                () => Module.GetRemotesInfo().Select(remote => new RemoteNode(remote, UICommands)).ToList(),
                OnReloadRemotes,
                OnAddRemote
            );
            */
        }

        void AddTree(Tree aTree)
        {
            aTree.TreeViewNode.SelectedImageKey = aTree.TreeViewNode.ImageKey;
            aTree.TreeViewNode.Tag = aTree;
            treeMain.Nodes.Add(aTree.TreeViewNode);
            rootNodes.Add(aTree);
        }

        private CancellationTokenSource _cancelledTokenSource;
        private void Cancel()
        {
            if (_cancelledTokenSource != null)
            {
                _cancelledTokenSource.Dispose();
                _cancelledTokenSource = null;
            }
        }

        /// <summary>Reloads the repo's objects tree.</summary>
        public void Reload()
        {
            // todo: task exception handling
            Cancel();
            _cancelledTokenSource = new CancellationTokenSource();
            Task previousTask = null;

            foreach (Tree rootNode in rootNodes)
            {
                Task task = rootNode.ReloadTask(_cancelledTokenSource.Token);
                if (previousTask == null)
                {
                    task.Start(TaskScheduler.Default);
                }
                else
                {
                    previousTask.ContinueWith((t) => task.Start(Task.Factory.Scheduler));
                }
            }
        }

        private void treeMain_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Nodes.Count > 0)
                {
                    e.Node.CheckAllChildNodes(e.Node.Checked);
                }
                e.Node.CheckParentNodes();
                List<TreeNode> nodes = new List<TreeNode>();
                treeMain.listNodes(nodes,rootNodes[0].TreeViewNode);
                ApplyTreeBranchFilter(nodes.ConvertAll<string>(n => (n.Tag as BranchNode).FullPath), true);
            }
        }


        public void ApplyTreeBranchFilter(List<string> checkedNodes, bool refresh)
        {
            bool success = RevisionGrid.SetAndApplyBranchFilter(checkedNodes.Join(' '));
            if (success && refresh)
                RevisionGrid.ForceRefreshRevisions();
        }
    }
}
