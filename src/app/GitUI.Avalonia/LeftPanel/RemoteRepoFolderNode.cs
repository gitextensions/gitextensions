using GitUI.Compat;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal sealed class RemoteRepoFolderNode : BaseRevisionNode
{
    public RemoteRepoFolderNode(RemoteBranchTree tree, NodeBase parent, string fullPath)
        : base(tree, parent, fullPath, gitRef: null, GitUI.Properties.Images.EyeClosed.AdaptLightness())
    {
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();
    }

    protected override Avalonia.Media.IImage GetVisibleIcon()
        => Images.EyeClosed.AdaptLightness();

    protected override string DisplayText()
    {
        return Name;
    }
}
