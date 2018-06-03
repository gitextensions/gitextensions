using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree : GitModuleControl
    {
        private readonly TranslationString _showBranchOnly =
            new TranslationString("Filter the revision grid to show this branch only\nTo show all branches, right click the revision grid, select 'view' and then the 'show all branches'");

        private readonly List<Tree> _rootNodes = new List<Tree>();
        private SearchControl<string> _txtBranchCriterion;
        private readonly CancellationTokenSequence _reloadCancellation = new CancellationTokenSequence();
        private CancellationToken _currentToken;
        private TreeNode _tagTreeRootNode;
        private TagTree _tagTree;
        private RemoteBranchTree _remoteTree;
        private List<TreeNode> _searchResult;
        private FilterBranchHelper _filterBranchHelper;
        private bool _searchCriteriaChanged;

        public RepoObjectsTree()
        {
            _currentToken = _reloadCancellation.Next();
            InitializeComponent();
            InitImageList();
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
            mnubtnFilterRemoteBranchInRevisionGrid.ToolTipText = _showBranchOnly.Text;
            mnubtnFilterLocalBranchInRevisionGrid.ToolTipText = _showBranchOnly.Text;
        }

        public void SetBranchFilterer(FilterBranchHelper filterBranchHelper)
        {
            _filterBranchHelper = filterBranchHelper;
        }

        public async Task ReloadAsync()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var token = CancelBackgroundTasks();
            Enabled = false;

            var currentBranch = Module.GetSelectedBranch();
            try
            {
                var tasks = _rootNodes.Select(r => r.ReloadAsync(token)).ToArray();
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                await this.SwitchToMainThreadAsync(token);
                Enabled = true;

                var selectedNode = treeMain.AllNodes().FirstOrDefault(n =>
                    _rootNodes.Any(rn => $"{rn.TreeViewNode.FullPath}{treeMain.PathSeparator}{currentBranch}" == n.FullPath));
                if (selectedNode != null)
                {
                    treeMain.SelectedNode = selectedNode;
                    treeMain.SelectedNode.EnsureVisible();
                }
            }
        }

        protected override void OnUICommandsSourceChanged(IGitUICommandsSource newSource)
        {
            base.OnUICommandsSourceChanged(newSource);

            CancelBackgroundTasks();

            var localBranchesRootNode = new TreeNode(Strings.BranchesText.Text)
            {
                ImageKey = nameof(MsVsImages.Repository_16x),
            };
            localBranchesRootNode.SelectedImageKey = localBranchesRootNode.ImageKey;
            AddTree(new BranchTree(localBranchesRootNode, newSource));

            var remoteBranchesRootNode = new TreeNode(Strings.RemotesText.Text)
            {
                ImageKey = nameof(MsVsImages.Repository_16x),
            };
            remoteBranchesRootNode.SelectedImageKey = remoteBranchesRootNode.ImageKey;
            _remoteTree = new RemoteBranchTree(remoteBranchesRootNode, newSource)
            {
                TreeViewNode =
                {
                    ContextMenuStrip = menuRemotes
                }
            };
            AddTree(_remoteTree);

            if (showTagsToolStripMenuItem.Checked)
            {
                AddTags();
            }
        }

        private static void AddTreeNodeToSearchResult(ICollection<TreeNode> ret, TreeNode node)
        {
            node.BackColor = Color.LightYellow;
            ret.Add(node);
        }

        private void AddTags()
        {
            _tagTreeRootNode = new TreeNode(Strings.TagsText.Text) { ImageKey = nameof(MsVsImages.Tag_16x) };
            _tagTreeRootNode.SelectedImageKey = _tagTreeRootNode.ImageKey;
            _tagTree = new TagTree(_tagTreeRootNode, UICommandsSource);
            AddTree(_tagTree);
            _searchResult = null;
        }

        private void AddTree(Tree tree)
        {
            tree.TreeViewNode.SelectedImageKey = tree.TreeViewNode.ImageKey;
            tree.TreeViewNode.Tag = tree;
            treeMain.Nodes.Add(tree.TreeViewNode);
            _rootNodes.Add(tree);
        }

        private CancellationToken CancelBackgroundTasks()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _currentToken = _reloadCancellation.Next();

            return _currentToken;
        }

        private IEnumerable<string> CollectFilterCandidates()
        {
            var list = new List<string>();

            foreach (TreeNode rootNode in treeMain.Nodes)
            {
                CollectFromNodes(rootNode.Nodes);
            }

            return list;

            void CollectFromNodes(TreeNodeCollection nodes)
            {
                foreach (TreeNode node in nodes)
                {
                    if (node.Tag is BaseBranchNode branch)
                    {
                        if (branch.Nodes.Count == 0)
                        {
                            list.Add(branch.FullPath);
                        }
                    }
                    else
                    {
                        list.Add(node.Text);
                    }

                    CollectFromNodes(node.Nodes);
                }
            }
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

        private void InitImageList()
        {
            treeMain.ImageList = new ImageList
            {
                ImageSize = DpiUtil.Scale(new Size(16, 16)), // Scale ImageSize and images scale automatically
                Images =
                {
                    { nameof(MsVsImages.Branch_16x), MsVsImages.Branch_16x },
                    { nameof(MsVsImages.Repository_16x), MsVsImages.Repository_16x },
                    { nameof(MsVsImages.BranchRemote_16x), MsVsImages.BranchRemote_16x },
                    { nameof(MsVsImages.Folder_grey_16x), MsVsImages.Folder_grey_16x },
                    { nameof(MsVsImages.Tag_16x), MsVsImages.Tag_16x },
                }
            };
            treeMain.ImageKey = nameof(MsVsImages.Branch_16x);
            treeMain.SelectedImageKey = treeMain.ImageKey;
        }

        private void InitiliazeSearchBox()
        {
            _txtBranchCriterion = new SearchControl<string>(SearchForBranch, i => { });
            _txtBranchCriterion.OnTextEntered += () =>
            {
                OnBranchCriterionChanged(null, null);
                OnBtnSearchClicked(null, null);
            };
            _txtBranchCriterion.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _txtBranchCriterion.Name = "txtBranchCritierion";
            _txtBranchCriterion.TabIndex = 1;
            _txtBranchCriterion.TextChanged += OnBranchCriterionChanged;
            _txtBranchCriterion.KeyDown += TxtBranchCriterion_KeyDown;
            branchSearchPanel.Controls.Add(_txtBranchCriterion, 1, 0);

            _txtBranchCriterion.PreviewKeyDown += OnPreviewKeyDown;
        }

        private IEnumerable<string> SearchForBranch(string arg)
        {
            return CollectFilterCandidates()
                .Where(r => r.IndexOf(arg, StringComparison.OrdinalIgnoreCase) != -1);
        }

        private static List<TreeNode> SearchTree(string text, TreeNodeCollection nodes)
        {
            var ret = new List<TreeNode>();
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is BaseBranchNode branch)
                {
                    if (branch.FullPath.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        AddTreeNodeToSearchResult(ret, node);
                    }
                }
                else
                {
                    if (node.Text.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        AddTreeNodeToSearchResult(ret, node);
                    }
                }

                ret.AddRange(SearchTree(text, node.Nodes));
            }

            return ret;
        }

        private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.F3 || e.KeyCode == Keys.Enter)
            {
                OnBtnSearchClicked(null, null);
            }
        }

        private void OnBtnSettingsClicked(object sender, EventArgs e)
        {
            btnSettings.ContextMenuStrip.Show(btnSettings, 0, btnSettings.Height);
        }

        private void ShowTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _searchResult = null;
            if (showTagsToolStripMenuItem.Checked)
            {
                AddTags();
                ThreadHelper.JoinableTaskFactory.RunAsync(() => _rootNodes.Last().ReloadAsync(_currentToken)).FileAndForget();
            }
            else
            {
                _rootNodes.Remove(_tagTree);
                treeMain.Nodes.Remove(_tagTreeRootNode);
            }
        }

        private void OnBtnSearchClicked(object sender, EventArgs e)
        {
            _txtBranchCriterion.CloseDropdown();
            if (_searchCriteriaChanged && _searchResult != null && _searchResult.Any())
            {
                _searchCriteriaChanged = false;
                foreach (var coloredNode in _searchResult)
                {
                    coloredNode.BackColor = SystemColors.Window;
                }

                _searchResult = null;
                if (_txtBranchCriterion.Text.IsNullOrWhiteSpace())
                {
                    _txtBranchCriterion.Focus();
                    return;
                }
            }

            if (_searchResult == null || !_searchResult.Any())
            {
                if (_txtBranchCriterion.Text.IsNotNullOrWhitespace())
                {
                    _searchResult = SearchTree(_txtBranchCriterion.Text, treeMain.Nodes);
                }
            }

            var node = GetNextSearchResult();
            if (node == null)
            {
                return;
            }

            node.EnsureVisible();
            treeMain.SelectedNode = node;
        }

        private void OnBranchCriterionChanged(object sender, EventArgs e)
        {
            _searchCriteriaChanged = true;
        }

        private void TxtBranchCriterion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            OnBtnSearchClicked(null, null);
            e.Handled = true;
        }

        private void OnNodeSelected(object sender, TreeViewEventArgs e)
        {
            Node.OnNode<Node>(e.Node, node => node.OnSelected());
        }

        private void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeMain.SelectedNode = e.Node;
            Node.OnNode<Node>(e.Node, node => node.OnClick());
        }

        private void OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Don't use e.Node, when folding/unfolding a node,
            // e.Node won't be the one you double clicked, but a child node instead
            Node.OnNode<Node>(treeMain.SelectedNode, node => node.OnDoubleClick());
        }
    }
}