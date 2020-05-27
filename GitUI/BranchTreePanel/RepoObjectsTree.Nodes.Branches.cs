﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands.Git;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.Properties;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        #region private classes

        private abstract class BaseBranchNode : Node
        {
            protected const char PathSeparator = '/';

            /// <summary>Short name of the branch/branch path. <example>"issue1344"</example></summary>
            protected string Name { get; set; }

            private string ParentPath { get; }
            protected string AheadBehind { get; set; }

            /// <summary>Full path of the branch. <example>"issues/issue1344"</example></summary>
            public string FullPath => ParentPath.Combine(PathSeparator.ToString(), Name);

            public override int GetHashCode()
            {
                return FullPath.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj is BaseBranchNode other && (ReferenceEquals(other, this) || string.Equals(FullPath, other.FullPath));
            }

            protected BaseBranchNode(Tree tree, string fullPath)
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
            }

            public void UpdateAheadBehind(string aheadBehindData)
            {
                AheadBehind = aheadBehindData;
            }

            public bool Rebase()
            {
                return UICommands.StartRebaseDialog(TreeViewNode.TreeView, FullPath);
            }

            public bool Reset()
            {
                return UICommands.StartResetCurrentBranchDialog(TreeViewNode.TreeView, FullPath);
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
                TreeViewNode.TreeView.BeginInvoke(new Action(() =>
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

            public BaseBranchLeafNode(Tree tree, string fullPath, string imageKeyUnmerged, string imageKeyMerged)
                : base(tree, fullPath)
            {
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
                    TreeViewNode.ToolTipText = IsMerged ? string.Format(Strings.ContainedInCurrentCommit, Name) : string.Empty;
                }
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = IsMerged ? _imageKeyMerged : _imageKeyUnmerged;
            }
        }

        private sealed class LocalBranchNode : BaseBranchLeafNode, IGitRefActions, ICanRename, ICanDelete
        {
            public LocalBranchNode(Tree tree, string fullPath, bool isCurrent)
                : base(tree, fullPath, nameof(Images.BranchLocal), nameof(Images.BranchLocalMerged))
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
            {
                return base.Equals(obj) && obj is LocalBranchNode localBranchNode && IsActive == localBranchNode.IsActive;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

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
                return UICommands.StartCheckoutBranch(FullPath, false);
            }

            public bool CreateBranch()
            {
                return UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            public bool Merge()
            {
                return UICommands.StartMergeBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            public bool Delete()
            {
                return UICommands.StartDeleteBranchDialog(ParentWindow(), FullPath);
            }

            public bool Rename()
            {
                return UICommands.StartRenameDialog(TreeViewNode.TreeView, FullPath);
            }
        }

        private class BasePathNode : BaseBranchNode
        {
            public BasePathNode(Tree tree, string fullPath) : base(tree, fullPath)
            {
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.BranchFolder);
            }
        }

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
                UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, null, newBranchNamePrefix);
            }
        }

        private sealed class BranchTree : Tree
        {
            private readonly IAheadBehindDataProvider _aheadBehindDataProvider;

            public BranchTree(TreeNode treeNode, IGitUICommandsSource uiCommands, [CanBeNull]IAheadBehindDataProvider aheadBehindDataProvider)
                : base(treeNode, uiCommands)
            {
                _aheadBehindDataProvider = aheadBehindDataProvider;
            }

            protected override Task OnAttachedAsync()
            {
                return ReloadNodesAsync(LoadNodesAsync);
            }

            protected override Task PostRepositoryChangedAsync()
            {
                return ReloadNodesAsync(LoadNodesAsync);
            }

            private async Task<Nodes> LoadNodesAsync(CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();

                var branchNames = Module.GetRefs(tags: false, branches: true, noLocks: true).Select(b => b.Name);
                return FillBranchTree(branchNames, token);
            }

            private Nodes FillBranchTree(IEnumerable<string> branches, CancellationToken token)
            {
                #region ex

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
                foreach (var branch in branches)
                {
                    token.ThrowIfCancellationRequested();
                    var localBranchNode = new LocalBranchNode(this, branch, branch == currentBranch);

                    if (aheadBehindData != null && aheadBehindData.ContainsKey(localBranchNode.FullPath))
                    {
                        localBranchNode.UpdateAheadBehind(aheadBehindData[localBranchNode.FullPath].ToDisplay());
                    }

                    var parent = localBranchNode.CreateRootNode(pathToNode, (tree, parentPath) => new BranchPathNode(tree, parentPath));
                    if (parent != null)
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

                var activeBranch = Nodes.DepthEnumerator<LocalBranchNode>().FirstOrDefault(b => b.IsActive);
                TreeViewNode.TreeView.SelectedNode = activeBranch?.TreeViewNode;
            }
        }

        #endregion
    }
}
