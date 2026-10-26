using Avalonia.Controls;
using GitUI.Properties;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

// Top-level nodes used to group SubmoduleNodes
internal sealed class SubmoduleFolderNode : Node
{
    private string _name;

    public SubmoduleFolderNode(Tree tree, NodeBase parent, string name)
        : base(tree, parent, name, Images.FolderClosed, isItalic: true)
    {
        _name = name;
    }

    internal string Name => _name;

    protected override string DisplayText()
    {
        return _name;
    }

    public override void ApplyStyle()
    {
        SetHeader(DisplayText(), Images.FolderClosed, isItalic: true);
        base.ApplyStyle();
    }

    /// <summary>
    ///  Compacts chains of single-child folder nodes by merging their names with "/" separators.
    ///  For example, a chain "extension" → "src" → "test" → "assets" becomes
    ///  a single folder node named "extension/src/test/assets".
    /// </summary>
    public void CompactSingleChildFolders()
    {
        while (TreeViewNode.Items is [TreeViewItem { Tag: SubmoduleFolderNode childFolder }])
        {
            _name += "/" + childFolder._name;
            TreeViewItem[] children = [.. childFolder.TreeViewNode.Items.Cast<TreeViewItem>()];
            TreeViewNode.Items.Clear();
            Nodes.Clear();
            foreach (TreeViewItem child in children)
            {
                Node node = (Node)child.Tag!;
                node.Reparent(this);
                Nodes.AddNode(node);
                TreeViewNode.Items.Add(child);
            }

            ApplyStyle();
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
