using Avalonia.Controls;
using Avalonia.Media;

using GitCommands;
using GitUI.Compat;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

/// <summary>A common base class for both <see cref="Node"/> and <see cref="Tree"/>.</summary>
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

    internal bool HasChildren => Nodes.Count > 0;

    public NodeBase? Parent { get; private set; }

    /// <summary>The corresponding tree node.</summary>
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
        ApplyStyle(); // toggle multi-selected node style

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

    #region style / appearance
    public virtual void ApplyStyle()
    {
        SetFont(GetFontStyle());
        ToolTip.SetTip(TreeViewNode, null);
    }

    protected virtual WinFormsShims.FontStyle GetFontStyle()
        => IsSelected ? WinFormsShims.FontStyle.Underline : WinFormsShims.FontStyle.Regular;

    private void SetFont(WinFormsShims.FontStyle style)
    {
        if (TreeViewNode.Header is not StackPanel panel
            || panel.Children.OfType<TextBlock>().FirstOrDefault() is not TextBlock text)
        {
            return;
        }

        text.FontFamily = new FontFamily(AppSettings.Font.Name);
        text.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size);
        text.FontWeight = style.HasFlag(WinFormsShims.FontStyle.Bold) ? FontWeight.Bold : FontWeight.Normal;
        text.FontStyle = style.HasFlag(WinFormsShims.FontStyle.Italic) ? Avalonia.Media.FontStyle.Italic : Avalonia.Media.FontStyle.Normal;
        text.TextDecorations = style.HasFlag(WinFormsShims.FontStyle.Underline)
            ? TextDecorations.Underline
            : null;
    }

    private void ResetFont()
        => SetFont(WinFormsShims.FontStyle.Regular);
    #endregion

    internal virtual void OnDoubleClick()
    {
    }
}
