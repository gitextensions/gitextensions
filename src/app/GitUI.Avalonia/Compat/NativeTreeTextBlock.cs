using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>Preserves native TREEVIEW label extents without changing the node's painted font.</summary>
internal sealed class NativeTreeTextBlock : TextBlock
{
    private readonly TextBlock _ambientFont = new();
    private TreeView? _tree;
    private bool _usesAmbientFont;

    public NativeTreeTextBlock()
    {
        // TVM_GETITEMRECT includes two text-slot pixels on each side of the ambient
        // HFONT advance. NodeFont affects paint, not the native item's cached width.
        Padding = new Thickness(2, 0);
    }

    /// <summary>
    ///  Gets or sets whether the native node has no authored NodeFont and paints with the tree font.
    /// </summary>
    internal bool UsesAmbientFont
    {
        get => _usesAmbientFont;
        set
        {
            if (_usesAmbientFont == value)
            {
                return;
            }

            _usesAmbientFont = value;
            if (value)
            {
                ClearValue(FontFamilyProperty);
                ClearValue(FontSizeProperty);
                ClearValue(FontWeightProperty);
                ClearValue(FontStyleProperty);
                UpdateAmbientPaintFont();
            }
        }
    }

    protected override Type StyleKeyOverride => typeof(TextBlock);

    protected override Size MeasureOverride(Size availableSize)
    {
        Size measured = base.MeasureOverride(availableSize);
        TreeView? tree = _tree ?? this.FindAncestorOfType<TreeView>();
        if (tree is null || string.IsNullOrEmpty(Text))
        {
            return measured;
        }

        _ambientFont.FontFamily = tree.FontFamily;
        _ambientFont.FontSize = tree.FontSize;
        _ambientFont.FontWeight = tree.FontWeight;
        _ambientFont.FontStyle = tree.FontStyle;
        double width = WinFormsRichEditTextMeasurer.TryGetTextWidth(_ambientFont, Text, out double nativeWidth)
            ? nativeWidth
            : WinFormsTextMeasurer.MeasureSize(_ambientFont, Text).Width;
        return new Size(width + Padding.Left + Padding.Right, measured.Height);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _tree = this.FindAncestorOfType<TreeView>();
        if (_tree is not null)
        {
            _tree.PropertyChanged += OnTreePropertyChanged;
        }

        UpdateAmbientPaintFont();
        InvalidateMeasure();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_tree is not null)
        {
            _tree.PropertyChanged -= OnTreePropertyChanged;
            _tree = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnTreePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == FontFamilyProperty || e.Property == FontSizeProperty
            || e.Property == FontWeightProperty || e.Property == FontStyleProperty)
        {
            UpdateAmbientPaintFont();
            InvalidateMeasure();
        }
    }

    private void UpdateAmbientPaintFont()
    {
        if (UsesAmbientFont && _tree is { } tree)
        {
            // A global TextBlock font style outranks Avalonia's inherited value. Native
            // NodeFont=null instead uses the control font, including runtime changes.
            FontFamily = tree.FontFamily;
            FontSize = tree.FontSize;
            FontWeight = tree.FontWeight;
            FontStyle = tree.FontStyle;
        }
    }
}
