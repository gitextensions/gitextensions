using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs;
using ResourceManager;

namespace GitUI.UserControls
{
    // "tags"
    partial class RepoObjectsTree
    {
        private class TagNode : BaseBranchNode
        {
            private const string ImageKey = "tag.png";
            private readonly GitRef _tagInfo;

            public TagNode(Tree aTree, string aFullPath, GitRef tagInfo) : base(aTree, aFullPath)
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

            /// <summary>Create a local branch from the remote branch.</summary>
            public void CreateBranch()
            {
                UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, new GitRevision(Module, _tagInfo.Guid));
            }

            public void Delete()
            {
                UICommands.StartDeleteTagDialog(TreeViewNode.TreeView, _tagInfo.Name);
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = ImageKey;
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
            public TagTree(TreeNode aTreeNode, IGitUICommandsSource uiCommands)
                : base(aTreeNode, uiCommands)
            {
                uiCommands.GitUICommandsChanged += uiCommands_GitUICommandsChanged;
            }

            private void uiCommands_GitUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
            {
                if (TreeViewNode == null || TreeViewNode.TreeView == null)
                {
                    return;
                }
                //select active branch after repo change
                TreeViewNode.TreeView.SelectedNode = null;
            }

            protected override void LoadNodes(System.Threading.CancellationToken token)
            {
                FillTagTree(Module.GetTagRefs(GitModule.GetTagRefsSortOrder.ByName));
            }

            private void FillTagTree(IEnumerable<GitRef> tags)
            {
                var nodes = new Dictionary<string, BaseBranchNode>();
                foreach (var tag in tags)
                {
                    var branchNode = new TagNode(this, tag.Name, tag);
                    var parent = branchNode.CreateRootNode(nodes,
                        (tree, parentPath) => new BasePathNode(tree, parentPath));
                    if (parent != null)
                        Nodes.AddNode(parent);
                }
            }

            protected override void FillTreeViewNode()
            {
                base.FillTreeViewNode();

                TreeViewNode.Text = string.Format("{0} ({1})", Strings.tags, Nodes.Count);

                TreeViewNode.Collapse();
            }
        }
    }
}
