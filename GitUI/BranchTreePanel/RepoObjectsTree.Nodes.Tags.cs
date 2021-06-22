using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        [DebuggerDisplay("(Tag) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
        private class TagNode : BaseBranchNode, IGitRefActions, ICanDelete
        {
            public TagNode(Tree tree, in ObjectId? objectId, string fullPath, bool visible)
                : base(tree, fullPath, visible)
            {
                ObjectId = objectId;
            }

            public ObjectId? ObjectId { get; }

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
                return UICommands.StartDeleteTagDialog(TreeViewNode.TreeView, FullPath);
            }

            public bool Merge()
            {
                return UICommands.StartMergeBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                    Visible
                        ? nameof(Images.TagHorizontal)
                        : nameof(Images.EyeClosed);
            }

            public bool Checkout()
            {
                using FormCheckoutRevision form = new(UICommands);
                form.SetRevision(FullPath);
                return form.ShowDialog(TreeViewNode.TreeView) != DialogResult.Cancel;
            }
        }

        private sealed class TagTree : Tree
        {
            private readonly ICheckRefs _refsSource;

            // Retains the list of currently loaded tags.
            // This is needed to apply filtering without reloading the data.
            // Whether or not force the reload of data is controlled by <see cref="_isFiltering"/> flag.
            private IReadOnlyList<IGitRef>? _loadedTags;

            public TagTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
                : base(treeNode, uiCommands)
            {
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

            /// <inheritdoc/>
            protected internal override void Refresh()
            {
                // Break the local cache to ensure the data is requeried to reflect the required sort order.
                _loadedTags = null;

                base.Refresh();
            }

            protected override async Task<Nodes> LoadNodesAsync(CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();

                if (!IsFiltering.Value || _loadedTags is null)
                {
                    _loadedTags = Module.GetRefs(RefsFilter.Tags);
                    token.ThrowIfCancellationRequested();
                }

                return FillTagTree(_loadedTags, token);
            }

            private Nodes FillTagTree(IReadOnlyList<IGitRef> tags, CancellationToken token)
            {
                Nodes nodes = new(this);
                Dictionary<string, BaseBranchNode> pathToNodes = new();
                foreach (IGitRef tag in tags)
                {
                    token.ThrowIfCancellationRequested();

                    bool isVisible = !IsFiltering.Value || (tag.ObjectId is not null && _refsSource.Contains(tag.ObjectId));
                    TagNode tagNode = new(this, tag.ObjectId, tag.Name, isVisible);
                    var parent = tagNode.CreateRootNode(pathToNodes, (tree, parentPath) => new BasePathNode(tree, parentPath));
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
                    TreeViewNode.Collapse();
                }
            }
        }
    }
}
