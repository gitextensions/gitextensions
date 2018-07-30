﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        private class TagNode : BaseBranchNode
        {
            private readonly IGitRef _tagInfo;

            public TagNode(Tree tree, string fullPath, IGitRef tagInfo) : base(tree, fullPath)
            {
                _tagInfo = tagInfo;
            }

            internal override void OnSelected()
            {
                base.OnSelected();
                SelectRevision();
            }

            internal override void OnDoubleClick()
            {
                CreateBranch();
            }

            public void CreateBranch()
            {
                UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, ObjectId.Parse(_tagInfo.Guid));
            }

            public void Delete()
            {
                UICommands.StartDeleteTagDialog(TreeViewNode.TreeView, _tagInfo.Name);
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.TagHorizontal);
            }

            public void Checkout()
            {
                using (var form = new FormCheckoutRevision(UICommands))
                {
                    form.SetRevision(FullPath);
                    form.ShowDialog(TreeViewNode.TreeView);
                }
            }
        }

        private class TagTree : Tree
        {
            public TagTree(TreeNode treeNode, IGitUICommandsSource uiCommands)
                : base(treeNode, uiCommands)
            {
                uiCommands.UICommandsChanged += OnUICommandsChanged;
            }

            private void OnUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
            {
                if (TreeViewNode?.TreeView == null)
                {
                    return;
                }

                TreeViewNode.TreeView.SelectedNode = null;
            }

            protected override async Task LoadNodesAsync(CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();
                FillTagTree(Module.GetTagRefs(GitModule.GetTagRefsSortOrder.ByName), token);
            }

            private void FillTagTree(IEnumerable<IGitRef> tags, CancellationToken token)
            {
                var nodes = new Dictionary<string, BaseBranchNode>();
                foreach (var tag in tags)
                {
                    token.ThrowIfCancellationRequested();
                    var branchNode = new TagNode(this, tag.Name, tag);
                    var parent = branchNode.CreateRootNode(nodes,
                        (tree, parentPath) => new BasePathNode(tree, parentPath));
                    if (parent != null)
                    {
                        Nodes.AddNode(parent);
                    }
                }
            }

            protected override void FillTreeViewNode()
            {
                base.FillTreeViewNode();

                TreeViewNode.Text = $@"{Strings.Tags} ({Nodes.Count})";

                TreeViewNode.Collapse();
            }
        }
    }
}
