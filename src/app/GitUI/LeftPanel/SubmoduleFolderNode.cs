using GitUI.Properties;

namespace GitUI.LeftPanel;

// Top-level nodes used to group SubmoduleNodes
internal sealed class SubmoduleFolderNode : Node
{
    private string _name;

    public SubmoduleFolderNode(Tree tree, string name)
        : base(tree)
    {
        _name = name;
    }

    protected override string DisplayText()
    {
        return string.Format(_name);
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();
        TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.FolderClosed);
    }

    protected override FontStyle GetFontStyle()
        => base.GetFontStyle() | FontStyle.Italic;

    /// <summary>
    ///  Compacts chains of single-child folder nodes by merging their names with "/" separators.
    ///  For example, a chain "extension" → "src" → "test" → "assets" becomes
    ///  a single folder node named "extension/src/test/assets".
    /// </summary>
    /// <summary>
    ///  Compacts chains of single-child folder nodes by merging their names with "/" separators.
    ///  For example, a chain "extension" → "src" → "test" → "assets" becomes
    ///  a single folder node named "extension/src/test/assets".
    /// </summary>
    internal void CompactSingleChildFolders()
    {
        while (Nodes.Count == 1)
        {
            SubmoduleFolderNode? childFolder = null;
            foreach (Node child in Nodes)
            {
                childFolder = child as SubmoduleFolderNode;
            }

            if (childFolder is null)
            {
                break;
            }

            _name += "/" + childFolder._name;
            List<Node> grandchildren = [.. childFolder.Nodes];
            Nodes.Clear();
            Nodes.AddNodes(grandchildren);
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(SubmoduleFolderNode node)
    {
        public string Name => node._name;
    }
}
