using System.ComponentModel;
using System.Drawing.Imaging;
using System.Reflection;
using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUI.Script;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    public sealed partial class RepoObjectsTree : GitModuleControl
    {
        public const string HotkeySettingsName = "LeftPanel";

        private readonly CancellationTokenSequence _selectionCancellationTokenSequence = new();
        private readonly TranslationString _searchTooltip = new("Search");

        private NativeTreeViewDoubleClickDecorator _doubleClickDecorator;
        private NativeTreeViewExplorerNavigationDecorator _explorerNavigationDecorator;
        private readonly List<Tree> _rootNodes = new();
        private readonly SearchControl<string> _txtBranchCriterion;
        private LocalBranchTree _branchesTree;
        private RemoteBranchTree _remotesTree;
        private TagTree _tagTree;
        private StashTree _stashTree;
        private SubmoduleTree _submoduleTree;
        private List<TreeNode>? _searchResult;
        private Action<string?> _filterRevisionGridBySpaceSeparatedRefs;
        private IAheadBehindDataProvider? _aheadBehindDataProvider;
        private bool _searchCriteriaChanged;
        private ICheckRefs _refsSource;
        private IScriptHostControl _scriptHost;
        private IRunScript _scriptRunner;

        public RepoObjectsTree()
        {
            Disposed += (s, e) => _selectionCancellationTokenSequence.Dispose();

            InitializeComponent();
            InitImageList();
            _txtBranchCriterion = CreateSearchBox();
            branchSearchPanel.Controls.Add(_txtBranchCriterion, 1, 0);

            mnubtnCollapse.AdaptImageLightness();
            tsbCollapseAll.AdaptImageLightness();
            mnubtnExpand.AdaptImageLightness();
            mnubtnFetchAllBranchesFromARemote.AdaptImageLightness();
            mnuBtnPruneAllBranchesFromARemote.AdaptImageLightness();
            mnuBtnFetchAllRemotes.AdaptImageLightness();
            mnuBtnPruneAllRemotes.AdaptImageLightness();
            mnubtnFetchCreateBranch.AdaptImageLightness();
            mnubtnPullFromRemoteBranch.AdaptImageLightness();
            InitializeComplete();

            ReloadHotkeys();
            HotkeysEnabled = true;

            RegisterContextActions();

            treeMain.ShowNodeToolTips = true;

            toolTip.SetToolTip(btnSearch, _searchTooltip.Text);
            tsbCollapseAll.ToolTipText = mnubtnCollapse.ToolTipText;

            tsbShowBranches.Checked = AppSettings.RepoObjectsTreeShowBranches;
            tsbShowRemotes.Checked = AppSettings.RepoObjectsTreeShowRemotes;
            tsbShowTags.Checked = AppSettings.RepoObjectsTreeShowTags;
            tsbShowSubmodules.Checked = AppSettings.RepoObjectsTreeShowSubmodules;
            tsbShowStashes.Checked = AppSettings.RepoObjectsTreeShowStashes;

            _doubleClickDecorator = new NativeTreeViewDoubleClickDecorator(treeMain);
            _doubleClickDecorator.BeforeDoubleClickExpandCollapse += BeforeDoubleClickExpandCollapse;

            _explorerNavigationDecorator = new NativeTreeViewExplorerNavigationDecorator(treeMain);
            _explorerNavigationDecorator.AfterSelect += OnNodeSelected;

            treeMain.NodeMouseClick += OnNodeClick;
            treeMain.NodeMouseDoubleClick += OnNodeDoubleClick;

            return;

            void InitImageList()
            {
                const int rowPadding = 1; // added to top and bottom, so doubled -- this value is scaled *after*, so consider 96dpi here

                treeMain.ImageList = new ImageList
                {
                    ColorDepth = ColorDepth.Depth32Bit,
                    ImageSize = DpiUtil.Scale(new Size(16, 16 + rowPadding + rowPadding)), // Scale ImageSize and images scale automatically
                    Images =
                    {
                        { nameof(Images.ArrowUp), Pad(Images.ArrowUp) },
                        { nameof(Images.ArrowDown), Pad(Images.ArrowDown) },
                        { nameof(Images.FolderClosed), Pad(Images.FolderClosed) },
                        { nameof(Images.BranchLocal), Pad(Images.BranchLocal) },
                        { nameof(Images.BranchLocalMerged), Pad(Images.BranchLocalMerged) },
                        { nameof(Images.Branch), Pad(Images.Branch.AdaptLightness()) },
                        { nameof(Images.Remote), Pad(Images.Remote) },
                        { nameof(Images.BitBucket), Pad(Images.BitBucket) },
                        { nameof(Images.GitHub), Pad(Images.GitHub) },
                        { nameof(Images.VisualStudioTeamServices), Pad(Images.VisualStudioTeamServices) },
                        { nameof(Images.BranchLocalRoot), Pad(Images.BranchLocalRoot) },
                        { nameof(Images.BranchRemoteRoot), Pad(Images.BranchRemoteRoot) },
                        { nameof(Images.BranchRemote), Pad(Images.BranchRemote) },
                        { nameof(Images.BranchRemoteMerged), Pad(Images.BranchRemoteMerged) },
                        { nameof(Images.BranchFolder), Pad(Images.BranchFolder) },
                        { nameof(Images.TagHorizontal), Pad(Images.TagHorizontal) },
                        { nameof(Images.EyeOpened), Pad(Images.EyeOpened) },
                        { nameof(Images.EyeClosed), Pad(Images.EyeClosed) },
                        { nameof(Images.RemoteEnableAndFetch), Pad(Images.RemoteEnableAndFetch) },
                        { nameof(Images.FileStatusModified), Pad(Images.FileStatusModified) },
                        { nameof(Images.FolderSubmodule), Pad(Images.FolderSubmodule) },
                        { nameof(Images.Stash), Pad(Images.Stash) },
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
                    Bitmap padded = new(image.Width, image.Height + rowPadding + rowPadding, PixelFormat.Format32bppArgb);
                    using var g = Graphics.FromImage(padded);
                    g.DrawImageUnscaled(image, 0, rowPadding);
                    return padded;
                }
            }

            SearchControl<string> CreateSearchBox()
            {
                SearchControl<string> search = new(SearchForBranch, i => { })
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Name = "txtBranchCritierion",
                    TabIndex = 1
                };
                search.OnTextEntered += () =>
                {
                    OnBranchCriterionChanged(this, EventArgs.Empty);
                    OnBtnSearchClicked(this, EventArgs.Empty);
                };
                search.TextChanged += OnBranchCriterionChanged;
                search.KeyDown += TxtBranchCriterion_KeyDown;

                search.SearchBoxBorderStyle = BorderStyle.FixedSingle;
                search.SearchBoxBorderDefaultColor = Color.LightGray.AdaptBackColor();
                search.SearchBoxBorderHoveredColor = SystemColors.Highlight.AdaptBackColor();
                search.SearchBoxBorderFocusedColor = SystemColors.HotTrack.AdaptBackColor();

                return search;

                IEnumerable<string> SearchForBranch(string arg)
                {
                    return CollectFilterCandidates()
                        .Where(r => r.IndexOf(arg, StringComparison.OrdinalIgnoreCase) != -1);
                }

                IEnumerable<string> CollectFilterCandidates()
                {
                    List<string> list = new();

                    foreach (TreeNode rootNode in treeMain.Nodes)
                    {
                        CollectFromNodes(rootNode.Nodes);
                    }

                    return list;

                    void CollectFromNodes(TreeNodeCollection nodes)
                    {
                        foreach (TreeNode node in nodes)
                        {
                            if (node.Tag is BaseRevisionNode branch)
                            {
                                if (!branch.HasChildren)
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
            // If node is an inner node, and overrides OnDoubleClick, then disable expand/collapse
            if (treeMain.SelectedNode?.Tag is Node node
                && node.HasChildren
                && IsOverride(node.GetType().GetMethod(nameof(OnDoubleClick), BindingFlags.Instance | BindingFlags.NonPublic)))
            {
                e.Cancel = true;
            }

            return;

            bool IsOverride(MethodInfo m)
            {
                return m is not null && m.GetBaseDefinition().DeclaringType != m.DeclaringType;
            }
        }

        public void Initialize(IAheadBehindDataProvider? aheadBehindDataProvider, Action<string?> filterRevisionGridBySpaceSeparatedRefs,
            ICheckRefs refsSource, IScriptHostControl scriptHost, IRunScript scriptRunner)
        {
            _aheadBehindDataProvider = aheadBehindDataProvider;
            _filterRevisionGridBySpaceSeparatedRefs = filterRevisionGridBySpaceSeparatedRefs;
            _refsSource = refsSource;
            _scriptHost = scriptHost;
            _scriptRunner = scriptRunner;

            // This lazily sets the command source, invoking OnUICommandsSourceSet, which is required for setting up
            // notifications for each Tree.
            _ = UICommandsSource;
        }

        /// <summary>
        /// FormBrowse refreshing the side panel when refreshing the grid.
        /// (Update the objects in the panel.)
        /// </summary>
        /// <param name="getRefs">Function to get refs.</param>
        /// <param name="getStashRevs">Lazy accessor for stash commits.</param>
        /// <param name="forceRefresh">Refresh may be required as references may have been changed.</param>
        public void RefreshRevisionsLoading(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs, Lazy<IReadOnlyCollection<GitRevision>> getStashRevs, bool forceRefresh)
        {
            _branchesTree.Refresh(getRefs, forceRefresh);
            _remotesTree.Refresh(getRefs, forceRefresh);
            _tagTree.Refresh(getRefs, forceRefresh);
            _stashTree.Refresh(getStashRevs);
        }

        /// <summary>
        /// FormBrowse refreshing the side panel after updating the grid.
        /// (Update the visibility for side panel objects.)
        /// </summary>
        public void RefreshRevisionsLoaded()
        {
            // Some refs may not be visible
            _branchesTree.UpdateVisibility();
            _remotesTree.UpdateVisibility();
            _tagTree.UpdateVisibility();
            _stashTree.UpdateVisibility();
        }

        /// <summary>
        /// Refresh after resorting.
        /// </summary>
        /// <param name="getRefs">Git references</param>
        public void ResortRefs(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            _branchesTree.Refresh(getRefs);
            _remotesTree.Refresh(getRefs);
            _tagTree.Refresh(getRefs);

            _branchesTree.UpdateVisibility();
            _remotesTree.UpdateVisibility();
            _tagTree.UpdateVisibility();
        }

        public void ReloadHotkeys()
        {
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
        }

        public void SelectionChanged(IReadOnlyList<GitRevision> selectedRevisions)
        {
            // If we arrived here through the chain of events after selecting a node in the tree,
            // and the selected revision is the one we have selected - do nothing.
            if ((selectedRevisions.Count == 0 && treeMain.SelectedNode is null)
                || (selectedRevisions.Count == 1 && selectedRevisions[0].ObjectId == GetSelectedNodeObjectId(treeMain.SelectedNode)))
            {
                return;
            }

            var cancellationToken = _selectionCancellationTokenSequence.Next();

            GitRevision selectedRevision = selectedRevisions.FirstOrDefault();
            string? selectedGuid = selectedRevision is null
                ? null
                : selectedRevision.IsArtificial
                    ? "HEAD"
                    : selectedRevision.Guid;
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                HashSet<string> mergedBranches = selectedGuid is null
                    ? new HashSet<string>()
                    : (await Module.GetMergedBranchesAsync(includeRemote: true, fullRefname: true, commit: selectedGuid)).ToHashSet();

                selectedRevision?.Refs.ForEach(gitRef => mergedBranches.Remove(gitRef.CompleteName));

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

                try
                {
                    treeMain.BeginUpdate();
                    cancellationToken.ThrowIfCancellationRequested();
                    _branchesTree?.DepthEnumerator<BaseBranchLeafNode>().ForEach(node => node.IsMerged = mergedBranches.Contains(GitRefName.RefsHeadsPrefix + node.FullPath));
                    cancellationToken.ThrowIfCancellationRequested();
                    _remotesTree?.DepthEnumerator<BaseBranchLeafNode>().ForEach(node => node.IsMerged = mergedBranches.Contains(GitRefName.RefsRemotesPrefix + node.FullPath));
                }
                finally
                {
                    treeMain.EndUpdate();
                }
            }).FileAndForget();

            static ObjectId? GetSelectedNodeObjectId(TreeNode treeNode)
            {
                // Local or remote branch nodes or tag nodes
                return Node.GetNodeSafe<BaseBranchLeafNode>(treeNode)?.ObjectId ??
                    Node.GetNodeSafe<TagNode>(treeNode)?.ObjectId;
            }
        }

        protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
            _selectionCancellationTokenSequence.CancelCurrent();

            base.OnUICommandsSourceSet(source);

            CreateBranches();
            CreateRemotes();
            CreateTags();
            CreateSubmodules();
            CreateStashes();

            FixInvalidTreeToPositionIndices();
            ShowEnabledTrees();
        }

        private static void AddTreeNodeToSearchResult(ICollection<TreeNode> ret, TreeNode node)
        {
            node.BackColor = SystemColors.Info;
            node.ForeColor = SystemColors.InfoText;
            ret.Add(node);
        }

        private void CreateBranches()
        {
            TreeNode rootNode = new(TranslatedStrings.Branches)
            {
                Name = TranslatedStrings.Branches,
                ImageKey = nameof(Images.BranchLocalRoot),
                SelectedImageKey = nameof(Images.BranchLocalRoot)
            };

            _branchesTree = new LocalBranchTree(rootNode, UICommandsSource, _aheadBehindDataProvider, _refsSource);
        }

        private void CreateRemotes()
        {
            TreeNode rootNode = new(TranslatedStrings.Remotes)
            {
                Name = TranslatedStrings.Remotes,
                ImageKey = nameof(Images.BranchRemoteRoot),
                SelectedImageKey = nameof(Images.BranchRemoteRoot)
            };

            _remotesTree = new RemoteBranchTree(rootNode, UICommandsSource, _aheadBehindDataProvider, _refsSource);
        }

        private void CreateTags()
        {
            TreeNode rootNode = new(TranslatedStrings.Tags)
            {
                Name = TranslatedStrings.Tags,
                ImageKey = nameof(Images.TagHorizontal),
                SelectedImageKey = nameof(Images.TagHorizontal)
            };

            _tagTree = new TagTree(rootNode, UICommandsSource, _refsSource);
        }

        private void CreateSubmodules()
        {
            TreeNode rootNode = new(TranslatedStrings.Submodules)
            {
                Name = TranslatedStrings.Submodules,
                ImageKey = nameof(Images.FolderSubmodule),
                SelectedImageKey = nameof(Images.FolderSubmodule)
            };

            _submoduleTree = new SubmoduleTree(rootNode, UICommandsSource);
        }

        private void CreateStashes()
        {
            TreeNode rootNode = new(TranslatedStrings.Stashes)
            {
                Name = TranslatedStrings.Stashes,
                ImageKey = nameof(Images.Stash),
                SelectedImageKey = nameof(Images.Stash)
            };

            _stashTree = new StashTree(rootNode, UICommandsSource, _refsSource);
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
            treeMain.Nodes.AddRange(nodeList.OrderBy(treeNode => treeToPositionIndex[(Tree)treeNode.Tag]).ToArray());
            treeMain.EndUpdate();

            treeMain.Font = AppSettings.Font;
            _rootNodes.Add(tree);

            tree.Attached();
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

            if (_searchCriteriaChanged && _searchResult is not null && _searchResult.Any())
            {
                _searchCriteriaChanged = false;
                foreach (var coloredNode in _searchResult)
                {
                    coloredNode.BackColor = SystemColors.Window;
                }

                _searchResult = null;
                if (string.IsNullOrWhiteSpace(_txtBranchCriterion.Text))
                {
                    _txtBranchCriterion.Focus();
                    return;
                }
            }

            if (_searchResult is null || !_searchResult.Any())
            {
                if (!string.IsNullOrWhiteSpace(_txtBranchCriterion.Text))
                {
                    _searchResult = SearchTree(_txtBranchCriterion.Text, treeMain.Nodes.Cast<TreeNode>());
                }
            }

            var node = GetNextSearchResult();

            if (node is null)
            {
                return;
            }

            node.EnsureVisible();
            treeMain.SelectedNode = node;

            return;

            TreeNode? GetNextSearchResult()
            {
                var first = _searchResult?.FirstOrDefault();

                if (first is null)
                {
                    return null;
                }

                _searchResult!.RemoveAt(0);
                _searchResult.Add(first);
                return first;
            }

            List<TreeNode> SearchTree(string text, IEnumerable<TreeNode> nodes)
            {
                Queue<TreeNode> queue = new(nodes);
                List<TreeNode> ret = new();

                while (queue.Count != 0)
                {
                    var n = queue.Dequeue();

                    if (n.Tag is BaseRevisionNode branch)
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

            OnBtnSearchClicked(this, EventArgs.Empty);
            e.Handled = true;
        }

        protected override CommandStatus ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.Delete: Node.OnNode<Node>(treeMain.SelectedNode, node => node.OnDelete()); return true;
                case Command.Rename: Node.OnNode<Node>(treeMain.SelectedNode, node => node.OnRename()); return true;
                case Command.Search: OnBtnSearchClicked(this, EventArgs.Empty); return true;
                case Command.MultiSelect:
                case Command.MultiSelectWithChildren:
                    OnNodeClick(this, new TreeNodeMouseClickEventArgs(treeMain.SelectedNode, MouseButtons.Left, clicks: 1, x: 0, y: 0));
                    return true;
                default: return base.ExecuteCommand(cmd);
            }
        }

        private void OnNodeSelected(object sender, TreeViewEventArgs e)
        {
            Node.OnNode<Node>(e.Node, node =>
            {
                // prevent selection of refs hidden from the revision grid by filtering
                if (node is not BaseRevisionNode revNode || revNode.Visible)
                {
                    node.OnSelected();
                }
            });
        }

        private IEnumerable<NodeBase> GetSelectedNodes()
            => _rootNodes.SelectMany(tree => tree.GetSelectedNodes());

        private void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node.Tag as NodeBase;

            if (e.Button == MouseButtons.Right && node.IsSelected)
            {
                return; // don't undo multi-selection on opening context menu, even without Ctrl
            }

            SelectNode(node, multiple: ModifierKeys.HasFlag(Keys.Control), includingDescendants: ModifierKeys.HasFlag(Keys.Shift));

            if (node is Node clickable)
            {
                clickable.OnClick();
            }
        }

        private void SelectNode(NodeBase node, bool multiple, bool includingDescendants)
        {
            if (multiple)
            {
                node.Select(!node.IsSelected, includingDescendants: includingDescendants);
            }
            else
            {
                // deselect all selected nodes
                foreach (NodeBase selected in GetSelectedNodes())
                {
                    selected.Select(false);
                }

                node.Select(true); // and only select the clicked one
            }
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
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RepoObjectsTree _repoObjectsTree;

            public TestAccessor(RepoObjectsTree repoObjectsTree)
            {
                _repoObjectsTree = repoObjectsTree;
            }

            public ContextMenuStrip ContextMenu => _repoObjectsTree.menuMain;
            public NativeTreeView TreeView => _repoObjectsTree.treeMain;

            public void OpenContextMenu()
            {
                _repoObjectsTree.contextMenu_Opening(ContextMenu, new CancelEventArgs());
                _repoObjectsTree.contextMenu_Opened(ContextMenu, new EventArgs());
            }

            /// <summary>Simulates a left click on the <see cref="TreeNode"/> in <see cref="TreeView"/>
            /// identified by the path of <paramref name="nodeTexts"/> for UI tests.</summary>
            /// <typeparam name="TExpected">The expected type of the selected node. The type of the returned node will be validated against this.</typeparam>
            /// <param name="nodeTexts">The path of node texts used to select a single node starting from the first tree level.</param>
            /// <param name="multiple">Whether to select multiple; simulates holding <see cref="Keys.Control"/> while clicking.</param>
            /// <param name="includingDescendants">Whether to include descendants in the multi-selection;
            /// simulates holding <see cref="Keys.Shift"/> while clicking.</param>
            /// <exception cref="ArgumentException">Thrown if either <paramref name="nodeTexts"/> don't point to an existing node
            /// or the selected node is not of type <typeparamref name="TExpected"/>.</exception>
            public void SelectNode<TExpected>(string[] nodeTexts, bool multiple = false, bool includingDescendants = false) where TExpected : Node
            {
                var nodes = TreeView.Nodes.Cast<TreeNode>();
                TreeNode node = null;

                foreach (var text in nodeTexts)
                {
                    node = nodes.SingleOrDefault(node => node.Text == text);

                    if (node == null)
                    {
                        throw new ArgumentException(
                            $"Node '{text}' not found. Available nodes on this level: " + nodes.Select(n => n.Text).Join(", "),
                            nameof(nodeTexts));
                    }

                    nodes = node.Nodes.Cast<TreeNode>();
                }

                if (node.Tag.GetType() != typeof(TExpected))
                {
                    throw new ArgumentException($"The selected node is of type {node.Tag.GetType()} instead of the expected type {typeof(TExpected)}.", nameof(TExpected));
                }

                TreeView.SelectedNode = node; // simulates a node click well enough for UI tests
                _repoObjectsTree.SelectNode(node.Tag as NodeBase, multiple, includingDescendants);
            }

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
