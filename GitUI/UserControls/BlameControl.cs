using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommitInfo;
using GitUI.Editor;
using GitUI.HelperDialogs;

namespace GitUI.Blame
{
    public sealed partial class BlameControl : GitModuleControl
    {
        private GitBlame _blame;
        private GitBlameLine _lastBlameLine = new GitBlameLine();
        private GitBlameLine _clickedBlameLine = new GitBlameLine();
        private RevisionGrid _revGrid;
        private string _blameHash;
        private string _fileName;
        private Encoding _encoding;

        public BlameControl()
        {
            InitializeComponent();
            Translate();

            BlameCommitter.IsReadOnly = true;
            BlameCommitter.EnableScrollBars(false);
            BlameCommitter.ShowLineNumbers = false;
            BlameCommitter.ScrollPosChanged += BlameCommitter_ScrollPosChanged;
            BlameCommitter.MouseMove += BlameCommitter_MouseMove;
            BlameCommitter.MouseLeave += BlameCommitter_MouseLeave;
            BlameCommitter.SelectedLineChanged += SelectedLineChanged;
            BlameCommitter.RequestDiffView += ActiveTextAreaControlDoubleClick;

            BlameFile.IsReadOnly = true;
            BlameFile.ScrollPosChanged += BlameFile_ScrollPosChanged;
            BlameFile.SelectedLineChanged += SelectedLineChanged;
            BlameFile.RequestDiffView += ActiveTextAreaControlDoubleClick;
            BlameFile.MouseMove += BlameFile_MouseMove;

            CommitInfo.CommandClick += commitInfo_CommandClick;
        }

        private void commitInfo_CommandClick(object sender, CommandEventArgs e)
        {
            if (CommandClick != null)
                CommandClick(sender, e);
        }

        public event EventHandler<CommandEventArgs> CommandClick;

        void BlameCommitter_MouseLeave(object sender, EventArgs e)
        {
            blameTooltip.Hide(this);
        }

        int lastTooltipX = -100;
        int lastTooltipY = -100;
        string lastTooltip = "";
        void BlameCommitter_MouseMove(object sender, MouseEventArgs e)
        {
            if (!BlameFile.Focused)
                BlameFile.Focus();

            if (_blame == null)
                return;

            int line = BlameCommitter.GetLineFromVisualPosY(e.Y);

            if (line >= _blame.Lines.Count)
                return;

            GitBlameHeader blameHeader = _blame.FindHeaderForCommitGuid(_blame.Lines[line].CommitGuid);

            string tooltipText = blameHeader.ToString();

            int newTooltipX = splitContainer2.SplitterDistance + 60;
            int newTooltipY = e.Y + splitContainer1.SplitterDistance + 20;

            if (lastTooltip != tooltipText || Math.Abs(lastTooltipX - newTooltipX) > 5 || Math.Abs(lastTooltipY - newTooltipY) > 5)
            {
                lastTooltip = tooltipText;
                lastTooltipX = newTooltipX;
                lastTooltipY = newTooltipY;
                blameTooltip.Show(tooltipText, this, newTooltipX, newTooltipY);
            }
        }

        GitBlameHeader _lastBlameHeader;

        void BlameFile_MouseMove(object sender, MouseEventArgs e)
        {
            if (_blame == null)
                return;

            int line = BlameFile.GetLineFromVisualPosY(e.Y);

            if (line >= _blame.Lines.Count)
                return;

            GitBlameHeader blameHeader = _blame.FindHeaderForCommitGuid(_blame.Lines[line].CommitGuid);

            if (blameHeader != _lastBlameHeader)
            {
                BlameCommitter.ClearHighlighting();
                BlameFile.ClearHighlighting();
                int startLine = -1;
                int prevLine = -1;
                for (int i = 0; i < _blame.Lines.Count; i++)
                {
                    if (_blame.Lines[i].CommitGuid == blameHeader.CommitGuid)
                    {
                        if (prevLine != i - 1 && startLine != -1)
                        {
                            BlameCommitter.HighlightLines(startLine, prevLine, Color.FromArgb(225, 225, 225));
                            BlameFile.HighlightLines(startLine, prevLine, Color.FromArgb(225, 225, 225));
                            startLine = -1;
                        }

                        prevLine = i;
                        if (startLine == -1)
                            startLine = i;
                    }
                }
                if (startLine != -1)
                {
                    BlameCommitter.HighlightLines(startLine, prevLine, Color.FromArgb(225, 225, 225));
                    BlameFile.HighlightLines(startLine, prevLine, Color.FromArgb(225, 225, 225));
                }
                BlameCommitter.Refresh();
                BlameFile.Refresh();
                _lastBlameHeader = blameHeader;
            }
        }

        void SelectedLineChanged(object sender, SelectedLineEventArgs e)
        {
            int selectedLine = e.SelectedLine;
            if (_blame == null || selectedLine >= _blame.Lines.Count)
                return;

            //TODO: Request GitRevision from RevisionGrid that contain all commits
            var newBlameLine = _blame.Lines[selectedLine];
            if (_lastBlameLine.CommitGuid == newBlameLine.CommitGuid)
                return;

            _lastBlameLine = newBlameLine;
            CommitInfo.Revision = Module.GetRevision(_lastBlameLine.CommitGuid);
        }

        bool _bChangeScrollPosition;

        void BlameCommitter_ScrollPosChanged(object sender, EventArgs e)
        {
            if (!_bChangeScrollPosition)
            {
                _bChangeScrollPosition = true;
                SyncBlameFileView();
                _bChangeScrollPosition = false;
            }
            Rectangle rect = BlameCommitter.ClientRectangle;
            rect = BlameCommitter.RectangleToScreen(rect);
            if (rect.Contains(MousePosition))
            {
                Point p = BlameCommitter.PointToClient(MousePosition);
                MouseEventArgs me = new MouseEventArgs(0, 0, p.X, p.Y, 0);
                BlameCommitter_MouseMove(null, me);
            }
        }

        private void SyncBlameFileView()
        {
            BlameFile.ScrollPos = BlameCommitter.ScrollPos;
        }

        void BlameFile_ScrollPosChanged(object sender, EventArgs e)
        {
            if (_bChangeScrollPosition)
                return;
            _bChangeScrollPosition = true;
            SyncBlameCommitterView();
            _bChangeScrollPosition = false;
        }

        private void SyncBlameCommitterView()
        {
            BlameCommitter.ScrollPos = BlameFile.ScrollPos;
        }
        
        private AsyncLoader blameLoader = new AsyncLoader();

        public void LoadBlame(GitRevision revision, List<string> children, string fileName, RevisionGrid revGrid, Control controlToMask, Encoding encoding)
        {
            //refresh only when something changed
            string guid = revision.Guid;
            if (guid.Equals(_blameHash) && fileName == _fileName && revGrid == _revGrid && encoding == _encoding)
                return;

            if (controlToMask != null)
                controlToMask.Mask();

            var scrollpos = BlameFile.ScrollPos;

            int line = 0;
            if (_clickedBlameLine.CommitGuid == guid)
                line = _clickedBlameLine.OriginLineNumber;
            _revGrid = revGrid;
            _fileName = fileName;
            _encoding = encoding;

            blameLoader.Load(() => _blame = Module.Blame(fileName, guid, encoding),
                () => ProcessBlame(revision, children, controlToMask, line, scrollpos));
        }

        private void ProcessBlame(GitRevision revision, List<string> children, Control controlToMask, int line, int scrollpos)
        {
            var blameCommitter = new StringBuilder();
            var blameFile = new StringBuilder();
            for (int i = 0; i < _blame.Lines.Count; i++)
            {
                GitBlameLine blameLine = _blame.Lines[i];
                GitBlameHeader blameHeader = _blame.FindHeaderForCommitGuid(blameLine.CommitGuid);
                if (i > 0 && _blame.Lines[i - 1].CommitGuid == blameLine.CommitGuid)
                {
                    blameCommitter.AppendLine(new string(' ', 200));
                }
                else
                {
                    blameCommitter.AppendLine(
                        (blameHeader.Author + " - " + blameHeader.AuthorTime + " - " + blameHeader.FileName +
                         new string(' ', 100)).Trim(new[] {'\r', '\n'}));
                }
                if (blameLine.LineText == null)
                    blameFile.AppendLine("");
                else
                    blameFile.AppendLine(blameLine.LineText.Trim(new[] {'\r', '\n'}));
            }

            BlameCommitter.ViewText("committer.txt", blameCommitter.ToString());
            BlameFile.ViewText(_fileName, blameFile.ToString());
            if (line == 0)
                BlameFile.ScrollPos = scrollpos;
            else
                BlameFile.GoToLine(line);

            _clickedBlameLine = new GitBlameLine();

            _blameHash = revision.Guid;
            CommitInfo.SetRevisionWithChildren(revision, children);

            if (controlToMask != null)
                controlToMask.UnMask();
        }

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            if (_lastBlameLine.CommitGuid == null)
                return;
            if (_revGrid != null)
            {
                _clickedBlameLine = _lastBlameLine;
                _revGrid.SetSelectedRevision(_lastBlameLine.CommitGuid);
            }
            else
            {
                using (var frm = new FormCommitDiff(UICommands, _lastBlameLine.CommitGuid))
                    frm.ShowDialog(this);
            }
        }

        private int GetBlameLine()
        {
            if (_blame == null)
                return -1;

            Point position = BlameCommitter.PointToClient(MousePosition);

            int line = BlameCommitter.GetLineFromVisualPosY(position.Y);

            if (line >= _blame.Lines.Count)
                return -1;

            return line;
        }

        private void contextMenu_Opened(object sender, EventArgs e)
        {
            contextMenu.Tag = GetBlameLine();
        }

        private string GetBlameCommit()
        {
            int line = (int?)contextMenu.Tag ?? -1;

            if (line < 0)
                return null;

            return _blame.Lines[line].CommitGuid;
        }

        private void copyLogMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string commit = GetBlameCommit();
            if (commit == null)
                return;
            GitBlameHeader blameHeader = _blame.FindHeaderForCommitGuid(commit);
            Clipboard.SetText(blameHeader.Summary);
        }

        private void blamePreviousRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int line = (int?)contextMenu.Tag ?? -1;
            if (line < 0)
                return;
            string commit = _blame.Lines[line].CommitGuid;
            int originalLine = _blame.Lines[line].OriginLineNumber;
            GitBlame blame = Module.Blame(_fileName, commit + "^", originalLine + ",+1", _encoding);
            if (blame.Lines.Count > 0)
            {
                var revision = blame.Lines[0].CommitGuid;
                if (_revGrid != null)
                {
                    _clickedBlameLine = blame.Lines[0];
                    _revGrid.SetSelectedRevision(revision);
                }
                else
                {
                    using (var frm = new FormCommitDiff(UICommands, revision))
                        frm.ShowDialog(this);
                }
            }
        }

        private void showChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string commit = GetBlameCommit();
            if (commit == null)
                return;
            using (var frm = new FormCommitDiff(UICommands, commit))
                frm.ShowDialog(this);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                blameLoader.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
