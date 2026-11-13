using GitUI.Compat;
using GitUI.Properties;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

// Top-level nodes used to group SubmoduleNodes
internal sealed class SubmoduleFolderNode(Tree tree, string name) : Node(tree)
{
    protected override string DisplayText()
    {
        return name;
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();
        TreeViewNode.SetImage(Images.FolderClosed);
    }

    /// <summary>
    ///  Compacts chains of single-child folder nodes by merging their names with "/" separators.
    ///  For example, a chain "extension" → "src" → "test" → "assets" becomes
    ///  a single folder node named "extension/src/test/assets".
    /// </summary>
    public void CompactSingleChildFolders()
    {
        while (Nodes is [SubmoduleFolderNode childFolder])
        {
            name += "/" + childFolder.DisplayText();

            Nodes = childFolder.Nodes;
        }
    }

    protected override WinFormsShims.FontStyle GetFontStyle()
    {
        return base.GetFontStyle() | WinFormsShims.FontStyle.Italic;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(SubmoduleFolderNode node)
    {
        public string DisplayText() => node.DisplayText();
    }
}
