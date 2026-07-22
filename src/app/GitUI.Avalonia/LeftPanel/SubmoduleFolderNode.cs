using Avalonia.Controls;
using GitUI.Properties;

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

    public void CompactSingleChildFolders()
    {
        while (TreeViewNode.Items is [TreeViewItem { Tag: SubmoduleFolderNode childFolder }])
        {
            _name += "/" + childFolder._name;
            TreeViewItem[] children = [.. childFolder.TreeViewNode.Items.Cast<TreeViewItem>()];
            TreeViewNode.Items.Clear();
            foreach (TreeViewItem child in children)
            {
                ((NodeBase)child.Tag!).Reparent(this);
                TreeViewNode.Items.Add(child);
            }

            SetHeader(_name, Images.FolderClosed, isItalic: true);
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(SubmoduleFolderNode node)
    {
        public string DisplayText() => node._name;
    }
}
