using System.Collections.Concurrent;
using System.Diagnostics;
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
using GitCommands.Patches;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Compat;
using GitUI.Editor.Diff;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Editor;

public partial class FileViewer : GitModuleControl
{
    private const long MaximumAutomaticPreviewLength = 5 * 1024 * 1024;
    private const string EndOfLineGlyph = "¶";

    private readonly CancellationTokenSequence _viewSequence = new();
    private readonly DiffBackgroundRenderer _diffBackgroundRenderer;
    private readonly DiffTextColorizer _diffTextColorizer;
    private CancellationTokenRegistration _externalCancellationRegistration;
    private bool _allowLinePatching;
    private IGitUICommandsSource? _commandsSource;
    private bool _hotkeysLoaded;
    private Bitmap? _image;
    private Action? _openWithDifftool;
    private string? _fileName;
    private bool? _showLineNumbers;
    private bool _showNonPrintingChars;
    private IGitUICommands? _settingsCommands;
    private bool _updatingEncoding;
    private int _lastCaretLine = -1;

    /// <summary>
    /// Raised when the Escape key is pressed (and only when no selection exists, as the default behaviour of escape is to clear the selection).
    /// </summary>
    public event Action? EscapePressed;

    private readonly TranslationString _largeFileSizeWarning = new("This file is {0:N1} MB. Showing large files can be slow. Click to show anyway.");
    private readonly TranslationString _cannotViewImage = new("Cannot view image {0}");
    private readonly TranslationString _fileSizeInMb = new("MB");
    private readonly TranslationString _bytes = new("bytes");
    private readonly TranslationString _binaryFile = new("Binary file: {0}");
    private readonly TranslationString _binaryFileDetected = new("Binary file: {0} (Detected)");

    /// <summary>
    ///  Raised when the caret moves to a different line (zero-based, like WinForms).
    /// </summary>
    public event EventHandler<SelectedLineEventArgs>? SelectedLineChanged;

    public event EventHandler? HScrollPositionChanged;

    public event EventHandler? VScrollPositionChanged;

    /// <summary>
    ///  Raised when scrolling below the last line.
    /// </summary>
    public event EventHandler? BottomScrollReached;

    /// <summary>
    ///  Raised when scrolling above the first line.
    /// </summary>
    public event EventHandler? TopScrollReached;

    public event EventHandler? RequestDiffView;

    /// <summary>
    ///  Raised when the editable document text changes.
    /// </summary>
    public event EventHandler? TextChanged;

    /// <summary>
    ///  Raised after text content has been displayed.
    /// </summary>
    public event EventHandler? TextLoaded;

    public event System.ComponentModel.CancelEventHandler? ContextMenuOpening;

    /// <summary>
    ///  Raised when the selected file encoding changes and the consumer should reload content.
    /// </summary>
    public event EventHandler<EventArgs>? ExtraDiffArgumentsChanged;

    /// <summary>
    ///  Raised after a selected-line or whole-file patch has been applied.
    /// </summary>
    public event EventHandler? PatchApplied;
    private readonly IFullPathResolver _fullPathResolver;
    private readonly TaskDialogPage _NO_TRANSLATE_resetSelectedLinesConfirmationDialog;
    private readonly ContinuousScrollEventManager _continuousScrollEventManager = new();

    // Cache for the configuration of a difftastic difftool
    private readonly Lock _difftasticCmdCacheLock = new();
    private readonly ConcurrentDictionary<string, Lazy<bool>> _difftasticCmdCache = [];
    private ViewMode _viewMode;
    private Encoding? _encoding;
    private Func<Task>? _deferShowFunc;
    private FileStatusItem? _viewItem;

    public FileViewer()
    {
        InitializeComponent();
        ignoreWhitespaceAtEol.Icon = Properties.Images.WhitespaceIgnoreEol.AdaptLightness();
        ignoreWhiteSpaces.Icon = Properties.Images.WhitespaceIgnore.AdaptLightness();
        ignoreAllWhitespaces.Icon = Properties.Images.WhitespaceIgnoreAll.AdaptLightness();
        Bitmap showWhitespaceImage = Properties.Images.ShowWhitespace.AdaptLightness();
        ((Image)showNonPrintChars.Content!).Source = showWhitespaceImage;
        ((Image)showNonprintableCharactersToolStripMenuItem.Icon!).Source = showWhitespaceImage;
        Bitmap syntaxHighlightingImage = ((Bitmap)((Image)showSyntaxHighlighting.Content!).Source!).AdaptLightness();
        ((Image)showSyntaxHighlighting.Content!).Source = syntaxHighlightingImage;
        ((Image)showSyntaxHighlightingToolStripMenuItem.Icon!).Source = syntaxHighlightingImage;

        _diffBackgroundRenderer = new DiffBackgroundRenderer(this);
        _diffTextColorizer = new DiffTextColorizer(this);
        TextEditor.TextArea.TextView.BackgroundRenderers.Add(_diffBackgroundRenderer);
        TextEditor.TextArea.TextView.LineTransformers.Add(_diffTextColorizer);
        TextEditor.TextChanged += (sender, e) => TextChanged?.Invoke(sender, e);
        TextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        TextEditor.TextArea.TextView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
        TextEditor.KeyDown += TextEditor_KeyDown;
        TextEditor.DoubleTapped += (_, _) => RequestDiffView?.Invoke(this, EventArgs.Empty);
        _continuousScrollEventManager.TopScrollReached += _continuousScrollEventManager_TopScrollReached;
        _continuousScrollEventManager.BottomScrollReached += _continuousScrollEventManager_BottomScrollReached;
        internalFileViewer.SetContinuousScrollManager(_continuousScrollEventManager);
        TextEditor.PointerMoved += (_, _) => ShowFileViewerToolbar();
        PointerExited += (_, _) => fileviewerToolbar.IsVisible = false;
        PictureBox.PointerWheelChanged += PictureBox_MouseWheel;
        DetachedFromLogicalTree += (_, _) =>
        {
            BindSettingsCommands(commands: null);
            if (_commandsSource is not null)
            {
                _commandsSource.UICommandsChanged -= OnUICommandsChanged;
                _commandsSource = null;
            }

            CancelPendingView();
            ClearImage();
        };

        _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);

        NumberOfContextLines = AppSettings.NumberOfContextLines;
        IgnoreWhitespace = AppSettings.IgnoreWhitespaceKind.GetValue(
            reload: !AppSettings.RememberIgnoreWhiteSpacePreference);
        ShowEntireFile = AppSettings.ShowEntireFile.GetValue(
            reload: !AppSettings.RememberShowEntireFilePreference);
        _NO_TRANSLATE_resetSelectedLinesConfirmationDialog = new TaskDialogPage
        {
            Text = TranslatedStrings.ResetSelectedLinesConfirmation,
            Caption = TranslatedStrings.ResetChangesCaption,
            Icon = TaskDialogIcon.Warning,
            Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
            DefaultButton = TaskDialogButton.Yes,
            SizeToContent = true,
        };

        stageSelectedLinesToolStripMenuItem.Click += stageSelectedLinesToolStripMenuItem_Click;
        unstageSelectedLinesToolStripMenuItem.Click += unstageSelectedLinesToolStripMenuItem_Click;
        resetSelectedLinesToolStripMenuItem.Click += resetSelectedLinesToolStripMenuItem_Click;
        copyToolStripMenuItem.Click += CopyToolStripMenuItemClick;
        copyPatchToolStripMenuItem.Click += CopyPatchToolStripMenuItemClick;
        copyNewVersionToolStripMenuItem.Click += copyNewVersionToolStripMenuItem_Click;
        copyOldVersionToolStripMenuItem.Click += copyOldVersionToolStripMenuItem_Click;
        increaseNumberOfLinesToolStripMenuItem.Click += IncreaseNumberOfLinesToolStripMenuItemClick;
        decreaseNumberOfLinesToolStripMenuItem.Click += DecreaseNumberOfLinesToolStripMenuItemClick;
        showEntireFileToolStripMenuItem.Click += ShowEntireFileToolStripMenuItemClick;
        showNonprintableCharactersToolStripMenuItem.Click += ShowNonprintableCharactersToolStripMenuItemClick;
        showSyntaxHighlightingToolStripMenuItem.Click += ShowSyntaxHighlighting_Click;
        ignoreWhitespaceAtEolToolStripMenuItem.Click += IgnoreWhitespaceAtEolToolStripMenuItem_Click;
        ignoreWhitespaceChangesToolStripMenuItem.Click += IgnoreWhitespaceChangesToolStripMenuItemClick;
        ignoreAllWhitespaceChangesToolStripMenuItem.Click += IgnoreAllWhitespaceChangesToolStripMenuItem_Click;
        findToolStripMenuItem.Click += FindToolStripMenuItemClick;
        replaceToolStripMenuItem.Click += FindToolStripMenuItemClick;
        goToLineToolStripMenuItem.Click += goToLineToolStripMenuItem_Click;
        showNonPrintChars.Click += ShowNonprintableCharactersToolStripMenuItemClick;
        showSyntaxHighlighting.Click += ShowSyntaxHighlighting_Click;
        nextChangeButton.Click += NextChangeButtonClick;
        previousChangeButton.Click += PreviousChangeButtonClick;
        increaseNumberOfLines.Click += IncreaseNumberOfLinesToolStripMenuItemClick;
        decreaseNumberOfLines.Click += DecreaseNumberOfLinesToolStripMenuItemClick;
        showEntireFileButton.Click += ShowEntireFileToolStripMenuItemClick;
        ignoreWhitespaceAtEol.Click += IgnoreWhitespaceAtEolToolStripMenuItem_Click;
        ignoreWhiteSpaces.Click += IgnoreWhitespaceChangesToolStripMenuItemClick;
        ignoreAllWhitespaces.Click += IgnoreAllWhitespaceChangesToolStripMenuItem_Click;
        showPatchToolStripMenuItem.Click += ResetPatchAppearanceToolStripMenuItemClick;
        showGitWordColoringToolStripMenuItem.Click += ToggleGitWordColoringToolStripMenuItemClick;
        showDifftasticToolStripMenuItem.Click += ToggleDifftasticToolStripMenuItemClick;
        treatAllFilesAsTextToolStripMenuItem.Click += TreatAllFilesAsTextToolStripMenuItemClick;
        automaticContinuousScrollToolStripMenuItem.Click += ContinuousScrollToolStripMenuItemClick;
        settingsButton.Click += settingsButton_Click;
        contextMenu.Opening += contextMenu_Opening;
        _NO_TRANSLATE_lblShowPreview.Click += llShowPreview_LinkClicked;
        encodingToolStripComboBox.SelectionChanged += encodingToolStripComboBox_SelectedIndexChanged;

        HotkeysEnabled = true;
        UICommandsSourceSet += OnUICommandsSourceSet;
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
        _ = AppSettings.DiffDisplayAppearance.GetValue(reload: !AppSettings.RememberDiffDisplayAppearance.Value);
        TreatAllFilesAsText = false;
        automaticContinuousScrollToolStripMenuItem.IsChecked = AppSettings.AutomaticContinuousScroll;
        automaticContinuousScrollToolStripMenuItem.Header = TranslatedStrings.ContScrollToNextFileOnlyWithAlt;
        ToggleNonPrintingChars(_showNonPrintingChars);
        UpdateSyntaxHighlightingToggleState();
        UpdateDiffOptionState();
        VRulerPosition = AppSettings.DiffVerticalRulerPosition;

        InitializeComplete();
    }

    /// <summary>
    ///  Gets the preamble detected while reading the current working-tree file.
    /// </summary>
    public byte[]? FilePreamble { get; private set; }

    public WinFormsShims.Font Font
    {
        get => internalFileViewer.Font;
        set => internalFileViewer.Font = value;
    }

    /// <summary>Gets or sets whether the editor can be changed.</summary>
    public bool IsReadOnly
    {
        get => internalFileViewer.IsReadOnly;
        set => internalFileViewer.IsReadOnly = value;
    }

    public bool EnableAutomaticContinuousScroll
    {
        get => automaticContinuousScrollToolStripMenuItem.IsVisible;
        set => automaticContinuousScrollToolStripMenuItem.IsVisible = value;
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
    ///  Scrolls to the first line.
    /// </summary>
    public void ScrollToTop() => internalFileViewer.ScrollToTop();

    /// <summary>
    ///  Scrolls to the last line.
    /// </summary>
    public void ScrollToBottom() => internalFileViewer.ScrollToBottom();

    public int HScrollPosition
    {
        get => internalFileViewer.HScrollPosition;
        set => internalFileViewer.HScrollPosition = value;
    }

    public int VScrollPosition
    {
        get => internalFileViewer.VScrollPosition;
        set => internalFileViewer.VScrollPosition = value;
    }

    public bool PatchUseGitColoring => showGitWordColoringToolStripMenuItem.IsChecked == true || AppSettings.UseGitColoring.Value;

    public Lazy<bool> IsDifftasticEnabled
    {
        get
        {
            lock (_difftasticCmdCacheLock)
            {
                // GetEffectiveSettings() checks Windows only, this need to be checked for each instance
                if (_difftasticCmdCache.TryGetValue(Module.WorkingDir, out Lazy<bool>? isEnabled))
                {
                    return isEnabled;
                }

                isEnabled = _difftasticCmdCache[Module.WorkingDir] = new Lazy<bool>(() =>
                {
                    try
                    {
                        const string difftasticCmd = "difftool.difftastic.cmd";
                        return !string.IsNullOrEmpty(Module.GetEffectiveSetting(difftasticCmd));
                    }
                    catch (Exception exception)
                    {
                        Trace.WriteLine(exception);
                        return false;
                    }
                });

                return isEnabled;
            }
        }
    }

    // Private properties
    private IgnoreWhitespaceKind IgnoreWhitespace { get; set; }

    private int NumberOfContextLines { get; set; }

    private bool ShowEntireFile { get; set; }

    private bool TreatAllFilesAsText { get; set; }

    private bool ShowSyntaxHighlightingInDiff { get; set; }

    // Public methods
    public void SetGitBlameGutter(IEnumerable<GitBlameEntry> gitBlameEntries)
    {
        internalFileViewer.ShowGutterAvatars = AppSettings.BlameShowAuthorAvatar;

        if (AppSettings.BlameShowAuthorAvatar)
        {
            internalFileViewer.SetGitBlameGutter(gitBlameEntries);
        }
    }

    public void ClearBlameGutter()
    {
        internalFileViewer.ShowGutterAvatars = false;
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
        stageSelectedLinesToolStripMenuItem.InputGesture = GetGesture(Command.StageLines);
        unstageSelectedLinesToolStripMenuItem.InputGesture = GetGesture(Command.UnstageLines);
        resetSelectedLinesToolStripMenuItem.InputGesture = GetGesture(Command.ResetLines);
        ignoreAllWhitespaceChangesToolStripMenuItem.InputGesture = GetGesture(Command.IgnoreAllWhitespace);
        increaseNumberOfLinesToolStripMenuItem.InputGesture = GetGesture(Command.IncreaseNumberOfVisibleLines);
        decreaseNumberOfLinesToolStripMenuItem.InputGesture = GetGesture(Command.DecreaseNumberOfVisibleLines);
        showEntireFileToolStripMenuItem.InputGesture = GetGesture(Command.ShowEntireFile);
        showSyntaxHighlightingToolStripMenuItem.InputGesture = GetGesture(Command.ShowSyntaxHighlighting);
        showGitWordColoringToolStripMenuItem.InputGesture = GetGesture(Command.ShowGitWordColoring);
        showDifftasticToolStripMenuItem.InputGesture = GetGesture(Command.ShowDifftastic);
        treatAllFilesAsTextToolStripMenuItem.InputGesture = GetGesture(Command.TreatFileAsText);
        findToolStripMenuItem.InputGesture = GetGesture(Command.Find);
        replaceToolStripMenuItem.InputGesture = GetGesture(Command.Replace);
        goToLineToolStripMenuItem.InputGesture = GetGesture(Command.GoToLine);

        UpdateTooltipWithShortcut(nextChangeButton, Command.NextChange);
        UpdateTooltipWithShortcut(previousChangeButton, Command.PreviousChange);
        UpdateTooltipWithShortcut(increaseNumberOfLines, Command.IncreaseNumberOfVisibleLines);
        UpdateTooltipWithShortcut(decreaseNumberOfLines, Command.DecreaseNumberOfVisibleLines);
        UpdateTooltipWithShortcut(showEntireFileButton, Command.ShowEntireFile);
        UpdateTooltipWithShortcut(showSyntaxHighlighting, Command.ShowSyntaxHighlighting);
        UpdateTooltipWithShortcut(ignoreAllWhitespaces, Command.IgnoreAllWhitespace);
        _hotkeysLoaded = true;

        return;

        KeyGesture? GetGesture(Command command)
            => KeysMapper.ToKeyGesture(Hotkeys.GetShortcutKey(command));
    }

    public Separator AddContextMenuSeparator()
    {
        // Avalonia context menus use Control items rather than WinForms ToolStripItem objects.
        Separator separator = new();
        contextMenu.Items.Add(separator);
        return separator;
    }

    public MenuItem AddContextMenuEntry(string text, EventHandler toolStripItem_Click)
    {
        // Avalonia context menus use Control items rather than WinForms ToolStripItem objects.
        MenuItem toolStripItem = new() { Header = text };
        contextMenu.Items.Add(toolStripItem);
        toolStripItem.Click += (sender, e) => toolStripItem_Click(sender, e);
        return toolStripItem;
    }

    public void EnableScrollBars(bool enable)
    {
        internalFileViewer.EnableScrollBars(enable);
    }

    /// <summary>
    ///  Builds the user-selected context and whitespace arguments for Git diff.
    /// </summary>
    public ArgumentString GetExtraDiffArguments(bool isRangeDiff = false, bool isCombinedDiff = false)
    {
        return new ArgumentBuilder
        {
            { IgnoreWhitespace == IgnoreWhitespaceKind.AllSpace, "--ignore-all-space" },
            { IgnoreWhitespace == IgnoreWhitespaceKind.Change, "--ignore-space-change" },
            { IgnoreWhitespace == IgnoreWhitespaceKind.Eol, "--ignore-space-at-eol" },
            { ShowEntireFile, "--inter-hunk-context=9000 --unified=9000", $"--unified={NumberOfContextLines}" },
            { isRangeDiff && NumberOfContextLines == 0, "--no-patch" },
            { TreatAllFilesAsText, "--text" },
            { !isCombinedDiff && AppSettings.DiffDisplayAppearance.Value == DiffDisplayAppearance.GitWordDiff, "--word-diff=color" },
        };
    }

    public (ArgumentString Args, string ExtraCacheKey) GetDifftasticArguments()
    {
        EnvironmentAbstraction env = new();
        StringBuilder extraCacheKey = new();

        // Difftastic coloring is always used (AppSettings.UseGitColoring.Value is not used).
        // Allow user to override with difftool command line options.
        SetEnvironmentVariable("DFT_COLOR", "always");

        // DFT_BACKGROUND="dark" applies bold-bold colors, "light" corresponds better with Git colors
        SetEnvironmentVariable("DFT_BACKGROUND", "light");
        SetEnvironmentVariable("DFT_SYNTAX_HIGHLIGHT", ShowSyntaxHighlightingInDiff ? "on" : "off");
        int contextLines = ShowEntireFile ? 9000 : NumberOfContextLines;
        SetEnvironmentVariable("DFT_CONTEXT", contextLines.ToString());

        // Reasonable similar to IgnoreWhitespaceKind.Eol
        SetEnvironmentVariable("DFT_STRIP_CR", IgnoreWhitespace == IgnoreWhitespaceKind.None ? "off" : "on");

        // Avalonia exposes device-independent pixels, so use the same character-width estimate
        // without WinForms' DpiUtil scaling and keep the even-width Difftastic contract.
        // Guess a reasonable even column number from viewer width, so scrollbar is (barely) activated.
        // At least 2*(2+linenoLength) of the width is used for difftastic lineno.
        // DFT_WIDTH is also used when parsing in GE, must be in environment.
        int width = Math.Max(88, Math.Min(200, (int)internalFileViewer.Bounds.Width / 7)) / 2 * 2;
        SetEnvironmentVariable("DFT_WIDTH", width.ToString());

        // Also export to WSL environment.
        env.SetEnvironmentVariable("WSLENV", "DFT_COLOR:DFT_BACKGROUND:DFT_SYNTAX_HIGHLIGHT:DFT_CONTEXT:DFT_STRIP_CR:DFT_WIDTH");

        return (new ArgumentBuilder
        {
            "--tool=difftastic",
            { TreatAllFilesAsText, "--text" },
        },
        extraCacheKey.ToString());

        void SetEnvironmentVariable(string variable, string value)
        {
            env.SetEnvironmentVariable(variable, value);
            extraCacheKey.AppendFormat($";{variable}={value}");
        }
    }

    public ArgumentString GetExtraGrepArguments()
    {
        int numberOfContextLines = ShowEntireFile ? 100_000 : NumberOfContextLines;
        return new ArgumentBuilder
        {
            "-h",
            $"--context={numberOfContextLines}",
            { TreatAllFilesAsText, "--text" },
        };
    }

    /// <summary>Gets the selected viewer text.</summary>
    public string GetSelectedText() => internalFileViewer.GetSelectedText();

    /// <summary>Gets the selected range start.</summary>
    public int GetSelectionPosition() => internalFileViewer.GetSelectionPosition();

    /// <summary>Gets the selected range length.</summary>
    public int GetSelectionLength() => internalFileViewer.GetSelectionLength();

    /// <summary>
    ///  Moves the caret to the given one-based line and scrolls it into view.
    /// </summary>
    public void GoToLine(int lineNumber)
    {
        TextDocument? document = TextEditor.Document;
        if (document is null || document.LineCount == 0)
        {
            return;
        }

        int documentLine = FindDocumentLine(lineNumber);
        documentLine = Math.Clamp(documentLine, 1, document.LineCount);
        TextEditor.TextArea.Caret.Position = new TextViewPosition(documentLine, column: 1);
        TextEditor.ScrollToLine(documentLine);
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

    /// <summary>Gets the one-based column number of the caret.</summary>
    public int CurrentFileColumn => TextEditor.TextArea.Caret.Column;

    /// <summary>
    ///  Gets the one-based line number of the caret.
    /// </summary>
    public int CurrentFileLine
    {
        get
        {
            DiffLineInfo? lineInfo = internalFileViewer.LineNumbersControl.GetLineInfo(TextEditor.TextArea.Caret.Line - 1);
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
    ///  Adds a background highlight for an inclusive range of zero-based lines.
    /// </summary>
    public void HighlightLines(int startLine, int endLine, System.Drawing.Color color)
    {
        internalFileViewer.HighlightLines(startLine, endLine, color);
    }

    /// <summary>
    ///  Removes all line highlights.
    /// </summary>
    public void ClearHighlighting()
    {
        internalFileViewer.ClearHighlighting();
    }

    internal void DontMarkGutterSelectedLine()
    {
        internalFileViewer.DontMarkGutterSelectedLine();
    }

    /// <summary>Gets the full viewer text.</summary>
    public string GetText() => internalFileViewer.GetText();

    /// <summary>
    /// Present the text as a patch in the file viewer.
    /// </summary>
    /// <param name="item">The gitItem to present.</param>
    /// <param name="text">The patch text.</param>
    /// <param name="line">The line number to display.</param>
    /// <param name="openWithDifftool">The action to open the difftool.</param>
    public async Task ViewPatchAsync(
        FileStatusItem item,
        string text,
        int? line,
        Action? openWithDifftool,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InvokeOnOwnerMainThreadAsync(() =>
        {
            ViewPatchCore(
                text,
                PatchUseGitColoring,
                isCombinedDiff: false,
                isGitWordDiff: AppSettings.DiffDisplayAppearance.Value == DiffDisplayAppearance.GitWordDiff,
                item.Item.Name,
                item,
                openWithDifftool);
            if (line is not null)
            {
                GoToLine(line.Value);
            }
        }, cancellationToken);
    }

    public async Task ViewCombinedDiffAsync(
        FileStatusItem item,
        string text,
        int? line,
        Action? openWithDifftool,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InvokeOnOwnerMainThreadAsync(() =>
        {
            ViewPatchCore(text, AppSettings.UseGitColoring.Value, isCombinedDiff: true, isGitWordDiff: false, item.Item.Name, item, openWithDifftool);
            if (line is not null)
            {
                GoToLine(line.Value);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Present the text as a patch in the file viewer.
    /// </summary>
    /// <param name="fileName">The fileName to present.</param>
    /// <param name="text">The patch text.</param>
    /// <param name="openWithDifftool">The action to open the difftool.</param>
    public async Task ViewFixedPatchAsync(
        string fileName,
        string text,
        Action? openWithDifftool = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InvokeOnOwnerMainThreadAsync(
            () => ViewFixedPatch(fileName, text, openWithDifftool, cancellationToken),
            cancellationToken);
    }

    /// <summary>
    ///  Shows a complete patch whose context and whitespace cannot be regenerated.
    /// </summary>
    public void ViewFixedPatch(
        string? fileName,
        string text,
        Action? openWithDifftool = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CancelPendingView();
        ResetView(ViewMode.FixedDiff, fileName, item: null, openWithDifftool);
        string parsedText = text;
        PatchHighlightService highlightService = new(ref parsedText, text.Contains('\u001b'), isGitWordDiff: false);
        SetDiffText(parsedText, highlightService, showLeftColumn: true);
        internalFileViewer.GoToFirstChange(NumberOfContextLines);
        TextLoaded?.Invoke(this, EventArgs.Empty);
    }

    public Task ViewDifftasticAsync(
        string fileName,
        string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CancelPendingView();
        ResetView(ViewMode.Difftastic, fileName);
        string parsedText = text;
        DifftasticHighlightService highlightService = new(
            ref parsedText,
            internalFileViewer.LineNumbersControl,
            out int rightColumnStart);
        VRulerPosition = rightColumnStart;
        SetDiffText(parsedText, highlightService, showLeftColumn: true);
        internalFileViewer.GoToFirstChange(NumberOfContextLines);
        TextLoaded?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task ViewRangeDiffAsync(
        string fileName,
        string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CancelPendingView();
        ResetView(ViewMode.RangeDiff, fileName);
        string parsedText = text;
        RangeDiffHighlightService highlightService = new(ref parsedText);
        SetDiffText(parsedText, highlightService, showLeftColumn: false);
        internalFileViewer.GoToFirstChange(NumberOfContextLines);
        TextLoaded?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task ViewGrepAsync(
        FileStatusItem item,
        string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CancelPendingView();
        ResetView(ViewMode.Grep, item.Item.Name, item);
        string parsedText = text;
        GrepHighlightService highlightService = new(ref parsedText, internalFileViewer.LineNumbersControl);
        SetDiffText(parsedText, highlightService, showLeftColumn: false);
        TextLoaded?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    /// <summary>
    ///  Shows plain text synchronously for WinForms-shaped callers.
    /// </summary>
    public void ViewText(string? fileName, string text, Action? openWithDifftool = null)
    {
        ThreadHelper.JoinableTaskFactory.Run(
            () => ViewTextAsync(
                fileName,
                text,
                item: null,
                line: null,
                openWithDifftool,
                cancellationToken: CancellationToken.None));
    }

    /// <summary>
    /// Present the text in the file viewer.
    /// </summary>
    /// <param name="fileName">The fileName to present.</param>
    /// <param name="text">The patch text.</param>
    /// <param name="line">The line to display.</param>
    /// <param name="openWithDifftool">The action to open the difftool.</param>
    /// <param name="checkGitAttributes">Check Git attributes to check for binary files.</param>
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

    /// <summary>
    /// View the git item with the TreeId.
    /// </summary>
    /// <param name="file">GitItem file, with TreeId.</param>
    /// <param name="objectId">Revision to present. Can be the zero <see cref="ObjectId"/> if file.TreeId is set.</param>
    /// <param name="item">Metadata for line patching and presentation.</param>
    /// <param name="line">The line to display.</param>
    /// <param name="openWithDifftool">difftool command</param>
    /// <returns>Task to view the item</returns>
    private async Task ViewGitItemAsync(
        GitItemStatus file,
        ObjectId objectId,
        FileStatusItem? item,
        int? line,
        Action? openWithDifftool,
        CancellationToken cancellationToken)
    {
        CancellationToken viewToken = BeginView(cancellationToken);

        // set fields possibly not set from git-diff (etc); treeGuid and IsSubmodule.
        // (git-status does not report submodule, IsSubmodule is not set if not TreeId is)
        // treeId (blobId) is only recalculated if required.
        await LoadWithErrorHandlingAsync(
            () => ViewGitItemCoreAsync(file, objectId, item, line, openWithDifftool, viewToken),
            viewToken);
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
    ///  Clears the viewer.
    /// </summary>
    public Task ClearAsync() => ViewTextAsync(string.Empty, string.Empty, cancellationToken: CancellationToken.None);

    public void Clear()
    {
        ThreadHelper.JoinableTaskFactory.Run(ClearAsync);
    }

    /// <summary>
    /// If the file viewer contents support line patches.
    /// </summary>
    public bool SupportLinePatching { get; private set; }

    /// <summary>
    /// Configuration to require that the form using the viewer reloads contents before allowing next line patch
    /// by clearing <see cref="AllowLinePatching" />
    /// Used for index/worktree where line patches modifies the diff.
    /// </summary>
    public bool LinePatchingBlocksUntilReload { private get; set; }

    /// <summary>
    /// Current state for line patching allowed for worktree/index
    /// Cleared when the file is reloaded.
    /// </summary>
    private bool AllowLinePatching
    {
        get => _allowLinePatching;
        set => _allowLinePatching = value;
    }

    /// <summary>Configures cross-file search navigation.</summary>
    public void SetFileLoader(GetNextFileFnc fileLoader)
    {
        internalFileViewer.SetFileLoader(fileLoader);
    }

    /// <summary>Applies every displayed change from a revision or stash to the worktree/index.</summary>
    public void CherryPickAllChanges()
    {
        if (SupportLinePatching)
        {
            ApplySelectedLines(allFile: true, reverse: false);
        }
    }

    private StagedStatus ViewItemStagedStatus()
    {
        StagedStatus stagedStatus = _viewItem?.Item.Staged ?? StagedStatus.Unknown;
        if (stagedStatus == StagedStatus.Unknown)
        {
            stagedStatus = GitModule.GetStagedStatus(
                _viewItem?.FirstRevision?.ObjectId ?? default,
                _viewItem?.SecondRevision.ObjectId ?? default,
                _viewItem?.SecondRevision.FirstParentId ?? default);
            if (_viewItem?.Item is not null)
            {
                _viewItem.Item.Staged = stagedStatus;
            }
        }

        return stagedStatus;
    }

    private void SetVisibilityDiffContextMenu(ViewMode viewMode)
    {
        bool isPartialTextView = viewMode.IsPartialTextView();

        // stage and reset has different implementation depending on the viewItem
        // For the user it looks the same and they expect the same menu item (and hotkey)
        bool isIndex = ViewItemStagedStatus() == StagedStatus.Index;
        stageSelectedLinesToolStripMenuItem.IsVisible = SupportLinePatching && !isIndex;
        unstageSelectedLinesToolStripMenuItem.IsVisible = SupportLinePatching && isIndex;
        resetSelectedLinesToolStripMenuItem.IsVisible = SupportLinePatching;

        // RangeDiff patch is undefined, could be new/old commit or to parents
        bool canCopyVersions = viewMode.IsNormalDiffView()
                               && AppSettings.DiffDisplayAppearance.Value == DiffDisplayAppearance.Patch;
        copyPatchToolStripMenuItem.IsVisible = canCopyVersions;
        copyNewVersionToolStripMenuItem.IsVisible = canCopyVersions;
        copyOldVersionToolStripMenuItem.IsVisible = canCopyVersions;

        bool diffCanBeModified = viewMode.IsDiffView()
                                 && viewMode is not (ViewMode.FixedDiff or ViewMode.Difftastic);
        ignoreWhitespaceAtEolToolStripMenuItem.IsVisible = diffCanBeModified || viewMode == ViewMode.Difftastic;
        ignoreWhitespaceChangesToolStripMenuItem.IsVisible = diffCanBeModified;
        ignoreAllWhitespaceChangesToolStripMenuItem.IsVisible = diffCanBeModified;

        bool isPartialFlexibleView = isPartialTextView && viewMode != ViewMode.FixedDiff;
        increaseNumberOfLinesToolStripMenuItem.IsVisible = isPartialFlexibleView;
        decreaseNumberOfLinesToolStripMenuItem.IsVisible = isPartialFlexibleView;
        showEntireFileToolStripMenuItem.IsVisible = isPartialFlexibleView;
        showSyntaxHighlightingToolStripMenuItem.IsVisible = isPartialFlexibleView;

        bool isDiffAppearanceVisible = viewMode is ViewMode.Diff or ViewMode.Difftastic;
        diffAppearanceToolStripMenuItem.IsVisible = isDiffAppearanceVisible;
        showGitWordColoringToolStripMenuItem.IsEnabled = isDiffAppearanceVisible;
        SetDifftasticEnabled();

        toolStripSeparator2.IsVisible = isPartialTextView;
        treatAllFilesAsTextToolStripMenuItem.IsVisible = isPartialTextView;

        nextChangeButton.IsVisible = isPartialTextView;
        previousChangeButton.IsVisible = isPartialTextView;
        increaseNumberOfLines.IsVisible = isPartialFlexibleView;
        decreaseNumberOfLines.IsVisible = isPartialFlexibleView;
        toolStripSeparator4.IsVisible = isPartialFlexibleView;
        showEntireFileButton.IsVisible = isPartialFlexibleView;
        ignoreWhitespaceAtEol.IsVisible = diffCanBeModified || viewMode == ViewMode.Difftastic;
        ignoreWhiteSpaces.IsVisible = diffCanBeModified;
        ignoreAllWhitespaces.IsVisible = diffCanBeModified;
        showSyntaxHighlighting.IsVisible = isPartialTextView;

        return;

        void SetDifftasticEnabled()
        {
            if (!isDiffAppearanceVisible || !TryGetUICommandsDirect(out _))
            {
                showDifftasticToolStripMenuItem.IsEnabled = false;
                return;
            }

            if (IsDifftasticEnabled.IsValueCreated)
            {
                showDifftasticToolStripMenuItem.IsEnabled = IsDifftasticEnabled.Value;
                return;
            }

            ThreadHelper.FileAndForget(async () =>
            {
                bool enabled = await Task.Run(() => IsDifftasticEnabled.Value);
                await this.SwitchToMainThreadAsync();
                showDifftasticToolStripMenuItem.IsEnabled = enabled;
            });
        }
    }

    private void OnExtraDiffArgumentsChanged()
    {
        ExtraDiffArgumentsChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task ShowOrDeferAsync(long contentLength, Func<Task> showFunc, CancellationToken cancellationToken)
    {
        if (contentLength > MaximumAutomaticPreviewLength)
        {
            await InvokeOnOwnerMainThreadAsync(() =>
            {
                ResetView(ViewMode.Text, fileName: null);
                SetText(string.Empty);
                _NO_TRANSLATE_lblShowPreview.Content = string.Format(
                    _largeFileSizeWarning.Text,
                    contentLength / (1024d * 1024));
                _NO_TRANSLATE_lblShowPreview.IsVisible = true;
                _deferShowFunc = showFunc;
            }, cancellationToken);
            return;
        }

        await InvokeOnOwnerMainThreadAsync(() =>
        {
            _NO_TRANSLATE_lblShowPreview.IsVisible = false;
            _deferShowFunc = null;
        }, cancellationToken);
        await showFunc();
    }

    private void OnIgnoreWhitespaceChanged()
    {
        UpdateDiffOptionState();
    }

    private void ResetView(
        ViewMode viewMode,
        string? fileName,
        FileStatusItem? item = null,
        Action? openWithDifftool = null,
        string? text = null)
    {
        _viewMode = viewMode;
        _fileName = fileName;
        TextEditor.Tag = fileName;
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

        bool hasModule = TryGetUICommandsDirect(out _);
        string? fullPath = hasModule ? _fullPathResolver.Resolve(fileName) : null;
        SupportLinePatching =
            ((_viewMode.IsNormalDiffView()
                    && (text?.Contains("@@", StringComparison.Ordinal) ?? false)
                    && AppSettings.DiffDisplayAppearance.Value != DiffDisplayAppearance.GitWordDiff
                    && File.Exists(fullPath))
                || ((item?.Item.IsNew ?? false)
                    && (item.Item.Staged is StagedStatus.WorkTree or StagedStatus.Index
                        || !File.Exists(fullPath))))
            && hasModule
            && !Module.IsBareRepository();
        AllowLinePatching = SupportLinePatching;

        ClearImage();
        PictureBox.IsVisible = _viewMode == ViewMode.Image;
        TextEditor.IsVisible = _viewMode != ViewMode.Image;
        ClearDiffHighlighting();
        SetVisibilityDiffContextMenu(_viewMode);
        ApplySyntaxHighlighting();
    }

    private static string ToHexDump(string text, StringBuilder str, int columnWidth = 8, int columnCount = 2)
    {
        if (text.Length == 0)
        {
            return string.Empty;
        }

        // Do not freeze GE when selecting large binary files
        // Show only the header of the binary file to indicate contents and files incorrectly handled
        // Use a dedicated editor to view the complete file
        int limit = Math.Min(text.Length, columnWidth * columnCount * 256);
        int i = 0;
        while (i < limit)
        {
            int baseIndex = i;
            if (i != 0)
            {
                str.AppendLine();
            }

            // OFFSET
            str.Append($"{baseIndex:X4}   ");

            // BYTES
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                // space between columns
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

            // ASCII
            i = baseIndex;
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                // space between columns
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

    private void SetStateOfContextLinesButtons()
    {
        increaseNumberOfLinesToolStripMenuItem.IsEnabled = !ShowEntireFile;
        decreaseNumberOfLinesToolStripMenuItem.IsEnabled = !ShowEntireFile;
        increaseNumberOfLines.IsEnabled = !ShowEntireFile;
        decreaseNumberOfLines.IsEnabled = !ShowEntireFile;
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

    // Event handlers
    private void OnUICommandsChanged(object? sender, GitUICommandsChangedEventArgs? e)
    {
        BindSettingsCommands((sender as IGitUICommandsSource)?.UICommands);
        ReloadHotkeys();
        Encoding = null;
    }

    private void UICommands_PostSettings(object? sender, GitUIPostActionEventArgs? e)
    {
        Dispatcher.UIThread.Post(() => VRulerPosition = AppSettings.DiffVerticalRulerPosition);
    }

    private void IgnoreWhitespaceAtEolToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        IgnoreWhitespace = IgnoreWhitespace == IgnoreWhitespaceKind.Eol
            ? IgnoreWhitespaceKind.None
            : IgnoreWhitespaceKind.Eol;
        OnIgnoreWhitespaceChanged();
        OnExtraDiffArgumentsChanged();
    }

    private void IgnoreWhitespaceChangesToolStripMenuItemClick(object? sender, EventArgs e)
    {
        IgnoreWhitespace = IgnoreWhitespace == IgnoreWhitespaceKind.Change
            ? IgnoreWhitespaceKind.None
            : IgnoreWhitespaceKind.Change;
        OnIgnoreWhitespaceChanged();
        OnExtraDiffArgumentsChanged();
    }

    private void IncreaseNumberOfLinesToolStripMenuItemClick(object? sender, EventArgs e)
    {
        NumberOfContextLines++;
        AppSettings.NumberOfContextLines = NumberOfContextLines;
        OnExtraDiffArgumentsChanged();
    }

    private void DecreaseNumberOfLinesToolStripMenuItemClick(object? sender, EventArgs e)
    {
        NumberOfContextLines = Math.Max(0, NumberOfContextLines - 1);
        AppSettings.NumberOfContextLines = NumberOfContextLines;
        OnExtraDiffArgumentsChanged();
    }

    private void ShowSyntaxHighlighting_Click(object? sender, EventArgs e)
    {
        ShowSyntaxHighlightingInDiff = !ShowSyntaxHighlightingInDiff;
        UpdateSyntaxHighlightingToggleState();
        AppSettings.ShowSyntaxHighlightingInDiff.Value = ShowSyntaxHighlightingInDiff;
        ApplySyntaxHighlighting();
        OnExtraDiffArgumentsChanged();
    }

    private void ShowEntireFileToolStripMenuItemClick(object? sender, EventArgs e)
    {
        ShowEntireFile = !ShowEntireFile;
        AppSettings.ShowEntireFile.Value = ShowEntireFile;
        UpdateDiffOptionState();
        OnExtraDiffArgumentsChanged();
    }

    private void ResetPatchAppearanceToolStripMenuItemClick(object? sender, EventArgs e)
    {
        // The other settings toggle, this just resets the appearance
        AppSettings.DiffDisplayAppearance.Value = DiffDisplayAppearance.Patch;
        UpdateDiffOptionState();
        OnExtraDiffArgumentsChanged();
    }

    private void ToggleGitWordColoringToolStripMenuItemClick(object? sender, EventArgs e)
    {
        AppSettings.DiffDisplayAppearance.Value = AppSettings.DiffDisplayAppearance.Value == DiffDisplayAppearance.GitWordDiff
            ? DiffDisplayAppearance.Patch
            : DiffDisplayAppearance.GitWordDiff;
        UpdateDiffOptionState();
        OnExtraDiffArgumentsChanged();
    }

    private void ToggleDifftasticToolStripMenuItemClick(object? sender, EventArgs e)
    {
        AppSettings.DiffDisplayAppearance.Value = AppSettings.DiffDisplayAppearance.Value == DiffDisplayAppearance.Difftastic
            ? DiffDisplayAppearance.Patch
            : DiffDisplayAppearance.Difftastic;
        UpdateDiffOptionState();
        OnExtraDiffArgumentsChanged();
    }

    private void _continuousScrollEventManager_BottomScrollReached(object? sender, EventArgs e)
        => BottomScrollReached?.Invoke(sender, e);

    private void _continuousScrollEventManager_TopScrollReached(object? sender, EventArgs e)
        => TopScrollReached?.Invoke(sender, e);

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

    private void PictureBox_MouseWheel(object? sender, PointerWheelEventArgs e)
        => e.Handled = RaiseContinuousScroll(e.Delta.Y, e.KeyModifiers);

    private void OnUICommandsSourceSet(object? sender, GitUICommandsSourceEventArgs e)
    {
        if (_commandsSource is not null)
        {
            _commandsSource.UICommandsChanged -= OnUICommandsChanged;
        }

        _commandsSource = e.GitUICommandsSource;
        _commandsSource.UICommandsChanged += OnUICommandsChanged;
        OnUICommandsChanged(_commandsSource, null);
    }

    private void TreatAllFilesAsTextToolStripMenuItemClick(object? sender, EventArgs e)
    {
        TreatAllFilesAsText = !TreatAllFilesAsText;
        treatAllFilesAsTextToolStripMenuItem.IsChecked = TreatAllFilesAsText;
        OnExtraDiffArgumentsChanged();
    }

    private void settingsButton_Click(object? sender, EventArgs e)
    {
        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            commands.StartSettingsDialog(GetOwner(), DiffViewerSettingsPage.GetPageReference());
        }
    }

    private void IgnoreAllWhitespaceChangesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        IgnoreWhitespace = IgnoreWhitespace == IgnoreWhitespaceKind.AllSpace
            ? IgnoreWhitespaceKind.None
            : IgnoreWhitespaceKind.AllSpace;
        OnIgnoreWhitespaceChanged();
        OnExtraDiffArgumentsChanged();
    }

    private void stageSelectedLinesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        StageSelectedLines();
    }

    private void unstageSelectedLinesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        UnstageSelectedLines();
    }

    private void resetSelectedLinesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ResetSelectedLines();
    }

    /// <summary>
    /// Update the current blob id for the GitItemStatus.
    /// TreeId is immutable for normal commits, must always be updated before use for Index.
    /// TreeId is irrelevant for worktree (if dirty), but this sets IsSubmodule
    /// (treeId is not updated if set already).
    /// TODO: add to GitModule, similar to GetFileBlobHash
    /// </summary>
    /// <param fileName="file">The GitStatusItem to update.</param>
    /// <param fileName="commitId">The commit to use..</param>
    /// <param fileName="cancellationToken">The cancellation token.</param>
    /// <returns>the current TreeId (normally blob id, could be commit id) to be used.
    /// For worktree null is always returned also if there is a treeid (that really applies to the index)</returns>
    public ObjectId GetUpdateTreeId(
        GitItemStatus file,
        ObjectId commitId,
        CancellationToken cancellationToken = default)
    {
        if (!file.TreeId.IsZero && !commitId.IsArtificial)
        {
            // current value is immutable (and IsSubmodule should have been set)
            return file.TreeId;
        }

        if (commitId == ObjectId.WorkTreeId && (!file.TreeId.IsZero || file.IsSubmodule))
        {
            // treeId already calculated, no point in doing it again.
            // (if treeId is set, it means that IsSubmodule is set).
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
    /// Use implementation matching the current viewItem.
    /// </summary>
    private bool StageSelectedLines()
    {
        if (!SupportLinePatching)
        {
            // Hotkey executed when menu is disabled
            return false;
        }

        if (ViewItemStagedStatus() == StagedStatus.WorkTree)
        {
            StageSelectedLines(stage: true);
        }
        else
        {
            ApplySelectedLines(allFile: false, reverse: false);
        }

        return true;
    }

    private bool UnstageSelectedLines()
    {
        if (!SupportLinePatching || ViewItemStagedStatus() != StagedStatus.Index)
        {
            // Hotkey executed when menu is disabled
            return false;
        }

        StageSelectedLines(stage: false);
        return true;
    }

    private bool ResetSelectedLines()
    {
        if (!SupportLinePatching)
        {
            // Hotkey executed when menu is disabled
            return false;
        }

        if (ViewItemStagedStatus() is StagedStatus.WorkTree or StagedStatus.Index)
        {
            ResetNoncommittedSelectedLines();
        }
        else
        {
            ApplySelectedLines(allFile: false, reverse: true);
        }

        return true;
    }

    /// <summary>Stages worktree lines or unstages index lines.</summary>
    public void StageSelectedLines(bool stage)
    {
        if (!AllowLinePatching || _viewItem is null)
        {
            // reload not completed
            return;
        }

        byte[]? patch;
        if (_viewItem.Item.IsNew)
        {
            byte[] filePreamble = FilePreamble ?? throw new InvalidOperationException("The new file preamble was not loaded.");
            ObjectId itemBlobId = GetUpdateTreeId(_viewItem.Item, _viewItem.SecondRevision.ObjectId);
            patch = PatchManager.GetSelectedLinesAsNewPatch(
                Module,
                _viewItem.Item.Name,
                GetText(),
                GetSelectionPosition(),
                GetSelectionLength(),
                Encoding,
                reset: false,
                filePreamble,
                itemBlobId.ToString());
        }
        else
        {
            patch = PatchManager.GetSelectedLinesAsPatch(
                GetText(),
                GetSelectionPosition(),
                GetSelectionLength(),
                isIndex: !stage,
                Encoding,
                reset: false,
                _viewItem.Item.IsNew,
                _viewItem.Item.IsRenamed);
        }

        if (patch?.Length is not > 0)
        {
            return;
        }

        GitArgumentBuilder args = new("apply")
        {
            "--cached",
            "--index",
            "--whitespace=nowarn",
            { !stage, "--reverse" },
        };
        ProcessApplyOutput(args, patch, patchUpdateDiff: true);
    }

    /// <summary>Resets selected worktree or index lines after confirmation.</summary>
    public void ResetNoncommittedSelectedLines()
    {
        if (!AllowLinePatching || _viewItem is null
            || TaskDialog.ShowDialog(GetOwner(), _NO_TRANSLATE_resetSelectedLinesConfirmationDialog) != TaskDialogButton.Yes)
        {
            // reload not completed
            return;
        }

        byte[]? patch;
        bool currentItemStaged = _viewItem.SecondRevision.ObjectId == ObjectId.IndexId;
        if (_viewItem.Item.IsNew)
        {
            byte[] filePreamble = FilePreamble ?? throw new InvalidOperationException("The new file preamble was not loaded.");
            ObjectId itemBlobId = GetUpdateTreeId(_viewItem.Item, _viewItem.SecondRevision.ObjectId);
            patch = PatchManager.GetSelectedLinesAsNewPatch(
                Module,
                _viewItem.Item.Name,
                GetText(),
                GetSelectionPosition(),
                GetSelectionLength(),
                Encoding,
                reset: true,
                filePreamble,
                itemBlobId.ToString());
        }
        else if (currentItemStaged)
        {
            patch = PatchManager.GetSelectedLinesAsPatch(
                GetText(),
                GetSelectionPosition(),
                GetSelectionLength(),
                isIndex: true,
                Encoding,
                reset: true,
                _viewItem.Item.IsNew,
                _viewItem.Item.IsRenamed);
        }
        else
        {
            patch = PatchManager.GetResetWorkTreeLinesAsPatch(
                GetText(),
                GetSelectionPosition(),
                GetSelectionLength(),
                Encoding);
        }

        if (patch?.Length is not > 0)
        {
            return;
        }

        GitArgumentBuilder args = new("apply")
        {
            "--whitespace=nowarn",
            { currentItemStaged, "--reverse --index" },
        };
        ProcessApplyOutput(args, patch, patchUpdateDiff: true);
    }

    /// <summary>
    /// Cherry-pick/revert patches (not worktree).
    /// </summary>
    /// <param name="reverse"><see langword="true"/> if patches is to be reversed; otherwise <see langword="false"/>.</param>.
    private void ApplySelectedLines(bool allFile, bool reverse)
    {
        if (!AllowLinePatching || _viewItem is null)
        {
            // reload not completed
            return;
        }

        int selectionStart = allFile ? 0 : GetSelectionPosition();
        int selectionLength = allFile ? GetText().Length : GetSelectionLength();
        if (selectionLength == 0)
        {
            return;
        }

        byte[]? patch;
        if (_viewItem.Item.IsNew)
        {
            byte[] filePreamble = FilePreamble ?? throw new InvalidOperationException("The new file preamble was not loaded.");
            ObjectId itemBlobId = reverse
                ? GetUpdateTreeId(_viewItem.Item, _viewItem.SecondRevision.ObjectId)
                : default;
            patch = PatchManager.GetSelectedLinesAsNewPatch(
                Module,
                _viewItem.Item.Name,
                GetText(),
                selectionStart,
                selectionLength,
                Encoding,
                reset: reverse,
                filePreamble,
                itemBlobId.ToString());
        }
        else if (!reverse)
        {
            patch = PatchManager.GetSelectedLinesAsPatch(
                GetText(),
                selectionStart,
                selectionLength,
                isIndex: false,
                Encoding,
                reset: false,
                _viewItem.Item.IsNew,
                _viewItem.Item.IsRenamed);
        }
        else
        {
            patch = PatchManager.GetResetWorkTreeLinesAsPatch(
                GetText(),
                selectionStart,
                selectionLength,
                Encoding);
        }

        if (patch?.Length is not > 0)
        {
            return;
        }

        GitArgumentBuilder args = new("apply")
        {
            "--3way",
            "--index",
            "--whitespace=nowarn",
        };
        ProcessApplyOutput(args, patch);
    }

    private void ProcessApplyOutput(GitArgumentBuilder args, byte[] patch, bool patchUpdateDiff = false)
    {
        // TODO Cleanup the handling and separate AllOutput to StandardOutput/StandardError
        ExecutionResult result = Module.GitExecutable.Execute(
            args,
            inputWriter => inputWriter.BaseStream.Write(patch),
            throwOnErrorExit: false,
            cancellationToken: CancellationToken.None);
        string output = result.AllOutput.Trim();
        if (!result.ExitedSuccessfully
            && (patchUpdateDiff || !MergeConflictHandler.HandleMergeConflicts(UICommands, GetOwner(), false, false)))
        {
            MessageBoxes.Show(
                GetOwner(),
                $"{output}{Environment.NewLine}{Environment.NewLine}{Encoding.GetString(patch)}",
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
        }
        else if (!result.ExitedSuccessfully
                 || output.StartsWith("error: ", StringComparison.Ordinal)
                 || output.StartsWith("warning: ", StringComparison.Ordinal))
        {
            // git-apply may fail on first attempt but succeed in subsequent attempts
            // Trace such occurrences that may be interesting, some of these should maybe be presented to the user
            System.Diagnostics.Trace.WriteLineIf(
                !string.IsNullOrWhiteSpace(output),
                $"Patch output: {result.ExitCode}:{output} for: git {args}");
        }

        if (patchUpdateDiff && LinePatchingBlocksUntilReload)
        {
            AllowLinePatching = false;
        }

        PatchApplied?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Copy selected text, excluding diff added/deleted information.
    /// </summary>
    /// <param name="sender">sender object.</param>
    /// <param name="e">event args.</param>
    private void CopyToolStripMenuItemClick(object? sender, EventArgs e)
    {
        string code = GetSelectedText();
        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        if (_viewMode.IsDiffView() && _viewMode != ViewMode.Difftastic)
        {
            int position = GetSelectionPosition();
            string fileText = GetText();
            int hunkPosition = fileText.IndexOf("\n@@", StringComparison.Ordinal);
            if (hunkPosition <= position)
            {
                // if header is selected then don't remove diff extra chars
                // for range-diff, copy all info (hpos will never match)
                // add artificial space if selected text is not starting from line beginning, it will be removed later
                if (position > 0 && fileText[position - 1] != '\n')
                {
                    code = " " + code;
                }

                code = string.Join("\n", code.LazySplit('\n').Select(RemovePrefix));
            }
        }

        ClipboardUtil.TrySetText(code.AdjustLineEndings(Module.GetEffectiveSetting<AutoCRLFType>("core.autocrlf")));

        return;

        string RemovePrefix(string line)
        {
            string[] specials = internalFileViewer.GetFullDiffPrefixes();
            foreach (string special in specials.Where(line.StartsWith))
            {
                return line[special.Length..];
            }

            return line;
        }
    }

    /// <summary>
    /// Copy selected text as a patch.
    /// </summary>
    /// <param name="sender">sender object.</param>
    /// <param name="e">event args.</param>
    private void CopyPatchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        string text = GetSelectedText();
        if (string.IsNullOrEmpty(text))
        {
            text = GetText();
        }

        if (!string.IsNullOrEmpty(text))
        {
            ClipboardUtil.TrySetText(text);
        }
    }

    private void copyNewVersionToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        internalFileViewer.CopyNotStartingWith('-');
    }

    private void copyOldVersionToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        internalFileViewer.CopyNotStartingWith('+');
    }

    /// <summary>
    /// Go to next change
    /// For normal diffs, this is the next block of lines with a difference.
    /// For range-diff, it is the next commit summary header.
    /// </summary>
    private void NextChangeButtonClick(object? sender, EventArgs e)
    {
        FocusViewer();
        internalFileViewer.GoToNextChange(NumberOfContextLines);
    }

    private void PreviousChangeButtonClick(object? sender, EventArgs e)
    {
        FocusViewer();
        internalFileViewer.GoToPreviousChange(NumberOfContextLines);
    }

    private void ContinuousScrollToolStripMenuItemClick(object? sender, EventArgs e)
    {
        AppSettings.AutomaticContinuousScroll = !AppSettings.AutomaticContinuousScroll;
        automaticContinuousScrollToolStripMenuItem.IsChecked = AppSettings.AutomaticContinuousScroll;
    }

    private void ShowNonprintableCharactersToolStripMenuItemClick(object? sender, EventArgs e)
    {
        ToggleNonPrintingChars(!_showNonPrintingChars);
        AppSettings.ShowNonPrintingChars.Value = _showNonPrintingChars;
    }

    private void FindToolStripMenuItemClick(object? sender, EventArgs e)
    {
        Find(sender == replaceToolStripMenuItem);
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
        OnExtraDiffArgumentsChanged();
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

    protected override bool ExecuteCommand(int command)
    {
        switch ((Command)command)
        {
            case Command.Find:
                Find(replace: false);
                break;
            case Command.Replace:
                if (TextEditor.IsReadOnly)
                {
                    // Don't handle the hotkey to let the control handle it if an action is bound to it
                    return false;
                }

                Find(replace: true);
                break;
            case Command.FindNextOrOpenWithDifftool:
                if (_openWithDifftool is not null)
                {
                    _openWithDifftool();
                }
                else
                {
                    this.InvokeAndForget(() => FindNextAsync(searchForwardOrOpenWithDifftool: true));
                }

                break;
            case Command.FindPrevious:
                this.InvokeAndForget(() => FindNextAsync(searchForwardOrOpenWithDifftool: false));
                break;
            case Command.GoToLine:
                goToLineToolStripMenuItem_Click(this, EventArgs.Empty);
                break;
            case Command.IncreaseNumberOfVisibleLines:
                if (!increaseNumberOfLines.IsVisible || !increaseNumberOfLines.IsEnabled)
                {
                    return false;
                }

                IncreaseNumberOfLinesToolStripMenuItemClick(this, EventArgs.Empty);
                break;
            case Command.DecreaseNumberOfVisibleLines:
                if (!decreaseNumberOfLines.IsVisible || !decreaseNumberOfLines.IsEnabled)
                {
                    return false;
                }

                DecreaseNumberOfLinesToolStripMenuItemClick(this, EventArgs.Empty);
                break;
            case Command.ShowEntireFile:
                if (!showEntireFileButton.IsVisible)
                {
                    return false;
                }

                ShowEntireFileToolStripMenuItemClick(this, EventArgs.Empty);
                break;
            case Command.ShowSyntaxHighlighting:
                if (!showSyntaxHighlightingToolStripMenuItem.IsVisible)
                {
                    return false;
                }

                ShowSyntaxHighlighting_Click(this, EventArgs.Empty);
                break;
            case Command.ShowGitWordColoring:
                if (!showGitWordColoringToolStripMenuItem.IsVisible)
                {
                    return false;
                }

                ToggleGitWordColoringToolStripMenuItemClick(this, EventArgs.Empty);
                break;
            case Command.ShowDifftastic:
                if (!showDifftasticToolStripMenuItem.IsVisible || !showDifftasticToolStripMenuItem.IsEnabled)
                {
                    return false;
                }

                ToggleDifftasticToolStripMenuItemClick(this, EventArgs.Empty);
                break;
            case Command.TreatFileAsText:
                if (!treatAllFilesAsTextToolStripMenuItem.IsVisible)
                {
                    return false;
                }

                TreatAllFilesAsTextToolStripMenuItemClick(this, EventArgs.Empty);
                break;
            case Command.NextChange:
                internalFileViewer.GoToNextChange(NumberOfContextLines);
                break;
            case Command.PreviousChange:
                internalFileViewer.GoToPreviousChange(NumberOfContextLines);
                break;
            case Command.NextOccurrence:
                GoToNextOccurrence();
                break;
            case Command.PreviousOccurrence:
                GoToPreviousOccurrence();
                break;
            case Command.StageLines:
                return StageSelectedLines();
            case Command.UnstageLines:
                return UnstageSelectedLines();
            case Command.ResetLines:
                return ResetSelectedLines();
            case Command.IgnoreAllWhitespace:
                if (!ignoreAllWhitespaces.IsVisible)
                {
                    return false;
                }

                IgnoreAllWhitespaceChangesToolStripMenuItem_Click(this, EventArgs.Empty);
                break;
            default:
                return base.ExecuteCommand(command);
        }

        return true;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FileViewer _control;

        public TestAccessor(FileViewer control)
        {
            _control = control;
        }

        public bool IsFindAndReplaceFormCreated => _control.internalFileViewer.GetTestAccessor().IsFindAndReplaceFormCreated;

        public FindAndReplaceForm FindAndReplaceForm => _control.internalFileViewer.GetTestAccessor().FindAndReplaceForm;

        public ComboBox EncodingToolStripComboBox => _control.encodingToolStripComboBox;

        public Border FileViewerToolbar => _control.fileviewerToolbar;

        public Separator ContextLinesSeparator => _control.toolStripSeparator4;

        public MenuItem ShowNonprintingCharactersMenuItem => _control.showNonprintableCharactersToolStripMenuItem;

        public Button ShowNonprintingCharactersButton => _control.showNonPrintChars;

        public MenuItem ShowSyntaxHighlightingMenuItem => _control.showSyntaxHighlightingToolStripMenuItem;

        public Button ShowSyntaxHighlightingButton => _control.showSyntaxHighlighting;

        public bool ShowSyntaxHighlightingInDiff => _control.ShowSyntaxHighlightingInDiff;

        public MenuItem DiffAppearanceMenuItem => _control.diffAppearanceToolStripMenuItem;

        public MenuItem ShowPatchMenuItem => _control.showPatchToolStripMenuItem;

        public MenuItem ShowGitWordColoringMenuItem => _control.showGitWordColoringToolStripMenuItem;

        public MenuItem ShowDifftasticMenuItem => _control.showDifftasticToolStripMenuItem;

        public MenuItem TreatAllFilesAsTextMenuItem => _control.treatAllFilesAsTextToolStripMenuItem;

        public MenuItem AutomaticContinuousScrollMenuItem => _control.automaticContinuousScrollToolStripMenuItem;

        public Button SettingsButton => _control.settingsButton;

        public Button NextChangeButton => _control.nextChangeButton;

        public MenuItem FindMenuItem => _control.findToolStripMenuItem;

        public int VRulerPosition => _control.VRulerPosition;

        public bool HasDiffHighlighting => _control.internalFileViewer.DiffHighlightService is not null;

        public HyperlinkButton ShowPreviewLink => _control._NO_TRANSLATE_lblShowPreview;

        public Image ImagePreview => _control.ImagePreview;

        public Border PictureBox => _control.PictureBox;

        public ViewMode ViewMode => _control._viewMode;

        public FileStatusItem? ViewItem => _control._viewItem;

        public Action? OpenWithDifftool => _control._openWithDifftool;

        public bool RaiseContinuousScroll(double delta, KeyModifiers keyModifiers)
            => _control.RaiseContinuousScroll(delta, keyModifiers);
    }

    internal ThemeAwareTextEditor TextEditor => internalFileViewer.Editor;

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
        FileStatusItem? item = null,
        Action? openWithDifftool = null)
    {
        ResetView(isCombinedDiff ? ViewMode.CombinedDiff : ViewMode.Diff, fileName, item, openWithDifftool, text);
        string parsedText = text ?? string.Empty;
        DiffHighlightService highlightService = isCombinedDiff
            ? new CombinedDiffHighlightService(ref parsedText, useGitColoring)
            : new PatchHighlightService(ref parsedText, useGitColoring, isGitWordDiff);
        SetDiffText(parsedText, highlightService, showLeftColumn: true);
        internalFileViewer.GoToFirstChange(NumberOfContextLines);
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
        internalFileViewer.GoToFirstChange(NumberOfContextLines);
        TextLoaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///  Shows plain text without diff coloring, like the WinForms text mode.
    /// </summary>
    public Task ViewTextAsync(string? fileName, string text, CancellationToken cancellationToken)
        => ViewTextAsync(fileName, text, item: null, line: null, openWithDifftool: null, checkGitAttributes: false, cancellationToken);

    private void SetText(string? text)
    {
        internalFileViewer.SetText(
            text ?? string.Empty,
            _openWithDifftool,
            _viewMode,
            useGitColoring: false,
            contentIdentification: _fileName);
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

        await InvokeOnOwnerMainThreadAsync(() =>
        {
            ResetView(ViewMode.Text, fileName, item, openWithDifftool, text);
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
                internalFileViewer.GoToFirstChange(NumberOfContextLines);
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
        }, cancellationToken);
    }

    private async Task ShowHexDumpAsync(
        string fileNameFormat,
        string fileName,
        string text,
        Action? openWithDifftool,
        CancellationToken cancellationToken)
    {
        await InvokeOnOwnerMainThreadAsync(() =>
        {
            ResetView(ViewMode.Text, fileName, item: null, openWithDifftool);
            DisplayAsHexDump(fileNameFormat, fileName, text);
            TextLoaded?.Invoke(this, EventArgs.Empty);
        }, cancellationToken);
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
            await InvokeOnOwnerMainThreadAsync(() =>
            {
                ResetView(ViewMode.Image, fileName, item, openWithDifftool);
                SetText(string.Empty);
                _image = imageToDispose;
                ImagePreview.Source = imageToDispose;
                imageToDispose = null;
            }, cancellationToken);
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

            await InvokeOnOwnerMainThreadAsync(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                ResetView(ViewMode.Text, fileName: null);
                SetText("Unsupported file: \n\n" + exception);
                TextLoaded?.Invoke(this, EventArgs.Empty);
            }, cancellationToken);
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

    private void ApplySyntaxHighlighting()
    {
        bool shouldHighlight = !string.IsNullOrEmpty(_fileName)
                               && (_viewMode == ViewMode.Text
                                   || (ShowSyntaxHighlightingInDiff && _viewMode.IsPartialTextView()));
        bool isGitMessage = shouldHighlight
            && (_fileName!.EndsWith("git-rebase-todo", StringComparison.Ordinal)
                || _fileName.EndsWith("COMMIT_EDITMSG", StringComparison.Ordinal));
        internalFileViewer.SetHighlightingForFile(isGitMessage ? _fileName : null, TryGetUICommandsDirect(out _) ? Module : null);
        string? extension = shouldHighlight && !isGitMessage ? Path.GetExtension(_fileName) : string.Empty;
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

    private void UpdateDiffOptionState()
    {
        showEntireFileToolStripMenuItem.IsChecked = ShowEntireFile;
        SetToolbarChecked(showEntireFileButton, ShowEntireFile);
        SetStateOfContextLinesButtons();

        bool ignoreEol = IgnoreWhitespace is IgnoreWhitespaceKind.Eol
            or IgnoreWhitespaceKind.Change
            or IgnoreWhitespaceKind.AllSpace;
        bool ignoreChanges = IgnoreWhitespace is IgnoreWhitespaceKind.Change
            or IgnoreWhitespaceKind.AllSpace;
        bool ignoreAll = IgnoreWhitespace == IgnoreWhitespaceKind.AllSpace;
        ignoreWhitespaceAtEolToolStripMenuItem.IsChecked = ignoreEol;
        ignoreWhitespaceChangesToolStripMenuItem.IsChecked = ignoreChanges;
        ignoreAllWhitespaceChangesToolStripMenuItem.IsChecked = ignoreAll;
        SetToolbarChecked(ignoreWhitespaceAtEol, ignoreEol);
        SetToolbarChecked(ignoreWhiteSpaces, ignoreChanges);
        SetToolbarChecked(ignoreAllWhitespaces, ignoreAll);
        showGitWordColoringToolStripMenuItem.IsChecked = AppSettings.DiffDisplayAppearance.Value == DiffDisplayAppearance.GitWordDiff;
        showDifftasticToolStripMenuItem.IsChecked = AppSettings.DiffDisplayAppearance.Value == DiffDisplayAppearance.Difftastic;
        showPatchToolStripMenuItem.IsChecked = showGitWordColoringToolStripMenuItem.IsChecked != true
                                              && showDifftasticToolStripMenuItem.IsChecked != true;
        treatAllFilesAsTextToolStripMenuItem.IsChecked = TreatAllFilesAsText;
        AppSettings.IgnoreWhitespaceKind.Value = IgnoreWhitespace;
    }

    private void UpdateLineNumberVisibility()
    {
        bool hasDiffLineNumbers = internalFileViewer.DiffHighlightService is not null;
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

    private void ClearImage()
    {
        ImagePreview.Source = null;
        _image?.Dispose();
        _image = null;
    }

    /// <summary>Shows the Find window for this editor.</summary>
    public void Find(bool replace)
    {
        internalFileViewer.Find(replace);
    }

    /// <summary>Finds the next or previous occurrence using the current Find settings.</summary>
    public Task FindNextAsync(bool searchForwardOrOpenWithDifftool)
    {
        return internalFileViewer.FindNextAsync(searchForwardOrOpenWithDifftool);
    }

    private void SetDiffText(string text, DiffHighlightService highlightService, bool showLeftColumn)
    {
        internalFileViewer.SetTextHighlightService(highlightService);
        _diffBackgroundRenderer.SetHighlightService(highlightService);
        _diffTextColorizer.SetHighlightService(highlightService);
        internalFileViewer.LineNumbersControl.DisplayLineNum(highlightService.LinesInfo, showLeftColumn);
        UpdateLineNumberVisibility();
        SetText(text);
    }

    private void ClearDiffHighlighting()
    {
        internalFileViewer.SetTextHighlightService(TextHighlightService.Instance);
        _diffBackgroundRenderer.SetHighlightService(null);
        _diffTextColorizer.SetHighlightService(null);
        internalFileViewer.LineNumbersControl.Clear();
        UpdateLineNumberVisibility();
    }

    public int MaxLineNumber
    {
        get
        {
            if (internalFileViewer.DiffHighlightService is null)
            {
                return TextEditor.Document?.LineCount ?? 1;
            }

            IEnumerable<int> mappedLines = internalFileViewer.DiffHighlightService.LinesInfo.DiffLines.Values
                .SelectMany(line => new[] { line.LeftLineNumber, line.RightLineNumber })
                .Where(line => line != DiffLineInfo.NotApplicableLineNum);
            return mappedLines.DefaultIfEmpty(1).Max();
        }
    }

    private int FindDocumentLine(int fileLine)
    {
        if (internalFileViewer.DiffHighlightService is null)
        {
            return fileLine;
        }

        DiffLineInfo? mapped = internalFileViewer.DiffHighlightService.LinesInfo.DiffLines.Values.FirstOrDefault(
            info => info.RightLineNumber == fileLine);
        mapped ??= internalFileViewer.DiffHighlightService.LinesInfo.DiffLines.Values.FirstOrDefault(
            info => info.LeftLineNumber == fileLine);
        return mapped?.LineNumInDiff ?? fileLine;
    }

    /// <summary>
    ///  Redraws the text view, like the WinForms control method.
    /// </summary>
    public void Refresh()
    {
        TextEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        internalFileViewer.LineNumbersControl.InvalidateVisual();
    }

    private void Caret_PositionChanged(object? sender, EventArgs e)
    {
        int line = TextEditor.TextArea.Caret.Line - 1;
        if (line == _lastCaretLine)
        {
            return;
        }

        _lastCaretLine = line;
        internalFileViewer.LineNumbersControl.InvalidateVisual();
        SelectedLineChanged?.Invoke(this, new SelectedLineEventArgs(CurrentFileLine - 1));
    }

    private void TextView_ScrollOffsetChanged(object? sender, EventArgs e)
    {
        HScrollPositionChanged?.Invoke(this, EventArgs.Empty);
        VScrollPositionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///  Loads and displays the diff represented by a file-status entry.
    /// </summary>
    public Task ViewChangesAsync(FileStatusItem? item, CancellationToken cancellationToken)
        => ViewChangesAsync(item, openWithDiffTool: null, cancellationToken);

    /// <summary>
    ///  Loads and displays the diff represented by a file-status entry and retains the
    ///  consumer's external-difftool action for the shared FileViewer hotkey.
    /// </summary>
    public async Task ViewChangesAsync(
        FileStatusItem? item,
        Action? openWithDiffTool = null,
        CancellationToken cancellationToken = default)
    {
        CancellationToken viewToken = BeginView(cancellationToken);
        if (item?.Item is null)
        {
            await InvokeOnOwnerMainThreadAsync(
                () => ViewPatchCore(null, useGitColoring: false, isCombinedDiff: false, isGitWordDiff: false),
                viewToken);
            return;
        }

        if (item.Item.IsStatusOnly)
        {
            await ShowTextAsync(item.Item.Name, item.Item.ErrorMessage ?? string.Empty, item, line: null, openWithDiffTool, checkGitAttributes: false, viewToken);
            return;
        }

        ObjectId firstId = item.FirstRevision?.ObjectId ?? item.SecondRevision.FirstParentId;
        ObjectId secondId = item.SecondRevision.ObjectId;
        if (!item.Item.IsSubmodule
            && (item.Item.IsNew || firstId.IsZero || (!item.Item.IsDeleted && FileHelper.IsImage(item.Item.Name))))
        {
            await ViewGitItemCoreAsync(item.Item, secondId, item, line: null, openWithDiffTool, viewToken);
            return;
        }

        bool isTracked = item.Item.IsTracked || (!item.Item.TreeId.IsZero && !secondId.IsZero);
        if (AppSettings.DiffDisplayAppearance.Value == DiffDisplayAppearance.Difftastic && IsDifftasticEnabled.Value)
        {
            (ArgumentString diffArgs, string extraCacheKey) = GetDifftasticArguments();
            ExecutionResult result = await Module.GetSingleDifftoolAsync(
                firstId,
                secondId,
                item.Item.Name,
                item.Item.OldName,
                diffArgs,
                cacheResult: true,
                extraCacheKey,
                isTracked,
                useGitColoring: true,
                viewToken);

            if (!result.ExitedSuccessfully)
            {
                string output = $"Git command exit code: {result.ExitCodeDisplay}{Environment.NewLine}{result.StandardError}";
                await ShowTextAsync(item.Item.Name, output, item, line: null, openWithDiffTool, checkGitAttributes: false, viewToken);
                return;
            }

            await InvokeOnOwnerMainThreadAsync(() =>
            {
                ResetView(ViewMode.Difftastic, item.Item.Name, item, openWithDiffTool);
                string parsedText = result.StandardOutput;
                DifftasticHighlightService highlightService = new(
                    ref parsedText,
                    internalFileViewer.LineNumbersControl,
                    out int rightColumnStart);
                VRulerPosition = rightColumnStart;
                SetDiffText(parsedText, highlightService, showLeftColumn: true);
                internalFileViewer.GoToFirstChange(NumberOfContextLines);
                TextLoaded?.Invoke(this, EventArgs.Empty);
            }, viewToken);
            return;
        }

        bool isGitWordDiff = AppSettings.DiffDisplayAppearance.Value == DiffDisplayAppearance.GitWordDiff;
        bool useGitColoring = isGitWordDiff || AppSettings.UseGitColoring.Value;

        (Patch? patch, string? errorMessage) = await Module.GetSingleDiffAsync(
            firstId,
            secondId,
            item.Item.Name,
            item.Item.OldName,
            extraDiffArguments: GetExtraDiffArguments().ToString(),
            Encoding,
            cacheResult: true,
            isTracked,
            useGitColoring,
            GitCommandConfiguration.Default,
            viewToken);

        await InvokeOnOwnerMainThreadAsync(() =>
        {
            viewToken.ThrowIfCancellationRequested();
            ViewPatchCore(
                patch?.Text ?? errorMessage,
                useGitColoring,
                isCombinedDiff: false,
                isGitWordDiff,
                item.Item.Name,
                item,
                openWithDiffTool);
        }, viewToken);
    }

    private async Task InvokeOnOwnerMainThreadAsync(Action action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            await Dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken);
        }
    }

    /// <summary>Gets the retained external-difftool action.</summary>
    public Action? OpenWithDifftool => _openWithDifftool;

    /// <summary>Gets the number of document lines.</summary>
    public int TotalNumberOfLines => internalFileViewer.TotalNumberOfLines;

    /// <summary>Moves to the next highlighted Find occurrence.</summary>
    public void GoToNextOccurrence() => internalFileViewer.GoToNextOccurrence();

    /// <summary>Moves to the previous highlighted Find occurrence.</summary>
    public void GoToPreviousOccurrence() => internalFileViewer.GoToPreviousOccurrence();

    private WinFormsShims.IWin32Window? GetOwner()
        => TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;

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

    private void contextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        ContextMenuOpening?.Invoke(this, e);
        if (e.Cancel)
        {
            return;
        }

        copyToolStripMenuItem.IsEnabled = TextEditor.SelectionLength > 0;
        stageSelectedLinesToolStripMenuItem.IsEnabled = AllowLinePatching;
        unstageSelectedLinesToolStripMenuItem.IsEnabled = AllowLinePatching;
        resetSelectedLinesToolStripMenuItem.IsEnabled = AllowLinePatching;
        replaceToolStripMenuItem.IsVisible = !TextEditor.IsReadOnly;
        goToLineToolStripMenuItem.IsEnabled = MaxLineNumber > 0;
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

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddToolTip(nameof(nextChangeButton), "Next change");
        AddToolTip(nameof(previousChangeButton), "Previous change");
        AddToolTip(nameof(increaseNumberOfLines), "Increase the number of lines of context");
        AddToolTip(nameof(decreaseNumberOfLines), "Decrease the number of lines of context");
        AddToolTip(nameof(showEntireFileButton), "Show entire file");
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
        AddToolTip(nameof(ignoreWhitespaceAtEol), "Ignore whitespace changes at end of line");
        AddToolTip(nameof(ignoreWhiteSpaces), "Ignore changes in amount of whitespace");
        AddToolTip(nameof(ignoreAllWhitespaces), "Ignore all whitespace changes");
        AddToolTip(nameof(settingsButton), "Settings");

        void AddToolTip(string name, string source)
            => translation.AddTranslationItem(nameof(FileViewer), name, "ToolTipText", source);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        SetTranslatedToolTip(nextChangeButton, nameof(nextChangeButton), "Next change");
        SetTranslatedToolTip(previousChangeButton, nameof(previousChangeButton), "Previous change");
        SetTranslatedToolTip(increaseNumberOfLines, nameof(increaseNumberOfLines), "Increase the number of lines of context");
        SetTranslatedToolTip(decreaseNumberOfLines, nameof(decreaseNumberOfLines), "Decrease the number of lines of context");
        SetTranslatedToolTip(showEntireFileButton, nameof(showEntireFileButton), "Show entire file");
        SetTranslatedToolTip(showNonPrintChars, nameof(showNonPrintChars), "Show nonprinting characters");
        SetTranslatedToolTip(showSyntaxHighlighting, nameof(showSyntaxHighlighting), "Show syntax highlighting");
        SetTranslatedToolTip(ignoreWhitespaceAtEol, nameof(ignoreWhitespaceAtEol), "Ignore whitespace changes at end of line");
        SetTranslatedToolTip(ignoreWhiteSpaces, nameof(ignoreWhiteSpaces), "Ignore changes in amount of whitespace");
        SetTranslatedToolTip(ignoreAllWhitespaces, nameof(ignoreAllWhitespaces), "Ignore all whitespace changes");
        SetTranslatedToolTip(settingsButton, nameof(settingsButton), "Settings");
        automaticContinuousScrollToolStripMenuItem.Header = TranslatedStrings.ContScrollToNextFileOnlyWithAlt;

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

    private bool RaiseContinuousScroll(double delta, KeyModifiers keyModifiers)
    {
        if (keyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return false;
        }

        if (delta > 0)
        {
            return _continuousScrollEventManager.RaiseTopScrollReached(keyModifiers);
        }

        if (delta < 0)
        {
            return _continuousScrollEventManager.RaiseBottomScrollReached(keyModifiers);
        }

        return false;
    }
}
