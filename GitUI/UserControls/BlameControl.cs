using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Avatars;
using GitUI.Editor;
using GitUI.HelperDialogs;
using GitUI.Properties;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using Microsoft;
using ResourceManager;

namespace GitUI.Blame
{
    public sealed partial class BlameControl : GitModuleControl
    {
        public event EventHandler<CommandEventArgs>? CommandClick;

        /// <summary>
        /// Raised when the Escape key is pressed (and only when no selection exists, as the default behaviour of escape is to clear the selection).
        /// </summary>
        public event Action? EscapePressed;

        private readonly AsyncLoader _blameLoader = new();
        private int _lineIndex;

        private GitBlameLine? _lastBlameLine;
        private GitBlameLine? _clickedBlameLine;
        private GitBlameCommit? _highlightedCommit;
        private GitBlame? _blame;
        private RevisionGridControl? _revGrid;
        private ObjectId? _blameId;
        private string? _fileName;
        private Encoding? _encoding;
        private int _lastTooltipX = -100;
        private int _lastTooltipY = -100;
        private GitBlameCommit? _tooltipCommit;
        private bool _changingScrollPosition;
        private IRepositoryHostPlugin? _gitHoster;
        private static readonly IList<Color> AgeBucketGradientColors = GetAgeBucketGradientColors();
        private IGitRevisionSummaryBuilder _gitRevisionSummaryBuilder;

        // Relative path of the file to blame when blaming a new revision
        public string? PathToBlame { get; private set; }

        public BlameControl()
        {
            InitializeComponent();
            InitializeComplete();

            BlameAuthor.IsReadOnly = true;
            BlameAuthor.EnableScrollBars(false);
            UpdateShowLineNumbers();
            BlameAuthor.HScrollPositionChanged += BlameAuthor_HScrollPositionChanged;
            BlameAuthor.VScrollPositionChanged += BlameAuthor_VScrollPositionChanged;
            BlameAuthor.MouseMove += BlameAuthor_MouseMove;
            BlameAuthor.MouseLeave += BlameAuthor_MouseLeave;
            BlameAuthor.SelectedLineChanged += SelectedLineChanged;
            BlameAuthor.RequestDiffView += ActiveTextAreaControlDoubleClick;
            BlameAuthor.EscapePressed += () => EscapePressed?.Invoke();

            BlameFile.IsReadOnly = true;
            BlameFile.VScrollPositionChanged += BlameFile_VScrollPositionChanged;
            BlameFile.SelectedLineChanged += SelectedLineChanged;
            BlameFile.MouseMove += BlameFile_MouseMove;
            BlameFile.EscapePressed += () => EscapePressed?.Invoke();
            BlameFile.EnableAutomaticContinuousScroll = false;

            CommitInfo.CommandClicked += commitInfo_CommandClicked;

            _gitRevisionSummaryBuilder = new GitRevisionSummaryBuilder();
        }

        public void ConfigureRepositoryHostPlugin(IRepositoryHostPlugin? gitHoster)
        {
            _gitHoster = gitHoster;
            _gitHoster?.ConfigureContextMenu(contextMenu);
        }

        public void UpdateShowLineNumbers()
        {
            BlameAuthor.ShowLineNumbers = AppSettings.BlameShowLineNumbers;
        }

        public int CurrentFileLine => BlameFile.CurrentFileLine;

        public void HideCommitInfo()
        {
            splitContainer1.Panel1Collapsed = true;
            CommitInfo.Visible = false;
            CommitInfo.CommandClicked -= commitInfo_CommandClicked;
        }

        public async Task LoadBlameAsync(GitRevision revision, IReadOnlyList<ObjectId>? children, string fileName, RevisionGridControl? revGrid, Control? controlToMask, Encoding encoding, int? initialLine = null, bool force = false, CancellationToken cancellationToken = default)
        {
            ObjectId objectId = revision.ObjectId;

            // refresh only when something changed
            if (!force && objectId == _blameId && fileName == _fileName && revGrid == _revGrid && encoding == _encoding)
            {
                if (initialLine is not null)
                {
                    BlameFile.GoToLine(initialLine.Value);
                }

                return;
            }

            int line = _clickedBlameLine is not null && _clickedBlameLine.Commit.ObjectId == objectId
                ? _clickedBlameLine.OriginLineNumber
                : initialLine ?? (fileName == _fileName ? BlameFile.CurrentFileLine : 1);
            _revGrid = revGrid;
            _fileName = fileName;
            _encoding = encoding;

            controlToMask?.Mask();

            // Clear the contents of the viewer while loading
            BlameAuthor.ClearBlameGutter();
            await BlameAuthor.ClearAsync();
            await BlameFile.ClearAsync();

            await _blameLoader.LoadAsync(cancellationToken => _blame = Module.Blame(fileName, objectId.ToString(), encoding, cancellationToken: cancellationToken),
                () => ProcessBlame(fileName, revision, children, controlToMask, line, cancellationToken));
        }

        private void commitInfo_CommandClicked(object sender, CommandEventArgs e)
        {
            CommandClick?.Invoke(sender, e);
        }

        private void BlameAuthor_MouseLeave(object sender, EventArgs e)
        {
            blameTooltip.Hide(this);
        }

        private void BlameAuthor_MouseMove(object sender, MouseEventArgs e)
        {
            if (!BlameFile.Focused)
            {
                BlameFile.Focus();
            }

            if (_blame is null)
            {
                return;
            }

            _lineIndex = BlameAuthor.GetLineFromVisualPosY(e.Y);

            var blameCommit = _lineIndex < _blame.Lines.Count
                ? _blame.Lines[_lineIndex].Commit
                : null;

            HighlightLinesForCommit(blameCommit);

            if (blameCommit is null)
            {
                return;
            }

            int newTooltipX = splitContainer2.SplitterDistance + 60;
            int newTooltipY = e.Y + splitContainer1.SplitterDistance + 20;

            if (_tooltipCommit != blameCommit || Math.Abs(_lastTooltipX - newTooltipX) > 5 || Math.Abs(_lastTooltipY - newTooltipY) > 5)
            {
                _tooltipCommit = blameCommit;
                _lastTooltipX = newTooltipX;
                _lastTooltipY = newTooltipY;
                blameTooltip.Show(blameCommit.ToString(_gitRevisionSummaryBuilder.BuildSummary), this, newTooltipX, newTooltipY);
            }
        }

        private void BlameFile_MouseMove(object sender, MouseEventArgs e)
        {
            if (_blame is null)
            {
                return;
            }

            var lineIndex = BlameFile.GetLineFromVisualPosY(e.Y);

            var blameCommit = lineIndex < _blame.Lines.Count
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
                return;
            }

            Validates.NotNull(_blame);

            int startLine = -1;
            int prevLine = -1;
            for (int i = 0; i < _blame.Lines.Count; i++)
            {
                if (ReferenceEquals(_blame.Lines[i].Commit, commit))
                {
                    if (prevLine != i - 1 && startLine != -1)
                    {
                        BlameAuthor.HighlightLines(startLine, prevLine, SystemColors.ControlLight);
                        BlameFile.HighlightLines(startLine, prevLine, SystemColors.ControlLight);
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
                BlameAuthor.HighlightLines(startLine, prevLine, SystemColors.ControlLight);
                BlameFile.HighlightLines(startLine, prevLine, SystemColors.ControlLight);
            }

            BlameAuthor.Refresh();
            BlameFile.Refresh();
        }

        private void SelectedLineChanged(object sender, SelectedLineEventArgs e)
        {
            int selectedLine = e.SelectedLine;

            if (_blame is null || selectedLine >= _blame.Lines.Count)
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
            CommitInfo.Revision = _revGrid is null ? Module.GetRevision(objectId) : _revGrid.GetActualRevision(objectId);
        }

        private void BlameAuthor_HScrollPositionChanged(object sender, EventArgs e)
        {
            BlameAuthor.HScrollPosition = 0;
        }

        private void BlameAuthor_VScrollPositionChanged(object sender, EventArgs e)
        {
            if (!_changingScrollPosition)
            {
                _changingScrollPosition = true;
                BlameFile.VScrollPosition = BlameAuthor.VScrollPosition;
                _changingScrollPosition = false;
            }

            Rectangle rect = BlameAuthor.ClientRectangle;
            rect = BlameAuthor.RectangleToScreen(rect);
            if (rect.Contains(MousePosition))
            {
                Point p = BlameAuthor.PointToClient(MousePosition);
                MouseEventArgs me = new(0, 0, p.X, p.Y, 0);
                BlameAuthor_MouseMove(this, me);
            }
        }

        private void BlameFile_VScrollPositionChanged(object sender, EventArgs e)
        {
            if (_changingScrollPosition)
            {
                return;
            }

            _changingScrollPosition = true;
            BlameAuthor.VScrollPosition = BlameFile.VScrollPosition;
            _changingScrollPosition = false;
        }

        private void ProcessBlame(string? filename, GitRevision revision, IReadOnlyList<ObjectId>? children, Control? controlToMask, int lineNumber, CancellationToken cancellationToken = default)
        {
            var avatarSize = BlameAuthor.Font.Height + 1;
            var (gutter, body, avatars) = BuildBlameContents(filename, avatarSize);
            cancellationToken.ThrowIfCancellationRequested();

            BlameAuthor.SetGitBlameGutter(avatars);

            Validates.NotNull(_fileName);

            ThreadHelper.JoinableTaskFactory.RunAsync(
                () => BlameAuthor.ViewTextAsync("committer.txt", gutter));
            cancellationToken.ThrowIfCancellationRequested();
            ThreadHelper.JoinableTaskFactory.RunAsync(
                () => BlameFile.ViewTextAsync(_fileName, body));
            cancellationToken.ThrowIfCancellationRequested();

            BlameFile.GoToLine(lineNumber);
            _clickedBlameLine = null;

            _blameId = revision.ObjectId;
            if (CommitInfo.Visible)
            {
                CommitInfo.SetRevisionWithChildren(revision, children);
            }

            controlToMask?.UnMask();
        }

        private (string gutter, string body, List<GitBlameEntry> gitBlameDisplays) BuildBlameContents(string? filename, int avatarSize)
        {
            Validates.NotNull(_blame);

            if (_blame.Lines.Count == 0)
            {
                return ("", "", new List<GitBlameEntry>(0));
            }

            StringBuilder body = new(capacity: 4096);

            GitBlameCommit? lastCommit = null;

            bool showAuthorAvatar = AppSettings.BlameShowAuthorAvatar;
            var gitBlameDisplays = showAuthorAvatar ? CalculateBlameGutterData(_blame.Lines) : new List<GitBlameEntry>(0);

            var dateTimeFormat = AppSettings.BlameShowAuthorTime
                ? CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " +
                  CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern
                : CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

            // NOTE EOL white-space supports highlight on mouse-over.
            // Highlighting is done via text background colour.
            // If it could be done with a solid rectangle around the text,
            // the extra spaces added here could be omitted.

            filename = filename?.ToPosixPath();

            var filePathLengthEstimate = _blame.Lines.Where(l => filename != l.Commit.FileName)
                                                     .Select(l => l.Commit.FileName.Length)
                                                     .DefaultIfEmpty(0)
                                                     .Max();
            var lineLengthEstimate = 25 + _blame.Lines.Max(l => l.Commit.Author?.Length ?? 0) + filePathLengthEstimate;
            var lineLength = Math.Max(80, lineLengthEstimate);
            StringBuilder lineBuilder = new(lineLength + 2);
            StringBuilder gutter = new(capacity: lineBuilder.Capacity * _blame.Lines.Count);
            string emptyLine = new(' ', lineLength);
            Dictionary<string, Image?> cacheAvatars = new();
            var noAuthorImage = (Image)new Bitmap(Images.User80, avatarSize, avatarSize);
            for (var index = 0; index < _blame.Lines.Count; index++)
            {
                var line = _blame.Lines[index];
                if (line.Commit == lastCommit)
                {
                    gutter.AppendLine(emptyLine);
                }
                else
                {
                    var authorEmail = line.Commit.AuthorMail?.Trim('<', '>');
                    if (showAuthorAvatar)
                    {
                        if (authorEmail is not null)
                        {
                            if (cacheAvatars.ContainsKey(authorEmail))
                            {
                                gitBlameDisplays[index].Avatar = cacheAvatars[authorEmail];
                            }
                            else
                            {
                                var avatar = ThreadHelper.JoinableTaskFactory.Run(() =>
                                    AvatarService.DefaultProvider.GetAvatarAsync(authorEmail, line.Commit.Author,
                                        avatarSize));
                                cacheAvatars.Add(authorEmail, avatar);
                                gitBlameDisplays[index].Avatar = avatar;
                            }
                        }
                        else
                        {
                            gitBlameDisplays[index].Avatar = noAuthorImage;
                        }
                    }

                    BuildAuthorLine(line, lineBuilder, dateTimeFormat, filename, AppSettings.BlameShowAuthor, AppSettings.BlameShowAuthorDate, AppSettings.BlameShowOriginalFilePath, AppSettings.BlameDisplayAuthorFirst);

                    gutter.Append(lineBuilder);
                    gutter.Append(' ', lineLength - lineBuilder.Length).AppendLine();
                    lineBuilder.Clear();
                }

                body.AppendLine(line.Text);

                lastCommit = line.Commit;
            }

            return (gutter.ToString(), body.ToString(), gitBlameDisplays);
        }

        private void BuildAuthorLine(GitBlameLine line, StringBuilder lineBuilder, string dateTimeFormat,
            string? filename, bool showAuthor, bool showAuthorDate, bool showOriginalFilePath, bool displayAuthorFirst)
        {
            if (showAuthor && displayAuthorFirst)
            {
                lineBuilder.Append(line.Commit.Author);
                if (showAuthorDate)
                {
                    lineBuilder.Append(" - ");
                }
            }

            if (showAuthorDate)
            {
                lineBuilder.Append(line.Commit.AuthorTime.ToString(dateTimeFormat));
            }

            if (showAuthor && !displayAuthorFirst)
            {
                if (showAuthorDate)
                {
                    lineBuilder.Append(" - ");
                }

                lineBuilder.Append(line.Commit.Author);
            }

            if (showOriginalFilePath && filename != line.Commit.FileName)
            {
                lineBuilder.Append(" - ");
                lineBuilder.Append(line.Commit.FileName);
            }
        }

        private static IList<Color> GetAgeBucketGradientColors()
        {
            // Color chosen from: https://colorbrewer2.org/#type=sequential&scheme=Greens&n=7
            return new[]
            {
                Color.FromArgb(247, 252, 245),
                Color.FromArgb(199, 233, 192),
                Color.FromArgb(161, 217, 155),
                Color.FromArgb(116, 196, 118),
                Color.FromArgb(65, 171, 93),
                Color.FromArgb(35, 139, 69),
                Color.FromArgb(0, 68, 27),
            }.Select(ColorHelper.AdaptBackColor).ToList();
        }

        public DateTime ArtificialOldBoundary => DateTime.Now.AddYears(-3);

        private List<GitBlameEntry> CalculateBlameGutterData(IReadOnlyList<GitBlameLine> blameLines)
        {
            var mostRecentDate = DateTime.Now.Ticks;
            var artificialOldBoundary = ArtificialOldBoundary;
            List<GitBlameEntry> gitBlameDisplays = new(blameLines.Count);

            var lessRecentDate = Math.Min(artificialOldBoundary.Ticks,
                                          blameLines.Select(l => l.Commit.AuthorTime)
                                                    .Where(d => d != DateTime.MinValue)
                                                    .DefaultIfEmpty(artificialOldBoundary)
                                                    .Min()
                                                    .Ticks);
            var intervalSize = (mostRecentDate - lessRecentDate + 1) / AgeBucketGradientColors.Count;
            foreach (var blame in blameLines)
            {
                var relativeTicks = Math.Max(0, blame.Commit.AuthorTime.Ticks - lessRecentDate);
                var ageBucketIndex = Math.Min((int)(relativeTicks / intervalSize), AgeBucketGradientColors.Count - 1);
                GitBlameEntry gitBlameDisplay = new()
                {
                    AgeBucketIndex = ageBucketIndex,
                    AgeBucketColor = AgeBucketGradientColors[ageBucketIndex]
                };
                gitBlameDisplays.Add(gitBlameDisplay);
            }

            return gitBlameDisplays;
        }

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            if (_lastBlameLine is null)
            {
                return;
            }

            ObjectId selectedId = _lastBlameLine.Commit.ObjectId;
            if (_revGrid is null)
            {
                using FormCommitDiff frm = new(UICommands, selectedId);
                frm.ShowDialog(this);
                return;
            }

            _clickedBlameLine = _lastBlameLine;
            if (!_revGrid.SetSelectedRevision(selectedId))
            {
                MessageBoxes.RevisionFilteredInGrid(this, selectedId);
            }
        }

        private int GetBlameLine()
        {
            if (_blame is null)
            {
                return -1;
            }

            Point position = BlameAuthor.PointToClient(MousePosition);

            int line = BlameAuthor.GetLineFromVisualPosY(position.Y);
            if (line >= _blame.Lines.Count)
            {
                return -1;
            }

            return line;
        }

        private void contextMenu_Opened(object sender, EventArgs e)
        {
            Validates.NotNull(_fileName);
            Validates.NotNull(_blameId);

            contextMenu.Tag = new GitBlameContext(_fileName, _lineIndex, GetBlameLine(), _blameId);

            if (!TryGetSelectedRevision(out var blameinfo))
            {
                blameRevisionToolStripMenuItem.Enabled = false;

                // Ignore if current revision is not visible in grid but parent is.
                blamePreviousRevisionToolStripMenuItem.Enabled = false;
                return;
            }

            blameRevisionToolStripMenuItem.Enabled = true;
            blamePreviousRevisionToolStripMenuItem.Enabled = RevisionHasParent(blameinfo.selectedRevision);

            // Get parent for the actual revision, the selected revision may have rewritten parents.
            // The menu will be slightly slower in this situation.
            bool RevisionHasParent(GitRevision? selectedRevision)
            {
                GitRevision actualRevision = _revGrid?.GetActualRevision(selectedRevision);
                return (actualRevision?.HasParent ?? false) && (_revGrid?.GetRevision(actualRevision?.FirstParentId) is not null);
            }
        }

        private GitBlameCommit? GetBlameCommit()
        {
            int line = (contextMenu.Tag as GitBlameContext)?.BlameLine ?? -1;
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
            var commit = GetBlameCommit();

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

        private bool TryGetSelectedRevision([NotNullWhen(returnValue: true)] out (GitRevision? selectedRevision, string? filename) blameInfo)
        {
            var blameCommit = GetBlameCommit();
            if (blameCommit is null)
            {
                blameInfo = (null, null);
                return false;
            }

            blameInfo = (_revGrid?.GetRevision(blameCommit.ObjectId), blameCommit.FileName);
            return blameInfo.selectedRevision is not null;
        }

        private void blameRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedRevision(out var blameInfo))
            {
                return;
            }

            BlameRevision(blameInfo.selectedRevision.ObjectId, blameInfo.filename);
        }

        private void blamePreviousRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedRevision(out var blameInfo))
            {
                return;
            }

            // Try get actual parent revision, get popup if it does not exist.
            // (The menu should be disabled if previous is not in grid).
            GitRevision? revision = _revGrid?.GetActualRevision(blameInfo.selectedRevision);
            BlameRevision(revision?.FirstParentId, blameInfo.filename);
        }

        /// <summary>
        /// Blame a specific revision
        /// </summary>
        /// <param name="revisionId">the commit id to blame</param>
        /// <param name="filename">the relative path of the file to blame in this commit (because it could have been renamed)</param>
        private void BlameRevision(ObjectId? revisionId, string filename)
        {
            if (_revGrid is not null)
            {
                PathToBlame = filename;
                if (!_revGrid.SetSelectedRevision(revisionId))
                {
                    MessageBoxes.RevisionFilteredInGrid(this, revisionId);
                }

                return;
            }

            using FormCommitDiff frm = new(UICommands, revisionId);
            frm.ShowDialog(this);
        }

        private void showChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var commit = GetBlameCommit();

            if (commit is null)
            {
                return;
            }

            using FormCommitDiff frm = new(UICommands, commit.ObjectId);
            frm.ShowDialog(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _blameLoader.Dispose();
            }

            base.Dispose(disposing);
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

            public DateTime ArtificialOldBoundary => _control.ArtificialOldBoundary;

            public void BuildAuthorLine(GitBlameLine line, StringBuilder lineBuilder, string dateTimeFormat, string filename, bool showAuthor, bool showAuthorDate, bool showOriginalFilePath, bool displayAuthorFirst)
                => _control.BuildAuthorLine(line, lineBuilder, dateTimeFormat, filename, showAuthor, showAuthorDate, showOriginalFilePath, displayAuthorFirst);

            public (string gutter, string body, List<GitBlameEntry> avatars) BuildBlameContents(string filename) => _control.BuildBlameContents(filename, avatarSize: 10);

            public List<GitBlameEntry> CalculateBlameGutterData(IReadOnlyList<GitBlameLine> blameLines)
                => _control.CalculateBlameGutterData(blameLines);
        }
    }
}
