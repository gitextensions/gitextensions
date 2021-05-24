using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        #region private classes

        [DebuggerDisplay("(Node) FullPath = {FullPath}")]
        private abstract class BaseBranchNode : Node
        {
            protected const char PathSeparator = '/';

            protected BaseBranchNode(Tree tree, string fullPath, bool visible)
                : base(tree)
            {
                fullPath = fullPath.Trim();
                if (string.IsNullOrEmpty(fullPath))
                {
                    throw new ArgumentNullException(nameof(fullPath));
                }

                var dirs = fullPath.Split(PathSeparator);
                Name = dirs[dirs.Length - 1];
                ParentPath = dirs.Take(dirs.Length - 1).Join(PathSeparator.ToString());
                Visible = visible;
            }

            protected string AheadBehind { get; set; }

            /// <summary>
            /// Short name of the branch/branch path. <example>"issue1344"</example>.
            /// </summary>
            public string Name { get; protected set; }

            protected string ParentPath { get; }

            /// <summary>
            /// Full path of the branch. <example>"issues/issue1344"</example>.
            /// </summary>
            public string FullPath => ParentPath.Combine(PathSeparator.ToString(), Name);

            /// <summary>
            /// Gets whether the commit that the node represents is currently visible in the revision grid.
            /// </summary>
            public bool Visible { get; }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                TreeViewNode.ForeColor = Visible && TreeViewNode.TreeView is not null ? TreeViewNode.TreeView.ForeColor : Color.Silver.AdaptTextColor();
                TreeViewNode.ImageKey =
                    TreeViewNode.SelectedImageKey = Visible ? null : nameof(Images.EyeClosed);
            }

            public override int GetHashCode() => FullPath.GetHashCode() ^ Visible.GetHashCode();

            public override bool Equals(object obj)
            {
                return obj is BaseBranchNode other
                    && (ReferenceEquals(other, this) || string.Equals(FullPath, other.FullPath))
                    && Visible == other.Visible;
            }

            public void UpdateAheadBehind(string aheadBehindData)
            {
                AheadBehind = aheadBehindData;
            }

            public bool Rebase()
            {
                return UICommands.StartRebaseDialog(ParentWindow(), onto: FullPath);
            }

            public bool Reset()
            {
                return UICommands.StartResetCurrentBranchDialog(ParentWindow(), branch: FullPath);
            }

            [CanBeNull]
            internal BaseBranchNode CreateRootNode(IDictionary<string, BaseBranchNode> pathToNode,
                Func<Tree, string, BaseBranchNode> createPathNode)
            {
                if (string.IsNullOrEmpty(ParentPath))
                {
                    return this;
                }

                BaseBranchNode result;

                if (pathToNode.TryGetValue(ParentPath, out var parent))
                {
                    result = null;
                }
                else
                {
                    parent = createPathNode(Tree, ParentPath);
                    pathToNode.Add(ParentPath, parent);
                    result = parent.CreateRootNode(pathToNode, createPathNode);
                }

                parent.Nodes.AddNode(this);

                return result;
            }

            protected override string DisplayText()
            {
                return string.IsNullOrEmpty(AheadBehind) ? Name : $"{Name} ({AheadBehind})";
            }

            protected void SelectRevision()
            {
                TreeViewNode.TreeView?.BeginInvoke(new Action(() =>
                {
                    UICommands.BrowseGoToRef(FullPath, showNoRevisionMsg: true, toggleSelection: ModifierKeys.HasFlag(Keys.Control));
                    TreeViewNode.TreeView?.Focus();
                }));
            }
        }

        private class BaseBranchLeafNode : BaseBranchNode
        {
            private readonly string _imageKeyMerged;
            private readonly string _imageKeyUnmerged;

            private bool _isMerged = false;

            public BaseBranchLeafNode(Tree tree, in ObjectId objectId, string fullPath, bool visible, string imageKeyUnmerged, string imageKeyMerged)
                : base(tree, fullPath, visible)
            {
                ObjectId = objectId;
                _imageKeyUnmerged = imageKeyUnmerged;
                _imageKeyMerged = imageKeyMerged;
            }

            public bool IsMerged
            {
                get => _isMerged;
                set
                {
                    if (_isMerged == value)
                    {
                        return;
                    }

                    _isMerged = value;

                    ApplyStyle();
                }
            }

            [CanBeNull]
            public ObjectId ObjectId { get; }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                    Visible
                        ? IsMerged ? _imageKeyMerged : _imageKeyUnmerged
                        : nameof(Images.EyeClosed);
                if (!Visible)
                {
                    TreeViewNode.ToolTipText = string.Format(Strings.InvisibleCommit, FullPath);
                }
                else if (_isMerged)
                {
                    TreeViewNode.ToolTipText = string.Format(Strings.ContainedInCurrentCommit, Name);
                }
            }
        }

        [DebuggerDisplay("(Local) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
        private sealed class LocalBranchNode : BaseBranchLeafNode, IGitRefActions, ICanRename, ICanDelete
        {
            public LocalBranchNode(Tree tree, in ObjectId objectId, string fullPath, bool isCurrent, bool visible)
                : base(tree, objectId, fullPath, visible, nameof(Images.BranchLocal), nameof(Images.BranchLocalMerged))
            {
                IsActive = isCurrent;
            }

            public bool IsActive { get; }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                SetNodeFont(IsActive ? FontStyle.Bold : FontStyle.Regular);
            }

            public override bool Equals(object obj)
                => base.Equals(obj)
                    && obj is LocalBranchNode localBranchNode
                    && IsActive == localBranchNode.IsActive;

            public override int GetHashCode()
                => base.GetHashCode() ^ IsActive.GetHashCode();

            internal override void OnDoubleClick()
            {
                Checkout();
            }

            internal override void OnSelected()
            {
                if (Tree.IgnoreSelectionChangedEvent)
                {
                    return;
                }

                base.OnSelected();
                SelectRevision();
            }

            public bool Checkout()
            {
                return UICommands.StartCheckoutBranch(ParentWindow(), branch: FullPath, remote: false);
            }

            public bool CreateBranch()
            {
                return UICommands.StartCreateBranchDialog(ParentWindow(), branch: FullPath);
            }

            public bool Merge()
            {
                return UICommands.StartMergeBranchDialog(ParentWindow(), branch: FullPath);
            }

            public bool Delete()
            {
                return UICommands.StartDeleteBranchDialog(ParentWindow(), branch: FullPath);
            }

            public bool Rename()
            {
                return UICommands.StartRenameDialog(ParentWindow(), branch: FullPath);
            }
        }

        private class BasePathNode : BaseBranchNode
        {
            public BasePathNode(Tree tree, string fullPath) : base(tree, fullPath, visible: true)
            {
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                    FullPath == Strings.Inactive ? nameof(Images.EyeClosed) : nameof(Images.BranchFolder);
            }
        }

        [DebuggerDisplay("(Branch path) FullPath = {FullPath}")]
        private class BranchPathNode : BasePathNode
        {
            public BranchPathNode(Tree tree, string fullPath)
                : base(tree, fullPath)
            {
            }

            public override string ToString()
            {
                return $"{Name}{PathSeparator}";
            }

            public void DeleteAll()
            {
                var branches = Nodes.DepthEnumerator<LocalBranchNode>().Select(branch => branch.FullPath);
                UICommands.StartDeleteBranchDialog(ParentWindow(), branches);
            }

            public void CreateBranch()
            {
                var newBranchNamePrefix = FullPath + PathSeparator;
                UICommands.StartCreateBranchDialog(ParentWindow(), objectId: null, newBranchNamePrefix);
            }
        }

        private sealed class BranchTree : Tree
        {
            private readonly IAheadBehindDataProvider _aheadBehindDataProvider;
            private readonly ICheckRefs _refsSource;

            // Retains the list of currently loaded branches.
            // This is needed to apply filtering without reloading the data.
            // Whether or not force the reload of data is controlled by <see cref="_isFiltering"/> flag.
            private IReadOnlyList<IGitRef> _loadedBranches;

            public BranchTree(TreeNode treeNode, IGitUICommandsSource uiCommands, [CanBeNull] IAheadBehindDataProvider aheadBehindDataProvider, ICheckRefs refsSource)
                : base(treeNode, uiCommands)
            {
                _aheadBehindDataProvider = aheadBehindDataProvider;
                _refsSource = refsSource;
            }

            protected override bool SupportsFiltering => true;

            protected override Task OnAttachedAsync()
            {
                IsFiltering.Value = false;
                return ReloadNodesAsync(LoadNodesAsync);
            }

            protected override Task PostRepositoryChangedAsync()
            {
                IsFiltering.Value = false;
                return ReloadNodesAsync(LoadNodesAsync);
            }

            protected override async Task<Nodes> LoadNodesAsync(CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();

                if (!IsFiltering.Value || _loadedBranches is null)
                {
                    _loadedBranches = Module.GetRefs(tags: false, branches: true);
                    token.ThrowIfCancellationRequested();
                }

                return FillBranchTree(_loadedBranches, token);
            }

            /// <inheritdoc/>
            protected internal override void Refresh()
            {
                // Break the local cache to ensure the data is requeried to reflect the required sort order.
                _loadedBranches = null;

                base.Refresh();
            }

            private Nodes FillBranchTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
            {
                #region example

                // (input)
                // a-branch
                // develop/crazy-branch
                // develop/features/feat-next
                // develop/features/feat-next2
                // develop/issues/iss444
                // develop/wild-branch
                // issues/iss111
                // master
                //
                // ->
                // (output)
                // 0 a-branch
                // 0 develop/
                // 1   features/
                // 2      feat-next
                // 2      feat-next2
                // 1   issues/
                // 2      iss444
                // 1   wild-branch
                // 1   wilds/
                // 2      card
                // 0 issues/
                // 1     iss111
                // 0 master

                #endregion

                var nodes = new Nodes(this);
                var aheadBehindData = _aheadBehindDataProvider?.GetData();

                var currentBranch = Module.GetSelectedBranch();
                var pathToNode = new Dictionary<string, BaseBranchNode>();
                foreach (IGitRef branch in branches)
                {
                    token.ThrowIfCancellationRequested();

                    bool isVisible = !IsFiltering.Value || _refsSource.Contains(branch.ObjectId);
                    var localBranchNode = new LocalBranchNode(this, branch.ObjectId, branch.Name, branch.Name == currentBranch, isVisible);

                    if (aheadBehindData is not null && aheadBehindData.ContainsKey(localBranchNode.FullPath))
                    {
                        localBranchNode.UpdateAheadBehind(aheadBehindData[localBranchNode.FullPath].ToDisplay());
                    }

                    var parent = localBranchNode.CreateRootNode(pathToNode, (tree, parentPath) => new BranchPathNode(tree, parentPath));
                    if (parent is not null)
                    {
                        nodes.AddNode(parent);
                    }
                }

                return nodes;
            }

            protected override void PostFillTreeViewNode(bool firstTime)
            {
                if (firstTime)
                {
                    TreeViewNode.Expand();
                }

                // Skip hidden node
                if (TreeViewNode.TreeView is null)
                {
                    return;
                }

                if (TreeViewNode.TreeView.SelectedNode is not null)
                {
                    // If there's a selected treenode, don't stomp over it
                    return;
                }

                var activeBranch = Nodes.DepthEnumerator<LocalBranchNode>().FirstOrDefault(b => b.IsActive);
                TreeViewNode.TreeView.SelectedNode = activeBranch?.TreeViewNode;
            }
        }

        #endregion
    }
}
