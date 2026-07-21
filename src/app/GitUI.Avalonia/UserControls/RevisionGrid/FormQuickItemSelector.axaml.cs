using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ResourceManager;
using DrawingPoint = System.Drawing.Point;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.UserControls.RevisionGrid;

// Avalonia twin of GitUI/UserControls/RevisionGrid/FormQuickItemSelector.cs.
internal partial class FormQuickItemSelector : GitExtensionsForm
{
    private const short MaxVisibleItemsWithoutScroll = 8;
    private const short MaxRefLength = 100;

    public FormQuickItemSelector()
    {
        InitializeComponent();
        AcceptButton = btnAction;
        btnAction.Click += (_, _) => AcceptSelection();
        InitializeComplete();
    }

    public object? SelectedItem => (lbxRefs.SelectedItem as ListBoxItem)?.Tag;

    public DrawingPoint Location
    {
        get => new(Position.X, Position.Y);
        set => Position = new PixelPoint(value.X, value.Y);
    }

    protected void Init(IReadOnlyList<ItemData> items, string buttonText, int selectedIndex = 0)
    {
        btnAction.Content = buttonText;
        if (items.Count == 0)
        {
            DialogResult = WinFormsShims.DialogResult.Cancel;
            return;
        }

        List<ListBoxItem> rows = [];
        int longestLabelLength = 0;
        foreach (ItemData item in items)
        {
            string label = item.Label.Length > MaxRefLength ? item.Label[..MaxRefLength] : item.Label;
            longestLabelLength = Math.Max(longestLabelLength, label.Length);
            rows.Add(new ListBoxItem { Content = item.Label, Tag = item.Item });
        }

        lbxRefs.ItemsSource = rows;
        lbxRefs.Height = Math.Min(items.Count, MaxVisibleItemsWithoutScroll) * 24;
        lbxRefs.Width = Math.Max(150, Math.Min(700, (longestLabelLength * 7) + 28));
        if (selectedIndex >= 0 && selectedIndex < rows.Count)
        {
            lbxRefs.SelectedIndex = selectedIndex;
        }

        lbxRefs.Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = WinFormsShims.DialogResult.Cancel;
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void lbxRefs_DoubleTapped(object? sender, TappedEventArgs e)
        => AcceptSelection();

    private void AcceptSelection()
    {
        if (SelectedItem is not null)
        {
            DialogResult = WinFormsShims.DialogResult.OK;
        }
    }

    internal sealed class ItemData
    {
        public ItemData(string label, object item)
        {
            Label = label;
            Item = item;
        }

        public string Label { get; }

        public object Item { get; }
    }
}
