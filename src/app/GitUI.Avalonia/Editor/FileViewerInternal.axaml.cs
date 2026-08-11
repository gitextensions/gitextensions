using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;

namespace GitUI.Editor;

/// <summary>Hosts the text editor and file-specific highlighting used by <see cref="FileViewer"/>.</summary>
public partial class FileViewerInternal : GitModuleControl
{
    private readonly CommitMessageValidationRenderer _validationRenderer;
    private readonly SelectionOccurrenceRenderer _selectionOccurrenceRenderer;
    private BlameAuthorMargin? _authorsAvatarMargin;
    private GitHighlightingStrategyBase? _gitHighlightingStrategy;
    private bool _showGutterAvatars;

    /// <summary>Initializes the internal editor.</summary>
    public FileViewerInternal()
    {
        InitializeComponent();
        _validationRenderer = new CommitMessageValidationRenderer();
        _selectionOccurrenceRenderer = new SelectionOccurrenceRenderer();
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(_validationRenderer);
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(_selectionOccurrenceRenderer);
        TextEditor.TextChanged += (_, _) => UpdateValidationMarkers();
        TextEditor.TextArea.SelectionChanged += SelectionManagerSelectionChanged;
        AttachedToVisualTree += (_, _) => TextEditor.ContextMenu = ContextMenu;
        InitializeComplete();
    }

    internal ThemeAwareTextEditor Editor => TextEditor;

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

    private void SelectionManagerSelectionChanged(object? sender, EventArgs e)
    {
        string word = TextEditor.SelectedText;
        List<global::GitUI.TextRange> markers = GetTextMarkersMatchingWord(word);
        _selectionOccurrenceRenderer.Markers = markers;
        TextEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }

    /// <summary>
    /// Create a list of text ranges in the Document that match the given text.
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

    /// <summary>
    /// Move the file viewer cursor position to the next occurrence matching the selection.
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
    /// Move the file viewer cursor position to the previous occurrence matching the selection.
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
    }
}
