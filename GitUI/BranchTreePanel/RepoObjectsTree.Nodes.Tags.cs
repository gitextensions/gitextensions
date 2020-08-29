using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        private class TagNode : BaseBranchNode, IGitRefActions, ICanDelete
        {
            public TagNode(Tree tree, in ObjectId objectId, string fullPath)
                : base(tree, fullPath)
            {
                ObjectId = objectId;
            }

            [CanBeNull]
            public ObjectId ObjectId { get; }

            internal override void OnSelected()
            {
                if (Tree.IgnoreSelectionChangedEvent)
                {
                    return;
                }

                base.OnSelected();
                SelectRevision();
            }

            internal override void OnDoubleClick()
            {
                CreateBranch();
            }

            public bool CreateBranch()
            {
                return UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, ObjectId);
            }

            public bool Delete()
            {
                return UICommands.StartDeleteTagDialog(TreeViewNode.TreeView, Name);
            }

            public bool Merge()
            {
                return UICommands.StartMergeBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.TagHorizontal);
            }

            public bool Checkout()
            {
                using (var form = new FormCheckoutRevision(UICommands))
                {
                    form.SetRevision(FullPath);
                    return form.ShowDialog(TreeViewNode.TreeView) != DialogResult.Cancel;
                }
            }
        }

        private sealed class TagTree : Tree
        {
            public TagTree(TreeNode treeNode, IGitUICommandsSource uiCommands)
                : base(treeNode, uiCommands)
            {
            }

            protected override Task OnAttachedAsync() => ReloadNodesAsync(LoadNodesAsync);

            protected override Task PostRepositoryChangedAsync() => ReloadNodesAsync(LoadNodesAsync);

            /// <summary>
            /// Requests to refresh the data tree retaining the current filtering rules.
            /// </summary>
            internal void RefreshRefs()
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await ReloadNodesAsync(LoadNodesAsync);
                });
            }

            private async Task<Nodes> LoadNodesAsync(CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();
                return FillTagTree(Module.GetRefs(tags: true, branches: false), token);
            }

            private Nodes FillTagTree(IReadOnlyList<IGitRef> tags, CancellationToken token)
            {
                var nodes = new Nodes(this);
                var pathToNodes = new Dictionary<string, BaseBranchNode>();
                foreach (IGitRef tag in tags)
                {
                    token.ThrowIfCancellationRequested();
                    var branchNode = new TagNode(this, tag.ObjectId, tag.Name);
                    var parent = branchNode.CreateRootNode(pathToNodes,
                        (tree, parentPath) => new BasePathNode(tree, parentPath));
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
                    TreeViewNode.Collapse();
                }
            }
        }
    }
}
