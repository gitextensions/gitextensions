using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitUI.Compat;
using DrawingColor = System.Drawing.Color;
using WinFormsBorderStyle = GitExtensions.Shims.WinForms.BorderStyle;
using AvaloniaSize = Avalonia.Size;

namespace GitUI.CommandsDialogs;

// Avalonia compiled XAML cannot target an open generic class. This non-generic visual base
// is the framework-required shell; SearchControl<T> below retains the original public API.
public partial class SearchControl : UserControl
{
    public SearchControl()
    {
        InitializeComponent();
    }

    protected TextBox SearchTextBox => txtSearchBox;

    protected ListBox SearchResultListBox => listBoxSearchResult;

    protected Border SearchTextBoxBorder => searchBoxBorder;
}

public partial class SearchControl<T> : SearchControl, IDisposable where T : class
{
    private readonly Func<string, IEnumerable<T>> _getCandidates;
    private readonly Action<AvaloniaSize> _onSizeChanged;
    private readonly AsyncLoader _backgroundLoader = new();
    private bool _isUpdatingTextFromCode;
    private DrawingColor _searchBoxBorderDefaultColor = System.Drawing.SystemColors.WindowFrame;
    private DrawingColor _searchBoxBorderHoveredColor = System.Drawing.SystemColors.Highlight;
    private DrawingColor _searchBoxBorderFocusedColor = System.Drawing.SystemColors.Highlight;

    public event Action? OnTextEntered;

    public event Action? OnCancelled;

    public event EventHandler? TextChanged;

    [AllowNull]
    public string Text
    {
        get => SearchTextBox.Text ?? string.Empty;
        set => SearchTextBox.Text = value;
    }

    public SearchControl(Func<string, IEnumerable<T>> getCandidates, Action<AvaloniaSize> onSizeChanged)
    {
        SearchTextBox.LostFocus += delegate { CloseDropDownWhenLostFocus(); };
        SearchResultListBox.LostFocus += delegate { CloseDropDownWhenLostFocus(); };
        SearchTextBox.SelectAll();

        _getCandidates = getCandidates;
        _onSizeChanged = onSizeChanged;
        WireEvents();
        ApplySearchBoxBorderColor();
        AutoFit();

        void CloseDropDownWhenLostFocus()
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!SearchTextBox.IsKeyboardFocusWithin && !SearchResultListBox.IsKeyboardFocusWithin)
                {
                    CloseDropdown();
                }
            });
        }
    }

    public void FocusSearchBox()
    {
        SearchTextBox.Focus();
    }

    public void CloseDropdown()
    {
        SearchResultListBox.IsVisible = false;
    }

    public WinFormsBorderStyle SearchBoxBorderStyle
    {
        get => SearchTextBoxBorder.BorderThickness == default ? WinFormsBorderStyle.None : WinFormsBorderStyle.FixedSingle;
        set => SearchTextBoxBorder.BorderThickness = value == WinFormsBorderStyle.None ? default : new Thickness(1);
    }

    public DrawingColor SearchBoxBorderDefaultColor
    {
        get => _searchBoxBorderDefaultColor;
        set
        {
            _searchBoxBorderDefaultColor = value;
            ApplySearchBoxBorderColor();
        }
    }

    public DrawingColor SearchBoxBorderHoveredColor
    {
        get => _searchBoxBorderHoveredColor;
        set
        {
            _searchBoxBorderHoveredColor = value;
            ApplySearchBoxBorderColor();
        }
    }

    public DrawingColor SearchBoxBorderFocusedColor
    {
        get => _searchBoxBorderFocusedColor;
        set
        {
            _searchBoxBorderFocusedColor = value;
            ApplySearchBoxBorderColor();
        }
    }

    private void SearchForCandidates(IEnumerable<T> candidates)
    {
        int selectionStart = SearchTextBox.SelectionStart;
        int selectionEnd = SearchTextBox.SelectionEnd;
        IReadOnlyList<T> items = candidates.Take(20).ToList();
        SearchResultListBox.ItemsSource = items;

        if (items.Count > 0)
        {
            SearchResultListBox.SelectedIndex = 0;
        }

        SearchTextBox.SelectionStart = selectionStart;
        SearchTextBox.SelectionEnd = selectionEnd;
        AutoFit();
    }

    private void AutoFit()
    {
        if (SearchResultListBox.ItemCount == 0)
        {
            SearchResultListBox.IsVisible = false;
            return;
        }

        SearchResultListBox.IsVisible = true;

        double renderScale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        Typeface typeface = new(
            SearchTextBox.FontFamily,
            SearchTextBox.FontStyle,
            SearchTextBox.FontWeight,
            SearchTextBox.FontStretch);
        double width = 300 / renderScale;
        foreach (object? item in SearchResultListBox.Items)
        {
            FormattedText measuredText = new(
                Convert.ToString(item) ?? string.Empty,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                SearchTextBox.FontSize,
                foreground: null);
            width = Math.Max(width, Math.Ceiling(measuredText.WidthIncludingTrailingWhitespace * renderScale) / renderScale);
        }

        // WinForms assigns this runtime popup size in physical pixels; Avalonia sizes in DIPs.
        double lineHeight = SearchTextBox.FontSize;
        if (FontManager.Current.TryGetGlyphTypeface(typeface, out GlyphTypeface? glyphTypeface))
        {
            FontMetrics metrics = glyphTypeface.Metrics;
            lineHeight = metrics.LineSpacing * SearchTextBox.FontSize / metrics.DesignEmHeight;
        }

        double itemHeight = Math.Ceiling(lineHeight * renderScale) / renderScale;
        double listHeight = Math.Min(800 / renderScale, itemHeight * (SearchResultListBox.ItemCount + 1));
        SearchResultListBox.Width = width;
        SearchResultListBox.Height = listHeight;

        _onSizeChanged(new AvaloniaSize(width, listHeight + Math.Max(22, SearchTextBox.Bounds.Height)));
    }

    public T? SelectedItem => (T?)SearchResultListBox.SelectedItem;

    void IDisposable.Dispose()
    {
        _backgroundLoader.Cancel();
        _backgroundLoader.Dispose();
        GC.SuppressFinalize(this);
    }

    private void txtSearchBox_TextChange(object? sender, EventArgs e)
    {
        TextChanged?.Invoke(this, e);
        if (_isUpdatingTextFromCode)
        {
            _isUpdatingTextFromCode = false;
            return;
        }

        string selectedText = SearchTextBox.Text ?? string.Empty;

        _backgroundLoader.LoadAsync(() => _getCandidates(selectedText), SearchForCandidates);
    }

    private void txtSearchBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            ItemSelectedFromList();
        }
        else if (e.Key == Key.Escape)
        {
            SearchResultListBox.SelectedItem = null;
            SearchResultListBox.IsVisible = false;
            e.Handled = true;
            OnCancelled?.Invoke();
        }
    }

    private void ItemSelectedFromList()
    {
        _isUpdatingTextFromCode = true;
        if (SearchResultListBox.SelectedItem is not null)
        {
            SearchTextBox.Text = SearchResultListBox.SelectedItem.ToString();
        }

        SearchResultListBox.IsVisible = false;
        OnTextEntered?.Invoke();
    }

    private void txtSearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            if (SearchResultListBox.ItemCount > 1)
            {
                SearchResultListBox.SelectedIndex = (SearchResultListBox.SelectedIndex + 1) % SearchResultListBox.ItemCount;
                e.Handled = true;
            }
        }

        if (e.Key == Key.Up)
        {
            if (SearchResultListBox.ItemCount > 1)
            {
                int newSelectedIndex = SearchResultListBox.SelectedIndex - 1;
                if (newSelectedIndex < 0)
                {
                    newSelectedIndex = SearchResultListBox.ItemCount - 1;
                }

                SearchResultListBox.SelectedIndex = newSelectedIndex;
                e.Handled = true;
            }
        }

        if (e.Key == Key.Enter || e.Key == Key.Escape)
        {
            e.Handled = true;
        }
    }

    private void listBoxSearchResult_DoubleClick(object? sender, TappedEventArgs e)
    {
        ItemSelectedFromList();
    }

    private void WireEvents()
    {
        SearchTextBox.TextChanged += txtSearchBox_TextChange;
        SearchTextBox.KeyDown += txtSearchBox_KeyDown;
        SearchTextBox.KeyUp += txtSearchBox_KeyUp;
        SearchTextBox.GotFocus += (_, _) => ApplySearchBoxBorderColor();
        SearchTextBox.LostFocus += (_, _) => ApplySearchBoxBorderColor();
        SearchTextBoxBorder.PointerEntered += (_, _) => ApplySearchBoxBorderColor();
        SearchTextBoxBorder.PointerExited += (_, _) => ApplySearchBoxBorderColor();
        SearchResultListBox.DoubleTapped += listBoxSearchResult_DoubleClick;
    }

    private void ApplySearchBoxBorderColor()
    {
        DrawingColor color = SearchTextBox.IsKeyboardFocusWithin
            ? _searchBoxBorderFocusedColor
            : SearchTextBoxBorder.IsPointerOver
                ? _searchBoxBorderHoveredColor
                : _searchBoxBorderDefaultColor;
        SearchTextBoxBorder.BorderBrush = new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(color));
    }
}
