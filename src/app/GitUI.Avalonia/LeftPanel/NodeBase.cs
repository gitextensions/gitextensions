using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.LeftPanel;

/// <summary>Common repository-tree model shared by roots and their child nodes.</summary>
internal abstract class NodeBase
{
    protected NodeBase(RepoObjectsTree owner, NodeBase? parent, string caption, IImage icon, bool isBold = false)
    {
        Owner = owner;
        Parent = parent;
        Caption = caption;
        TreeViewNode = new TreeViewItem
        {
            Header = RepoObjectsTree.CreateHeader(caption, icon, isBold),
            Tag = this,
        };
        owner.PrepareTreeViewItem(this);
    }

    protected RepoObjectsTree Owner { get; }

    public NodeBase? Parent { get; }

    public TreeViewItem TreeViewNode { get; }

    public virtual string SearchText => Caption;

    protected string Caption { get; private set; }

    public IEnumerable<NodeBase> DescendantsAndSelf()
    {
        yield return this;
        foreach (TreeViewItem childItem in TreeViewNode.Items.Cast<TreeViewItem>())
        {
            foreach (NodeBase node in ((NodeBase)childItem.Tag!).DescendantsAndSelf())
            {
                yield return node;
            }
        }
    }

    protected void AddChild(NodeBase node)
        => TreeViewNode.Items.Add(node.TreeViewNode);

    protected void SetHeader(string caption, IImage icon, bool isBold = false)
    {
        Caption = caption;
        TreeViewNode.Header = RepoObjectsTree.CreateHeader(caption, icon, isBold);
    }

    internal virtual void OnDoubleClick()
    {
    }
}
