using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.LeftPanel;

/// <summary>Common repository-tree model shared by roots and their child nodes.</summary>
internal abstract class NodeBase
{
    protected NodeBase(RepoObjectsTree owner, NodeBase? parent, string caption, IImage icon, bool isBold = false, bool isItalic = false)
    {
        Owner = owner;
        Parent = parent;
        Caption = caption;
        Nodes = new Nodes(this is Tree tree ? tree : parent?.Nodes.Tree);
        TreeViewNode = new TreeViewItem
        {
            Header = RepoObjectsTree.CreateHeader(caption, icon, isBold, isItalic),
            Tag = this,
        };
        owner.PrepareTreeViewItem(this);
    }

    protected RepoObjectsTree Owner { get; }

    /// <summary>The child nodes.</summary>
    protected internal Nodes Nodes { get; protected set; }

    internal bool HasChildren => TreeViewNode.Items.Count > 0;

    public NodeBase? Parent { get; private set; }

    public TreeViewItem TreeViewNode { get; }

    /// <summary>
    /// Marks this node to be included in multi-selection. See <see cref="Select(bool, bool)"/>.
    /// Avalonia owns the selected-item collection, so this property projects the native selection
    /// state instead of retaining a second selection flag.
    /// </summary>
    protected internal bool IsSelected
    {
        get => Owner.IsNodeSelected(TreeViewNode);
        set => Owner.SetNodeSelected(TreeViewNode, value);
    }

    /// <summary>
    /// Gets whether the commit that the node represents is currently visible in the revision grid.
    /// </summary>
    public bool Visible { get; set; } = true;

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

    protected internal void AddChild(NodeBase node)
    {
        if (node is Node child)
        {
            Nodes.AddNode(child);
        }

        TreeViewNode.Items.Add(node.TreeViewNode);
    }

    protected internal void Select(bool select, bool includingDescendants = false)
    {
        IsSelected = select;

        // recursively process descendants if required
        if (includingDescendants && HasChildren)
        {
            foreach (NodeBase child in DescendantsAndSelf().Skip(1))
            {
                child.IsSelected = select;
            }
        }
    }

    internal void Reparent(NodeBase parent)
        => Parent = parent;

    protected void SetHeader(string caption, IImage icon, bool isBold = false, bool isItalic = false)
    {
        Caption = caption;
        TreeViewNode.Header = RepoObjectsTree.CreateHeader(caption, icon, isBold, isItalic);
    }

    internal virtual void OnDoubleClick()
    {
    }
}
