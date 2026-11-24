using System.Diagnostics;
using GitUI.Compat;
using GitUI.Properties;

namespace GitUI.LeftPanel;

[DebuggerDisplay("(Folder) FullPath = {FullPath}")]
internal sealed class RemoteRepoFolderNode : BaseRevisionNode
{
    public RemoteRepoFolderNode(Tree tree, string name) : base(tree, name, true)
    {
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();
        TreeViewNode.SetImage(Images.EyeClosed.AdaptLightness());
    }

    protected override string DisplayText()
    {
        return Name;
    }
}
