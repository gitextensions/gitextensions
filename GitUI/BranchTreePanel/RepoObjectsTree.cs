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

        private NativeTreeViewDoubleClickDecorator _doubleClickDecorator;
        private NativeTreeViewExplorerNavigationDecorator _explorerNavigationDecorator;
        private readonly List<Tree> _rootNodes = new List<Tree>();
        private readonly SearchControl<string> _txtBranchCriterion;
        private BranchTree _branchesTree;
        private RemoteBranchTree _remotesTree;
        private TagTree _tagTree;
        private SubmoduleTree _submoduleTree;
        private List<TreeNode> _searchResult;
        private FilterBranchHelper _filterBranchHelper;
        private IAheadBehindDataProvider _aheadBehindDataProvider;
        private bool _searchCriteriaChanged;
        private IUserScriptMenuBuilder _userScriptMenuBuilder;

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

            toolTip.SetToolTip(btnCollapseAll, mnubtnCollapseAll.ToolTipText);
            toolTip.SetToolTip(btnSearch, _searchTooltip.Text);
            toolTip.SetToolTip(btnSettings, _showHideRefsTooltip.Text);
            tsmiShowBranches.Checked = AppSettings.RepoObjectsTreeShowBranches;
            tsmiShowRemotes.Checked = AppSettings.RepoObjectsTreeShowRemotes;
            tsmiShowTags.Checked = AppSettings.RepoObjectsTreeShowTags;
            tsmiShowSubmodules.Checked = AppSettings.RepoObjectsTreeShowSubmodules;

            _doubleClickDecorator = new NativeTreeViewDoubleClickDecorator(treeMain);
            _doubleClickDecorator.BeforeDoubleClickExpandCollapse += BeforeDoubleClickExpandCollapse;

            _explorerNavigationDecorator = new NativeTreeViewExplorerNavigationDecorator(treeMain);
            _explorerNavigationDecorator.AfterSelect += OnNodeSelected;

            treeMain.NodeMouseClick += OnNodeClick;
            treeMain.NodeMouseDoubleClick += OnNodeDoubleClick;

            mnubtnFilterRemoteBranchInRevisionGrid.ToolTipText = _showBranchOnly.Text;
            mnubtnFilterLocalBranchInRevisionGrid.ToolTipText = _showBranchOnly.Text;

            return;

            void InitImageList()
            {
                const int rowPadding = 1; // added to top and bottom, so doubled -- this value is scaled *after*, so consider 96dpi here

                treeMain.ImageList = new ImageList
                {
                    ImageSize = DpiUtil.Scale(new Size(16, 16 + rowPadding + rowPadding)), // Scale ImageSize and images scale automatically
                    Images =
                    {
                        { nameof(Images.ArrowUp), Pad(Images.ArrowUp) },
                        { nameof(Images.ArrowDown), Pad(Images.ArrowDown) },
                        { nameof(Images.FolderClosed), Pad(Images.FolderClosed) },
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
                        { nameof(Images.EyeOpened), Pad(Images.EyeOpened) },
                        { nameof(Images.EyeClosed), Pad(Images.EyeClosed) },
                        { nameof(Images.RemoteEnableAndFetch), Pad(Images.RemoteEnableAndFetch) },
                        { nameof(Images.FileStatusModified), Pad(Images.FileStatusModified) },
                        { nameof(Images.FolderSubmodule), Pad(Images.FolderSubmodule) },
                        { nameof(Images.SubmoduleDirty), Pad(Images.SubmoduleDirty) },
                        { nameof(Images.SubmoduleRevisionUp), Pad(Images.SubmoduleRevisionUp) },
                        { nameof(Images.SubmoduleRevisionDown), Pad(Images.SubmoduleRevisionDown) },
                        { nameof(Images.SubmoduleRevisionSemiUp), Pad(Images.SubmoduleRevisionSemiUp) },
                        { nameof(Images.SubmoduleRevisionSemiDown), Pad(Images.SubmoduleRevisionSemiDown) },
                        { nameof(Images.SubmoduleRevisionUpDirty), Pad(Images.SubmoduleRevisionUpDirty) },
                        { nameof(Images.SubmoduleRevisionDownDirty), Pad(Images.SubmoduleRevisionDownDirty) },
                        { nameof(Images.SubmoduleRevisionSemiUpDirty), Pad(Images.SubmoduleRevisionSemiUpDirty) },
                        { nameof(Images.SubmoduleRevisionSemiDownDirty), Pad(Images.SubmoduleRevisionSemiDownDirty) },
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

        public void Initialize([CanBeNull]IAheadBehindDataProvider aheadBehindDataProvider, FilterBranchHelper filterBranchHelper, IUserScriptMenuBuilder userScriptMenuBuilder)
        {
            _aheadBehindDataProvider = aheadBehindDataProvider;
            _filterBranchHelper = filterBranchHelper;
            _userScriptMenuBuilder = userScriptMenuBuilder;

            // This lazily sets the command source, invoking OnUICommandsSourceSet, which is required for setting up
            // notifications for each Tree.
            _ = UICommandsSource;
        }

        protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
            base.OnUICommandsSourceSet(source);

            CreateBranches();
            CreateRemotes();
            CreateTags();
            CreateSubmodules();

            FixInvalidTreeToPositionIndices();
            ShowEnabledTrees();
            RebuildMenuSettings();
        }

        private static void AddTreeNodeToSearchResult(ICollection<TreeNode> ret, TreeNode node)
        {
            node.BackColor = Color.LightYellow;
            ret.Add(node);
        }

        private void CreateBranches()
        {
            var rootNode = new TreeNode(Strings.Branches)
            {
                Name = Strings.Branches,
                ImageKey = nameof(Images.BranchLocalRoot),
                SelectedImageKey = nameof(Images.BranchLocalRoot),
            };
            _branchesTree = new BranchTree(rootNode, UICommandsSource, _aheadBehindDataProvider);
        }

        private void CreateRemotes()
        {
            var rootNode = new TreeNode(Strings.Remotes)
            {
                Name = Strings.Remotes,
                ImageKey = nameof(Images.BranchRemoteRoot),
                SelectedImageKey = nameof(Images.BranchRemoteRoot),
            };
            _remotesTree = new RemoteBranchTree(rootNode, UICommandsSource)
            {
                TreeViewNode =
                {
                    ContextMenuStrip = menuRemotes
                }
            };
        }

        private void CreateTags()
        {
            var rootNode = new TreeNode(Strings.Tags)
            {
                Name = Strings.Tags,
                ImageKey = nameof(Images.TagHorizontal),
                SelectedImageKey = nameof(Images.TagHorizontal),
            };
            _tagTree = new TagTree(rootNode, UICommandsSource);
        }

        private void CreateSubmodules()
        {
            var rootNode = new TreeNode(Strings.Submodules)
            {
                Name = Strings.Submodules,
                ImageKey = nameof(Images.FolderSubmodule),
                SelectedImageKey = nameof(Images.FolderSubmodule),
            };
            _submoduleTree = new SubmoduleTree(rootNode, UICommandsSource);
        }

        private void AddTree(Tree tree)
        {
            tree.TreeViewNode.SelectedImageKey = tree.TreeViewNode.ImageKey;
            tree.TreeViewNode.Tag = tree;

            // Add Tree's node in position index order. Because TreeNodeCollections cannot be sorted,
            // we create a list from it, sort it, then clear and re-add the nodes back to the collection.
            treeMain.BeginUpdate();
            List<TreeNode> nodeList = treeMain.Nodes.OfType<TreeNode>().ToList();
            nodeList.Add(tree.TreeViewNode);
            treeMain.Nodes.Clear();
            var treeToPositionIndex = GetTreeToPositionIndex();
            treeMain.Nodes.AddRange(nodeList.OrderBy(treeNode => treeToPositionIndex[treeNode.Tag as Tree]).ToArray());
            treeMain.EndUpdate();

            treeMain.Font = AppSettings.Font;
            _rootNodes.Add(tree);

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await tree.AttachedAsync();
            }).FileAndForget();
        }

        private void RemoveTree(Tree tree)
        {
            tree.Detached();
            _rootNodes.Remove(tree);
            treeMain.Nodes.Remove(tree.TreeViewNode);
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

        private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                OnBtnSearchClicked(null, null);
            }
        }

        private void OnNodeSelected(object sender, TreeViewEventArgs e)
        {
            Node.OnNode<Node>(e.Node, node => node.OnSelected());
        }

        private void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
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
            }

            public NativeTreeView TreeView => _repoObjectsTree.treeMain;

            public void ReorderTreeNode(TreeNode node, bool up)
            {
                _repoObjectsTree.ReorderTreeNode(node, up);
            }

            public void SetTreeVisibleByIndex(int index, bool visible)
            {
                var tree = _repoObjectsTree.GetTreeToPositionIndex().FirstOrDefault(kvp => kvp.Value == index).Key;
                if (tree.TreeViewNode.IsVisible == visible)
                {
                    return;
                }

                if (visible)
                {
                    _repoObjectsTree.AddTree(tree);
                }
                else
                {
                    _repoObjectsTree.RemoveTree(tree);
                }
            }
        }
    }
}