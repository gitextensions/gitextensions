using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI.CommandsDialogs;
using ResourceManager;

namespace GitUI.UserControls
{
    /// <summary>Tree-like structure for a repo's objects.</summary>
    public partial class RepoObjectsTree : GitModuleControl
    {
        private readonly TranslationString showBranchOnly =
            new TranslationString("Filter the revision grid to show this branch only\nTo show all branches, right click the revision grid, select 'view' and then the 'show all branches'");

        public FilterBranchHelper FilterBranchHelper { private get; set; }

        List<Tree> rootNodes = new List<Tree>();
        /// <summary>Image key for a head branch.</summary>
        private SearchControl<string> txtBranchCriterion;
        private readonly HashSet<string> _branchCriterionAutoCompletionSrc = new HashSet<string>();

        public RepoObjectsTree()
        {
            InitializeComponent();
            InitiliazeSearchBox();
            treeMain.PreviewKeyDown += OnPreviewKeyDown;

            btnSearch.PreviewKeyDown += OnPreviewKeyDown;
            PreviewKeyDown += OnPreviewKeyDown;
            Translate();

            RegisterContextActions();

            treeMain.ShowNodeToolTips = true;
            treeMain.HideSelection = false;
            treeMain.NodeMouseClick += OnNodeClick;
            treeMain.NodeMouseDoubleClick += OnNodeDoubleClick;
            mnubtnFilterRemoteBranchInRevisionGrid.ToolTipText = showBranchOnly.Text;
            mnubtnFilterLocalBranchInRevisionGrid.ToolTipText = showBranchOnly.Text;
        }

        private void InitiliazeSearchBox()
        {
            txtBranchCriterion = new SearchControl<string>(SearchForBranch, i => { });
            txtBranchCriterion.OnTextEntered += () =>
            {
                OnBranchCriterionChanged(null, null);
                OnBtnSearchClicked(null, null);
            };
            //
            // txtBranchCriterion
            //
            this.txtBranchCriterion.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.txtBranchCriterion.Name = "txtBranchCritierion";
            this.txtBranchCriterion.TabIndex = 1;
            this.txtBranchCriterion.TextChanged += OnBranchCriterionChanged;
            this.txtBranchCriterion.KeyDown += txtBranchCriterion_KeyDown;
            this.branchSearchPanel.Controls.Add(txtBranchCriterion, 1, 0);

            txtBranchCriterion.PreviewKeyDown += OnPreviewKeyDown;
        }

        private IList<string> SearchForBranch(string arg)
        {
            return _branchCriterionAutoCompletionSrc
                .Where(r => r.IndexOf(arg, StringComparison.OrdinalIgnoreCase) != -1)
                .ToList();
        }

        private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.F3 || e.KeyCode == Keys.Enter)
            {
                OnBtnSearchClicked(null, null);
            }
        }

        protected override void OnUICommandsSourceChanged(object sender, IGitUICommandsSource newSource)
        {
            base.OnUICommandsSourceChanged(sender, newSource);

            CancelBackgroundTasks();

            var localBranchesRootNode = new TreeNode(Strings.branches.Text)
            {
                ImageKey = "LocalRepo.png",
            };
            localBranchesRootNode.SelectedImageKey = localBranchesRootNode.ImageKey;
            AddTree(new BranchTree(localBranchesRootNode, newSource));

            var remoteBranchesRootNode = new TreeNode(Strings.remotes.Text)
            {
                ImageKey = "RemoteRepo.png",
            };
            remoteBranchesRootNode.SelectedImageKey = remoteBranchesRootNode.ImageKey;
            _remoteTree = new RemoteBranchTree(remoteBranchesRootNode, newSource)
            {
                TreeViewNode = {ContextMenuStrip = menuRemotes}
            };
            AddTree(_remoteTree);

            if (showTagsToolStripMenuItem.Checked)
            {
                AddTags();
            }
        }

        private void AddBranchesToAutoCompletionSrc(List<string> branchPaths)
        {
            foreach (var branchFullPath in branchPaths)
            {
                AddBranchToAutoCompletionSrc(branchFullPath);
            }
        }
        private void AddBranchToAutoCompletionSrc(string branchFullPath)
        {
            var lastPart = branchFullPath
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .LastOrDefault();

            _branchCriterionAutoCompletionSrc.Add(branchFullPath);

            if(lastPart != null && lastPart != branchFullPath)
            {
                if (!_branchCriterionAutoCompletionSrc.Contains(lastPart))
                {
                    _branchCriterionAutoCompletionSrc.Add(lastPart);
                }
            }
        }

        void AddTree(Tree aTree)
        {
            aTree.OnBranchesAdded += AddBranchesToAutoCompletionSrc;
            aTree.TreeViewNode.SelectedImageKey = aTree.TreeViewNode.ImageKey;
            aTree.TreeViewNode.Tag = aTree;
            treeMain.Nodes.Add(aTree.TreeViewNode);
            rootNodes.Add(aTree);
        }

        private CancellationTokenSource _cancelledTokenSource;
        private TreeNode _tagTreeRootNode;
        private TagTree _tagTree;
        private RemoteBranchTree _remoteTree;
        private List<TreeNode> _searchResult;
        private bool _searchCriteriaChanged = false;
        private Task[] _tasks;

        private void CancelBackgroundTasks()
        {
            if (_cancelledTokenSource != null)
            {
                _cancelledTokenSource.Cancel();
                _cancelledTokenSource.Dispose();
                _cancelledTokenSource = null;
                if (_tasks != null)
                {
                    Task.WaitAll(_tasks);
                }
                _branchCriterionAutoCompletionSrc.Clear();
            }
            _cancelledTokenSource = new CancellationTokenSource();
        }

        /// <summary>Reloads the repo's objects tree.</summary>
        public void Reload()
        {
            // todo: task exception handling
            CancelBackgroundTasks();
            var token = _cancelledTokenSource.Token;
            _tasks = rootNodes.Select(r => r.ReloadTask(token)).ToArray();
            Task.Factory.ContinueWhenAll(_tasks,
                (t) =>
                {
                    if (!t.All(r => r.Status == TaskStatus.RanToCompletion))
                    {
                        return;
                    }
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    BeginInvoke(new Action(() =>
                    {
                        var autoCompletionSrc = new AutoCompleteStringCollection();
                        autoCompletionSrc.AddRange(
                            _branchCriterionAutoCompletionSrc.ToArray());
                    }));
                }, _cancelledTokenSource.Token);
            _tasks.ToList().ForEach(t => t.Start());
        }

        private void OnBtnSettingsClicked(object sender, EventArgs e)
        {
            btnSettings.ContextMenuStrip.Show(btnSettings, 0, btnSettings.Height);
        }

        private void showTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _searchResult = null;
            if (showTagsToolStripMenuItem.Checked)
            {
                AddTags();
                var task = rootNodes.Last().ReloadTask(_cancelledTokenSource.Token);
                task.Start(TaskScheduler.Default);
            }
            else
            {
                rootNodes.Remove(_tagTree);
                treeMain.Nodes.Remove(_tagTreeRootNode);
            }
        }

        private void AddTags()
        {
            _tagTreeRootNode = new TreeNode(Strings.tags.Text) {ImageKey = "tags.png"};
            _tagTreeRootNode.SelectedImageKey = _tagTreeRootNode.ImageKey;
            _tagTree = new TagTree(_tagTreeRootNode, UICommandsSource);
            AddTree(_tagTree);
            _searchResult = null;
        }

        private void OnBtnSearchClicked(object sender, EventArgs e)
        {
            txtBranchCriterion.CloseDropdown();
            if (_searchCriteriaChanged && _searchResult != null && _searchResult.Any())
            {
                _searchCriteriaChanged = false;
                foreach (var coloredNode in _searchResult)
                {
                    coloredNode.BackColor = SystemColors.Window;
                }
                _searchResult = null;
                if (txtBranchCriterion.Text.IsNullOrWhiteSpace())
                {
                    txtBranchCriterion.Focus();
                    return;
                }
            }
            if (_searchResult == null || !_searchResult.Any())
            {
                if (txtBranchCriterion.Text.IsNotNullOrWhitespace())
                {
                    _searchResult = SearchTree(txtBranchCriterion.Text, treeMain.Nodes);
                }
            }
            var node = GetNextSearchResult();
            if (node != null)
            {
                node.EnsureVisible();
                treeMain.SelectedNode = node;
            }
        }

        private static List<TreeNode> SearchTree(string text, TreeNodeCollection nodes)
        {
            var ret = new List<TreeNode>();
            foreach (TreeNode node in nodes)
            {
                var branch = node.Tag as BaseBranchNode;
                if (branch != null)
                {
                    if (branch.FullPath.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        AddTreeNodeToSearchResult(ret, node);
                    }
                }
                else
                {
                    if(node.Text.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        AddTreeNodeToSearchResult(ret, node);
                    }
                }
                ret.AddRange(SearchTree(text, node.Nodes));
            }
            return ret;
        }

        private static void AddTreeNodeToSearchResult(List<TreeNode> ret, TreeNode node)
        {
            node.BackColor = Color.LightYellow;
            ret.Add(node);
        }

        private TreeNode GetNextSearchResult()
        {
            if (_searchResult == null || !_searchResult.Any())
            {
                return null;
            }

            var node = _searchResult.First();
            _searchResult.RemoveAt(0);
            _searchResult.Add(node);
            return node;
        }

        private void OnBranchCriterionChanged(object sender, EventArgs e)
        {
            _searchCriteriaChanged = true;
        }

        private void txtBranchCriterion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OnBtnSearchClicked(null, null);
                e.Handled = true;
            }
        }

        /// <summary>Occurs when a <see cref="TreeNode"/> is selected.</summary>
        void OnNodeSelected(object sender, TreeViewEventArgs e)
        {
            Node.OnNode<Node>(e.Node, node => node.OnSelected());
        }

        /// <summary>Occurs when a <see cref="TreeNode"/> is clicked.</summary>
        void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeMain.SelectedNode = e.Node;
            Node.OnNode<Node>(e.Node, node => node.OnClick());
        }

        /// <summary>Occurs when a <see cref="TreeNode"/> is double-clicked.
        /// <remarks>Expand/Collapse still executes for any node with children.</remarks></summary>
        void OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // When folding/unfolding a node, e.Node won't be the one you double clicked, but a child node instead
            Node.OnNode<Node>(treeMain.SelectedNode, node => node.OnDoubleClick());
        }
    }
}