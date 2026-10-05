using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace GitUI.Compat;

/// <summary>
/// An editable toolbar combo retaining the Fluent ComboBox behavior with a compact desktop
/// drop-down column that cannot be changed through an Avalonia style setter.
/// </summary>
public class ToolbarComboBox : ComboBox
{
    private const int BranchDropDownMinWidth = 200;
    private const int BranchDropDownMaxWidth = 600;
    private Popup? _popup;
    private TextBox? _editor;
    private IDisposable? _popupWidthValue;
    private IDisposable? _popupMaximumWidthValue;
    private (int Minimum, int Maximum)? _resizeBounds;
    private bool _resizeQueued;
    private int _selectionTextDepth;

    /// <summary>
    ///  Defines the source ComboBox's explicitly sized dropdown width.
    /// </summary>
    public static readonly StyledProperty<double> DropDownWidthProperty =
        AvaloniaProperty.Register<ToolbarComboBox, double>(nameof(DropDownWidth), double.NaN);

    public ToolbarComboBox()
    {
        Classes.Add("gitextensions-toolbar-input");
        DropDownOpened += (_, _) => QueueResizeDropDownWidth();
    }

    /// <summary>
    ///  Occurs when the hosted editor updates text, independently of selection text changes.
    /// </summary>
    public event EventHandler? TextUpdate;

    /// <summary>
    ///  Gets or sets the source dropdown width independently of the compact editor width.
    /// </summary>
    public double DropDownWidth
    {
        get => GetValue(DropDownWidthProperty);
        set => SetValue(DropDownWidthProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(ComboBox);

    /// <summary>
    ///  Applies the source ComboBoxExtensions Graphics.MeasureString width calculation.
    /// </summary>
    /// <param name="minWidth">The source minimum dropdown width in logical pixels.</param>
    /// <param name="maxWidth">The source maximum dropdown width in logical pixels.</param>
    public void ResizeDropDownWidth(int minWidth = BranchDropDownMinWidth, int maxWidth = BranchDropDownMaxWidth)
    {
        _resizeBounds = (minWidth, maxWidth);
        int width = Items.Cast<object?>().Select(item =>
            (int)WinFormsGraphicsTextMeasurer.MeasureSize(this, item?.ToString() ?? string.Empty).Width)
            .DefaultIfEmpty(0).Max();
        double scrollBarWidth = _popup?.Child?.GetVisualDescendants().OfType<ScrollBar>()
            .Where(scrollBar => scrollBar.Orientation == Orientation.Vertical && scrollBar.IsVisible)
            .Select(scrollBar => scrollBar.Bounds.Width).DefaultIfEmpty(0).Max() ?? 0;
        DropDownWidth = Math.Min(Math.Max(width + (int)scrollBarWidth, minWidth), maxWidth);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _popupWidthValue?.Dispose();
        _popupMaximumWidthValue?.Dispose();
        _popupWidthValue = null;
        _popupMaximumWidthValue = null;
        base.OnApplyTemplate(e);

        _popup = e.NameScope.Find<Popup>("PART_Popup");
        UpdateDropDownWidth();

        _editor = e.NameScope.Find<TextBox>("PART_EditableTextBox");
        Grid? layout = _editor?.GetVisualParent<Grid>();
        if (layout is not null && layout.ColumnDefinitions.Count > 1)
        {
            layout.ColumnDefinitions[1].Width = new GridLength(16);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        bool selectionText = change.Property == SelectedItemProperty || change.Property == ItemsSourceProperty;
        bool editorUpdate = change.Property == TextProperty && _selectionTextDepth == 0
            && _editor is not null && _editor.Text == change.GetNewValue<string?>();
        if (selectionText)
        {
            _selectionTextDepth++;
        }

        try
        {
            base.OnPropertyChanged(change);
        }
        finally
        {
            if (selectionText)
            {
                _selectionTextDepth--;
            }
        }

        if (editorUpdate)
        {
            // Avalonia funnels editor and selection text through one property. Native
            // ComboBox.TextUpdate belongs only to the former; replacing ItemsSource
            // during a selection text update would clear the just-selected source row.
            TextUpdate?.Invoke(this, EventArgs.Empty);
        }

        if (change.Property == DropDownWidthProperty)
        {
            UpdateDropDownWidth();
        }
        else if (change.Property == FontFamilyProperty
            || change.Property == FontSizeProperty
            || change.Property == FontStyleProperty
            || change.Property == FontWeightProperty)
        {
            QueueResizeDropDownWidth();
        }
    }

    private void QueueResizeDropDownWidth()
    {
        if (_resizeQueued || _resizeBounds is null)
        {
            return;
        }

        _resizeQueued = true;

        // Native control-host typography is applied during owner layout. The actual
        // popup's scrollbar is also unavailable before layout; measure those values
        // after the framework has applied them, without changing the editor's width.
        Dispatcher.UIThread.Post(() =>
        {
            _resizeQueued = false;
            if (_resizeBounds is { } bounds)
            {
                ResizeDropDownWidth(bounds.Minimum, bounds.Maximum);
            }
        }, DispatcherPriority.Loaded);
    }

    private void UpdateDropDownWidth()
    {
        _popupWidthValue?.Dispose();
        _popupMaximumWidthValue?.Dispose();
        _popupWidthValue = null;
        _popupMaximumWidthValue = null;
        if (_popup is not null && double.IsFinite(DropDownWidth) && DropDownWidth > 0)
        {
            // The source editor is 100px wide while its list has an independent width.
            // Keep the existing popup/input template and constrain only its native list.
            _popupWidthValue = _popup.SetValue(WidthProperty, DropDownWidth, BindingPriority.Template);
            _popupMaximumWidthValue = _popup.SetValue(MaxWidthProperty, DropDownWidth, BindingPriority.Template);
        }
    }
}
