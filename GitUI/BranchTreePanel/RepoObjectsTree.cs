using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.UserControls;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree : GitModuleControl
    {
        private readonly TranslationString _showBranchOnly =
            new TranslationString("Filter the revision grid to show this branch only\nTo show all branches, right click the revision grid, select 'view' and then the 'show all branches'");
        private readonly TranslationString _searchTooltip = new TranslationString("Search");
        private readonly TranslationString _showHideRefsTooltip = new TranslationString("Show/hide branches/remotes/tags");

        private readonly Dictionary<Tree, int> _treeToPositionIndex = new Dictionary<Tree, int>();
        private NativeTreeViewDoubleClickDecorator _doubleClickDecorator;
        private readonly List<Tree> _rootNodes = new List<Tree>();
        private readonly SearchControl<string> _txtBranchCriterion;
        private TreeNode _branchesTreeRootNode;
        private TreeNode _remotesTreeRootNode;
        private TreeNode _tagTreeRootNode;
        private BranchTree _branchesTree;
        private RemoteBranchTree _remotesTree;
        private TagTree _tagTree;
        private List<TreeNode> _searchResult;
        private FilterBranchHelper _filterBranchHelper;
        private IAheadBehindDataProvider _aheadBehindDataProvider;
        private bool _searchCriteriaChanged;

        public RepoObjectsTree()
        {
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

            toolTip.SetToolTip(btnCollapseAll, mnubtnCollapseAll.ToolTipText);
            toolTip.SetToolTip(btnSearch, _searchTooltip.Text);
            toolTip.SetToolTip(btnSettings, _showHideRefsTooltip.Text);
            tsmiShowBranches.Checked = AppSettings.RepoObjectsTreeShowBranches;
            tsmiShowRemotes.Checked = AppSettings.RepoObjectsTreeShowRemotes;
            tsmiShowTags.Checked = AppSettings.RepoObjectsTreeShowTags;

            _doubleClickDecorator = new NativeTreeViewDoubleClickDecorator(treeMain);
            _doubleClickDecorator.BeforeDoubleClickExpandCollapse += BeforeDoubleClickExpandCollapse;

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
                        { nameof(Images.Remote), Pad(Images.Remote) },
                        { nameof(Images.BitBucket), Pad(Images.BitBucket) },
                        { nameof(Images.GitHub), Pad(Images.GitHub) },
                        { nameof(Images.VisualStudioTeamServices), Pad(Images.VisualStudioTeamServices) },
                        { nameof(Images.BranchLocalRoot), Pad(Images.BranchLocalRoot) },
                        { nameof(Images.BranchRemoteRoot), Pad(Images.BranchRemoteRoot) },
                        { nameof(Images.BranchRemote), Pad(Images.BranchRemote) },
                        { nameof(Images.BranchFolder), Pad(Images.BranchFolder) },
                        { nameof(Images.TagHorizontal), Pad(Images.TagHorizontal) },
                        { nameof(Images.FolderClosed), Pad(Images.FolderClosed) },
                        { nameof(Images.EyeOpened), Pad(Images.EyeOpened) },
                        { nameof(Images.EyeClosed), Pad(Images.EyeClosed) },
                        { nameof(Images.RemoteEnableAndFetch), Pad(Images.RemoteEnableAndFetch) },
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

        private void BeforeDoubleClickExpandCollapse(object sender, CancelEventArgs e)
        {
            var node = treeMain.SelectedNode?.Tag as Node;

            // If node is an inner node, and overrides OnDoubleClick, then disable expand/collapse
            if (node != null
                && node.Nodes.Count > 0
                && IsOverride(node.GetType().GetMethod("OnDoubleClick", BindingFlags.Instance | BindingFlags.NonPublic)))
            {
                e.Cancel = true;
            }

            return;

            bool IsOverride(MethodInfo m)
            {
                return m != null && m.GetBaseDefinition().DeclaringType != m.DeclaringType;
            }
        }

        public void Initialize([CanBeNull]IAheadBehindDataProvider aheadBehindDataProvider, FilterBranchHelper filterBranchHelper)
        {
            _aheadBehindDataProvider = aheadBehindDataProvider;
            _filterBranchHelper = filterBranchHelper;

            // This lazily sets the command source, invoking OnUICommandsSourceSet, which is required for setting up
            // notifications for each Tree.
            _ = UICommandsSource;
        }

        public void RefreshTree()
        {
            foreach (var n in _rootNodes)
            {
                n.RefreshTree();
            }
        }

        protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
            base.OnUICommandsSourceSet(source);

            if (tsmiShowBranches.Checked)
            {
                AddBranches();
            }

            if (tsmiShowRemotes.Checked)
            {
                AddRemotes();
            }

            if (tsmiShowTags.Checked)
            {
                AddTags();
            }
        }

        private static void AddTreeNodeToSearchResult(ICollection<TreeNode> ret, TreeNode node)
        {
            node.BackColor = Color.LightYellow;
            ret.Add(node);
        }

        private void AddBranches()
        {
            _branchesTreeRootNode = new TreeNode(Strings.Branches)
            {
                ImageKey = nameof(Images.BranchLocalRoot),
                SelectedImageKey = nameof(Images.BranchLocalRoot),
            };
            _branchesTree = new BranchTree(_branchesTreeRootNode, UICommandsSource, _aheadBehindDataProvider);
            AddTree(_branchesTree, 0);
            _searchResult = null;
        }

        private void AddRemotes()
        {
            _remotesTreeRootNode = new TreeNode(Strings.Remotes)
            {
                ImageKey = nameof(Images.BranchRemoteRoot),
                SelectedImageKey = nameof(Images.BranchRemoteRoot),
            };
            _remotesTree = new RemoteBranchTree(_remotesTreeRootNode, UICommandsSource)
            {
                TreeViewNode =
                {
                    ContextMenuStrip = menuRemotes
                }
            };
            AddTree(_remotesTree, 1);
            _searchResult = null;
        }

        private void AddTags()
        {
            _tagTreeRootNode = new TreeNode(Strings.Tags)
            {
                ImageKey = nameof(Images.TagHorizontal),
                SelectedImageKey = nameof(Images.TagHorizontal),
            };
            _tagTree = new TagTree(_tagTreeRootNode, UICommandsSource);
            AddTree(_tagTree, 2);
            _searchResult = null;
        }

        private void AddTree(Tree tree, int positionIndex)
        {
            tree.TreeViewNode.SelectedImageKey = tree.TreeViewNode.ImageKey;
            tree.TreeViewNode.Tag = tree;

            // Remember current Tree's position index
            _treeToPositionIndex[tree] = positionIndex;

            // Add Tree's node in position index order. Because TreeNodeCollections cannot be sorted,
            // we create a list from it, sort it, then clear and re-add the nodes back to the collection.
            treeMain.BeginUpdate();
            List<TreeNode> nodeList = treeMain.Nodes.OfType<TreeNode>().ToList();
            nodeList.Add(tree.TreeViewNode);
            treeMain.Nodes.Clear();
            treeMain.Nodes.AddRange(nodeList.OrderBy(treeNode => _treeToPositionIndex[treeNode.Tag as Tree]).ToArray());
            treeMain.EndUpdate();

            treeMain.Font = AppSettings.Font;
            _rootNodes.Add(tree);
            tree.RefreshTree();
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

        private void OnBtnSearchClicked(object sender, EventArgs e)
        {
            DoSearch();
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            treeMain.CollapseAll();
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
            // Don't consider-double clicking on the PlusMinus as a double-click event
            // for nodes in tree. This prevents opening inner submodules, for example,
            // when quickly collapsing/expanding them.
            var hitTest = treeMain.HitTest(e.Location);
            if (hitTest.Location == TreeViewHitTestLocations.PlusMinus)
            {
                return;
            }

            // Don't use e.Node, when folding/unfolding a node,
            // e.Node won't be the one you double clicked, but a child node instead
            Node.OnNode<Node>(treeMain.SelectedNode, node => node.OnDoubleClick());
        }

        internal TestAccessor GetTestAccessor()
            => new TestAccessor(this);

        public readonly struct TestAccessor
        {
            private readonly RepoObjectsTree _repoObjectsTree;

            public TestAccessor(RepoObjectsTree repoObjectsTree)
            {
                _repoObjectsTree = repoObjectsTree;
                TreeView = repoObjectsTree.treeMain;
            }

            public NativeTreeView TreeView { get; }
        }
    }
}