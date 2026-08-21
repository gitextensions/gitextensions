using Avalonia;
using Avalonia.Controls;
using GitUI.Compat;
using ResourceManager;
using AvaloniaSize = Avalonia.Size;

namespace GitUI.CommandsDialogs;

// Avalonia compiled XAML cannot target an open generic class. This non-generic visual base
// is the framework-required shell; SearchWindow<T> below retains the original public API.
[Untranslated]
public partial class SearchWindow : GitExtensionsFormBase
{
    public SearchWindow()
        : this(initializeComplete: true)
    {
    }

    protected SearchWindow(bool initializeComplete)
    {
        InitializeComponent();
        if (initializeComplete)
        {
            InitializeComplete();
        }
    }

    protected ContentControl SearchControlHost => tableLayoutPanel1.Children
        .OfType<ContentControl>()
        .Single(control => Grid.GetRow(control) == 1);

    protected Grid SearchTableLayoutPanel => tableLayoutPanel1;

    protected Label SearchPromptLabel => lblEnterFileName;
}

public partial class SearchWindow<T> : SearchWindow where T : class
{
    private readonly SearchControl<T> _searchControl;

    public SearchWindow(Func<string, IEnumerable<T>> getCandidates)
        : base(initializeComplete: false)
    {
        _searchControl = new SearchControl<T>(getCandidates, OnChildSizeChanged);
        _searchControl.OnTextEntered += Close;
        _searchControl.OnCancelled += Close;
        SearchControlHost.Content = _searchControl;
        InitializeComplete();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _searchControl.FocusSearchBox();
    }

    protected override void OnClosed(EventArgs e)
    {
        ((IDisposable)_searchControl).Dispose();
        base.OnClosed(e);
    }

    private void OnChildSizeChanged(AvaloniaSize newSize)
    {
        double labelHeight = Math.Max(24, SearchPromptLabel.Bounds.Height);
        SearchTableLayoutPanel.Width = newSize.Width;
        Width = newSize.Width;
        SearchTableLayoutPanel.Height = newSize.Height + labelHeight;
        Height = SearchTableLayoutPanel.Height;
    }

    public T? SelectedItem => _searchControl.SelectedItem;
}
