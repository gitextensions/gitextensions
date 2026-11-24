using Avalonia.Controls;
using Avalonia.Media;

using GitCommands;
using GitUI.Compat;
using Microsoft;
using TreeNode = Avalonia.Controls.TreeViewItem;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

/// <summary>A common base class for both <see cref="Node"/> and <see cref="Tree"/>.</summary>
internal abstract class NodeBase
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="NodeBase"/> class for <see cref="Tree"/> nodes.
    /// </summary>
    protected NodeBase(TreeNode treeNode)
    {
        Nodes = new Nodes((Tree)this);
        TreeViewNode = treeNode;
        treeNode.Tag = this;
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="NodeBase"/> class for non-<see cref="Tree"/> nodes.
    /// </summary>
    protected NodeBase(Tree? tree)
    {
        Nodes = new Nodes(tree);
    }

    protected RepoObjectsTree Owner
    {
        get
        {
            Validates.NotNull(Nodes.Tree);
            return Nodes.Tree.OwnerControl;
        }
    }

    /// <summary>The child nodes.</summary>
    protected internal Nodes Nodes { get; protected set; }

    internal bool HasChildren => Nodes.Count > 0;

    // Framework constraint: the parent is the node of the Avalonia item that contains this node's item.
    public NodeBase? Parent => (TreeViewNode.Parent as TreeNode)?.Tag as NodeBase;

    /// <summary>The corresponding tree node.</summary>
    protected internal virtual TreeNode TreeViewNode { get => field ?? throw new NotImplementedException(); set; }

    /// <summary>
    /// Marks this node to be included in multi-selection. See <see cref="Select(bool, bool)"/>.
    /// This is remembered here instead of relying on the status of <see cref="TreeViewNode"/>
    /// because <see cref="Nodes.FillTreeViewNode(TreeNode)"/> recycles <see cref="TreeNode"/>s
    /// and may change the association between <see cref="Node"/> and <see cref="TreeNode"/>.
    /// </summary>
    protected internal bool IsSelected { get; set; }

    /// <summary>
    /// Gets whether the commit that the node represents is currently visible in the revision grid.
    /// </summary>
    public bool Visible { get; set; }

    public virtual string SearchText => Caption;

    protected string Caption => TreeViewNode.GetText();

    public IEnumerable<NodeBase> DescendantsAndSelf()
    {
        yield return this;
        foreach (Node child in Nodes)
        {
            foreach (NodeBase node in child.DescendantsAndSelf())
            {
                yield return node;
            }
        }
    }

    protected internal void Select(bool select, bool includingDescendants = false)
    {
        IsSelected = select;
        ApplyStyle(); // toggle multi-selected node style

        // recursively process descendants if required
        if (includingDescendants && HasChildren)
        {
            foreach (Node child in Nodes)
            {
                child.Select(select, includingDescendants);
            }
        }
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

        if (style == WinFormsShims.FontStyle.Regular)
        {
            // For regular, reset to use the NativeTreeView font
            ResetFont();
            return;
        }

        if (text is NativeTreeTextBlock nativeText)
        {
            nativeText.UsesAmbientFont = false;
            nativeText.UsesNodeFontForMeasurement = style.HasFlag(WinFormsShims.FontStyle.Bold) || style.HasFlag(WinFormsShims.FontStyle.Italic);
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
    {
        if (TreeViewNode.Header is StackPanel panel
            && panel.Children.OfType<NativeTreeTextBlock>().FirstOrDefault() is { } text)
        {
            text.UsesAmbientFont = true;
            text.UsesNodeFontForMeasurement = false;
            text.TextDecorations = null;
        }
    }
    #endregion

    internal virtual void OnDoubleClick()
    {
    }
}
