using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
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

            protected string? AheadBehind { get; set; }

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

            internal BaseBranchNode? CreateRootNode(IDictionary<string, BaseBranchNode> pathToNode,
                Func<Tree, string, BaseBranchNode> createPathNode)
            {
                if (string.IsNullOrEmpty(ParentPath))
                {
                    return this;
                }

                BaseBranchNode? result;

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
    }
}
