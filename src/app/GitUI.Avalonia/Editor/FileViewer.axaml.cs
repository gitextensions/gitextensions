using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.UserControls;

using ResourceManager;

namespace GitUI.Editor;

// Reduced twin of GitUI/Editor/FileViewer.cs. It renders unified diffs and supports the
// file-to-file continuous scrolling used by FormStash, plus the plain-text mode with
// line highlighting used by the blame view. Blob/range modes, syntax highlighting, and
// line patching remain deferred.
public partial class FileViewer : GitModuleControl
{
    private readonly List<HighlightedLines> _lineHighlights = [];
    private bool _isDiffView;
    private int _lastCaretLine = -1;

    public FileViewer()
    {
        InitializeComponent();

        TextEditor.TextArea.TextView.LineTransformers.Add(new DiffLineColorizer(() => _isDiffView));
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(new HighlightBackgroundRenderer(_lineHighlights));
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
        _isDiffView = true;
        SetText(text);
    }

    /// <summary>
    ///  Shows plain text without diff coloring, like the WinForms text mode.
    /// </summary>
    public async Task ViewTextAsync(string? fileName, string text, CancellationToken cancellationToken = default)
    {
        await this.SwitchToMainThreadAsync(cancellationToken);

        _isDiffView = false;
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
    }

    /// <summary>
    ///  Gets the one-based line number of the caret.
    /// </summary>
    public int CurrentFileLine => TextEditor.TextArea.Caret.Line;

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

        line = Math.Clamp(line, 1, document.LineCount);
        TextEditor.TextArea.Caret.Position = new TextViewPosition(line, column: 1);
        TextEditor.ScrollToLine(line);
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
    }

    private void Caret_PositionChanged(object? sender, EventArgs e)
    {
        int line = TextEditor.TextArea.Caret.Line - 1;
        if (line == _lastCaretLine)
        {
            return;
        }

        _lastCaretLine = line;
        SelectedLineChanged?.Invoke(this, new SelectedLineEventArgs(line));
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

    /// <summary>
    ///  Colors added/removed/section lines of a unified diff, approximating the WinForms
    ///  diff highlight service until the full highlighting port lands.
    /// </summary>
    private sealed class DiffLineColorizer : DocumentColorizingTransformer
    {
        private static readonly IBrush _addedBrush = new SolidColorBrush(Colors.SeaGreen).ToImmutable();
        private static readonly IBrush _removedBrush = new SolidColorBrush(Colors.IndianRed).ToImmutable();
        private static readonly IBrush _sectionBrush = new SolidColorBrush(Colors.SteelBlue).ToImmutable();

        private readonly Func<bool> _isDiffView;

        public DiffLineColorizer(Func<bool> isDiffView)
        {
            _isDiffView = isDiffView;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (!_isDiffView())
            {
                return;
            }

            string text = CurrentContext.Document.GetText(line.Offset, Math.Min(line.Length, 4));
            IBrush? brush = text switch
            {
                _ when text.StartsWith("+++") || text.StartsWith("---") => _sectionBrush,
                _ when text.StartsWith("@@") => _sectionBrush,
                _ when text.StartsWith('+') => _addedBrush,
                _ when text.StartsWith('-') => _removedBrush,
                _ => null,
            };

            if (brush is not null)
            {
                ChangeLinePart(line.Offset, line.EndOffset, element => element.TextRunProperties.SetForegroundBrush(brush));
            }
        }
    }
}
