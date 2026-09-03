using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using GitUI.Compat;
using ResourceManager;
using DrawingPoint = System.Drawing.Point;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.UserControls.RevisionGrid;

// The original has no emitted translation keys; its title and action text are supplied by callers.
[Untranslated]
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

    /// <summary>
    /// Gets the item selected by the user.
    /// </summary>
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
            // assume that the branch names or tags are never longer than MaxRefLength symbols long
            // if they are (sanity!) - don't resize past beyond certain limit
            string label = item.Label.Length > MaxRefLength ? item.Label[..MaxRefLength] : item.Label;
            longestLabelLength = Math.Max(longestLabelLength, label.Length);
            rows.Add(new ListBoxItem { Content = item.Label, Tag = item.Item });
        }

        lbxRefs.ItemsSource = rows;

        // What is this magic number "MaxVisibleItemsWithoutScroll + 0.5"?
        // We are limiting number of visible items in the listbox before enabling vscroll, this is to reduce the risk of not fitting on user's screen.
        // When listbox.IntegralHeight=true, the listbox automatically calculates its size and only show full items (i.e. you can't render it
        // at 20px high if the ItemHeight=13px, it can only be 13, 26, 39 etc (NB: slightly more if you account for the furniture)).
        // listbox.PreferredHeight returns the height of the listbox showing all items.
        // For some strange reason, MaxVisibleItemsWithoutScroll*listbox.ItemHeight renders the listbox showing MaxVisibleItemsWithoutScroll-1 items.
        //
        // To fix this, trick the listbox and set its height to show MaxVisibleItemsWithoutScroll+0.5
        // this internally forces it to re-calculates its bounds and then to calculate its preferred height.
        //
        lbxRefs.Height = Math.Min(items.Count, MaxVisibleItemsWithoutScroll) * 24;

        // resize the form to look nice
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
