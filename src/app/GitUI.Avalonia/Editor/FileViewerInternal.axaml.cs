using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;

namespace GitUI.Editor;

/// <summary>Hosts the text editor and file-specific highlighting used by <see cref="FileViewer"/>.</summary>
public partial class FileViewerInternal : GitModuleControl
{
    private readonly CommitMessageValidationRenderer _validationRenderer;
    private GitHighlightingStrategyBase? _gitHighlightingStrategy;

    /// <summary>Initializes the internal editor.</summary>
    public FileViewerInternal()
    {
        InitializeComponent();
        _validationRenderer = new CommitMessageValidationRenderer();
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(_validationRenderer);
        TextEditor.TextChanged += (_, _) => UpdateValidationMarkers();
        AttachedToVisualTree += (_, _) => TextEditor.ContextMenu = ContextMenu;
        InitializeComplete();
    }

    internal ThemeAwareTextEditor Editor => TextEditor;

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
    }
}
