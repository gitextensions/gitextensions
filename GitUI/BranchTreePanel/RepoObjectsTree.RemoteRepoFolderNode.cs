using System.Diagnostics;
using GitUI.Properties;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        [DebuggerDisplay("(Folder) FullPath = {FullPath}")]
        private sealed class RemoteRepoFolderNode : BaseBranchNode
        {
            public RemoteRepoFolderNode(Tree tree, string name) : base(tree, name, true)
            {
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.EyeClosed);
            }

            protected override string DisplayText()
            {
                return Name;
            }
        }
    }
}
