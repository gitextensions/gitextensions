using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
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
        private readonly SearchControl<string> _txtBranchCriterion;
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
            _txtBranchCriterion = CreateSearchBox();
            branchSearchPanel.Controls.Add(_txtBranchCriterion, 1, 0);

            treeMain.PreviewKeyDown += OnPreviewKeyDown;
            btnSearch.PreviewKeyDown += OnPreviewKeyDown;
            PreviewKeyDown += OnPreviewKeyDown;
            InitializeComplete();

            RegisterContextActions();

            treeMain.ShowNodeToolTips = true;
            treeMain.HideSelection = false;
            treeMain.NodeMouseClick += OnNodeClick;
            treeMain.NodeMouseDoubleClick += OnNodeDoubleClick;
            mnubtnFilterRemoteBranchInRevisionGrid.ToolTipText = _showBranchOnly.Text;
            mnubtnFilterLocalBranchInRevisionGrid.ToolTipText = _showBranchOnly.Text;

            void InitImageList()
            {
                const int rowPadding = 1; // added to top and bottom, so doubled -- this value is scaled *after*, so consider 96dpi here

                treeMain.ImageList = new ImageList
                {
                    ImageSize = DpiUtil.Scale(new Size(16, 16 + rowPadding + rowPadding)), // Scale ImageSize and images scale automatically
                    Images =
                    {
                        { nameof(Images.BranchDocument), Pad(Images.BranchDocument) },
                        { nameof(Images.Branch), Pad(Images.Branch) },
                        { nameof(Images.Remote), Pad(Images.RemoteRepo) },
                        { nameof(Images.BitBucket), Pad(Images.BitBucket) },
                        { nameof(Images.GitHub), Pad(Images.GitHub) },
                        { nameof(Images.VisualStudioTeamServices), Pad(Images.VisualStudioTeamServices) },
                        { nameof(Images.BranchLocalRoot), Pad(Images.BranchLocalRoot) },
                        { nameof(Images.BranchRemoteRoot), Pad(Images.BranchRemoteRoot) },
                        { nameof(Images.BranchRemote), Pad(Images.BranchRemote) },
                        { nameof(Images.BranchFolder), Pad(Images.BranchFolder) },
                        { nameof(Images.TagHorizontal), Pad(Images.TagHorizontal) },
                    }
                };
                treeMain.SelectedImageKey = treeMain.ImageKey;

                Image Pad(Image image)
                {
                    var padded = new Bitmap(image.Width, image.Height + rowPadding + rowPadding, PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(padded))
                    {
                        g.DrawImageUnscaled(image, 0, rowPadding);
                        return padded;
                    }
                }
            }

            SearchControl<string> CreateSearchBox()
            {
                var search = new SearchControl<string>(SearchForBranch, i => { })
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Name = "txtBranchCritierion",
                    TabIndex = 1
                };
                search.OnTextEntered += () =>
                {
                    OnBranchCriterionChanged(null, null);
                    OnBtnSearchClicked(null, null);
                };
                search.TextChanged += OnBranchCriterionChanged;
                search.KeyDown += TxtBranchCriterion_KeyDown;
                search.PreviewKeyDown += OnPreviewKeyDown;
                return search;

                IEnumerable<string> SearchForBranch(string arg)
                {
                    return CollectFilterCandidates()
                        .Where(r => r.IndexOf(arg, StringComparison.OrdinalIgnoreCase) != -1);
                }

                IEnumerable<string> CollectFilterCandidates()
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
            }
        }

        public void SetBranchFilterer(FilterBranchHelper filterBranchHelper)
        {
            _filterBranchHelper = filterBranchHelper;
        }

        public async Task ReloadAsync()
        {
            var currentBranch = Module.GetSelectedBranch();
            await this.SwitchToMainThreadAsync();

            var token = CancelBackgroundTasks();
            Enabled = false;

            try
            {
                treeMain.BeginUpdate();
                _rootNodes.ForEach(t => t.IgnoreSelectionChangedEvent = true);
                var tasks = _rootNodes.Select(r => r.ReloadAsync(token)).ToArray();
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                await this.SwitchToMainThreadAsync();
                if (!token.IsCancellationRequested)
                {
                    Enabled = true;
                    var selectedNode = treeMain.AllNodes().FirstOrDefault(n =>
                        _rootNodes.Any(rn => $"{rn.TreeViewNode.FullPath}{treeMain.PathSeparator}{currentBranch}" == n.FullPath));
                    if (selectedNode != null)
                    {
                        treeMain.SelectedNode = selectedNode;
                        treeMain.SelectedNode.EnsureVisible();
                    }

                    _rootNodes.ForEach(t => t.IgnoreSelectionChangedEvent = false);
                }

                treeMain.EndUpdate();
            }
        }

        protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
            base.OnUICommandsSourceSet(source);

            CancelBackgroundTasks();

            var localBranchesRootNode = new TreeNode(Strings.Branches)
            {
                ImageKey = nameof(Images.BranchLocalRoot),
                SelectedImageKey = nameof(Images.BranchLocalRoot),
            };
            AddTree(new BranchTree(localBranchesRootNode, source));

            var remoteBranchesRootNode = new TreeNode(Strings.Remotes)
            {
                ImageKey = nameof(Images.BranchRemoteRoot),
                SelectedImageKey = nameof(Images.BranchRemoteRoot),
            };
            _remoteTree = new RemoteBranchTree(remoteBranchesRootNode, source)
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
            _tagTreeRootNode = new TreeNode(Strings.Tags) { ImageKey = nameof(Images.TagHorizontal) };
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
            treeMain.Font = AppSettings.Font;
            _rootNodes.Add(tree);
        }

        private CancellationToken CancelBackgroundTasks()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _currentToken = _reloadCancellation.Next();

            return _currentToken;
        }

        private void DoSearch()
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
                    _searchResult = SearchTree(_txtBranchCriterion.Text, treeMain.Nodes.Cast<TreeNode>());
                }
            }

            var node = GetNextSearchResult();

            if (node == null)
            {
                return;
            }

            node.EnsureVisible();
            treeMain.SelectedNode = node;

            return;

            TreeNode GetNextSearchResult()
            {
                var first = _searchResult?.FirstOrDefault();

                if (first == null)
                {
                    return null;
                }

                _searchResult.RemoveAt(0);
                _searchResult.Add(first);
                return first;
            }

            List<TreeNode> SearchTree(string text, IEnumerable<TreeNode> nodes)
            {
                var queue = new Queue<TreeNode>(nodes);
                var ret = new List<TreeNode>();

                while (queue.Count != 0)
                {
                    var n = queue.Dequeue();

                    if (n.Tag is BaseBranchNode branch)
                    {
                        if (branch.FullPath.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1)
                        {
                            AddTreeNodeToSearchResult(ret, n);
                        }
                    }
                    else
                    {
                        if (n.Text.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1)
                        {
                            AddTreeNodeToSearchResult(ret, n);
                        }
                    }

                    foreach (TreeNode subNode in n.Nodes)
                    {
                        queue.Enqueue(subNode);
                    }
                }

                return ret;
            }
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
            DoSearch();
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