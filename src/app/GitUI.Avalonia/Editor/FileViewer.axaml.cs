using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
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
// file-to-file continuous scrolling used by FormStash. Blob/blame/range modes, syntax
// highlighting, and line patching remain deferred.
public partial class FileViewer : GitModuleControl
{
    public FileViewer()
    {
        InitializeComponent();

        TextEditor.TextArea.TextView.LineTransformers.Add(new DiffLineColorizer());
        TextEditor.KeyDown += TextEditor_KeyDown;
        TextEditor.PointerWheelChanged += TextEditor_PointerWheelChanged;

        InitializeComplete();
    }

    /// <summary>
    ///  Raised when Escape is pressed in the diff editor.
    /// </summary>
    public event Action? EscapePressed;

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
        TextEditor.Document ??= new TextDocument();
        TextEditor.Document.Text = text ?? string.Empty;
        TextEditor.ScrollToHome();
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
    ///  Colors added/removed/section lines of a unified diff, approximating the WinForms
    ///  diff highlight service until the full highlighting port lands.
    /// </summary>
    private sealed class DiffLineColorizer : DocumentColorizingTransformer
    {
        private static readonly IBrush _addedBrush = new SolidColorBrush(Colors.SeaGreen).ToImmutable();
        private static readonly IBrush _removedBrush = new SolidColorBrush(Colors.IndianRed).ToImmutable();
        private static readonly IBrush _sectionBrush = new SolidColorBrush(Colors.SteelBlue).ToImmutable();

        protected override void ColorizeLine(DocumentLine line)
        {
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
