using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    public partial class RepoObjectsTree
    {
        abstract class RepoObjectsTreeSet
        {
            protected GitModule git;
            protected readonly TreeNode _treeNode;

            protected RepoObjectsTreeSet(GitModule git, TreeNode treeNode)
            {
                this.git = git;
                _treeNode = treeNode;
            }

            /// <summary>Readies the tree set for a new repo.</summary>
            public virtual void NewRepo(GitModule git)
            {
                this.git = git;
            }
            public abstract Task ReloadAsync();

            public virtual void ApplyTreeNodeStyle(TreeNode treeNode)
            {
               RepoObjectsTree.ApplyTreeNodeStyle(treeNode);
            }

        }

        abstract class RepoObjectsTreeSet<T> : RepoObjectsTreeSet
        {
            protected readonly Func<GitModule, ICollection<T>> _getValues;
            protected readonly Action<ICollection<T>> _onReload;
            protected ListWatcher<T> _Watcher;

            protected RepoObjectsTreeSet(
                GitModule git,
                TreeNode treeNode,
                Func<GitModule, ICollection<T>> getValues,
                Action<ICollection<T>> onReload)
                : base(git, treeNode)
            {
                _getValues = getValues;
                _onReload = onReload;
            }

            /// <summary>Readies the tree set for a new repo.</summary>
            public override void NewRepo(GitModule git)
            {
                _Watcher = new ListWatcher<T>(() => _getValues(git), ReloadNodes);
            }

            public override Task ReloadAsync()
            {
                return _Watcher.CheckUpdateAsync();
            }

            protected virtual void ReloadNodes(ICollection<T> items)
            {
                _treeNode.Nodes.Clear();

                foreach (T item in items)
                {
                    TreeNode child = AddChild(_treeNode.Nodes, item);
                    ApplyStyle(child);
                }

                _onReload(items);
            }

            protected abstract TreeNode AddChild(TreeNodeCollection nodes, T item);

            protected virtual void ApplyStyle(TreeNode treeNode)
            {
                base.ApplyTreeNodeStyle(treeNode);
            }
        }

        class EasyRepoTreeSet<T> : RepoObjectsTreeSet<T>
        {
            readonly Func<TreeNodeCollection, T, TreeNode> _onAddChild;

            public EasyRepoTreeSet(
                GitModule git,
                TreeNode treeNode,
                Func<GitModule, ICollection<T>> getValues,
                Action<ICollection<T>> onReload,
                Func<TreeNodeCollection, T, TreeNode> onAddChild)
                : base(git, treeNode, getValues, onReload)
            {
                _onAddChild = onAddChild;
            }

            protected override TreeNode AddChild(TreeNodeCollection nodes, T item)
            {
                return _onAddChild(nodes, item);
            }
        }

    }
}
