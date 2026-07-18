using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Editor.Diff;
using GitUI.UserControls;

using ResourceManager;

namespace GitUI.Editor;

// Functional twin of GitUI/Editor/FileViewer.cs. It renders parsed patch, combined, word,
// and range diffs and supports the file-to-file continuous scrolling used by FormStash,
// plus the plain-text mode with line highlighting used by blame. Blob modes, search,
// syntax highlighting, and line patching remain deferred.
public partial class FileViewer : GitModuleControl
{
    private readonly DiffBackgroundRenderer _diffBackgroundRenderer;
    private readonly DiffTextColorizer _diffTextColorizer;
    private readonly DiffViewerLineNumberControl _diffViewerLineNumberControl;
    private readonly List<HighlightedLines> _lineHighlights = [];
    private DiffHighlightService? _diffHighlightService;
    private int _lastCaretLine = -1;

    public FileViewer()
    {
        InitializeComponent();

        _diffViewerLineNumberControl = new DiffViewerLineNumberControl(TextEditor);
        _diffViewerLineNumberControl.Clear();
        TextEditor.TextArea.LeftMargins.Insert(0, _diffViewerLineNumberControl);

        _diffBackgroundRenderer = new DiffBackgroundRenderer(this);
        _diffTextColorizer = new DiffTextColorizer(this);
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(_diffBackgroundRenderer);
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(new HighlightBackgroundRenderer(_lineHighlights));
        TextEditor.TextArea.TextView.LineTransformers.Add(_diffTextColorizer);
        TextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        TextEditor.KeyDown += TextEditor_KeyDown;
        TextEditor.PointerWheelChanged += TextEditor_PointerWheelChanged;

        InitializeComplete();
    }

    /// <summary>
    ///  Raised when Escape is pressed in the diff editor.
    /// </summary>
    public event Action? EscapePressed;

    /// <summary>
    ///  Raised when the caret moves to a different line (zero-based, like WinForms).
    /// </summary>
    public event EventHandler<SelectedLineEventArgs>? SelectedLineChanged;

    /// <summary>
    ///  Raised when scrolling above the first line.
    /// </summary>
    public event EventHandler? TopScrollReached;

    /// <summary>
    ///  Raised when scrolling below the last line.
    /// </summary>
    public event EventHandler? BottomScrollReached;

    /// <summary>
    ///  Gets whether the current diff supports line patching.
    /// </summary>
    public bool SupportLinePatching => false;

    /// <summary>
    ///  Shows a unified diff (patch) text.
    /// </summary>
    public void ViewPatch(string? text)
    {
        ViewPatch(text, useGitColoring: false);
    }

    /// <summary>
    /// Shows a patch using Git's ANSI coloring, combined-diff parsing, or word-diff parsing.
    /// </summary>
    public void ViewPatch(string? text, bool useGitColoring, bool isCombinedDiff = false, bool isGitWordDiff = false)
    {
        string parsedText = text ?? string.Empty;
        DiffHighlightService highlightService = isCombinedDiff
            ? new CombinedDiffHighlightService(ref parsedText, useGitColoring)
            : new PatchHighlightService(ref parsedText, useGitColoring, isGitWordDiff);
        SetDiffText(parsedText, highlightService, showLeftColumn: true);
    }

    /// <summary>
    /// Shows the output of git range-diff with its single right-side line-number column.
    /// </summary>
    public void ViewRangeDiff(string? text)
    {
        string parsedText = text ?? string.Empty;
        RangeDiffHighlightService highlightService = new(ref parsedText);
        SetDiffText(parsedText, highlightService, showLeftColumn: false);
    }

    /// <summary>
    ///  Shows plain text without diff coloring, like the WinForms text mode.
    /// </summary>
    public async Task ViewTextAsync(string? fileName, string text, CancellationToken cancellationToken = default)
    {
        await this.SwitchToMainThreadAsync(cancellationToken);

        ClearDiffHighlighting();
        SetText(text);
    }

    /// <summary>
    ///  Clears the viewer.
    /// </summary>
    public Task ClearAsync()
        => ViewTextAsync("", "");

    private void SetText(string? text)
    {
        ClearHighlighting();
        TextEditor.Document ??= new TextDocument();
        TextEditor.Document.Text = text ?? string.Empty;
        TextEditor.ScrollToHome();
        TextEditor.TextArea.TextView.Redraw();
    }

    private void SetDiffText(string text, DiffHighlightService highlightService, bool showLeftColumn)
    {
        _diffHighlightService = highlightService;
        _diffBackgroundRenderer.SetHighlightService(highlightService);
        _diffTextColorizer.SetHighlightService(highlightService);
        _diffViewerLineNumberControl.DisplayLineNum(highlightService.LinesInfo, showLeftColumn);
        TextEditor.ShowLineNumbers = false;
        SetText(text);
    }

    private void ClearDiffHighlighting()
    {
        _diffHighlightService = null;
        _diffBackgroundRenderer.SetHighlightService(null);
        _diffTextColorizer.SetHighlightService(null);
        _diffViewerLineNumberControl.Clear();
        TextEditor.ShowLineNumbers = true;
    }

    /// <summary>
    ///  Gets the one-based line number of the caret.
    /// </summary>
    public int CurrentFileLine
    {
        get
        {
            DiffLineInfo? lineInfo = _diffViewerLineNumberControl.GetLineInfo(TextEditor.TextArea.Caret.Line - 1);
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
    }

    /// <summary>
    ///  Moves the caret to the given one-based line and scrolls it into view.
    /// </summary>
    public void GoToLine(int line)
    {
        TextDocument? document = TextEditor.Document;
        if (document is null || document.LineCount == 0)
        {
            return;
        }

        int documentLine = FindDocumentLine(line);
        documentLine = Math.Clamp(documentLine, 1, document.LineCount);
        TextEditor.TextArea.Caret.Position = new TextViewPosition(documentLine, column: 1);
        TextEditor.ScrollToLine(documentLine);
    }

    private int FindDocumentLine(int fileLine)
    {
        if (_diffHighlightService is null)
        {
            return fileLine;
        }

        DiffLineInfo? mapped = _diffHighlightService.LinesInfo.DiffLines.Values.FirstOrDefault(
            info => info.RightLineNumber == fileLine);
        mapped ??= _diffHighlightService.LinesInfo.DiffLines.Values.FirstOrDefault(
            info => info.LeftLineNumber == fileLine);
        return mapped?.LineNumInDiff ?? fileLine;
    }

    /// <summary>
    ///  Gets the zero-based line index at a y position relative to this control,
    ///  or a value past the last line when no line is there (like WinForms).
    /// </summary>
    public int GetLineFromVisualPosY(double visualPosY)
    {
        AvaloniaEdit.Rendering.TextView textView = TextEditor.TextArea.TextView;
        VisualLine? visualLine = textView.GetVisualLineFromVisualTop(visualPosY + textView.ScrollOffset.Y);
        return visualLine is null ? int.MaxValue : visualLine.FirstDocumentLine.LineNumber - 1;
    }

    /// <summary>
    ///  Adds a background highlight for an inclusive range of zero-based lines.
    /// </summary>
    public void HighlightLines(int startLine, int endLine, System.Drawing.Color color)
    {
        _lineHighlights.Add(new HighlightedLines(
            startLine,
            endLine,
            new SolidColorBrush(Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B)).ToImmutable()));
    }

    /// <summary>
    ///  Removes all line highlights.
    /// </summary>
    public void ClearHighlighting()
    {
        _lineHighlights.Clear();
    }

    /// <summary>
    ///  Redraws the text view, like the WinForms control method.
    /// </summary>
    public void Refresh()
    {
        TextEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        _diffViewerLineNumberControl.InvalidateVisual();
    }

    private void Caret_PositionChanged(object? sender, EventArgs e)
    {
        int line = TextEditor.TextArea.Caret.Line - 1;
        if (line == _lastCaretLine)
        {
            return;
        }

        _lastCaretLine = line;
        _diffViewerLineNumberControl.InvalidateVisual();
        SelectedLineChanged?.Invoke(this, new SelectedLineEventArgs(CurrentFileLine - 1));
    }

    /// <summary>
    ///  Loads and displays the diff represented by a file-status entry.
    /// </summary>
    public async Task ViewChangesAsync(FileStatusItem? item, CancellationToken cancellationToken)
    {
        if (item?.Item is null)
        {
            ViewPatch(null);
            return;
        }

        if (item.Item.IsStatusOnly)
        {
            ViewPatch(item.Item.ErrorMessage);
            return;
        }

        ObjectId firstId = item.FirstRevision?.ObjectId ?? item.SecondRevision.FirstParentId;
        ObjectId secondId = item.SecondRevision.ObjectId;
        bool isTracked = item.Item.IsTracked || (!item.Item.TreeId.IsZero && !secondId.IsZero);

        (Patch? patch, string? errorMessage) = await Module.GetSingleDiffAsync(
            firstId,
            secondId,
            item.Item.Name,
            item.Item.OldName,
            extraDiffArguments: "",
            Module.FilesEncoding,
            cacheResult: true,
            isTracked,
            useGitColoring: false,
            GitCommandConfiguration.Default,
            cancellationToken);

        await this.SwitchToMainThreadAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        ViewPatch(patch?.Text ?? errorMessage);
    }

    /// <summary>
    ///  Scrolls to the first line.
    /// </summary>
    public void ScrollToTop() => TextEditor.ScrollToHome();

    /// <summary>
    ///  Scrolls to the last line.
    /// </summary>
    public void ScrollToBottom() => TextEditor.ScrollToEnd();

    /// <summary>
    ///  Focuses the text editor hosted by this viewer.
    /// </summary>
    public void FocusViewer()
    {
        if (!TextEditor.TextArea.Focus())
        {
            Dispatcher.UIThread.Post(() => TextEditor.TextArea.Focus());
        }
    }

    private void TextEditor_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            EscapePressed?.Invoke();
            e.Handled = true;
        }
    }

    private void TextEditor_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        AvaloniaEdit.Rendering.TextView textView = TextEditor.TextArea.TextView;
        if (e.Delta.Y > 0 && textView.ScrollOffset.Y <= 0)
        {
            TopScrollReached?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
        else if (e.Delta.Y < 0
                 && textView.ScrollOffset.Y + textView.Bounds.Height >= textView.DocumentHeight)
        {
            BottomScrollReached?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    /// <summary>
    ///  An inclusive range of zero-based lines drawn with a background brush.
    /// </summary>
    private sealed record HighlightedLines(int StartLine, int EndLine, IBrush Brush);

    /// <summary>
    ///  Draws the line highlights (used by the blame view for the hovered commit)
    ///  behind the text.
    /// </summary>
    private sealed class HighlightBackgroundRenderer : IBackgroundRenderer
    {
        private readonly List<HighlightedLines> _highlights;

        public HighlightBackgroundRenderer(List<HighlightedLines> highlights)
        {
            _highlights = highlights;
        }

        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(AvaloniaEdit.Rendering.TextView textView, DrawingContext drawingContext)
        {
            if (_highlights.Count == 0 || !textView.VisualLinesValid)
            {
                return;
            }

            foreach (VisualLine visualLine in textView.VisualLines)
            {
                int index = visualLine.FirstDocumentLine.LineNumber - 1;
                foreach (HighlightedLines highlight in _highlights)
                {
                    if (index >= highlight.StartLine && index <= highlight.EndLine)
                    {
                        drawingContext.FillRectangle(
                            highlight.Brush,
                            new Avalonia.Rect(
                                0,
                                visualLine.VisualTop - textView.ScrollOffset.Y,
                                textView.Bounds.Width,
                                visualLine.Height));
                        break;
                    }
                }
            }
        }
    }
}
