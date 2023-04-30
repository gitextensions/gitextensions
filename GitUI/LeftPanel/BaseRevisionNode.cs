using System.Diagnostics;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel
{
    [DebuggerDisplay("(Node) FullPath = {FullPath}")]
    internal abstract class BaseRevisionNode : Node
    {
        protected const char PathSeparator = '/';

        protected BaseRevisionNode(Tree tree, string fullPath, bool visible)
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
        /// ObjectId for nodes with a revision.
        /// </summary>
        public ObjectId? ObjectId { get; init; }

        public override void ApplyStyle()
        {
            base.ApplyStyle();

            TreeViewNode.ForeColor = Visible && TreeViewNode.TreeView is not null ? TreeViewNode.TreeView.ForeColor : Color.Silver.AdaptTextColor();
            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = Visible ? null : nameof(Images.EyeClosed);
        }

        public override int GetHashCode() => FullPath.GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is BaseRevisionNode other
                && (ReferenceEquals(other, this) || string.Equals(FullPath, other.FullPath));
        }

        public bool Rebase()
        {
            return UICommands.StartRebaseDialog(ParentWindow(), onto: FullPath);
        }

        public bool Reset()
        {
            return UICommands.StartResetCurrentBranchDialog(ParentWindow(), branch: FullPath);
        }

        internal BaseRevisionNode? CreateRootNode(IDictionary<string, BaseRevisionNode> pathToNode,
            Func<Tree, string, BaseRevisionNode> createPathNode)
        {
            if (string.IsNullOrEmpty(ParentPath))
            {
                return this;
            }

            BaseRevisionNode? result;

            if (pathToNode.TryGetValue(ParentPath, out BaseRevisionNode parent))
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
            return Name;
        }

        protected virtual void SelectRevision()
        {
            if (ObjectId is null)
            {
                return;
            }

            TreeViewNode.TreeView?.BeginInvoke(() =>
            {
                UICommands.BrowseGoToRef(ObjectId.ToString(), showNoRevisionMsg: true, toggleSelection: Control.ModifierKeys.HasFlag(Keys.Control));
                TreeViewNode.TreeView?.Focus();
            });
        }
    }
}
