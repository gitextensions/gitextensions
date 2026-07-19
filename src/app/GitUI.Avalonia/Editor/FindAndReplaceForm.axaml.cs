using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using GitExtUtils;
using GitUI.UserControls;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

/// <summary>Loads the next file while a search spans multiple files.</summary>
/// <param name="seekBackward">Whether to select the previous file.</param>
/// <param name="loop">Whether navigation may wrap at the end of the list.</param>
/// <param name="fileStatusItem">Receives the selected file.</param>
/// <param name="loadFileContent">Receives the asynchronous content load.</param>
/// <returns><see langword="true"/> when another file was selected.</returns>
public delegate bool GetNextFileFnc(bool seekBackward, bool loop, out FileStatusItem? fileStatusItem, out Task loadFileContent);

/// <summary>Finds and optionally replaces text in an AvaloniaEdit editor.</summary>
public partial class FindAndReplaceForm : GitExtensionsForm
{
    private readonly TranslationString _findAndReplaceString = new("Find & replace");
    private readonly TranslationString _findString = new("Find");
    private readonly TranslationString _selectionOnlyString = new("selection only");
    private readonly TranslationString _textNotFoundString = new("Text not found");
    private readonly TranslationString _noSearchString = new("No string specified to look for!");
    private readonly TranslationString _textNotFoundString2 = new("Search text not found.");
    private readonly TranslationString _notFoundString = new("Not found");
    private readonly TranslationString _noOccurrencesFoundString = new("No occurrences found.");
    private readonly TranslationString _replacedOccurrencesString = new("Replaced {0} occurrences.");

    private readonly Dictionary<TextEditor, HighlightGroup> _highlightGroups = [];
    private readonly TextEditorSearcher _search;
    private TextEditor? _editor;
    private bool _lastSearchLoopedAround;
    private bool _lastSearchWasBackward;
    private GetNextFileFnc? _fileLoader;

    /// <summary>Initializes a reusable Find/Replace window.</summary>
    public FindAndReplaceForm()
    {
        InitializeComponent();

        btnFindPrevious.Click += (_, _) => this.InvokeAndForget(
            () => FindNextAsync(viaF3: false, searchBackward: true, _textNotFoundString.Text));
        btnFindNext.Click += (_, _) => this.InvokeAndForget(
            () => FindNextAsync(viaF3: false, searchBackward: false, _textNotFoundString.Text));
        btnReplace.Click += (_, _) => this.InvokeAndForget(ReplaceAsync);
        btnReplaceAll.Click += (_, _) => this.InvokeAndForget(ReplaceAllAsync);
        btnHighlightAll.Click += btnHighlightAll_Click;
        btnCancel.Click += (_, _) => Hide();
        Closing += FindAndReplaceForm_Closing;
        Closed += FindAndReplaceForm_Closed;

        InitializeComplete();

        _search = new TextEditorSearcher();
        _search.ScanRegionChanged += ScanRegionChanged;
        AcceptButton = btnFindNext;
    }

    /// <summary>Gets or sets whether replacement controls are displayed.</summary>
    public bool ReplaceMode
    {
        get => txtReplaceWith.IsVisible;
        set
        {
            btnReplace.IsVisible = value;
            btnReplaceAll.IsVisible = value;
            lblReplaceWith.IsVisible = value;
            txtReplaceWith.IsVisible = value;
            btnHighlightAll.IsVisible = !value;
            AcceptButton = value ? btnReplace : btnFindNext;
            UpdateTitleBar();
        }
    }

    /// <summary>Gets the current search text.</summary>
    public string LookFor => txtLookFor.Text ?? string.Empty;

    private void UpdateTitleBar()
    {
        string text = ReplaceMode ? _findAndReplaceString.Text : _findString.Text;
        if (_search.HasScanRegion)
        {
            text += " (" + _selectionOnlyString.Text + ")";
        }

        Text = text;
    }

    /// <summary>Shows this reusable window for the specified editor.</summary>
    public void ShowFor(TextEditor editor, bool replaceMode)
    {
        SetEditor(editor);

        _search.ClearScanRegion();
        if (editor.SelectionLength > 0)
        {
            string selectedText = editor.SelectedText;
            if (selectedText.IndexOfAny(['\r', '\n']) < 0)
            {
                txtLookFor.Text = selectedText;
            }
            else
            {
                _search.SetScanRegion(editor.SelectionStart, editor.SelectionLength);
            }
        }
        else
        {
            txtLookFor.Text = GetWordAtCaret(editor);
        }

        ReplaceMode = replaceMode && !editor.IsReadOnly;

        Window? owner = TopLevel.GetTopLevel(editor) as Window;
        if (!IsVisible)
        {
            if (owner is not null)
            {
                Position = owner.Position + new PixelPoint(100, 100);
                Show(owner);
            }
            else
            {
                Show();
            }
        }
        else
        {
            Activate();
        }

        txtLookFor.SelectAll();
        txtLookFor.Focus();
    }

    private static string GetWordAtCaret(TextEditor editor)
    {
        string text = editor.Text;
        int start = Math.Clamp(editor.CaretOffset, 0, text.Length);
        int end = start;
        while (start > 0 && IsWordCharacter(text[start - 1]))
        {
            start--;
        }

        while (end < text.Length && IsWordCharacter(text[end]))
        {
            end++;
        }

        return text[start..end];
    }

    private static bool IsWordCharacter(char character)
        => char.IsLetterOrDigit(character) || character == '_';

    /// <summary>Finds and selects the next matching range.</summary>
    public async Task<TextRange?> FindNextAsync(bool viaF3, bool searchBackward, string? messageIfNotFound)
    {
        if (string.IsNullOrEmpty(LookFor))
        {
            MessageBoxes.Show(
                this,
                _noSearchString.Text,
                Text ?? string.Empty,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Information);
            return null;
        }

        TextEditor editor = _editor ?? throw new InvalidOperationException("The search window has no text editor.");

        _lastSearchWasBackward = searchBackward;
        _search.LookFor = LookFor;
        _search.MatchCase = chkMatchCase.IsChecked == true;
        _search.MatchWholeWordOnly = chkMatchWholeWord.IsChecked == true;

        FileStatusItem? startItem = null;
        FileStatusItem? currentItem = null;
        TextRange? range;
        do
        {
            if (viaF3
                && _search.HasScanRegion
                && !Globals.IsInRange(editor.CaretOffset, _search.BeginOffset, _search.EndOffset))
            {
                _search.ClearScanRegion();
            }

            int startFrom = editor.CaretOffset - (searchBackward ? 1 : 0);
            if (startFrom == -1)
            {
                startFrom = _search.EndOffset;
            }

            bool isMultiFileSearch = _fileLoader is not null && !_search.HasScanRegion;
            range = _search.FindNext(startFrom, searchBackward, out _lastSearchLoopedAround);
            if (range is not null && (!_lastSearchLoopedAround || !isMultiFileSearch))
            {
                SelectResult(range);
            }
            else if (isMultiFileSearch)
            {
                range = null;
                if (!IsVisible)
                {
                    break;
                }

                if (currentItem is not null && startItem is null)
                {
                    startItem = currentItem;
                }

                if (_fileLoader!(searchBackward, true, out FileStatusItem? selectedItem, out Task loadFileContent))
                {
                    currentItem = selectedItem;
                    try
                    {
                        await loadFileContent;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }
        while (range is null && startItem != currentItem && currentItem is not null);

        if (range is null && messageIfNotFound is not null)
        {
            MessageBoxes.Show(
                this,
                messageIfNotFound,
                " ",
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Information);
        }

        return range;
    }

    private void SelectResult(TextRange range)
    {
        TextEditor editor = _editor ?? throw new InvalidOperationException("The search window has no text editor.");
        editor.Select(range.Offset, range.Length);
        TextLocation location = editor.Document.GetLocation(range.Offset);
        editor.ScrollTo(location.Line, location.Column);
    }

    private void ScanRegionChanged(object? sender, EventArgs e)
    {
        UpdateTitleBar();
    }

    private void btnHighlightAll_Click(object? sender, EventArgs e)
    {
        TextEditor editor = _editor ?? throw new InvalidOperationException("The search window has no text editor.");
        if (!_highlightGroups.TryGetValue(editor, out HighlightGroup? group))
        {
            group = new HighlightGroup(editor);
            _highlightGroups[editor] = group;
        }

        group.ClearMarkers();
        if (string.IsNullOrEmpty(LookFor))
        {
            return;
        }

        _search.LookFor = LookFor;
        _search.MatchCase = chkMatchCase.IsChecked == true;
        _search.MatchWholeWordOnly = chkMatchWholeWord.IsChecked == true;

        int offset = _search.BeginOffset;
        int count = 0;
        while (true)
        {
            TextRange? range = _search.FindNext(offset, searchBackward: false, out bool looped);
            if (range is null || looped)
            {
                break;
            }

            offset = range.EndOffset;
            count++;
            group.AddMarker(range);
        }

        if (count == 0)
        {
            MessageBoxes.Show(
                this,
                _textNotFoundString2.Text,
                _notFoundString.Text,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Information);
        }
        else
        {
            Hide();
        }
    }

    private void FindAndReplaceForm_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (e.CloseReason == WindowCloseReason.WindowClosing && !e.IsProgrammatic)
        {
            e.Cancel = true;
            Hide();
            _search.ClearScanRegion();
            _editor?.TextArea.TextView.Redraw();
        }
    }

    private void FindAndReplaceForm_Closed(object? sender, EventArgs e)
    {
        _search.Dispose();
        foreach (HighlightGroup group in _highlightGroups.Values)
        {
            group.Dispose();
        }

        _highlightGroups.Clear();
    }

    private async Task ReplaceAsync()
    {
        TextEditor editor = _editor ?? throw new InvalidOperationException("The search window has no text editor.");
        if (string.Equals(editor.SelectedText, LookFor, StringComparison.OrdinalIgnoreCase))
        {
            InsertText(txtReplaceWith.Text ?? string.Empty);
        }

        await FindNextAsync(viaF3: false, _lastSearchWasBackward, _textNotFoundString.Text);
    }

    private async Task ReplaceAllAsync()
    {
        TextEditor editor = _editor ?? throw new InvalidOperationException("The search window has no text editor.");
        editor.CaretOffset = _search.BeginOffset;

        int count = 0;
        using (editor.Document.RunUpdate())
        {
            while (await FindNextAsync(viaF3: false, searchBackward: false, messageIfNotFound: null) is not null)
            {
                if (_lastSearchLoopedAround)
                {
                    break;
                }

                count++;
                InsertText(txtReplaceWith.Text ?? string.Empty);
            }
        }

        if (count == 0)
        {
            MessageBoxes.Show(
                this,
                _noOccurrencesFoundString.Text,
                "Information",
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Information);
        }
        else
        {
            MessageBoxes.Show(
                this,
                string.Format(_replacedOccurrencesString.Text, count),
                "Information",
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Information);
            Hide();
        }
    }

    private void InsertText(string text)
    {
        TextEditor editor = _editor ?? throw new InvalidOperationException("The search window has no text editor.");
        int removedLength = editor.SelectionLength;
        bool hasScanRegion = _search.HasScanRegion;
        int beginOffset = _search.BeginOffset;
        int selectionLength = _search.EndOffset - beginOffset;

        editor.SelectedText = text;
        if (hasScanRegion)
        {
            _search.SetScanRegion(beginOffset, selectionLength + text.Length - removedLength);
        }
    }

    private void SetEditor(TextEditor editor)
    {
        _editor = editor;
        _search.Document = editor.Document;
        UpdateTitleBar();
    }

    internal void SetFileLoader(GetNextFileFnc fileLoader)
    {
        _fileLoader = fileLoader;
    }

    internal void GoToOccurrence(TextEditor editor, bool searchBackward)
    {
        if (!_highlightGroups.TryGetValue(editor, out HighlightGroup? group) || group.Markers.Count == 0)
        {
            return;
        }

        int caretOffset = editor.TextArea.Caret.Offset;
        TextRange? marker = searchBackward
            ? group.Markers.LastOrDefault(candidate => candidate.Offset < caretOffset)
            : group.Markers.FirstOrDefault(candidate => candidate.Offset > caretOffset);
        if (marker is null)
        {
            return;
        }

        editor.TextArea.Caret.Offset = marker.Offset;
        editor.ScrollToLine(editor.Document.GetLineByOffset(marker.Offset).LineNumber);
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FindAndReplaceForm _control;

        public TestAccessor(FindAndReplaceForm control)
        {
            _control = control;
        }

        public TextEditorSearcher Search => _control._search;
        public TextBox TxtLookFor => _control.txtLookFor;
        public TextBox TxtReplaceWith => _control.txtReplaceWith;
        public CheckBox ChkMatchCase => _control.chkMatchCase;
        public CheckBox ChkMatchWholeWord => _control.chkMatchWholeWord;

        public Task ReplaceAsync() => _control.ReplaceAsync();

        public void SetEditor(TextEditor editor)
        {
            _control.SetEditor(editor);
        }
    }
}

/// <summary>A matching document range.</summary>
public sealed class TextRange : ISegment
{
    /// <summary>Initializes a matching range.</summary>
    /// <param name="offset">The zero-based document offset.</param>
    /// <param name="length">The number of matching characters.</param>
    public TextRange(int offset, int length)
    {
        Offset = offset;
        Length = length;
    }

    /// <summary>Gets the zero-based document offset.</summary>
    public int Offset { get; }

    /// <summary>Gets the number of matching characters.</summary>
    public int Length { get; }

    /// <summary>Gets the first offset after this range.</summary>
    public int EndOffset => Offset + Length;
}

/// <summary>Finds occurrences in an AvaloniaEdit text document without owning UI.</summary>
public sealed class TextEditorSearcher : IDisposable
{
    private TextDocument? _document;
    private string? _lookForComparison;
    private int? _scanOffset;
    private int _scanLength;

    /// <summary>Occurs when the bounded scan region changes.</summary>
    public event EventHandler? ScanRegionChanged;

    /// <summary>Gets or sets whether character casing must match.</summary>
    public bool MatchCase { get; set; }

    /// <summary>Gets or sets whether matches must have word boundaries.</summary>
    public bool MatchWholeWordOnly { get; set; }

    /// <summary>Gets or sets the document to search.</summary>
    public TextDocument? Document
    {
        get => _document;
        set
        {
            if (ReferenceEquals(_document, value))
            {
                return;
            }

            ClearScanRegion();
            _document = value;
        }
    }

    /// <summary>Gets whether searching is restricted to a range.</summary>
    public bool HasScanRegion => _scanOffset is not null;

    /// <summary>Gets the first searchable offset.</summary>
    public int BeginOffset => _scanOffset ?? 0;

    /// <summary>Gets the first offset after the searchable range.</summary>
    public int EndOffset => _scanOffset is int offset
        ? offset + _scanLength
        : _document?.TextLength ?? 0;

    /// <summary>Gets or sets the text to find.</summary>
    public string? LookFor { get; set; }

    public void Dispose()
    {
        ClearScanRegion();
        GC.SuppressFinalize(this);
    }

    /// <summary>Restricts searching to a document range.</summary>
    /// <param name="offset">The zero-based range offset.</param>
    /// <param name="length">The range length.</param>
    public void SetScanRegion(int offset, int length)
    {
        TextDocument document = _document ?? throw new InvalidOperationException("The searcher has no document.");
        if (_scanOffset is not null)
        {
            document.TextChanged -= DocumentOnTextChanged;
        }

        _scanOffset = offset;
        _scanLength = length;
        document.TextChanged += DocumentOnTextChanged;
        ScanRegionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Restores searching across the complete document.</summary>
    public void ClearScanRegion()
    {
        if (_scanOffset is null)
        {
            return;
        }

        if (_document is not null)
        {
            _document.TextChanged -= DocumentOnTextChanged;
        }

        _scanOffset = null;
        _scanLength = 0;
        ScanRegionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Finds the next match, wrapping once when necessary.</summary>
    /// <param name="beginAtOffset">The offset at which searching starts.</param>
    /// <param name="searchBackward">Whether to search toward the document start.</param>
    /// <param name="loopedAround">Receives whether the search wrapped.</param>
    /// <returns>The matching range, or <see langword="null"/>.</returns>
    public TextRange? FindNext(int beginAtOffset, bool searchBackward, out bool loopedAround)
    {
        string lookFor = LookFor ?? throw new InvalidOperationException("No search text was specified.");
        if (lookFor.Length == 0)
        {
            loopedAround = false;
            return null;
        }

        loopedAround = false;
        int startAt = BeginOffset;
        int endAt = EndOffset;
        int currentOffset = Globals.InRange(beginAtOffset, startAt, endAt);
        _lookForComparison = MatchCase ? lookFor : lookFor.ToUpperInvariant();

        TextRange? result;
        if (searchBackward)
        {
            result = FindNextIn(startAt, currentOffset, searchBackward: true);
            if (result is null)
            {
                loopedAround = true;
                result = FindNextIn(currentOffset, endAt, searchBackward: true);
            }
        }
        else
        {
            result = FindNextIn(currentOffset, endAt, searchBackward: false);
            if (result is null)
            {
                loopedAround = true;
                result = FindNextIn(startAt, currentOffset, searchBackward: false);
            }
        }

        return result;
    }

    private TextRange? FindNextIn(int offset1, int offset2, bool searchBackward)
    {
        TextDocument document = _document ?? throw new InvalidOperationException("The searcher has no document.");
        string lookFor = LookFor ?? throw new InvalidOperationException("No search text was specified.");
        string comparison = _lookForComparison ?? throw new InvalidOperationException("Search comparison text was not initialized.");

        offset2 -= lookFor.Length;
        if (searchBackward)
        {
            for (int offset = offset2; offset >= offset1; offset--)
            {
                if (MatchesAt(document, comparison, lookFor.Length, offset))
                {
                    return new TextRange(offset, lookFor.Length);
                }
            }
        }
        else
        {
            for (int offset = offset1; offset <= offset2; offset++)
            {
                if (MatchesAt(document, comparison, lookFor.Length, offset))
                {
                    return new TextRange(offset, lookFor.Length);
                }
            }
        }

        return null;
    }

    private bool MatchesAt(TextDocument document, string comparison, int length, int offset)
    {
        string candidate = document.GetText(offset, length);
        if (!MatchCase)
        {
            candidate = candidate.ToUpperInvariant();
        }

        return candidate == comparison
            && (!MatchWholeWordOnly || (IsWordBoundary(document, offset) && IsWordBoundary(document, offset + length)));
    }

    private static bool IsWordBoundary(TextDocument document, int offset)
    {
        return offset <= 0
            || offset >= document.TextLength
            || !IsAlphaNumeric(document.GetCharAt(offset - 1))
            || !IsAlphaNumeric(document.GetCharAt(offset));
    }

    private static bool IsAlphaNumeric(char character)
        => char.IsLetterOrDigit(character) || character == '_';

    private void DocumentOnTextChanged(object? sender, EventArgs e)
    {
        ClearScanRegion();
    }
}

/// <summary>Owns the highlighted search-result ranges for one editor.</summary>
public sealed class HighlightGroup : IDisposable
{
    private readonly TextEditor _editor;
    private readonly SearchHighlightRenderer _renderer;

    /// <summary>Initializes a marker group for one editor.</summary>
    /// <param name="editor">The editor that renders the markers.</param>
    public HighlightGroup(TextEditor editor)
    {
        _editor = editor;
        _renderer = new SearchHighlightRenderer(editor);
        editor.TextArea.TextView.BackgroundRenderers.Add(_renderer);
    }

    internal IReadOnlyList<TextRange> Markers => _renderer.Markers;

    public void Dispose()
    {
        _editor.TextArea.TextView.BackgroundRenderers.Remove(_renderer);
        _editor.TextArea.TextView.Redraw();
        GC.SuppressFinalize(this);
    }

    /// <summary>Adds and renders one highlighted range.</summary>
    /// <param name="marker">The range to highlight.</param>
    public void AddMarker(TextRange marker)
    {
        _renderer.Markers.Add(marker);
        _editor.TextArea.TextView.Redraw();
    }

    /// <summary>Removes all highlighted ranges.</summary>
    public void ClearMarkers()
    {
        _renderer.Markers.Clear();
        _editor.TextArea.TextView.Redraw();
    }

    private sealed class SearchHighlightRenderer : IBackgroundRenderer
    {
        private readonly TextEditor _editor;

        public SearchHighlightRenderer(TextEditor editor)
        {
            _editor = editor;
        }

        public List<TextRange> Markers { get; } = [];
        public KnownLayer Layer => KnownLayer.Selection;

        public void Draw(AvaloniaEdit.Rendering.TextView textView, DrawingContext drawingContext)
        {
            if (!textView.VisualLinesValid)
            {
                return;
            }

            foreach (TextRange marker in Markers)
            {
                foreach (Rect rectangle in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    drawingContext.FillRectangle(_editor.SearchResultsBrush, rectangle);
                }
            }
        }
    }
}

/// <summary>Small range helpers retained from the WinForms search implementation.</summary>
public static class Globals
{
    /// <summary>Clamps a value to an inclusive range.</summary>
    public static int InRange(int value, int lowerBound, int upperBound)
        => value < lowerBound ? lowerBound : value > upperBound ? upperBound : value;

    /// <summary>Tests whether a value is in an inclusive range.</summary>
    public static bool IsInRange(int value, int lowerBound, int upperBound)
        => value >= lowerBound && value <= upperBound;
}
