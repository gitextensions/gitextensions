using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Avatars;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Editor;
using GitUI.HelperDialogs;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Blame;

// Twin of GitUI/UserControls/BlameControl.cs. The author gutter is a BlameAuthorMargin
// inside the file editor instead of a second scroll-synchronised editor, so the
// scroll-position handlers of the original have no twin. Repository-hoster context-menu
// items remain owned by their provider phase.
public sealed partial class BlameControl : GitModuleControl
{
    /// <summary>
    /// Raised when the Escape key is pressed (and only when no selection exists, as the default behaviour of escape is to clear the selection).
    /// </summary>
    public event Action? EscapePressed;

    private readonly CancellationTokenSequence _blameLoadSequence = new();
    private static readonly IAvatarProvider _noAuthorAvatarProvider = new StaticImageAvatarProvider(AvatarImage.Encode(Images.User80));
    private readonly IAvatarProvider _avatarProvider;
    private int _lineIndex = -1;
    private GitBlameLine? _lastBlameLine;
    private GitBlameLine? _clickedBlameLine;
    private GitBlameCommit? _highlightedCommit;
    private GitBlame? _blame;
    private IRevisionGridInfo? _revisionGridInfo;
    private IRevisionGridFileUpdate? _revisionGridFileUpdate;
    private ObjectId? _blameId;
    private string? _fileName;
    private Encoding? _encoding;
    private GitBlameCommit? _tooltipCommit;
    private static readonly TranslationString _blameActualPreviousRevision = new("&Blame previous revision");
    private static readonly TranslationString _blameVisiblePreviousRevision = new("&Blame previous visible revision");
    private readonly IGitRevisionSummaryBuilder _gitRevisionSummaryBuilder;
    private readonly IGitBlameParser _gitBlameParser;
    private bool _loading;

    internal BlameAuthorMargin BlameAuthor { get; }

    public BlameControl()
        : this(AvatarService.DefaultProvider)
    {
    }

    internal BlameControl(IAvatarProvider avatarProvider)
    {
        _avatarProvider = avatarProvider ?? throw new ArgumentNullException(nameof(avatarProvider));
        InitializeComponent();

        BlameAuthor = new BlameAuthorMargin(
            new Avalonia.Media.Typeface(BlameFile.TextEditor.FontFamily),
            BlameFile.TextEditor.FontSize);
        BlameFile.TextEditor.TextArea.LeftMargins.Insert(0, BlameAuthor);
        UpdateShowLineNumbers();
        BlameAuthor.PointerMoved += BlameAuthor_MouseMove;
        BlameAuthor.PointerExited += BlameAuthor_MouseLeave;

        BlameFile.PointerMoved += BlameFile_MouseMove;
        BlameFile.SelectedLineChanged += SelectedLineChanged;
        BlameFile.DoubleTapped += ActiveTextAreaControlDoubleClick;
        BlameFile.EscapePressed += () => EscapePressed?.Invoke();

        contextMenu.Opening += contextMenu_Opened;
        blameRevisionToolStripMenuItem.Click += blameRevisionToolStripMenuItem_Click;
        blamePreviousRevisionToolStripMenuItem.Click += blamePreviousRevisionToolStripMenuItem_Click;
        showChangesToolStripMenuItem.Click += showChangesToolStripMenuItem_Click;
        commitHashToolStripMenuItem.Click += copyCommitHashToClipboardToolStripMenuItem_Click;
        commitMessageToolStripMenuItem.Click += copyLogMessageToolStripMenuItem_Click;
        allCommitInfoToolStripMenuItem.Click += copyAllCommitInfoToClipboardToolStripMenuItem_Click;
        ActualThemeVariantChanged += (_, _) => RefreshThemeResources();

        InitializeComplete();

        _gitRevisionSummaryBuilder = new GitRevisionSummaryBuilder();
        _gitBlameParser = new GitBlameParser(() => UICommands.Module);
    }

    public void InitSplitterManager(NestedSplitterManager splitterManager)
    {
        NestedSplitterManager nested = new(splitterManager, Name ?? nameof(BlameControl));
        if (CommitInfo.IsVisible)
        {
            nested.AddSplitter(splitContainer1, defaultDistance: 26);
        }

        nested.AddSplitter(BlameAuthor, "splitContainer2", defaultDistance: 26);
    }

    public void HideCommitInfo()
    {
        CommitInfo.IsVisible = false;
        splitContainer1.RowDefinitions[0].Height = new GridLength(0);
        splitContainer1.RowDefinitions[1].Height = new GridLength(0);
    }

    internal void CancelBackgroundTasks()
    {
        _blameLoadSequence.CancelCurrent();
        BlameAuthor.Clear();
    }

    public void UpdateShowLineNumbers()
    {
        BlameFile.TextEditor.ShowLineNumbers = AppSettings.BlameShowLineNumbers;
    }

    public int CurrentFileColumn => BlameFile.CurrentFileColumn;

    public int CurrentFileLine => BlameFile.CurrentFileLine;

    public async Task LoadBlameAsync(
        GitRevision revision,
        string fileName,
        IRevisionGridInfo? revisionGridInfo,
        IRevisionGridFileUpdate? revisionGridFileUpdate,
        Encoding encoding,
        int? initialLine = null,
        bool force = false,
        CancellationTokenSequence? cancellationTokenSequence = null,
        JoinableTaskFactory? joinableTaskFactory = null)
    {
        ObjectId objectId = revision.ObjectId;

        // refresh only when something changed
        if (!force && objectId == _blameId && fileName == _fileName && revisionGridInfo == _revisionGridInfo && _revisionGridFileUpdate == revisionGridFileUpdate && encoding == _encoding)
        {
            if (initialLine is not null && !_loading)
            {
                BlameFile.GoToLine(initialLine.Value);
            }

            return;
        }

        CancellationToken ownerCancellationToken = cancellationTokenSequence?.Next() ?? default;
        CancellationToken loadCancellationToken = _blameLoadSequence.Next();
        using CancellationTokenSource linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            ownerCancellationToken,
            loadCancellationToken);
        CancellationToken cancellationToken = linkedCancellation.Token;
        JoinableTaskFactory mainThreadFactory = joinableTaskFactory ?? ThreadHelper.JoinableTaskFactory;
        _loading = true;

        int line = _clickedBlameLine?.OriginLineNumber ?? initialLine ?? (fileName == _fileName ? BlameFile.CurrentFileLine : 1);
        _revisionGridInfo = revisionGridInfo;
        _revisionGridFileUpdate = revisionGridFileUpdate;
        _fileName = fileName;
        _encoding = encoding;

        // Clear the contents of the viewer while loading
        BlameAuthor.Clear();
        await BlameFile.ClearAsync();

        try
        {
            await TaskScheduler.Default;
            cancellationToken.ThrowIfCancellationRequested();
            _blame = Module.Blame(fileName, objectId.ToString(), encoding, lines: null, cancellationToken: cancellationToken);
            await mainThreadFactory.SwitchToMainThreadAsync(cancellationToken);
            await ProcessBlameAsync(fileName, revision, line, mainThreadFactory, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ExternalOperationException ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await mainThreadFactory.SwitchToMainThreadAsync();
            _blame = null;
            await BlameFile.ViewTextAsync(fileName, ex.Message);
        }
        finally
        {
            _loading = false;
        }
    }

    private void BlameAuthor_MouseLeave(object? sender, PointerEventArgs e)
    {
        _tooltipCommit = null;
    }

    private void BlameAuthor_MouseMove(object? sender, PointerEventArgs e)
    {
        if (_blame is null)
        {
            return;
        }

        _lineIndex = BlameAuthor.GetLineFromVisualPosY(e.GetPosition(BlameAuthor).Y);

        GitBlameCommit? blameCommit = _lineIndex < _blame.Lines.Count
            ? _blame.Lines[_lineIndex].Commit
            : null;

        HighlightLinesForCommit(blameCommit);

        if (blameCommit is null)
        {
            return;
        }

        if (_tooltipCommit != blameCommit)
        {
            _tooltipCommit = blameCommit;
            ToolTip.SetTip(BlameAuthor, blameCommit.ToString(_gitRevisionSummaryBuilder.BuildSummary));
        }
    }

    private void BlameFile_MouseMove(object? sender, PointerEventArgs e)
    {
        if (_blame is null)
        {
            return;
        }

        int lineIndex = BlameFile.GetLineFromVisualPosY(e.GetPosition(BlameFile).Y);

        // Track the hovered line for the context menu too (there is no MousePosition
        // to query when the menu opens).
        _lineIndex = lineIndex;

        GitBlameCommit? blameCommit = lineIndex < _blame.Lines.Count
            ? _blame.Lines[lineIndex].Commit
            : null;

        HighlightLinesForCommit(blameCommit);
    }

    private void HighlightLinesForCommit(GitBlameCommit? commit)
    {
        if (commit == _highlightedCommit)
        {
            return;
        }

        _highlightedCommit = commit;

        BlameAuthor.ClearHighlighting();
        BlameFile.ClearHighlighting();

        if (commit is null)
        {
            BlameAuthor.Refresh();
            BlameFile.Refresh();
            return;
        }

        Validates.NotNull(_blame);
        Color commitHighlightColor = GetThemeColor("GitExtensionsBlameHighlightBrush", SystemColors.ControlLight);

        int startLine = -1;
        int prevLine = -1;
        for (int i = 0; i < _blame.Lines.Count; i++)
        {
            if (ReferenceEquals(_blame.Lines[i].Commit, commit))
            {
                if (prevLine != i - 1 && startLine != -1)
                {
                    BlameAuthor.HighlightLines(startLine, prevLine, commitHighlightColor);
                    BlameFile.HighlightLines(startLine, prevLine, commitHighlightColor);
                    startLine = -1;
                }

                prevLine = i;
                if (startLine == -1)
                {
                    startLine = i;
                }
            }
        }

        if (startLine != -1)
        {
            BlameAuthor.HighlightLines(startLine, prevLine, commitHighlightColor);
            BlameFile.HighlightLines(startLine, prevLine, commitHighlightColor);
        }

        BlameAuthor.Refresh();
        BlameFile.Refresh();
    }

    private void SelectedLineChanged(object? sender, SelectedLineEventArgs e)
    {
        int selectedLine = e.SelectedLine;

        if (_blame is null || selectedLine < 0 || selectedLine >= _blame.Lines.Count)
        {
            return;
        }

        GitBlameLine newBlameLine = _blame.Lines[selectedLine];

        if (ReferenceEquals(_lastBlameLine?.Commit, newBlameLine.Commit))
        {
            return;
        }

        _lastBlameLine = newBlameLine;
        ObjectId objectId = _lastBlameLine.Commit.ObjectId;
        CommitInfo.Revision = _revisionGridInfo is null ? Module.GetRevision(objectId) : _revisionGridInfo.GetActualRevision(objectId);
    }

    private async Task ProcessBlameAsync(
        string? filename,
        GitRevision revision,
        int lineNumber,
        JoinableTaskFactory mainThreadFactory,
        CancellationToken cancellationToken = default)
    {
        (string gutter, string body, List<GitBlameEntry> gitBlameEntries) = BuildBlameContents(filename);
        cancellationToken.ThrowIfCancellationRequested();

        bool showAuthorAvatar = AppSettings.BlameShowAuthorAvatar;
        int contentVersion = BlameAuthor.Initialize(gutter, gitBlameEntries, showAuthorAvatar);

        Validates.NotNull(_fileName);
        await BlameFile.ViewTextAsync(_fileName, body, cancellationToken);
        BlameFile.GoToLine(Math.Min(lineNumber, _blame!.Lines.Count));
        _clickedBlameLine = null;

        _blameId = revision.ObjectId;
        CommitInfo.Revision = revision;

        if (showAuthorAvatar)
        {
            await LoadAvatarsAsync(gitBlameEntries, contentVersion, mainThreadFactory, cancellationToken);
        }
    }

    private async Task LoadAvatarsAsync(
        IReadOnlyList<GitBlameEntry> gitBlameEntries,
        int contentVersion,
        JoinableTaskFactory mainThreadFactory,
        CancellationToken cancellationToken)
    {
        Validates.NotNull(_blame);

        int avatarSize = BlameAuthor.AvatarSize;
        GitBlameCommit? lastCommit = null;
        Dictionary<string, Task<byte[]?>> avatarRequests = new(StringComparer.OrdinalIgnoreCase);
        List<Task> updates = [];

        for (int index = 0; index < _blame.Lines.Count; index++)
        {
            GitBlameLine line = _blame.Lines[index];
            if (line.Commit == lastCommit)
            {
                continue;
            }

            string? authorEmail = line.Commit.AuthorMail?.Trim('<', '>');
            Task<byte[]?> request;
            if (authorEmail is null)
            {
                request = _noAuthorAvatarProvider.GetAvatarAsync(string.Empty, line.Commit.Author, avatarSize);
            }
            else if (avatarRequests.TryGetValue(authorEmail, out Task<byte[]?>? cachedRequest))
            {
                request = cachedRequest;
            }
            else
            {
                request = _avatarProvider.GetAvatarAsync(authorEmail, line.Commit.Author, avatarSize);
                avatarRequests.Add(authorEmail, request);
            }

            updates.Add(ApplyAvatarAsync(index, request));
            lastCommit = line.Commit;
        }

        await Task.WhenAll(updates);

        async Task ApplyAvatarAsync(int lineIndex, Task<byte[]?> request)
        {
            byte[]? imageData = await request.WaitAsync(cancellationToken);
            await mainThreadFactory.SwitchToMainThreadAsync(cancellationToken);
            if (lineIndex < gitBlameEntries.Count)
            {
                BlameAuthor.SetAvatar(lineIndex, imageData, contentVersion);
            }
        }
    }

    private (string gutter, string body, List<GitBlameEntry> gitBlameDisplays) BuildBlameContents(string? filename)
    {
        Validates.NotNull(_blame);

        if (_blame.Lines.Count == 0)
        {
            return ("", "", new List<GitBlameEntry>(0));
        }

        StringBuilder body = new(capacity: 4096);

        GitBlameCommit? lastCommit = null;

        bool showAuthorAvatar = AppSettings.BlameShowAuthorAvatar;
        List<GitBlameEntry> gitBlameDisplays = showAuthorAvatar ? CalculateBlameGutterData(_blame.Lines) : [];

        string dateTimeFormat = AppSettings.BlameShowAuthorTime
            ? CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " +
              CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern
            : CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        filename = filename?.ToPosixPath();

        int filePathLengthEstimate = _blame.Lines.Where(l => filename != l.Commit.FileName)
                                                 .Select(l => l.Commit.FileName.Length)
                                                 .DefaultIfEmpty(0)
                                                 .Max();
        int lineLengthEstimate = 25 + _blame.Lines.Max(l => l.Commit.Author?.Length ?? 0) + filePathLengthEstimate;
        int lineLength = Math.Max(80, lineLengthEstimate);
        StringBuilder lineBuilder = new(lineLength + 2);
        StringBuilder gutter = new(capacity: lineBuilder.Capacity * _blame.Lines.Count);
        string emptyLine = new(' ', lineLength);
        Dictionary<ObjectId, string> authorLineCache = [];
        for (int index = 0; index < _blame.Lines.Count; index++)
        {
            GitBlameLine line = _blame.Lines[index];
            if (line.Commit == lastCommit)
            {
                gutter.AppendLine(emptyLine);
            }
            else
            {
                if (!authorLineCache.TryGetValue(line.Commit.ObjectId, out string? authorLine))
                {
                    authorLine = BuildAuthorLine(line, lineBuilder, lineLength, dateTimeFormat, filename, AppSettings.BlameShowAuthor, AppSettings.BlameShowAuthorDate, AppSettings.BlameShowOriginalFilePath, AppSettings.BlameDisplayAuthorFirst);
                    authorLineCache.Add(line.Commit.ObjectId, authorLine);
                    lineBuilder.Clear();
                }

                gutter.Append(authorLine);
            }

            body.AppendLine(line.Text);

            lastCommit = line.Commit;
        }

        return (gutter.ToString(), body.ToString(), gitBlameDisplays);
    }

    private static string BuildAuthorLine(GitBlameLine line, StringBuilder authorLineBuilder, int lineLength, string dateTimeFormat,
        string? filename, bool showAuthor, bool showAuthorDate, bool showOriginalFilePath, bool displayAuthorFirst)
    {
        if (showAuthor && displayAuthorFirst)
        {
            authorLineBuilder.Append(line.Commit.Author);
            if (showAuthorDate)
            {
                authorLineBuilder.Append(" - ");
            }
        }

        if (showAuthorDate)
        {
            authorLineBuilder.Append(line.Commit.AuthorTime.ToString(dateTimeFormat));
        }

        if (showAuthor && !displayAuthorFirst)
        {
            if (showAuthorDate)
            {
                authorLineBuilder.Append(" - ");
            }

            authorLineBuilder.Append(line.Commit.Author);
        }

        if (showOriginalFilePath && filename != line.Commit.FileName)
        {
            authorLineBuilder.Append(" - ");
            authorLineBuilder.Append(line.Commit.FileName);
        }

        authorLineBuilder.Append(' ', Math.Max(0, lineLength - authorLineBuilder.Length)).AppendLine();

        return authorLineBuilder.ToString();
    }

    private Color[] GetAgeBucketGradientColors()
    {
        Color[] fallbacks =
        [
            Color.FromArgb(247, 252, 245),
            Color.FromArgb(199, 233, 192),
            Color.FromArgb(161, 217, 155),
            Color.FromArgb(116, 196, 118),
            Color.FromArgb(65, 171, 93),
            Color.FromArgb(35, 139, 69),
            Color.FromArgb(0, 68, 27),
        ];
        return [.. fallbacks.Select((fallback, index) => GetThemeColor($"GitExtensionsBlameAge{index}Brush", fallback))];
    }

    public DateTime ArtificialOldBoundary => DateTime.Now.AddYears(-3);

    private List<GitBlameEntry> CalculateBlameGutterData(IReadOnlyList<GitBlameLine> blameLines)
    {
        long mostRecentDate = DateTime.Now.Ticks;
        DateTime artificialOldBoundary = ArtificialOldBoundary;
        List<GitBlameEntry> gitBlameDisplays = new(blameLines.Count);

        long lessRecentDate = Math.Min(artificialOldBoundary.Ticks,
                                      blameLines.Select(l => l.Commit.AuthorTime)
                                                .Where(d => d != DateTime.MinValue)
                                                .DefaultIfEmpty(artificialOldBoundary)
                                                .Min()
                                                .Ticks);
        Color[] ageBucketGradientColors = GetAgeBucketGradientColors();
        long intervalSize = (mostRecentDate - lessRecentDate + 1) / ageBucketGradientColors.Length;
        foreach (GitBlameLine blame in blameLines)
        {
            long relativeTicks = Math.Max(0, blame.Commit.AuthorTime.Ticks - lessRecentDate);
            int ageBucketIndex = Math.Min((int)(relativeTicks / intervalSize), ageBucketGradientColors.Length - 1);
            GitBlameEntry gitBlameDisplay = new()
            {
                AgeBucketIndex = ageBucketIndex,
                AgeBucketColor = ageBucketGradientColors[ageBucketIndex]
            };
            gitBlameDisplays.Add(gitBlameDisplay);
        }

        return gitBlameDisplays;
    }

    private void ActiveTextAreaControlDoubleClick(object? sender, EventArgs e)
    {
        if (_lastBlameLine is not null
            && TryGetRevision(_lastBlameLine.Commit, out (GitRevision? SelectedRevision, string? Filename) blameInfo))
        {
            BlameRevision(_lastBlameLine.Commit.ObjectId, blameInfo.Filename!, _lastBlameLine);
        }
    }

    private int GetBlameLine()
    {
        if (_blame is null)
        {
            return -1;
        }

        // The hover handlers track the line under the pointer.
        return _lineIndex >= 0 && _lineIndex < _blame.Lines.Count ? _lineIndex : -1;
    }

    private void contextMenu_Opened(object sender, EventArgs e)
    {
        // Unlike WinForms, the menu can open before a blame is loaded; disable instead
        // of asserting.
        if (_fileName is null || _blame is null || _blameId is null
            || !TryGetRevision(GetBlameCommit(), out (GitRevision? SelectedRevision, string? Filename) blameinfo))
        {
            blameRevisionToolStripMenuItem.IsEnabled = false;

            // Ignore if current revision is not visible in grid but parent is.
            blamePreviousRevisionToolStripMenuItem.IsEnabled = false;
            return;
        }

        blameRevisionToolStripMenuItem.IsEnabled = true;

        // Get parent for the actual revision, the selected revision may have rewritten parents.
        // The menu will be slightly slower in this situation.
        if (RevisionHasParent(_revisionGridInfo?.GetActualRevision(blameinfo.SelectedRevision!)))
        {
            blamePreviousRevisionToolStripMenuItem.IsEnabled = true;
            blamePreviousRevisionToolStripMenuItem.Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_blameActualPreviousRevision.Text);
        }
        else
        {
            blamePreviousRevisionToolStripMenuItem.IsEnabled = RevisionHasParent(blameinfo.SelectedRevision);
            blamePreviousRevisionToolStripMenuItem.Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_blameVisiblePreviousRevision.Text);
        }

        return;

        bool RevisionHasParent(GitRevision? revision)
            => (revision?.HasParent is true) && (_revisionGridInfo?.GetRevision(revision!.FirstParentId) is not null);
    }

    private GitBlameCommit? GetBlameCommit()
    {
        int line = GetBlameLine();
        if (line < 0)
        {
            return null;
        }

        Validates.NotNull(_blame);

        return _blame.Lines[line].Commit;
    }

    private void copyLogMessageToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CopyToClipboard(c => c.Summary);
    }

    private void CopyToClipboard(Func<GitBlameCommit, string> formatter)
    {
        GitBlameCommit? commit = GetBlameCommit();

        if (commit is null)
        {
            return;
        }

        ClipboardUtil.TrySetText(formatter(commit));
    }

    private void copyAllCommitInfoToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CopyToClipboard(c => c.ToString());
    }

    private void copyCommitHashToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CopyToClipboard(c => c.ObjectId.ToString());
    }

    private bool TryGetRevision(GitBlameCommit? blameCommit, [System.Diagnostics.CodeAnalysis.NotNullWhen(returnValue: true)] out (GitRevision? selectedRevision, string? filename) blameInfo)
    {
        if (blameCommit is null)
        {
            blameInfo = (null, null);
            return false;
        }

        blameInfo = (_revisionGridInfo?.GetRevision(blameCommit.ObjectId), blameCommit.FileName);
        return blameInfo.selectedRevision is not null;
    }

    private void blameRevisionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (!TryGetRevision(GetBlameCommit(), out (GitRevision? SelectedRevision, string? Filename) blameInfo))
        {
            return;
        }

        BlameRevision(blameInfo.SelectedRevision!.ObjectId, blameInfo.Filename!, _lastBlameLine!);
    }

    private void blamePreviousRevisionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (!TryGetRevision(GetBlameCommit(), out (GitRevision? SelectedRevision, string? Filename) blameInfo))
        {
            return;
        }

        GitRevision? selectedRevision = blameInfo.SelectedRevision;
        if ((blamePreviousRevisionToolStripMenuItem.Header as string) == AvaloniaTranslationUtils.ToAvaloniaMnemonics(_blameActualPreviousRevision.Text))
        {
            // Try get actual parent revision, get popup if it does not exist.
            // (The menu should be disabled if previous is not in grid).
            selectedRevision = _revisionGridInfo!.GetActualRevision(selectedRevision!);
        }

        // Origin line of commit selected is final line of the previous blame commit
        int finalLineNumberOfPreviousBlame = _lastBlameLine!.OriginLineNumber;
        int originalLineNumberOfPreviousBlame = _gitBlameParser.GetOriginalLineInPreviousCommit(selectedRevision!, blameInfo.Filename!, finalLineNumberOfPreviousBlame);

        GitBlameLine blameLine = new(_lastBlameLine.Commit, finalLineNumberOfPreviousBlame, originalLineNumberOfPreviousBlame, "Dummy Git blame line used only to store the good 'originLineNumber' value to display and select it");
        BlameRevision(selectedRevision!.FirstParentId, blameInfo.Filename!, blameLine);
    }

    /// <summary>
    /// Blame a specific revision
    /// </summary>
    /// <param name="commitId">the commit id to blame</param>
    /// <param name="filename">the relative path of the file to blame in this commit (because it could have been renamed)</param>
    private void BlameRevision(ObjectId commitId, string filename, GitBlameLine blameLine)
    {
        _clickedBlameLine = blameLine;

        if (_revisionGridFileUpdate is not null)
        {
            if (!_revisionGridFileUpdate.SelectFileInRevision(commitId, RelativePath.From(filename)))
            {
                MessageBoxes.RevisionFilteredInGrid(GetOwner(), commitId);
            }

            return;
        }

        using FormCommitDiff frm = new(UICommands, commitId);
        frm.ShowDialog(GetOwner());
    }

    private void showChangesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        GitBlameCommit? commit = GetBlameCommit();

        if (commit is null)
        {
            return;
        }

        using FormCommitDiff frm = new(UICommands, commit.ObjectId);
        frm.ShowDialog(GetOwner());
    }

    private WinFormsShims.IWin32Window? GetOwner()
        => TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;

    private Color GetThemeColor(string resourceKey, Color fallback)
    {
        if (Avalonia.Application.Current?.TryGetResource(resourceKey, ActualThemeVariant, out object? resource) == true
            && resource is Avalonia.Media.ISolidColorBrush brush)
        {
            Avalonia.Media.Color color = brush.Color;
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        return fallback;
    }

    private void RefreshThemeResources()
    {
        if (_blame is not null)
        {
            (_, _, List<GitBlameEntry> entries) = BuildBlameContents(_fileName);
            BlameAuthor.UpdateAgeBuckets(entries);
        }

        GitBlameCommit? highlightedCommit = _highlightedCommit;
        _highlightedCommit = null;
        HighlightLinesForCommit(highlightedCommit);
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly BlameControl _control;

        public TestAccessor(BlameControl control)
        {
            _control = control;
        }

        public GitBlame? Blame
        {
            get => _control._blame;
            set => _control._blame = value;
        }

        public FileViewer BlameFile => _control.BlameFile;

        public Grid SplitContainer => _control.splitContainer1;

        public double AuthorMarginWidth
        {
            get => ((IPersistedSplitter)_control.BlameAuthor).SplitterDistance;
            set => ((IPersistedSplitter)_control.BlameAuthor).SplitterDistance = value;
        }

        public DateTime ArtificialOldBoundary => _control.ArtificialOldBoundary;

        public void BuildAuthorLine(GitBlameLine line, StringBuilder lineBuilder, int lineLength, string dateTimeFormat, string filename, bool showAuthor, bool showAuthorDate, bool showOriginalFilePath, bool displayAuthorFirst)
            => BlameControl.BuildAuthorLine(line, lineBuilder, lineLength, dateTimeFormat, filename, showAuthor, showAuthorDate, showOriginalFilePath, displayAuthorFirst);

        public (string gutter, string body, List<GitBlameEntry> gitBlameDisplays) BuildBlameContents(string filename) => _control.BuildBlameContents(filename);

        public Task LoadAvatarsAsync(IReadOnlyList<GitBlameEntry> entries, int contentVersion, CancellationToken cancellationToken = default)
            => _control.LoadAvatarsAsync(entries, contentVersion, ThreadHelper.JoinableTaskFactory, cancellationToken);

        public List<GitBlameEntry> CalculateBlameGutterData(IReadOnlyList<GitBlameLine> blameLines)
            => _control.CalculateBlameGutterData(blameLines);

        public IReadOnlyList<Color> GetAgeBucketGradientColors()
            => _control.GetAgeBucketGradientColors();

        public Color GetCommitHighlightColor()
            => _control.GetThemeColor("GitExtensionsBlameHighlightBrush", SystemColors.ControlLight);
    }
}
