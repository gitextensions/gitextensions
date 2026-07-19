using System.Diagnostics.CodeAnalysis;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUI.Editor.Diff;
using GitUI.UserControls;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Editor;

// Functional twin of GitUI/Editor/FileViewer.cs. It renders parsed patch, combined, word,
// and range diffs and supports the file-to-file continuous scrolling used by FormStash,
// plus the plain-text mode with line highlighting used by blame. Search/navigation uses
// the original dialog and command boundaries. Blob, image, binary, large-file, and encoding
// behavior follows the original loading boundary. Syntax and display options use AvaloniaEdit
// while remaining independent from semantic diff rendering. Line patching remains deferred.
public partial class FileViewer : GitModuleControl
{
    private const long MaximumAutomaticPreviewLength = 5 * 1024 * 1024;
    private const string EndOfLineGlyph = "¶";

    private readonly TranslationString _largeFileSizeWarning = new("This file is {0:N1} MB. Showing large files can be slow. Click to show anyway.");
    private readonly TranslationString _cannotViewImage = new("Cannot view image {0}");
    private readonly TranslationString _fileSizeInMb = new("MB");
    private readonly TranslationString _bytes = new("bytes");
    private readonly TranslationString _binaryFile = new("Binary file: {0}");
    private readonly TranslationString _binaryFileDetected = new("Binary file: {0} (Detected)");

    private readonly CancellationTokenSequence _viewSequence = new();
    private readonly DiffBackgroundRenderer _diffBackgroundRenderer;
    private readonly DiffTextColorizer _diffTextColorizer;
    private readonly DiffViewerLineNumberControl _diffViewerLineNumberControl;
    private readonly FindAndReplaceForm _findAndReplaceForm;
    private readonly IFullPathResolver _fullPathResolver;
    private readonly List<HighlightedLines> _lineHighlights = [];
    private DiffHighlightService? _diffHighlightService;
    private Func<Task>? _deferShowFunc;
    private Encoding? _encoding;
    private CancellationTokenRegistration _externalCancellationRegistration;
    private bool _hotkeysLoaded;
    private Bitmap? _image;
    private Action? _openWithDifftool;
    private string? _fileName;
    private bool? _showLineNumbers;
    private bool _showNonPrintingChars;
    private IGitUICommands? _settingsCommands;
    private bool _updatingEncoding;
    private int _lastCaretLine = -1;
    private FileStatusItem? _viewItem;
    private ViewMode _viewMode;

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
        TextEditor.PointerMoved += (_, _) => ShowFileViewerToolbar();
        PointerExited += (_, _) => fileviewerToolbar.IsVisible = false;
        PictureBox.PointerWheelChanged += PictureBox_PointerWheelChanged;
        DetachedFromLogicalTree += (_, _) =>
        {
            BindSettingsCommands(commands: null);
            CancelPendingView();
            ClearImage();
        };

        _findAndReplaceForm = new FindAndReplaceForm();
        _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);

        copyToolStripMenuItem.Click += CopyToolStripMenuItemClick;
        showNonprintableCharactersToolStripMenuItem.Click += ShowNonprintableCharactersToolStripMenuItemClick;
        showSyntaxHighlightingToolStripMenuItem.Click += ShowSyntaxHighlighting_Click;
        findToolStripMenuItem.Click += FindToolStripMenuItemClick;
        replaceToolStripMenuItem.Click += FindToolStripMenuItemClick;
        goToLineToolStripMenuItem.Click += goToLineToolStripMenuItem_Click;
        showNonPrintChars.Click += ShowNonprintableCharactersToolStripMenuItemClick;
        showSyntaxHighlighting.Click += ShowSyntaxHighlighting_Click;
        contextMenu.Opening += contextMenu_Opening;
        _NO_TRANSLATE_lblShowPreview.Click += llShowPreview_LinkClicked;
        encodingToolStripComboBox.SelectionChanged += encodingToolStripComboBox_SelectedIndexChanged;

        HotkeysEnabled = true;
        UICommandsSourceSet += (_, e) =>
        {
            BindSettingsCommands(e.GitUICommandsSource.UICommands);
            ReloadHotkeys();
            Encoding = null;
        };
        AttachedToLogicalTree += (_, _) =>
        {
            if (TryGetUICommandsDirect(out IGitUICommands? commands))
            {
                BindSettingsCommands(commands);
            }
        };

        PopulateEncodings();
        _showNonPrintingChars = AppSettings.ShowNonPrintingChars.GetValue(
            reload: !AppSettings.RememberShowNonPrintingCharsPreference);
        ShowSyntaxHighlightingInDiff = AppSettings.ShowSyntaxHighlightingInDiff.GetValue(
            reload: !AppSettings.RememberShowSyntaxHighlightingInDiff);
        ToggleNonPrintingChars(_showNonPrintingChars);
        UpdateSyntaxHighlightingToggleState();
        VRulerPosition = AppSettings.DiffVerticalRulerPosition;

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
    ///  Raised after text content has been displayed.
    /// </summary>
    public event EventHandler? TextLoaded;

    /// <summary>
    ///  Raised when the selected file encoding changes and the consumer should reload content.
    /// </summary>
    public event EventHandler<EventArgs>? ExtraDiffArgumentsChanged;

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
    ///  Gets the preamble detected while reading the current working-tree file.
    /// </summary>
    public byte[]? FilePreamble { get; private set; }

    /// <summary>
    ///  Gets or sets the encoding used for file and Git blob content.
    /// </summary>
    [NotNull]
    public Encoding? Encoding
    {
        get => _encoding ??= Module.FilesEncoding;
        set
        {
            _encoding = value;
            UpdateEncodingSelection();
        }
    }

    /// <summary>
    ///  Gets or sets whether ordinary document line numbers are shown. A <see langword="null" />
    ///  value keeps the original mode-dependent behavior.
    /// </summary>
    public bool? ShowLineNumbers
    {
        get => _showLineNumbers;
        set
        {
            _showLineNumbers = value;
            UpdateLineNumberVisibility();
        }
    }

    private bool ShowSyntaxHighlightingInDiff { get; set; }

    private int VRulerPosition
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
        CancelPendingView();
        ViewPatchCore(text, useGitColoring, isCombinedDiff, isGitWordDiff);
    }

    private void ViewPatchCore(
        string? text,
        bool useGitColoring,
        bool isCombinedDiff,
        bool isGitWordDiff,
        string? fileName = null,
        FileStatusItem? item = null)
    {
        ResetView(isCombinedDiff ? ViewMode.CombinedDiff : ViewMode.Diff, fileName, item);
        string parsedText = text ?? string.Empty;
        DiffHighlightService highlightService = isCombinedDiff
            ? new CombinedDiffHighlightService(ref parsedText, useGitColoring)
            : new PatchHighlightService(ref parsedText, useGitColoring, isGitWordDiff);
        SetDiffText(parsedText, highlightService, showLeftColumn: true);
        TextLoaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Shows the output of git range-diff with its single right-side line-number column.
    /// </summary>
    public void ViewRangeDiff(string? text)
    {
        CancelPendingView();
        ResetView(ViewMode.RangeDiff, fileName: null);
        string parsedText = text ?? string.Empty;
        RangeDiffHighlightService highlightService = new(ref parsedText);
        SetDiffText(parsedText, highlightService, showLeftColumn: false);
        TextLoaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///  Shows plain text without diff coloring, like the WinForms text mode.
    /// </summary>
    public Task ViewTextAsync(string? fileName, string text, CancellationToken cancellationToken)
        => ViewTextAsync(fileName, text, item: null, line: null, openWithDifftool: null, checkGitAttributes: false, cancellationToken);

    /// <summary>
    ///  Shows plain text without diff coloring, like the WinForms text mode.
    /// </summary>
    public Task ViewTextAsync(
        string? fileName,
        string text,
        FileStatusItem? item = null,
        int? line = null,
        Action? openWithDifftool = null,
        bool checkGitAttributes = false,
        CancellationToken cancellationToken = default)
    {
        CancellationToken viewToken = BeginView(cancellationToken);
        return ShowOrDeferAsync(
            text.Length,
            () => ShowTextAsync(fileName, text, item, line, openWithDifftool, checkGitAttributes, viewToken),
            viewToken);
    }

    /// <summary>
    ///  Shows plain text synchronously for WinForms-shaped callers.
    /// </summary>
    public void ViewText(string? fileName, string text, Action? openWithDifftool = null)
    {
        ThreadHelper.JoinableTaskFactory.Run(
            () => ViewTextAsync(fileName, text, item: null, line: null, openWithDifftool));
    }

    /// <summary>
    ///  Clears the viewer.
    /// </summary>
    public Task ClearAsync() => ViewTextAsync(string.Empty, string.Empty);

    private void SetText(string? text)
    {
        ClearHighlighting();
        TextEditor.Document ??= new TextDocument();
        TextEditor.Document.Text = text ?? string.Empty;
        TextEditor.ScrollToHome();
        TextEditor.TextArea.TextView.Redraw();
    }

    /// <summary>
    ///  Loads a file from the working tree using the same image/text/binary decisions as the
    ///  original viewer.
    /// </summary>
    public Task ViewFileAsync(
        string fileName,
        bool isSubmodule = false,
        FileStatusItem? item = null,
        int? line = null,
        Action? openWithDifftool = null,
        CancellationToken cancellationToken = default)
    {
        CancellationToken viewToken = BeginView(cancellationToken);
        return LoadWithErrorHandlingAsync(
            () => ViewFileCoreAsync(fileName, isSubmodule, item, line, openWithDifftool, viewToken),
            viewToken);
    }

    /// <summary>
    ///  Loads the blob or working-tree content represented by a file-status item.
    /// </summary>
    public Task ViewGitItemAsync(
        FileStatusItem item,
        int? line = null,
        Action? openWithDifftool = null,
        CancellationToken cancellationToken = default)
        => ViewGitItemAsync(item.Item, item.SecondRevision.ObjectId, item, line, openWithDifftool, cancellationToken);

    /// <summary>
    ///  Loads the blob or working-tree content represented by a Git item.
    /// </summary>
    public Task ViewGitItemAsync(
        GitItemStatus file,
        ObjectId objectId,
        int? line = null,
        Action? openWithDifftool = null,
        CancellationToken cancellationToken = default)
        => ViewGitItemAsync(file, objectId, item: null, line, openWithDifftool, cancellationToken);

    private async Task ViewGitItemAsync(
        GitItemStatus file,
        ObjectId objectId,
        FileStatusItem? item,
        int? line,
        Action? openWithDifftool,
        CancellationToken cancellationToken)
    {
        CancellationToken viewToken = BeginView(cancellationToken);
        await LoadWithErrorHandlingAsync(
            () => ViewGitItemCoreAsync(file, objectId, item, line, openWithDifftool, viewToken),
            viewToken);
    }

    private async Task ViewGitItemCoreAsync(
        GitItemStatus file,
        ObjectId objectId,
        FileStatusItem? item,
        int? line,
        Action? openWithDifftool,
        CancellationToken viewToken)
    {
        ObjectId blobId = GetUpdateTreeId(file, objectId, viewToken);
        if (blobId.IsZero)
        {
            await ViewFileCoreAsync(file.Name, file.IsSubmodule, item, line, openWithDifftool, viewToken);
            return;
        }

        FilePreamble = [];
        if (file.IsSubmodule)
        {
            string text = await Task.Run(
                () => SubmoduleResources.GetSubmoduleText(Module, file.Name.TrimEnd('/'), blobId.ToString()),
                viewToken);
            await ShowTextAsync(file.Name, text, item, line: null, openWithDifftool, checkGitAttributes: false, viewToken);
            return;
        }

        if (FileHelper.IsImage(file.Name))
        {
            Bitmap? image = await LoadBlobImageAsync(blobId, viewToken);
            if (image is not null)
            {
                await ShowImageAsync(file.Name, item, image, openWithDifftool, viewToken);
                return;
            }

            string failedImageText = await LoadBlobTextAsync(file.Name, blobId, viewToken);
            await ShowHexDumpAsync(_cannotViewImage.Text, file.Name, failedImageText, openWithDifftool, viewToken);
            return;
        }

        string textContent = await LoadBlobTextAsync(file.Name, blobId, viewToken);
        await ShowOrDeferAsync(
            textContent.Length,
            () => ShowTextAsync(file.Name, textContent, item, line, openWithDifftool, checkGitAttributes: true, viewToken),
            viewToken);
    }

    private async Task ViewFileCoreAsync(
        string fileName,
        bool isSubmodule,
        FileStatusItem? item,
        int? line,
        Action? openWithDifftool,
        CancellationToken viewToken)
    {
        viewToken.ThrowIfCancellationRequested();
        string? fullPath = _fullPathResolver.Resolve(fileName);
        ArgumentNullException.ThrowIfNull(fullPath);

        if (!isSubmodule
            && (item is null || item.Item.TreeId.IsZero)
            && (fileName.EndsWith('/') || Directory.Exists(fullPath)))
        {
            if (!GitModule.IsValidGitWorkingDir(fullPath))
            {
                await ShowTextAsync(fileName, "Directory: " + fileName, item, line, openWithDifftool, checkGitAttributes: false, viewToken);
                return;
            }

            isSubmodule = true;
        }

        if (!isSubmodule && !File.Exists(fullPath))
        {
            await ShowTextAsync(fileName, $"File {fullPath} does not exist", item, line, openWithDifftool, checkGitAttributes: false, viewToken);
            return;
        }

        long contentLength = GetFileLength(fullPath);
        await ShowOrDeferAsync(
            contentLength,
            async () =>
            {
                if (isSubmodule)
                {
                    string text = await Task.Run(
                        () => SubmoduleResources.GetSubmoduleText(Module, fileName.TrimEnd('/'), ""),
                        viewToken);
                    await ShowTextAsync(fileName, text, item, line: null, openWithDifftool, checkGitAttributes: false, viewToken);
                    return;
                }

                if (FileHelper.IsImage(fileName))
                {
                    Bitmap? image = await LoadFileImageAsync(fullPath, viewToken);
                    if (image is not null)
                    {
                        await ShowImageAsync(fileName, item, image, openWithDifftool, viewToken);
                        return;
                    }

                    (string failedImageText, byte[] preamble) = await LoadFileTextAsync(fullPath, viewToken);
                    FilePreamble = preamble;
                    await ShowHexDumpAsync(_cannotViewImage.Text, fileName, failedImageText, openWithDifftool, viewToken);
                    return;
                }

                (string textContent, byte[] filePreamble) = await LoadFileTextAsync(fullPath, viewToken);
                FilePreamble = filePreamble;
                await ShowTextAsync(fileName, textContent, item, line, openWithDifftool, checkGitAttributes: true, viewToken);
            },
            viewToken);
    }

    private async Task ShowTextAsync(
        string? fileName,
        string text,
        FileStatusItem? item,
        int? line,
        Action? openWithDifftool,
        bool checkGitAttributes,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool isBinary = (checkGitAttributes && FileHelper.IsBinaryFileName(Module, fileName))
                        || FileHelper.IsBinaryFileAccordingToContent(text);

        await this.SwitchToMainThreadAsync(cancellationToken);
        ResetView(ViewMode.Text, fileName, item, openWithDifftool);
        if (isBinary)
        {
            try
            {
                DisplayAsHexDump(_binaryFile.Text, fileName ?? string.Empty, text);
            }
            catch
            {
                SetText(string.Format(_binaryFileDetected.Text, fileName));
            }
        }
        else if (_viewMode == ViewMode.FixedDiff)
        {
            string parsedText = text;
            PatchHighlightService highlightService = new(ref parsedText, text.Contains('\u001b'), isGitWordDiff: false);
            SetDiffText(parsedText, highlightService, showLeftColumn: true);
        }
        else
        {
            SetText(text);
        }

        if (line is not null)
        {
            GoToLine(line.Value);
        }

        TextLoaded?.Invoke(this, EventArgs.Empty);
    }

    private async Task ShowHexDumpAsync(
        string fileNameFormat,
        string fileName,
        string text,
        Action? openWithDifftool,
        CancellationToken cancellationToken)
    {
        await this.SwitchToMainThreadAsync(cancellationToken);
        ResetView(ViewMode.Text, fileName, item: null, openWithDifftool);
        DisplayAsHexDump(fileNameFormat, fileName, text);
        TextLoaded?.Invoke(this, EventArgs.Empty);
    }

    private async Task ShowImageAsync(
        string fileName,
        FileStatusItem? item,
        Bitmap image,
        Action? openWithDifftool,
        CancellationToken cancellationToken)
    {
        Bitmap? imageToDispose = image;
        try
        {
            await this.SwitchToMainThreadAsync(cancellationToken);
            ResetView(ViewMode.Image, fileName, item, openWithDifftool);
            SetText(string.Empty);
            _image = imageToDispose;
            ImagePreview.Source = imageToDispose;
            imageToDispose = null;
        }
        finally
        {
            imageToDispose?.Dispose();
        }
    }

    private async Task<string> LoadBlobTextAsync(string fileName, ObjectId blobId, CancellationToken cancellationToken)
    {
        bool stripAnsiEscapeCodes = string.IsNullOrEmpty(fileName)
            || (!fileName.EndsWith(".diff", StringComparison.OrdinalIgnoreCase)
                && !fileName.EndsWith(".patch", StringComparison.OrdinalIgnoreCase));
        return await Task.Run(
            () => Module.GetFileText(blobId, Encoding, stripAnsiEscapeCodes) ?? string.Empty,
            cancellationToken);
    }

    private async Task<Bitmap?> LoadBlobImageAsync(ObjectId blobId, CancellationToken cancellationToken)
    {
        using MemoryStream? stream = await Module.GetFileStreamAsync(blobId.ToString(), cancellationToken);
        return stream is null ? null : TryCreateImage(stream, cancellationToken);
    }

    private static async Task<Bitmap?> LoadFileImageAsync(string fullPath, CancellationToken cancellationToken)
    {
        try
        {
            await using FileStream stream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 4096, useAsync: true);
            using MemoryStream copy = new();
            await stream.CopyToAsync(copy, cancellationToken);
            copy.Position = 0;
            return TryCreateImage(copy, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static Bitmap? TryCreateImage(Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new Bitmap(stream);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private async Task<(string Text, byte[] Preamble)> LoadFileTextAsync(string fullPath, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 4096, useAsync: true);
        using StreamReader reader = EncodingFileReader.OpenStream(stream, Encoding);
        string content = await reader.ReadToEndAsync(cancellationToken);
        return (content, reader.CurrentEncoding.GetPreamble());
    }

    private static long GetFileLength(string fullPath)
    {
        try
        {
            return File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task ShowOrDeferAsync(long contentLength, Func<Task> showFunc, CancellationToken cancellationToken)
    {
        await this.SwitchToMainThreadAsync(cancellationToken);
        if (contentLength > MaximumAutomaticPreviewLength)
        {
            ResetView(ViewMode.Text, fileName: null);
            SetText(string.Empty);
            _NO_TRANSLATE_lblShowPreview.Content = string.Format(
                _largeFileSizeWarning.Text,
                contentLength / (1024d * 1024));
            _NO_TRANSLATE_lblShowPreview.IsVisible = true;
            _deferShowFunc = showFunc;
            return;
        }

        _NO_TRANSLATE_lblShowPreview.IsVisible = false;
        _deferShowFunc = null;
        await showFunc();
    }

    private async Task LoadWithErrorHandlingAsync(Func<Task> load, CancellationToken cancellationToken)
    {
        try
        {
            await load();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await this.SwitchToMainThreadAsync();
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            ResetView(ViewMode.Text, fileName: null);
            SetText("Unsupported file: \n\n" + exception);
            TextLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    private CancellationToken BeginView(CancellationToken cancellationToken)
    {
        CancellationToken viewToken = _viewSequence.Next();
        _externalCancellationRegistration.Dispose();
        _externalCancellationRegistration = cancellationToken.CanBeCanceled
            ? cancellationToken.Register(_viewSequence.CancelCurrent)
            : default;
        return viewToken;
    }

    private void CancelPendingView()
    {
        _ = BeginView(CancellationToken.None);
        _externalCancellationRegistration.Dispose();
        _deferShowFunc = null;
        _NO_TRANSLATE_lblShowPreview.IsVisible = false;
    }

    private void ResetView(
        ViewMode viewMode,
        string? fileName,
        FileStatusItem? item = null,
        Action? openWithDifftool = null)
    {
        _viewMode = viewMode;
        _fileName = fileName;
        _viewItem = item;
        _openWithDifftool = openWithDifftool;
        _deferShowFunc = null;
        _NO_TRANSLATE_lblShowPreview.IsVisible = false;
        fileviewerToolbar.IsVisible = false;
        if (_viewMode == ViewMode.Text
            && !string.IsNullOrEmpty(fileName)
            && (fileName.EndsWith(".diff", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".patch", StringComparison.OrdinalIgnoreCase)))
        {
            _viewMode = ViewMode.FixedDiff;
        }

        ClearImage();
        PictureBox.IsVisible = _viewMode == ViewMode.Image;
        TextEditor.IsVisible = _viewMode != ViewMode.Image;
        ClearDiffHighlighting();
        UpdateDisplayOptionVisibility();
        ApplySyntaxHighlighting();
    }

    private void ApplySyntaxHighlighting()
    {
        bool shouldHighlight = !string.IsNullOrEmpty(_fileName)
                               && (_viewMode == ViewMode.Text
                                   || (ShowSyntaxHighlightingInDiff && _viewMode.IsPartialTextView()));
        string? extension = shouldHighlight ? Path.GetExtension(_fileName) : string.Empty;
        TextEditor.SyntaxHighlighting = string.IsNullOrEmpty(extension)
            ? null
            : HighlightingManager.Instance.GetDefinitionByExtension(extension);
        TextEditor.TextArea.TextView.Redraw();
    }

    private void BindSettingsCommands(IGitUICommands? commands)
    {
        if (ReferenceEquals(_settingsCommands, commands))
        {
            return;
        }

        if (_settingsCommands is not null)
        {
            _settingsCommands.PostSettings -= UICommands_PostSettings;
        }

        _settingsCommands = commands;
        if (_settingsCommands is not null)
        {
            _settingsCommands.PostSettings += UICommands_PostSettings;
        }
    }

    private void UICommands_PostSettings(object? sender, GitUIPostActionEventArgs? e)
    {
        Dispatcher.UIThread.Post(() => VRulerPosition = AppSettings.DiffVerticalRulerPosition);
    }

    private void UpdateDisplayOptionVisibility()
    {
        bool isPartialTextView = _viewMode.IsPartialTextView();
        showSyntaxHighlightingToolStripMenuItem.IsVisible = isPartialTextView && _viewMode != ViewMode.FixedDiff;
        showSyntaxHighlighting.IsVisible = isPartialTextView;
    }

    private void UpdateLineNumberVisibility()
    {
        bool hasDiffLineNumbers = _diffHighlightService is not null;
        TextEditor.ShowLineNumbers = ShowLineNumbers ?? !hasDiffLineNumbers;
    }

    private void UpdateSyntaxHighlightingToggleState()
    {
        showSyntaxHighlightingToolStripMenuItem.IsChecked = ShowSyntaxHighlightingInDiff;
        SetToolbarChecked(showSyntaxHighlighting, ShowSyntaxHighlightingInDiff);
    }

    private static void SetToolbarChecked(Button button, bool isChecked)
    {
        if (isChecked)
        {
            if (!button.Classes.Contains("checked"))
            {
                button.Classes.Add("checked");
            }
        }
        else
        {
            button.Classes.Remove("checked");
        }
    }

    private void ToggleNonPrintingChars(bool show)
    {
        _showNonPrintingChars = show;
        TextEditor.Options.ShowSpaces = show;
        TextEditor.Options.ShowTabs = show;
        TextEditor.Options.ShowEndOfLine = show;
        TextEditor.Options.EndOfLineCRLFGlyph = AppSettings.ShowEolMarkerAsGlyph ? EndOfLineGlyph : "\\r\\n";
        TextEditor.Options.EndOfLineCRGlyph = AppSettings.ShowEolMarkerAsGlyph ? EndOfLineGlyph : "\\r";
        TextEditor.Options.EndOfLineLFGlyph = AppSettings.ShowEolMarkerAsGlyph ? EndOfLineGlyph : "\\n";
        showNonprintableCharactersToolStripMenuItem.IsChecked = show;
        SetToolbarChecked(showNonPrintChars, show);
    }

    private void ClearImage()
    {
        ImagePreview.Source = null;
        _image?.Dispose();
        _image = null;
    }

    private void DisplayAsHexDump(string fileNameFormat, string fileName, string data)
    {
        StringBuilder summary = new StringBuilder()
            .AppendLine(string.Format(fileNameFormat, fileName))
            .AppendLine();

        double mb = data.Length / (1024d * 1024);
        if (mb >= 0.1)
        {
            summary.Append($"{mb:N1}").Append(' ').Append(_fileSizeInMb.Text).Append(" / ");
        }

        summary.Append($"{data.Length:N0}").Append(' ').Append(_bytes.Text).AppendLine(":")
            .AppendLine();
        SetText(ToHexDump(data, summary));
    }

    private static string ToHexDump(string text, StringBuilder str, int columnWidth = 8, int columnCount = 2)
    {
        if (text.Length == 0)
        {
            return string.Empty;
        }

        int limit = Math.Min(text.Length, columnWidth * columnCount * 256);
        int i = 0;
        while (i < limit)
        {
            int baseIndex = i;
            if (i != 0)
            {
                str.AppendLine();
            }

            str.Append($"{baseIndex:X4}   ");
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                if (columnIndex != 0)
                {
                    str.Append("  ");
                }

                for (int j = 0; j < columnWidth; j++)
                {
                    if (j != 0)
                    {
                        str.Append(' ');
                    }

                    str.Append(i < text.Length ? ((byte)text[i]).ToString("X2") : "  ");
                    i++;
                }
            }

            str.Append("   ");
            i = baseIndex;
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                if (columnIndex != 0)
                {
                    str.Append(' ');
                }

                for (int j = 0; j < columnWidth; j++)
                {
                    if (i < text.Length)
                    {
                        char c = text[i];
                        str.Append(char.IsControl(c) ? '.' : c);
                    }
                    else
                    {
                        str.Append(' ');
                    }

                    i++;
                }
            }
        }

        if (text.Length > limit)
        {
            str.AppendLine();
            str.Append("[Truncated]");
        }

        return str.ToString();
    }

    /// <summary>Shows the Find window for this editor.</summary>
    public void Find(bool replace)
    {
        _findAndReplaceForm.ShowFor(TextEditor, replace && !TextEditor.IsReadOnly);
    }

    /// <summary>Finds the next or previous occurrence using the current Find settings.</summary>
    public Task FindNextAsync(bool searchForwardOrOpenWithDifftool)
    {
        return _findAndReplaceForm.FindNextAsync(
            viaF3: true,
            searchBackward: !searchForwardOrOpenWithDifftool,
            messageIfNotFound: "Text not found");
    }

    /// <summary>Reloads the configurable FileViewer hotkeys.</summary>
    public void ReloadHotkeys()
    {
        IGitUICommands? commands = TryGetUICommandsDirect(out IGitUICommands? directCommands)
            ? directCommands
            : this.GetLogicalAncestors().OfType<IGitModuleForm>().FirstOrDefault()?.UICommands;
        if (commands?.GetService(typeof(IHotkeySettingsLoader)) is not IHotkeySettingsLoader)
        {
            return;
        }

        LoadHotkeys(HotkeySettingsName);
        _hotkeysLoaded = true;
    }

    private void SetDiffText(string text, DiffHighlightService highlightService, bool showLeftColumn)
    {
        _diffHighlightService = highlightService;
        _diffBackgroundRenderer.SetHighlightService(highlightService);
        _diffTextColorizer.SetHighlightService(highlightService);
        _diffViewerLineNumberControl.DisplayLineNum(highlightService.LinesInfo, showLeftColumn);
        UpdateLineNumberVisibility();
        SetText(text);
    }

    private void ClearDiffHighlighting()
    {
        _diffHighlightService = null;
        _diffBackgroundRenderer.SetHighlightService(null);
        _diffTextColorizer.SetHighlightService(null);
        _diffViewerLineNumberControl.Clear();
        UpdateLineNumberVisibility();
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

    private int MaxLineNumber
    {
        get
        {
            if (_diffHighlightService is null)
            {
                return TextEditor.Document?.LineCount ?? 1;
            }

            IEnumerable<int> mappedLines = _diffHighlightService.LinesInfo.DiffLines.Values
                .SelectMany(line => new[] { line.LeftLineNumber, line.RightLineNumber })
                .Where(line => line != DiffLineInfo.NotApplicableLineNum);
            return mappedLines.DefaultIfEmpty(1).Max();
        }
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
        CancellationToken viewToken = BeginView(cancellationToken);
        if (item?.Item is null)
        {
            await this.SwitchToMainThreadAsync(viewToken);
            ViewPatchCore(null, useGitColoring: false, isCombinedDiff: false, isGitWordDiff: false);
            return;
        }

        if (item.Item.IsStatusOnly)
        {
            await ShowTextAsync(item.Item.Name, item.Item.ErrorMessage ?? string.Empty, item, line: null, openWithDifftool: null, checkGitAttributes: false, viewToken);
            return;
        }

        ObjectId firstId = item.FirstRevision?.ObjectId ?? item.SecondRevision.FirstParentId;
        ObjectId secondId = item.SecondRevision.ObjectId;
        if (!item.Item.IsSubmodule
            && (item.Item.IsNew || firstId.IsZero || (!item.Item.IsDeleted && FileHelper.IsImage(item.Item.Name))))
        {
            await ViewGitItemCoreAsync(item.Item, secondId, item, line: null, openWithDifftool: null, viewToken);
            return;
        }

        bool isTracked = item.Item.IsTracked || (!item.Item.TreeId.IsZero && !secondId.IsZero);

        (Patch? patch, string? errorMessage) = await Module.GetSingleDiffAsync(
            firstId,
            secondId,
            item.Item.Name,
            item.Item.OldName,
            extraDiffArguments: "",
            Encoding,
            cacheResult: true,
            isTracked,
            useGitColoring: false,
            GitCommandConfiguration.Default,
            viewToken);

        await this.SwitchToMainThreadAsync(viewToken);
        viewToken.ThrowIfCancellationRequested();
        ViewPatchCore(
            patch?.Text ?? errorMessage,
            useGitColoring: false,
            isCombinedDiff: false,
            isGitWordDiff: false,
            item.Item.Name,
            item);
    }

    /// <summary>
    ///  Updates and returns the blob identifier for a Git item, preserving the original
    ///  worktree/index distinction.
    /// </summary>
    public ObjectId GetUpdateTreeId(
        GitItemStatus file,
        ObjectId commitId,
        CancellationToken cancellationToken = default)
    {
        if (!file.TreeId.IsZero && !commitId.IsArtificial)
        {
            return file.TreeId;
        }

        if (commitId == ObjectId.WorkTreeId && (!file.TreeId.IsZero || file.IsSubmodule))
        {
            return default;
        }

        cancellationToken.ThrowIfCancellationRequested();
        IObjectGitItem[] items = [.. Module.GetTree(commitId, full: true, file.Name, cancellationToken)];
        if (items.Length == 1)
        {
            IObjectGitItem gitItem = items[0];
            file.IsSubmodule = gitItem.ObjectType == GitObjectType.Commit;
            file.TreeId = gitItem.ObjectId;
            return commitId == ObjectId.WorkTreeId ? default : file.TreeId;
        }

        return default;
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
        if (!_hotkeysLoaded)
        {
            ReloadHotkeys();
        }

        if (ProcessHotkey(KeysMapper.ToKeys(e)))
        {
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            EscapePressed?.Invoke();
            e.Handled = true;
        }
    }

    private void contextMenu_Opening(object? sender, EventArgs e)
    {
        copyToolStripMenuItem.IsEnabled = TextEditor.SelectionLength > 0;
        replaceToolStripMenuItem.IsVisible = !TextEditor.IsReadOnly;
        goToLineToolStripMenuItem.IsEnabled = MaxLineNumber > 0;
    }

    private void ShowNonprintableCharactersToolStripMenuItemClick(object? sender, EventArgs e)
    {
        ToggleNonPrintingChars(!_showNonPrintingChars);
        AppSettings.ShowNonPrintingChars.Value = _showNonPrintingChars;
    }

    private void ShowSyntaxHighlighting_Click(object? sender, EventArgs e)
    {
        ShowSyntaxHighlightingInDiff = !ShowSyntaxHighlightingInDiff;
        UpdateSyntaxHighlightingToggleState();
        AppSettings.ShowSyntaxHighlightingInDiff.Value = ShowSyntaxHighlightingInDiff;
        ApplySyntaxHighlighting();
        ExtraDiffArgumentsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PopulateEncodings()
    {
        _updatingEncoding = true;
        try
        {
            encodingToolStripComboBox.ItemsSource = GetAvailableEncodings()
                .Select(encoding => encoding.EncodingName)
                .ToArray();
        }
        finally
        {
            _updatingEncoding = false;
        }
    }

    private void ShowFileViewerToolbar()
    {
        fileviewerToolbar.IsVisible = true;
        if (TryGetUICommandsDirect(out _))
        {
            _ = Encoding;
            UpdateEncodingSelection();
        }
    }

    private void UpdateEncodingSelection()
    {
        void Update()
        {
            _updatingEncoding = true;
            try
            {
                encodingToolStripComboBox.SelectedItem = _encoding?.EncodingName;
                if (_encoding is null)
                {
                    encodingToolStripComboBox.SelectedIndex = -1;
                }
            }
            finally
            {
                _updatingEncoding = false;
            }
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Update();
        }
        else
        {
            Dispatcher.UIThread.Post(Update);
        }
    }

    private void encodingToolStripComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_updatingEncoding || encodingToolStripComboBox.SelectedItem is not string encodingName)
        {
            return;
        }

        Encoding encoding = GetAvailableEncodings()
            .FirstOrDefault(candidate => candidate.EncodingName == encodingName)
            ?? Module.FilesEncoding;
        if (encoding.Equals(Encoding))
        {
            return;
        }

        Encoding = encoding;
        ExtraDiffArgumentsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static IReadOnlyList<Encoding> GetAvailableEncodings()
    {
        if (AppSettings.AvailableEncodings.Count > 0)
        {
            return [.. AppSettings.AvailableEncodings.Values];
        }

        return
        [
            .. new Encoding[]
            {
                Encoding.Default,
                Encoding.ASCII,
                Encoding.Unicode,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            }.DistinctBy(encoding => encoding.WebName),
        ];
    }

    private void llShowPreview_LinkClicked(object? sender, EventArgs e)
    {
        Func<Task>? show = _deferShowFunc;
        _deferShowFunc = null;
        _NO_TRANSLATE_lblShowPreview.IsVisible = false;
        if (show is not null)
        {
            this.InvokeAndForget(show);
        }
    }

    private void CopyToolStripMenuItemClick(object? sender, EventArgs e)
    {
        TextEditor.Copy();
    }

    private void FindToolStripMenuItemClick(object? sender, EventArgs e)
    {
        Find(sender == replaceToolStripMenuItem);
    }

    private void goToLineToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using FormGoToLine formGoToLine = new();
        formGoToLine.SetMaxLineNumber(MaxLineNumber);
        if (formGoToLine.ShowDialog(TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window) == WinFormsShims.DialogResult.OK)
        {
            GoToLine(formGoToLine.GetLineNumber());
        }
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(
            nameof(FileViewer),
            nameof(showNonPrintChars),
            "ToolTipText",
            "Show nonprinting characters");
        translation.AddTranslationItem(
            nameof(FileViewer),
            nameof(showSyntaxHighlighting),
            "ToolTipText",
            "Show syntax highlighting");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        SetTranslatedToolTip(showNonPrintChars, nameof(showNonPrintChars), "Show nonprinting characters");
        SetTranslatedToolTip(showSyntaxHighlighting, nameof(showSyntaxHighlighting), "Show syntax highlighting");

        return;

        void SetTranslatedToolTip(Control control, string name, string source)
        {
            string text = translation.TranslateItem(
                nameof(FileViewer),
                name,
                "ToolTipText",
                () => source) ?? source;
            if (ToolTip.GetTip(control) is TextBlock textBlock)
            {
                textBlock.Text = text;
            }
            else
            {
                ToolTip.SetTip(control, new TextBlock { Text = text });
            }
        }
    }

    /// <summary>Gets the persisted hotkey category name.</summary>
    public static readonly string HotkeySettingsName = "FileViewer";

    internal enum Command
    {
        Find = 0,
        Replace = 16,
        FindNextOrOpenWithDifftool = 8,
        FindPrevious = 9,
        GoToLine = 1,
        IncreaseNumberOfVisibleLines = 2,
        DecreaseNumberOfVisibleLines = 3,
        ShowEntireFile = 4,
        ShowSyntaxHighlighting = 17,
        ShowGitWordColoring = 18,
        ShowDifftastic = 19,
        TreatFileAsText = 5,
        NextChange = 6,
        PreviousChange = 7,
        NextOccurrence = 10,
        PreviousOccurrence = 11,
        StageLines = 12,
        UnstageLines = 13,
        ResetLines = 14,
        IgnoreAllWhitespace = 15,
    }

    protected override bool ExecuteCommand(int cmd)
    {
        switch ((Command)cmd)
        {
            case Command.Find:
                Find(replace: false);
                break;
            case Command.Replace:
                if (TextEditor.IsReadOnly)
                {
                    return false;
                }

                Find(replace: true);
                break;
            case Command.FindNextOrOpenWithDifftool:
                this.InvokeAndForget(() => FindNextAsync(searchForwardOrOpenWithDifftool: true));
                break;
            case Command.FindPrevious:
                this.InvokeAndForget(() => FindNextAsync(searchForwardOrOpenWithDifftool: false));
                break;
            case Command.GoToLine:
                goToLineToolStripMenuItem_Click(this, EventArgs.Empty);
                break;
            case Command.ShowSyntaxHighlighting:
                if (!showSyntaxHighlightingToolStripMenuItem.IsVisible)
                {
                    return false;
                }

                ShowSyntaxHighlighting_Click(this, EventArgs.Empty);
                break;
            default:
                return base.ExecuteCommand(cmd);
        }

        return true;
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

    private void PictureBox_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            TopScrollReached?.Invoke(sender, EventArgs.Empty);
            e.Handled = true;
        }
        else if (e.Delta.Y < 0)
        {
            BottomScrollReached?.Invoke(sender, EventArgs.Empty);
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

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FileViewer _control;

        public TestAccessor(FileViewer control)
        {
            _control = control;
        }

        public FindAndReplaceForm FindAndReplaceForm => _control._findAndReplaceForm;

        public ComboBox EncodingToolStripComboBox => _control.encodingToolStripComboBox;

        public Border FileViewerToolbar => _control.fileviewerToolbar;

        public MenuItem ShowNonprintingCharactersMenuItem => _control.showNonprintableCharactersToolStripMenuItem;

        public Button ShowNonprintingCharactersButton => _control.showNonPrintChars;

        public MenuItem ShowSyntaxHighlightingMenuItem => _control.showSyntaxHighlightingToolStripMenuItem;

        public Button ShowSyntaxHighlightingButton => _control.showSyntaxHighlighting;

        public bool ShowSyntaxHighlightingInDiff => _control.ShowSyntaxHighlightingInDiff;

        public int VRulerPosition => _control.VRulerPosition;

        public bool HasDiffHighlighting => _control._diffHighlightService is not null;

        public HyperlinkButton ShowPreviewLink => _control._NO_TRANSLATE_lblShowPreview;

        public Image ImagePreview => _control.ImagePreview;

        public Border PictureBox => _control.PictureBox;

        public ViewMode ViewMode => _control._viewMode;

        public FileStatusItem? ViewItem => _control._viewItem;

        public Action? OpenWithDifftool => _control._openWithDifftool;
    }
}
