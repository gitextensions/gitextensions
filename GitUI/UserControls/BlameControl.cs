using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Avatars;
using GitUI.BranchTreePanel;
using GitUI.CommitInfo;
using GitUI.Editor;
using GitUI.HelperDialogs;
using GitUI.Properties;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using ICSharpCode.TextEditor;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.Blame
{
    public sealed partial class BlameControl : GitModuleControl
    {
        public event EventHandler<CommandEventArgs> CommandClick;

        /// <summary>
        /// Raised when the Escape key is pressed (and only when no selection exists, as the default behaviour of escape is to clear the selection).
        /// </summary>
        public event Action EscapePressed;

        private readonly AsyncLoader _blameLoader = new AsyncLoader();
        private int _lineIndex;

        [CanBeNull] private GitBlameLine _lastBlameLine;
        [CanBeNull] private GitBlameLine _clickedBlameLine;
        private GitBlameCommit _highlightedCommit;
        private GitBlame _blame;
        private RevisionGridControl _revGrid;
        [CanBeNull] private ObjectId _blameId;
        private string _fileName;
        private Encoding _encoding;
        private int _lastTooltipX = -100;
        private int _lastTooltipY = -100;
        private GitBlameCommit _tooltipCommit;
        private bool _changingScrollPosition;
        private IRepositoryHostPlugin _gitHoster;
        private static readonly IList<Color> AgeBucketGradientColors = GetAgeBucketGradientColors();

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
            BlameFile.RequestDiffView += ActiveTextAreaControlDoubleClick;
            BlameFile.MouseMove += BlameFile_MouseMove;
            BlameFile.EscapePressed += () => EscapePressed?.Invoke();

            CommitInfo.CommandClicked += commitInfo_CommandClicked;
        }

        public void ConfigureRepositoryHostPlugin(IRepositoryHostPlugin gitHoster)
        {
            _gitHoster = gitHoster;
            if (_gitHoster == null)
            {
                return;
            }

            _gitHoster.ConfigureContextMenu(contextMenu);
        }

        public void UpdateShowLineNumbers()
        {
            BlameAuthor.ShowLineNumbers = AppSettings.BlameShowLineNumbers;
        }

        public void LoadBlame(GitRevision revision, [CanBeNull] IReadOnlyList<ObjectId> children, string fileName, RevisionGridControl revGrid, Control controlToMask, Encoding encoding, int? initialLine = null, bool force = false)
        {
            var objectId = revision.ObjectId;

            // refresh only when something changed
            if (!force && objectId == _blameId && fileName == _fileName && revGrid == _revGrid && encoding == _encoding)
            {
                return;
            }

            controlToMask?.Mask();

            var scrollPos = BlameFile.VScrollPosition;

            var line = _clickedBlameLine != null && _clickedBlameLine.Commit.ObjectId == objectId
                ? _clickedBlameLine.OriginLineNumber
                : initialLine ?? 0;

            _revGrid = revGrid;
            _fileName = fileName;
            _encoding = encoding;

            _blameLoader.LoadAsync(() => _blame = Module.Blame(fileName, objectId.ToString(), encoding),
                () => ProcessBlame(fileName, revision, children, controlToMask, line, scrollPos));
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

            if (_blame == null)
            {
                return;
            }

            _lineIndex = BlameAuthor.GetLineFromVisualPosY(e.Y);

            var blameCommit = _lineIndex < _blame.Lines.Count
                ? _blame.Lines[_lineIndex].Commit
                : null;

            HighlightLinesForCommit(blameCommit);

            if (blameCommit == null)
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
                blameTooltip.Show(blameCommit.ToString(), this, newTooltipX, newTooltipY);
            }
        }

        private void BlameFile_MouseMove(object sender, MouseEventArgs e)
        {
            if (_blame == null)
            {
                return;
            }

            var lineIndex = BlameFile.GetLineFromVisualPosY(e.Y);

            var blameCommit = lineIndex < _blame.Lines.Count
                ? _blame.Lines[lineIndex].Commit
                : null;

            HighlightLinesForCommit(blameCommit);
        }

        private void HighlightLinesForCommit([CanBeNull] GitBlameCommit commit)
        {
            if (commit == _highlightedCommit)
            {
                return;
            }

            _highlightedCommit = commit;

            BlameAuthor.ClearHighlighting();
            BlameFile.ClearHighlighting();

            if (commit == null)
            {
                return;
            }

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

            if (_blame == null || selectedLine >= _blame.Lines.Count)
            {
                return;
            }

            // TODO: Request GitRevision from RevisionGrid that contain all commits
            var newBlameLine = _blame.Lines[selectedLine];

            if (ReferenceEquals(_lastBlameLine?.Commit, newBlameLine.Commit))
            {
                return;
            }

            _lastBlameLine = newBlameLine;
            CommitInfo.Revision = Module.GetRevision(_lastBlameLine.Commit.ObjectId);
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
                var me = new MouseEventArgs(0, 0, p.X, p.Y, 0);
                BlameAuthor_MouseMove(null, me);
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

        private void ProcessBlame(string filename, GitRevision revision, IReadOnlyList<ObjectId> children, Control controlToMask, int lineNumber, int scrollpos)
        {
            var avatarSize = BlameAuthor.Font.Height + 1;
            var (gutter, body, avatars) = BuildBlameContents(filename, avatarSize);

            BlameAuthor.SetGitBlameGutter(avatars);

            ThreadHelper.JoinableTaskFactory.RunAsync(
                () => BlameAuthor.ViewTextAsync("committer.txt", gutter));
            ThreadHelper.JoinableTaskFactory.RunAsync(
                () => BlameFile.ViewTextAsync(_fileName, body));

            if (lineNumber > 0)
            {
                BlameFile.GoToLine(lineNumber - 1);
            }
            else
            {
                BlameFile.VScrollPosition = scrollpos;
            }

            _clickedBlameLine = null;

            _blameId = revision.ObjectId;
            CommitInfo.SetRevisionWithChildren(revision, children);

            controlToMask?.UnMask();
        }

        private (string gutter, string body, List<GitBlameEntry> gitBlameDisplays) BuildBlameContents(string filename, int avatarSize)
        {
            var body = new StringBuilder(capacity: 4096);

            GitBlameCommit lastCommit = null;

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

            var filePathLengthEstimate = _blame.Lines.Where(l => filename != l.Commit.FileName)
                                                     .Select(l => l.Commit.FileName.Length)
                                                     .DefaultIfEmpty(0)
                                                     .Max();
            var lineLengthEstimate = 25 + _blame.Lines.Max(l => l.Commit.Author?.Length ?? 0) + filePathLengthEstimate;
            var lineLength = Math.Max(80, lineLengthEstimate);
            var lineBuilder = new StringBuilder(lineLength + 2);
            var gutter = new StringBuilder(capacity: lineBuilder.Capacity * _blame.Lines.Count);
            var emptyLine = new string(' ', lineLength);
            var cacheAvatars = new Dictionary<string, Image>();
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
                        if (authorEmail != null)
                        {
                            if (cacheAvatars.ContainsKey(authorEmail))
                            {
                                gitBlameDisplays[index].Avatar = cacheAvatars[authorEmail];
                            }
                            else
                            {
                                var avatar = ThreadHelper.JoinableTaskFactory.Run(() =>
                                    AvatarService.Default.GetAvatarAsync(authorEmail, line.Commit.Author,
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
            string filename, bool showAuthor, bool showAuthorDate, bool showOriginalFilePath, bool displayAuthorFirst)
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
            var gitBlameDisplays = new List<GitBlameEntry>(blameLines.Count);

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
                var gitBlameDisplay = new GitBlameEntry
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
            if (_lastBlameLine == null)
            {
                return;
            }

            if (_revGrid != null)
            {
                _clickedBlameLine = _lastBlameLine;
                _revGrid.SetSelectedRevision(_lastBlameLine.Commit.ObjectId);
            }
            else
            {
                using (var frm = new FormCommitDiff(UICommands, _lastBlameLine.Commit.ObjectId))
                {
                    frm.ShowDialog(this);
                }
            }
        }

        private int GetBlameLine()
        {
            if (_blame == null)
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
            contextMenu.Tag = new GitBlameContext(_fileName, _lineIndex, GetBlameLine(), _blameId);

            if (_revGrid == null || !TryGetSelectedRevision(out var selectedRevision))
            {
                blameRevisionToolStripMenuItem.Enabled = false;
                blamePreviousRevisionToolStripMenuItem.Enabled = false;
                return;
            }

            blameRevisionToolStripMenuItem.Enabled = true;
            blamePreviousRevisionToolStripMenuItem.Enabled = selectedRevision.HasParent;
        }

        [CanBeNull]
        private GitBlameCommit GetBlameCommit()
        {
            int line = (contextMenu.Tag as GitBlameContext)?.BlameLine ?? -1;
            if (line < 0)
            {
                return null;
            }

            return _blame.Lines[line].Commit;
        }

        private void copyLogMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyToClipboard(c => c.Summary);
        }

        private void CopyToClipboard(Func<GitBlameCommit, string> formatter)
        {
            var commit = GetBlameCommit();

            if (commit == null)
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

        private bool TryGetSelectedRevision(out GitRevision selectedRevision)
        {
            var blameCommit = GetBlameCommit();
            if (blameCommit == null)
            {
                selectedRevision = null;
                return false;
            }

            selectedRevision = _revGrid?.GetRevision(blameCommit.ObjectId);
            return selectedRevision != null;
        }

        private void blameRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedRevision(out var selectedRevision))
            {
                return;
            }

            BlameRevision(selectedRevision.ObjectId);
        }

        private void blamePreviousRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedRevision(out var selectedRevision) || !selectedRevision.HasParent)
            {
                return;
            }

            BlameRevision(selectedRevision.FirstParentId);
        }

        private void BlameRevision(ObjectId revisionId)
        {
            if (_revGrid != null)
            {
                _revGrid.SetSelectedRevision(revisionId);
                return;
            }

            using (var frm = new FormCommitDiff(UICommands, revisionId))
            {
                frm.ShowDialog(this);
            }
        }

        private void showChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var commit = GetBlameCommit();

            if (commit == null)
            {
                return;
            }

            using (var frm = new FormCommitDiff(UICommands, commit.ObjectId))
            {
                frm.ShowDialog(this);
            }
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
            => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly BlameControl _control;

            public TestAccessor(BlameControl control)
            {
                _control = control;
            }

            public GitBlame Blame
            {
                get => _control._blame;
                set => _control._blame = value;
            }

            public DateTime ArtificialOldBoundary => _control.ArtificialOldBoundary;

            public void BuildAuthorLine(GitBlameLine line, StringBuilder lineBuilder, string dateTimeFormat, string filename, bool showAuthor, bool showAuthorDate, bool showOriginalFilePath, bool displayAuthorFirst)
                => _control.BuildAuthorLine(line, lineBuilder, dateTimeFormat, filename, showAuthor, showAuthorDate, showOriginalFilePath, displayAuthorFirst);

            public (string gutter, string body, List<GitBlameEntry> avatars) BuildBlameContents(string filename) => _control.BuildBlameContents(filename, avatarSize: 10);

            public List<GitBlameEntry> CalculateBlameGutterData(IReadOnlyList<GitBlameLine> blameLines)
                => _control.CalculateBlameGutterData(blameLines);
        }
    }
}
