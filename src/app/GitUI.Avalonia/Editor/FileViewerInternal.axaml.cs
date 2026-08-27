using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Editor.Diff;
using GitUI.Theming;
using Color = System.Drawing.Color;
using Font = GitExtensions.Shims.WinForms.Font;
using KeyEventHandler = System.EventHandler<Avalonia.Input.KeyEventArgs>;
using MouseEventHandler = System.EventHandler<Avalonia.Input.PointerEventArgs>;

namespace GitUI.Editor;

/// <summary>Hosts the text editor and file-specific highlighting used by <see cref="FileViewer"/>.</summary>
public partial class FileViewerInternal : GitModuleControl, IFileViewer
{
    private readonly CommitMessageValidationRenderer _validationRenderer;
    private readonly SelectionOccurrenceRenderer _selectionOccurrenceRenderer;
    private readonly List<HighlightedLines> _lineHighlights = [];
    private GetNextFileFnc? _findAndReplaceFileLoader;
    private GitHighlightingStrategyBase? _gitHighlightingStrategy;

    /// <summary>
    /// Raised when the Escape key is pressed (and only when no selection exists, as the default behaviour of escape is to clear the selection).
    /// </summary>
    public event Action? EscapePressed;

    public event EventHandler<SelectedLineEventArgs>? SelectedLineChanged;
    public event MouseEventHandler? MouseMove;
    public event EventHandler? MouseEnter;
    public event EventHandler? MouseLeave;
    public new event KeyEventHandler? KeyUp;
    public event EventHandler? DoubleClick;
    private FindAndReplaceForm? _findAndReplaceForm;
    private readonly CurrentViewPositionCache _currentViewPositionCache;
    private readonly DiffViewerLineNumberControl _lineNumbersControl;
    private ITextHighlightService _textHighlightService = TextHighlightService.Instance;
    private bool _shouldScrollToTop;
    private bool _shouldScrollToBottom;
    private readonly int _bottomBlankHeight = 300;
    private ContinuousScrollEventManager? _continuousScrollEventManager;
    private BlameAuthorMargin? _authorsAvatarMargin;
    private bool _showGutterAvatars;

    /// <summary>Initializes the internal editor.</summary>
    public FileViewerInternal()
    {
        InitializeComponent();
        _validationRenderer = new CommitMessageValidationRenderer();
        _selectionOccurrenceRenderer = new SelectionOccurrenceRenderer();
        _currentViewPositionCache = new CurrentViewPositionCache(this);
        _lineNumbersControl = new DiffViewerLineNumberControl(TextEditor);
        _lineNumbersControl.Clear();
        TextEditor.TextArea.LeftMargins.Insert(0, _lineNumbersControl);
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(_validationRenderer);
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(_selectionOccurrenceRenderer);
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(new HighlightBackgroundRenderer(_lineHighlights));
        TextEditor.TextChanged += (sender, e) =>
        {
            UpdateValidationMarkers();
            TextChanged?.Invoke(sender, e);
        };
        TextEditor.TextArea.SelectionChanged += SelectionManagerSelectionChanged;
        TextEditor.TextArea.Caret.PositionChanged += (_, _) =>
            SelectedLineChanged?.Invoke(this, new SelectedLineEventArgs(TextEditor.TextArea.Caret.Line - 1));
        TextEditor.TextArea.TextView.ScrollOffsetChanged += (_, _) =>
        {
            HScrollPositionChanged?.Invoke(this, EventArgs.Empty);
            VScrollPositionChanged?.Invoke(this, EventArgs.Empty);
        };
        TextEditor.PointerMoved += (sender, e) => MouseMove?.Invoke(sender, e);
        TextEditor.PointerEntered += (sender, e) => MouseEnter?.Invoke(sender, e);
        TextEditor.PointerExited += (sender, e) => MouseLeave?.Invoke(sender, e);
        TextEditor.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Escape && TextEditor.SelectionLength == 0)
            {
                EscapePressed?.Invoke();
            }
        };
        TextEditor.KeyUp += (sender, e) => KeyUp?.Invoke(sender, e);
        TextEditor.DoubleTapped += (_, _) => DoubleClick?.Invoke(this, EventArgs.Empty);
        TextEditor.PointerWheelChanged += TextArea_MouseWheel;
        AttachedToVisualTree += (_, _) => TextEditor.ContextMenu = ContextMenu;
        DetachedFromVisualTree += (_, _) =>
        {
            //// _textHighlightService not disposable
            //// _lineNumbersControl not disposable
            //// _currentViewPositionCache not disposable
            CloseFindAndReplaceForm();
        };
        VRulerPosition = AppSettings.DiffVerticalRulerPosition;
        InitializeComplete();
    }

    public void DontMarkGutterSelectedLine()
    {
        _lineNumbersControl.DontMarkSelectedLine();
    }

    public void SetContinuousScrollManager(ContinuousScrollEventManager continuousScrollEventManager)
    {
        _continuousScrollEventManager = continuousScrollEventManager;
    }

    internal void GutterSelectedLineChanged(object? sender, EventArgs e)
    {
        GutterSelectedLineChanged(TextEditor.TextArea.Caret.Line - 1);
    }

    internal void GutterSelectedLineChanged(int lineNo)
    {
        _lineNumbersControl.InvalidateVisual();
    }

    private void SelectionManagerSelectionChanged(object? sender, EventArgs e)
    {
        string word = TextEditor.SelectedText;
        List<global::GitUI.TextRange> markers = GetTextMarkersMatchingWord(word);
        _selectionOccurrenceRenderer.Markers = markers;
        TextEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }

    /// <summary>
    /// Create a list of TextMarker instances in the Document that match the given text.
    /// </summary>
    /// <param name="word">The text to match.</param>
    private List<global::GitUI.TextRange> GetTextMarkersMatchingWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return [];
        }

        List<global::GitUI.TextRange> selectionMarkers = [];
        string textContent = TextEditor.Text;
        int indexMatch = -1;
        do
        {
            indexMatch = textContent.IndexOf(word, indexMatch + 1, StringComparison.OrdinalIgnoreCase);
            if (indexMatch >= 0)
            {
                selectionMarkers.Add(new global::GitUI.TextRange(indexMatch, word.Length));
            }
        }
        while (indexMatch >= 0 && indexMatch < textContent.Length - 1);

        return selectionMarkers;
    }

    public Font Font
    {
        get
        {
            GitExtensions.Shims.WinForms.FontStyle style = GitExtensions.Shims.WinForms.FontStyle.Regular;
            if (TextEditor.FontWeight == FontWeight.Bold)
            {
                style |= GitExtensions.Shims.WinForms.FontStyle.Bold;
            }

            if (TextEditor.FontStyle == FontStyle.Italic)
            {
                style |= GitExtensions.Shims.WinForms.FontStyle.Italic;
            }

            return new Font(TextEditor.FontFamily.Name, (float)TextEditor.FontSize, style);
        }

        set
        {
            TextEditor.FontFamily = new FontFamily(value.Name);
            TextEditor.FontSize = value.Size;
            TextEditor.FontWeight = value.Bold ? FontWeight.Bold : FontWeight.Normal;
            TextEditor.FontStyle = value.Italic ? FontStyle.Italic : FontStyle.Normal;
        }
    }

    public Action? OpenWithDifftool { get; private set; }

    /// <summary>
    /// Move the file viewer cursor position to the next TextMarker found in the document that matches the AppColor.HighlightAllOccurences.
    /// </summary>
    public void GoToNextOccurrence()
    {
        int offset = TextEditor.TextArea.Caret.Offset;
        global::GitUI.TextRange? marker = _selectionOccurrenceRenderer.Markers
            .FirstOrDefault(candidate => candidate.Offset > offset);
        if (marker is not null)
        {
            TextEditor.TextArea.Caret.Offset = marker.Offset;
            TextEditor.ScrollToLine(TextEditor.Document.GetLineByOffset(marker.Offset).LineNumber);
        }
    }

    /// <summary>
    /// Move the file viewer cursor position to the previous TextMarker found in the document that matches the AppColor.HighlightAllOccurences.
    /// </summary>
    public void GoToPreviousOccurrence()
    {
        int offset = TextEditor.TextArea.Caret.Offset;
        global::GitUI.TextRange? marker = _selectionOccurrenceRenderer.Markers
            .LastOrDefault(candidate => candidate.Offset < offset);
        if (marker is not null)
        {
            TextEditor.TextArea.Caret.Offset = marker.Offset;
            TextEditor.ScrollToLine(TextEditor.Document.GetLineByOffset(marker.Offset).LineNumber);
        }
    }

    public void Find(bool replace)
    {
        GetOrCreateFindAndReplaceForm().ShowFor(TextEditor, replace && !IsReadOnly);
        OnVScrollPositionChanged(EventArgs.Empty);
    }

    public async Task FindNextAsync(bool searchForwardOrOpenWithDifftool)
    {
        FindAndReplaceForm findAndReplaceForm = GetOrCreateFindAndReplaceForm();
        if (searchForwardOrOpenWithDifftool
            && OpenWithDifftool is not null
            && string.IsNullOrEmpty(findAndReplaceForm.LookFor))
        {
            OpenWithDifftool();
            return;
        }

        await findAndReplaceForm.FindNextAsync(
            viaF3: true,
            searchBackward: !searchForwardOrOpenWithDifftool,
            messageIfNotFound: "Text not found");
        OnVScrollPositionChanged(EventArgs.Empty);
    }

    public event EventHandler? HScrollPositionChanged;
    public event EventHandler? VScrollPositionChanged;
    public event EventHandler? TextChanged;

    public void ScrollToTop()
    {
        _shouldScrollToTop = true;
    }

    public void ScrollToBottom()
    {
        _shouldScrollToBottom = true;
    }

    public string GetText() => TextEditor.Text;

    public bool? ShowLineNumbers
    {
        get => TextEditor.ShowLineNumbers;
        set => TextEditor.ShowLineNumbers = value ?? false;
    }

    /// <summary>
    /// Set plain text in the editor.
    /// </summary>
    /// <param name="text">The text to set in the editor.</param>
    /// <param name="openWithDifftool">The command to open the difftool.</param>
    public void SetText(string text, Action? openWithDifftool)
    {
        SetText(text, openWithDifftool, viewMode: ViewMode.Text, useGitColoring: false, contentIdentification: null);
    }

    /// <summary>
    /// Set plain text in the editor.
    /// </summary>
    /// <param name="text">The text to set in the editor.</param>
    /// <param name="openWithDifftool">The command to open the difftool.</param>
    /// <param name="viewMode">the view viewMode in the file viewer, the kind of info shown</param>
    /// <returns><see langword="true"/> if a position was set.</returns>
    public bool SetText(string text, Action? openWithDifftool, ViewMode viewMode, bool useGitColoring, string? contentIdentification)
    {
        _currentViewPositionCache.Capture();
        OpenWithDifftool = openWithDifftool;
        ClearHighlighting();
        TextEditor.Document ??= new TextDocument();
        TextEditor.Document.Text = text;
        TextEditor.ScrollToHome();

        // important to set after the text was changed
        // otherwise the may be rendering artifacts as noted in #5568
        TextEditor.TextArea.TextView.Redraw();

        // Restore position if contentIdentification matches the capture
        bool positionSet = _currentViewPositionCache.Restore(contentIdentification) && LineAtCaret > FirstLineAfterHeader;

        if (_shouldScrollToBottom)
        {
            TextEditor.ScrollToEnd();
            TextEditor.ScrollToVerticalOffset(Math.Max(0, VScrollPosition - _bottomBlankHeight));
            positionSet = true;
        }
        else if (_shouldScrollToTop)
        {
            TextEditor.ScrollToHome();
            positionSet = true;
        }

        _shouldScrollToTop = false;
        _shouldScrollToBottom = false;
        return positionSet;
    }

    public void SetHighlighting(string syntax)
    {
        SetHighlightingStrategy(string.IsNullOrEmpty(syntax)
            ? null
            : HighlightingManager.Instance.GetDefinition(syntax));
    }

    public void SetHighlightingForFile(string filename)
    {
        string extension = Path.GetExtension(filename);
        SetHighlightingStrategy(string.IsNullOrEmpty(extension)
            ? null
            : HighlightingManager.Instance.GetDefinitionByExtension(extension));
    }

    private void SetHighlightingStrategy(IHighlightingDefinition? highlightingStrategy)
    {
        TextEditor.SyntaxHighlighting = highlightingStrategy;
        TextEditor.TextArea.TextView.Redraw();
    }

    public string GetSelectedText() => TextEditor.SelectedText;

    public int GetSelectionPosition() => TextEditor.SelectionStart;

    public int GetSelectionLength() => TextEditor.SelectionLength;

    public void EnableScrollBars(bool enable)
    {
        ScrollBarVisibility visibility = enable ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
        TextEditor.HorizontalScrollBarVisibility = visibility;
        TextEditor.VerticalScrollBarVisibility = visibility;
    }

    /// <summary>
    /// Add text highlighting to the document.
    /// This is primarily used to highlight changed files for diffs.
    /// </summary>
    public void AddTextHighlighting()
    {
        if (TextEditor.Document is not null)
        {
            _textHighlightService.AddTextHighlighting(TextEditor.Document);
            TextEditor.TextArea.TextView.Redraw();
        }
    }

    /// <summary>
    /// Get the full prefix for lines with differences.
    /// </summary>
    /// <returns>An array with the prefixes.</returns>
    /// <exception cref="ArgumentException"></exception>
    public string[] GetFullDiffPrefixes()
    {
        if (_textHighlightService is not DiffHighlightService highlightService)
        {
            throw new ArgumentException($"Unexpected highlight service {_textHighlightService.GetType()}, not a diff type.");
        }

        return highlightService.GetFullDiffPrefixes();
    }

    /// <summary>
    /// Check if the line is a search match.
    /// For normal diff, this a added deleted line.
    /// For range-diff, this is a header.
    /// </summary>
    /// <returns>An array with the prefixes.</returns>
    /// <param name="indexInText">The line to check.</param>
    private bool IsSearchMatch(int indexInText)
    {
        // AvaloniaEdit's caret is one-based; the shared diff-line map retains Scintilla's zero-based indexes.
        return _textHighlightService.IsSearchMatch(_lineNumbersControl, indexInText - 1);
    }

    private int FirstLineAfterHeader
        => _textHighlightService is (PatchHighlightService or CombinedDiffHighlightService) ? 5 : 0;

    /// <summary>
    /// Go to the first change.
    /// For normal diffs, this is the first diff.
    /// For range-diff, it is the first block of commit summary header.
    /// </summary>
    public void GoToFirstChange(int contextLines)
    {
        GoToNextChange(contextLines, fromTop: true);
    }

    /// <summary>
    /// Go to the next change.
    /// For normal diffs, this is the next diff.
    /// For range-diff, it is the next block of commit summary header.
    /// </summary>
    /// <param name="contextLines">Number of context lines, to include header for new diff.</param>
    public void GoToNextChange(int contextLines)
    {
        GoToNextChange(contextLines, fromTop: false);
    }

    private void GoToNextChange(int contextLines, bool fromTop)
    {
        // Skip the file header
        int firstValidIndex = FirstLineAfterHeader;
        int startIndex = fromTop ? firstValidIndex : Math.Max(firstValidIndex, LineAtCaret);
        int totalNumberOfLines = TotalNumberOfLines;

        bool emptyLineCheck = fromTop;
        for (int line = startIndex; line < totalNumberOfLines; line++)
        {
            if (IsSearchMatch(line))
            {
                if (emptyLineCheck)
                {
                    if (fromTop && IsLineVisible(line))
                    {
                        // Keep FirstVisibleLine, but let it be clamped in order to avoid scrolling the text out of view
                        FirstVisibleLine = FirstVisibleLine;
                    }
                    else
                    {
                        // Include the header with the (possible) function summary line
                        FirstVisibleLine = Math.Max(line - contextLines - 1, 0);
                    }

                    LineAtCaret = line;
                    return;
                }
            }
            else
            {
                emptyLineCheck = true;
            }
        }

        return;

        bool IsLineVisible(int line)
        {
            int firstVisibleLine = FirstVisibleLine;
            int visibleLineCount = TextEditor.TextArea.TextView.VisualLinesValid
                ? TextEditor.TextArea.TextView.VisualLines.Count
                : 0;
            return firstVisibleLine <= line && line < firstVisibleLine + visibleLineCount;
        }
    }

    public void GoToPreviousChange(int contextLines)
    {
        // Skip the file header
        bool hasDiffHeader = _textHighlightService is (PatchHighlightService or CombinedDiffHighlightService);
        int firstValidIndex = hasDiffHeader ? 4 : 0;
        int startIndex = LineAtCaret;
        while (startIndex > firstValidIndex && IsSearchMatch(startIndex))
        {
            // Go to line before of current diff block
            startIndex--;
        }

        // Find the start of previous diff
        bool emptyLineCheck = false;
        for (int line = startIndex; line >= firstValidIndex; line--)
        {
            if (IsSearchMatch(line) && line > firstValidIndex)
            {
                emptyLineCheck = true;
                continue;
            }

            if (!emptyLineCheck)
            {
                continue;
            }

            if (!IsSearchMatch(line))
            {
                // Restore to last diff
                line++;
            }

            // Include the header with the (possible) function summary line
            FirstVisibleLine = Math.Max(line - contextLines - 1, 0);
            LineAtCaret = line;
            return;
        }
    }

    /// <summary>
    /// Copy the the text selected in the editor, filtering out lines starting with the given character.
    /// </summary>
    /// <param name="startChar">The start character to ignore for diffs.</param>
    public void CopyNotStartingWith(char startChar)
    {
        string text = GetSelectedText();
        bool noSelection = false;
        if (string.IsNullOrEmpty(text))
        {
            text = GetText();
            noSelection = true;
        }

        if (_textHighlightService is DiffHighlightService)
        {
            int position = noSelection ? 0 : GetSelectionPosition();
            string fileText = GetText();
            if (position > 0 && fileText[position - 1] != '\n')
            {
                // add artificial space if selected text is not starting from line beginning, it will be removed later
                text = " " + text;
            }

            IEnumerable<string> lines = text.LazySplit('\n')
                .Where(line => line.Length == 0 || line[0] != startChar || (line.Length > 2 && line[1] == line[0] && line[2] == line[0]));
            int headerPosition = fileText.IndexOf("\n@@", StringComparison.Ordinal);
            if (headerPosition <= position)
            {
                // if header is selected then don't remove diff extra chars
                const string specials = " -+";
                lines = lines.Select(line => line.Length > 0 && specials.Contains(line[0]) ? line[1..] : line);
            }

            text = string.Join("\n", lines);
        }

        ClipboardUtil.TrySetText(text.AdjustLineEndings(Module.GetEffectiveSetting<AutoCRLFType>("core.autocrlf")));
    }

    public int HScrollPosition
    {
        get => (int)TextEditor.TextArea.TextView.ScrollOffset.X;
        set => TextEditor.ScrollToHorizontalOffset(value);
    }

    public int VScrollPosition
    {
        get => (int)TextEditor.TextArea.TextView.ScrollOffset.Y;
        set => TextEditor.ScrollToVerticalOffset(value);
    }

    public EolMarkerStyle EolMarkerStyle
    {
        get => !TextEditor.Options.ShowEndOfLine
            ? EolMarkerStyle.None
            : TextEditor.Options.EndOfLineLFGlyph == "¶"
                ? EolMarkerStyle.Glyph
                : EolMarkerStyle.Text;
        set
        {
            TextEditor.Options.ShowEndOfLine = value != EolMarkerStyle.None;
            TextEditor.Options.EndOfLineCRLFGlyph = value == EolMarkerStyle.Glyph ? "¶" : "\\r\\n";
            TextEditor.Options.EndOfLineCRGlyph = value == EolMarkerStyle.Glyph ? "¶" : "\\r";
            TextEditor.Options.EndOfLineLFGlyph = value == EolMarkerStyle.Glyph ? "¶" : "\\n";
        }
    }

    public bool ShowSpaces
    {
        get => TextEditor.Options.ShowSpaces;
        set => TextEditor.Options.ShowSpaces = value;
    }

    public bool ShowTabs
    {
        get => TextEditor.Options.ShowTabs;
        set => TextEditor.Options.ShowTabs = value;
    }

    public int VRulerPosition
    {
        get => TextEditor.Options.ShowColumnRulers
            ? TextEditor.Options.ColumnRulerPositions.FirstOrDefault()
            : 0;
        set
        {
            TextEditor.Options.ShowColumnRulers = value > 0;
            TextEditor.Options.ColumnRulerPositions = value > 0 ? [value] : [];
        }
    }

    private int FirstVisibleLine
    {
        get
        {
            if (!TextEditor.TextArea.TextView.VisualLinesValid)
            {
                return Math.Max(0, TextEditor.TextArea.Caret.Line - 1);
            }

            VisualLine? first = TextEditor.TextArea.TextView.VisualLines.FirstOrDefault();
            return first?.FirstDocumentLine.LineNumber - 1 ?? 0;
        }

        set => TextEditor.ScrollToLine(Math.Max(1, value + 1));
    }

    public int GetLineFromVisualPosY(int visualPosY)
    {
        TextView textView = TextEditor.TextArea.TextView;
        VisualLine? visualLine = textView.GetVisualLineFromVisualTop(visualPosY + textView.ScrollOffset.Y);
        return visualLine is null ? int.MaxValue : visualLine.FirstDocumentLine.LineNumber - 1;
    }

    private string GetLineText(int line)
    {
        TextDocument? document = TextEditor.Document;
        if (document is null || line < 0 || line >= document.LineCount)
        {
            return string.Empty;
        }

        DocumentLine documentLine = document.GetLineByNumber(line + 1);
        return document.GetText(documentLine);
    }

    public void GoToLine(int lineNumber)
    {
        TextDocument? document = TextEditor.Document;
        if (document is null || document.LineCount == 0)
        {
            return;
        }

        int documentLine = Math.Clamp(GetCaretOffset(lineNumber, rightFile: true) + 1, 1, document.LineCount);
        TextEditor.TextArea.Caret.Position = new TextViewPosition(documentLine, column: 1);
        TextEditor.ScrollToLine(documentLine);
    }

    /// <summary>
    /// Convert the line number to the offset for the caret.
    /// </summary>
    /// <param name="lineNumber">The line number seen in the editor.</param>
    /// <param name="rightFile">If the line number for the right file is preferred.</param>
    /// <returns>The caret offset</returns>
    private int GetCaretOffset(int lineNumber, bool rightFile)
    {
        if (ShowLineNumbers == true)
        {
            // All contents is shown, just translate the lineNumber (that is an offset)
            return lineNumber - 1;
        }

        for (int offset = 0; offset < TotalNumberOfLines; offset++)
        {
            DiffLineInfo? diffLineNum = _lineNumbersControl.GetLineInfo(offset);
            if (diffLineNum is not null)
            {
                int diffLine = rightFile ? diffLineNum.RightLineNumber : diffLineNum.LeftLineNumber;
                if (diffLine != DiffLineInfo.NotApplicableLineNum && diffLine >= lineNumber)
                {
                    return offset;
                }
            }
        }

        return 0;
    }

    public int MaxLineNumber => ShowLineNumbers == true
        ? TotalNumberOfLines
        : Math.Max(1, _lineNumbersControl.MaxLineNumber);

    public int CurrentFileColumn => TextEditor.TextArea.Caret.Column;

    public int CurrentFileLine()
    {
        bool isPartial = _textHighlightService is DiffHighlightService;
        _currentViewPositionCache.Capture();
        if (!string.IsNullOrEmpty(_currentViewPositionCache._currentIdentification))
        {
            return _currentViewPositionCache.CurrentFileLine(isPartial);
        }

        DiffLineInfo? lineInfo = _lineNumbersControl.GetLineInfo(TextEditor.TextArea.Caret.Line - 1);
        if (lineInfo is null)
        {
            return TextEditor.TextArea.Caret.Line;
        }

        return lineInfo.RightLineNumber != DiffLineInfo.NotApplicableLineNum
            ? lineInfo.RightLineNumber
            : lineInfo.LeftLineNumber != DiffLineInfo.NotApplicableLineNum
                ? lineInfo.LeftLineNumber
                : TextEditor.TextArea.Caret.Line;
    }

    private int LineAtCaret
    {
        // AvaloniaEdit and the portable diff-line service expose the same one-based line identity here.
        get => TextEditor.TextArea.Caret.Line;
        set => TextEditor.TextArea.Caret.Position = new TextViewPosition(Math.Max(1, value), TextEditor.TextArea.Caret.Column);
    }

    public void HighlightLines(int startLine, int endLine, Color color)
    {
        _lineHighlights.Add(new HighlightedLines(
            startLine,
            endLine,
            new SolidColorBrush(Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B)).ToImmutable()));
        TextEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
    }

    public void ClearHighlighting()
    {
        _lineHighlights.Clear();
        TextEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
    }

    public int TotalNumberOfLines => TextEditor.Document?.LineCount ?? 0;

    public bool IsReadOnly
    {
        get => TextEditor.IsReadOnly;
        set => TextEditor.IsReadOnly = value;
    }

    public void SetFileLoader(GetNextFileFnc fileLoader)
    {
        _findAndReplaceFileLoader = fileLoader;
        _findAndReplaceForm?.SetFileLoader(fileLoader);
    }

    private void TextArea_MouseWheel(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            const int scrollAmount = 8;
            HScrollPosition = e.Delta.Y switch
            {
                > 0 => Math.Max(0, HScrollPosition - scrollAmount),
                < 0 => HScrollPosition + scrollAmount,
                _ => HScrollPosition,
            };
            e.Handled = true;
            return;
        }

        TextView textView = TextEditor.TextArea.TextView;
        if (e.Delta.Y > 0 && textView.ScrollOffset.Y <= 0)
        {
            e.Handled = _continuousScrollEventManager?.RaiseTopScrollReached(e.KeyModifiers) == true;
        }
        else if (e.Delta.Y < 0
                 && textView.ScrollOffset.Y + textView.Bounds.Height >= textView.DocumentHeight)
        {
            e.Handled = _continuousScrollEventManager?.RaiseBottomScrollReached(e.KeyModifiers) == true;
        }
    }

    private void OnHScrollPositionChanged(EventArgs e) => HScrollPositionChanged?.Invoke(this, e);

    private void OnVScrollPositionChanged(EventArgs e) => VScrollPositionChanged?.Invoke(this, e);

    public void SetGitBlameGutter(IEnumerable<GitBlameEntry> gitBlameEntries)
    {
        if (!_showGutterAvatars)
        {
            return;
        }

        GitBlameEntry[] entries = [.. gitBlameEntries];
        _authorsAvatarMargin?.Initialize(
            string.Join('\n', Enumerable.Repeat(string.Empty, entries.Length)),
            entries,
            showAvatars: true);
    }

    public bool ShowGutterAvatars
    {
        get => _showGutterAvatars;
        set
        {
            _showGutterAvatars = value;
            if (!_showGutterAvatars)
            {
                if (_authorsAvatarMargin is not null)
                {
                    _authorsAvatarMargin.IsVisible = false;
                }

                return;
            }

            if (_authorsAvatarMargin is null)
            {
                _authorsAvatarMargin = new BlameAuthorMargin(
                    new Typeface(TextEditor.FontFamily),
                    TextEditor.FontSize);
                TextEditor.TextArea.LeftMargins.Insert(0, _authorsAvatarMargin);
            }
            else
            {
                _authorsAvatarMargin.IsVisible = true;
            }
        }
    }

    internal sealed class CurrentViewPositionCache
    {
        private readonly FileViewerInternal _viewer;
        public string? _currentIdentification;
        public string? _capturedIdentification;
        private ViewPosition _currentViewPosition;

        public CurrentViewPositionCache(FileViewerInternal viewer)
        {
            _viewer = viewer;
        }

        public void Capture()
        {
            if (_viewer.TotalNumberOfLines <= 1 || string.IsNullOrEmpty(_currentIdentification))
            {
                return;
            }

            // store the previous view position
            _capturedIdentification = _currentIdentification;
            ViewPosition currentViewPosition = new()
            {
                ActiveLineNum = null,
                FirstLine = _viewer.GetLineText(0),
                TotalNumberOfLines = _viewer.TotalNumberOfLines,
                CaretPosition = _viewer.TextEditor.TextArea.Caret.Position,
                FirstVisibleLine = _viewer.FirstVisibleLine,
            };
            int caretLine = currentViewPosition.CaretPosition.Line - 1;
            int visibleLineCount = _viewer.TextEditor.TextArea.TextView.VisualLinesValid
                ? Math.Max(1, _viewer.TextEditor.TextArea.TextView.VisualLines.Count)
                : 1;
            currentViewPosition.CaretVisible = caretLine >= currentViewPosition.FirstVisibleLine
                                               && caretLine < currentViewPosition.FirstVisibleLine + visibleLineCount;

            if (_viewer.TextEditor.ShowLineNumbers)
            {
                // Full contents is shown, just set the line number
                _currentViewPosition = currentViewPosition;
                return;
            }

            int initialActiveLine = currentViewPosition.CaretVisible
                ? caretLine
                : currentViewPosition.FirstVisibleLine;

            // search downwards for a text line, i.e. a line with line numbers
            int activeLine = initialActiveLine;
            while (activeLine < currentViewPosition.TotalNumberOfLines && currentViewPosition.ActiveLineNum is null)
            {
                SetActiveLineNum(activeLine);
                ++activeLine;
            }

            // if none found, search upwards
            activeLine = initialActiveLine - 1;
            while (activeLine >= 0 && currentViewPosition.ActiveLineNum is null)
            {
                SetActiveLineNum(activeLine);
                --activeLine;
            }

            _currentViewPosition = currentViewPosition;
            return;

            void SetActiveLineNum(int line)
            {
                currentViewPosition.ActiveLineNum = _viewer._lineNumbersControl.GetLineInfo(line);
                if (currentViewPosition.ActiveLineNum is not null
                    && currentViewPosition.ActiveLineNum.LeftLineNumber == DiffLineInfo.NotApplicableLineNum
                    && currentViewPosition.ActiveLineNum.RightLineNumber == DiffLineInfo.NotApplicableLineNum)
                {
                    currentViewPosition.ActiveLineNum = null;
                }
            }
        }

        public bool Restore(string? contentIdentification)
        {
            _currentIdentification = contentIdentification;
            if (_viewer.TotalNumberOfLines <= 1
                || string.IsNullOrEmpty(contentIdentification)
                || string.IsNullOrEmpty(_currentIdentification))
            {
                return false;
            }

            bool sameIdentification = contentIdentification == _capturedIdentification;
            if (!sameIdentification)
            {
                return false;
            }

            ViewPosition viewPosition = _currentViewPosition;
            if (viewPosition.ActiveLineNum is not null)
            {
                // prefer the LeftLineNum because the base revision will not change
                int line = viewPosition.ActiveLineNum.LeftLineNumber != DiffLineInfo.NotApplicableLineNum
                    ? _viewer.GetCaretOffset(viewPosition.ActiveLineNum.LeftLineNumber, rightFile: false)
                    : _viewer.GetCaretOffset(viewPosition.ActiveLineNum.RightLineNumber, rightFile: true);
                _viewer.TextEditor.TextArea.Caret.Position = new TextViewPosition(
                    Math.Max(1, line + 1),
                    viewPosition.CaretPosition.Column);
                if (viewPosition.CaretVisible)
                {
                    _viewer.TextEditor.ScrollToLine(Math.Max(1, line + 1));
                }
                else
                {
                    _viewer.FirstVisibleLine = line;
                }
            }
            else
            {
                _viewer.FirstVisibleLine = viewPosition.FirstVisibleLine;
                int restoredLine = Math.Clamp(viewPosition.CaretPosition.Line, 1, _viewer.TotalNumberOfLines);
                _viewer.TextEditor.TextArea.Caret.Position = new TextViewPosition(
                    restoredLine,
                    viewPosition.CaretPosition.Column);
                if (!viewPosition.CaretVisible)
                {
                    _viewer.FirstVisibleLine = viewPosition.FirstVisibleLine;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the line number at the current view position offset.
        /// </summary>
        /// <param name="isDiff">If the current contents is a file diff.</param>
        /// <returns>The current line number at the caret offset</returns>
        public int CurrentFileLine(bool isDiff)
        {
            ViewPosition viewPosition = _currentViewPosition;
            if (isDiff && _viewer.GetLineText(0) == viewPosition.FirstLine && viewPosition.ActiveLineNum is not null)
            {
                // prefer the RightLineNum that is for the current revision
                return viewPosition.ActiveLineNum.RightLineNumber != DiffLineInfo.NotApplicableLineNum
                    ? viewPosition.ActiveLineNum.RightLineNumber
                    : viewPosition.ActiveLineNum.LeftLineNumber;
            }

            // Convert from offset to line number
            return viewPosition.CaretPosition.Line;
        }
    }

    internal struct ViewPosition
    {
        internal string FirstLine; // contains the file names in case of a diff
        internal int TotalNumberOfLines; // if changed, CaretPosition and FirstVisibleLine must be ignored and the line number must be searched
        internal TextViewPosition CaretPosition;
        internal int FirstVisibleLine;
        internal bool CaretVisible; // if not, FirstVisibleLine has priority for restoring
        internal DiffLineInfo? ActiveLineNum;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FileViewerInternal _control;

        public TestAccessor(FileViewerInternal control)
        {
            _control = control;
        }

        public ThemeAwareTextEditor TextEditor => _control.TextEditor;
        public GitHighlightingStrategyBase? HighlightingStrategy => _control._gitHighlightingStrategy;
        public IReadOnlyList<global::GitUI.TextRange> SelectionOccurrences => _control._selectionOccurrenceRenderer.Markers;
        public bool IsFindAndReplaceFormCreated => _control._findAndReplaceForm is not null;
        public FindAndReplaceForm FindAndReplaceForm => _control.GetOrCreateFindAndReplaceForm();
    }

    internal ThemeAwareTextEditor Editor => TextEditor;

    internal DiffViewerLineNumberControl LineNumbersControl => _lineNumbersControl;

    internal DiffHighlightService? DiffHighlightService => _textHighlightService as DiffHighlightService;

    internal void SetTextHighlightService(ITextHighlightService textHighlightService)
    {
        _textHighlightService = textHighlightService;
    }

    private FindAndReplaceForm GetOrCreateFindAndReplaceForm()
    {
        if (_findAndReplaceForm is null)
        {
            // Avalonia allocates a native top-level during Window construction, so defer reusable windows until first use.
            _findAndReplaceForm = new FindAndReplaceForm();
            _findAndReplaceForm.Closed += FindAndReplaceForm_Closed;
            if (_findAndReplaceFileLoader is not null)
            {
                _findAndReplaceForm.SetFileLoader(_findAndReplaceFileLoader);
            }
        }

        return _findAndReplaceForm;
    }

    private void FindAndReplaceForm_Closed(object? sender, EventArgs e)
    {
        if (ReferenceEquals(_findAndReplaceForm, sender))
        {
            _findAndReplaceForm = null;
        }
    }

    private void CloseFindAndReplaceForm()
    {
        FindAndReplaceForm? form = _findAndReplaceForm;
        if (form is null)
        {
            return;
        }

        _findAndReplaceForm = null;
        form.Closed -= FindAndReplaceForm_Closed;
        form.Close();
    }

    internal void SetHighlightingForFile(string? filename, IGitModule? module)
    {
        if (_gitHighlightingStrategy is not null)
        {
            TextEditor.TextArea.TextView.LineTransformers.Remove(_gitHighlightingStrategy);
        }

        _gitHighlightingStrategy = filename switch
        {
            not null when module is not null && filename.EndsWith("git-rebase-todo", StringComparison.Ordinal) => new RebaseTodoHighlightingStrategy(module),
            not null when module is not null && filename.EndsWith("COMMIT_EDITMSG", StringComparison.Ordinal) => new CommitMessageHighlightingStrategy(module),
            _ => null,
        };

        if (_gitHighlightingStrategy is not null)
        {
            TextEditor.TextArea.TextView.LineTransformers.Add(_gitHighlightingStrategy);
        }

        UpdateValidationMarkers();
        TextEditor.TextArea.TextView.Redraw();
    }

    private void UpdateValidationMarkers()
    {
        if (_gitHighlightingStrategy is CommitMessageHighlightingStrategy commitStrategy
            && TextEditor.Document is not null)
        {
            commitStrategy.UpdateValidationMarkers(TextEditor.Document);
            _validationRenderer.Markers = commitStrategy.ValidationMarkers;
        }
        else
        {
            _validationRenderer.Markers = [];
        }

        TextEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }

    private sealed class CommitMessageValidationRenderer : IBackgroundRenderer
    {
        public IReadOnlyList<CommitMessageValidationMarker> Markers { get; set; } = [];

        public KnownLayer Layer => KnownLayer.Selection;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!textView.VisualLinesValid || textView.Document is null)
            {
                return;
            }

            Pen pen = new(Brushes.Red, 1);
            foreach (CommitMessageValidationMarker marker in Markers)
            {
                if (marker.Length <= 0 || marker.Offset >= textView.Document.TextLength)
                {
                    continue;
                }

                SimpleSegment segment = new(marker.Offset, Math.Min(marker.Length, textView.Document.TextLength - marker.Offset));
                foreach (Avalonia.Rect rectangle in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
                {
                    double y = rectangle.Bottom - 1;
                    for (double x = rectangle.Left; x < rectangle.Right; x += 4)
                    {
                        drawingContext.DrawLine(pen, new Avalonia.Point(x, y), new Avalonia.Point(Math.Min(x + 2, rectangle.Right), y - 2));
                        drawingContext.DrawLine(pen, new Avalonia.Point(Math.Min(x + 2, rectangle.Right), y - 2), new Avalonia.Point(Math.Min(x + 4, rectangle.Right), y));
                    }
                }
            }
        }
    }

    private sealed class SelectionOccurrenceRenderer : IBackgroundRenderer
    {
        public IReadOnlyList<global::GitUI.TextRange> Markers { get; set; } = [];

        public KnownLayer Layer => KnownLayer.Selection;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!textView.VisualLinesValid || textView.Document is null || Markers.Count == 0)
            {
                return;
            }

            System.Drawing.Color color = AvaloniaThemeResources.ResolveAppColor(
                ThemeModule.Settings,
                AppColor.HighlightAllOccurences);
            IBrush brush = new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(color)).ToImmutable();
            foreach (global::GitUI.TextRange marker in Markers)
            {
                foreach (Avalonia.Rect rectangle in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    drawingContext.FillRectangle(brush, rectangle);
                }
            }
        }
    }

    /// <summary>
    /// An inclusive range of zero-based lines drawn with a background brush.
    /// </summary>
    private sealed record HighlightedLines(int StartLine, int EndLine, IBrush Brush);

    /// <summary>
    /// Draws line highlights behind the text.
    /// </summary>
    private sealed class HighlightBackgroundRenderer : IBackgroundRenderer
    {
        private readonly List<HighlightedLines> _highlights;

        public HighlightBackgroundRenderer(List<HighlightedLines> highlights)
        {
            _highlights = highlights;
        }

        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_highlights.Count == 0 || !textView.VisualLinesValid)
            {
                return;
            }

            foreach (VisualLine visualLine in textView.VisualLines)
            {
                int index = visualLine.FirstDocumentLine.LineNumber - 1;
                HighlightedLines? highlight = _highlights.FirstOrDefault(
                    candidate => index >= candidate.StartLine && index <= candidate.EndLine);
                if (highlight is not null)
                {
                    drawingContext.FillRectangle(
                        highlight.Brush,
                        new Avalonia.Rect(
                            0,
                            visualLine.VisualTop - textView.ScrollOffset.Y,
                            textView.Bounds.Width,
                            visualLine.Height));
                }
            }
        }
    }
}
