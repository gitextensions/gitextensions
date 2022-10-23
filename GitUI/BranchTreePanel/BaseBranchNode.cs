﻿using System.Diagnostics;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;

namespace GitUI.BranchTreePanel
{
    [DebuggerDisplay("(Node) FullPath = {FullPath}")]
    internal abstract class BaseBranchNode : Node
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

            string[] dirs = fullPath.Split(PathSeparator);
            Name = dirs[dirs.Length - 1];
            ParentPath = dirs.Take(dirs.Length - 1).Join(PathSeparator.ToString());
            Visible = visible;
        }

        protected string? AheadBehind { get; set; }

        protected string? RelatedBranch { get; set; }

        /// <summary>
        /// Short name of the branch/branch path. <example>"issue1344"</example>.
        /// </summary>
        public string Name { get; }

        protected string ParentPath { get; }

        /// <summary>
        /// Full path of the branch. <example>"issues/issue1344"</example>.
        /// </summary>
        public string FullPath => ParentPath.Combine(PathSeparator.ToString(), Name)!;

        /// <summary>
        /// Gets whether the commit that the node represents is currently visible in the revision grid.
        /// </summary>
        public bool Visible { get; }

        protected override void ApplyStyle()
        {
            base.ApplyStyle();

            TreeViewNode.ForeColor = Visible && TreeViewNode.TreeView is not null ? TreeViewNode.TreeView.ForeColor : Color.Silver.AdaptTextColor();
            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = Visible ? null : nameof(Images.EyeClosed);
        }

        public override int GetHashCode() => FullPath.GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is BaseBranchNode other
                && (ReferenceEquals(other, this) || string.Equals(FullPath, other.FullPath));
        }

        public void UpdateAheadBehind(string aheadBehindData, string relatedBranch)
        {
            AheadBehind = aheadBehindData;
            RelatedBranch = relatedBranch;
        }

        public bool Rebase()
        {
            return UICommands.StartRebaseDialog(ParentWindow(), onto: FullPath);
        }

        public bool Reset()
        {
            return UICommands.StartResetCurrentBranchDialog(ParentWindow(), branch: FullPath);
        }

        internal BaseBranchNode? CreateRootNode(IDictionary<string, BaseBranchNode> pathToNode,
            Func<Tree, string, BaseBranchNode> createPathNode)
        {
            if (string.IsNullOrEmpty(ParentPath))
            {
                return this;
            }

            BaseBranchNode? result;

            if (pathToNode.TryGetValue(ParentPath, out BaseBranchNode parent))
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
                string branch = RelatedBranch is null || !Control.ModifierKeys.HasFlag(Keys.Alt)
                    ? FullPath : RelatedBranch.Substring(startIndex: GitRefName.RefsRemotesPrefix.Length);
                UICommands.BrowseGoToRef(branch, showNoRevisionMsg: true, toggleSelection: Control.ModifierKeys.HasFlag(Keys.Control));
                TreeViewNode.TreeView?.Focus();
            }));
        }
    }
}
