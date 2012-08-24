﻿using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.Blame
{
    public sealed partial class BlameControl : GitExtensionsControl
    {
        private GitBlame _blame;
        private string _lastRevision;
        private RevisionGrid _revGrid;
        private string _fileName;

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

            BlameFile.IsReadOnly = true;
            BlameFile.ScrollPosChanged += BlameFile_ScrollPosChanged;
            BlameFile.SelectedLineChanged += BlameFile_SelectedLineChanged;
            BlameFile.RequestDiffView += ActiveTextAreaControlDoubleClick;
            BlameFile.MouseMove += BlameFile_MouseMove;
        }

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

        GitBlameHeader lastBlameHeader;

        void BlameFile_MouseMove(object sender, MouseEventArgs e)
        {
            if (_blame == null)
                return;

            int line = BlameFile.GetLineFromVisualPosY(e.Y);

            if (line >= _blame.Lines.Count)
                return;

            GitBlameHeader blameHeader = _blame.FindHeaderForCommitGuid(_blame.Lines[line].CommitGuid);

            if (blameHeader != lastBlameHeader)
            {
                BlameCommitter.ClearHighlighting();
                BlameFile.ClearHighlighting();
                for (int i = 0; i < _blame.Lines.Count; i++)
                {
                    if (_blame.Lines[i].CommitGuid == blameHeader.CommitGuid)
                    {
                        BlameCommitter.HighlightLine(i, Color.FromArgb(225, 225, 225));
                        BlameFile.HighlightLine(i, Color.FromArgb(225, 225, 225));
                    }
                }
                BlameCommitter.Refresh();
                BlameFile.Refresh();
                lastBlameHeader = blameHeader;
            }
        }

        void BlameFile_SelectedLineChanged(object sender, int selectedLine)
        {
            if (_blame == null || selectedLine >= _blame.Lines.Count)
                return;

            var newRevision = _blame.Lines[selectedLine].CommitGuid;

            if (_lastRevision == newRevision)
                return;

            _lastRevision = newRevision;
            commitInfo.SetRevision(_lastRevision);
        }

        bool bChangeScrollPosition;

        void BlameCommitter_ScrollPosChanged(object sender, EventArgs e)
        {
            if (!bChangeScrollPosition)
            {
                bChangeScrollPosition = true;
                SyncBlameFileView();
                bChangeScrollPosition = false;
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
            if (bChangeScrollPosition)
                return;
            bChangeScrollPosition = true;
            SyncBlameCommitterView();
            bChangeScrollPosition = false;
        }

        private void SyncBlameCommitterView()
        {
            BlameCommitter.ScrollPos = BlameFile.ScrollPos;
        }
        
        private AsyncLoader blameLoader = new AsyncLoader();

        public void LoadBlame(string guid, string fileName, RevisionGrid revGrid, Control controlToMask)
        {
            //refresh only when something changed
            if (guid.Equals(commitInfo.GetRevision()) && fileName.Equals(fileName) && revGrid == _revGrid)
                return;

            if (controlToMask != null)
                controlToMask.Mask();

            var scrollpos = BlameFile.ScrollPos;

            var blameCommitter = new StringBuilder();
            var blameFile = new StringBuilder();
            _revGrid = revGrid;
            _fileName = fileName;

            blameLoader.Load(() =>
            {
                _blame = GitModule.Current.Blame(fileName, guid);
            },
            () =>
            {

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
                        blameCommitter.AppendLine((blameHeader.Author + " - " + blameHeader.AuthorTime + " - " + blameHeader.FileName + new string(' ', 100)).Trim(new[] { '\r', '\n' }));
                    }
                    if (blameLine.LineText == null)
                        blameFile.AppendLine("");
                    else
                        blameFile.AppendLine(blameLine.LineText.Trim(new char[] { '\r', '\n' }));
                }

                BlameCommitter.ViewText("committer.txt", blameCommitter.ToString());
                BlameFile.ViewText(fileName, blameFile.ToString());
                BlameFile.ScrollPos = scrollpos;

                commitInfo.SetRevision(guid);

                if (controlToMask != null)
                    controlToMask.UnMask();
            });
        }

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            if (_lastRevision == null)
                return;
            var gitRevision = new GitRevision(_lastRevision) { ParentGuids = new[] { _lastRevision + "^" } };
            if (_revGrid != null)
            {
                _revGrid.SetSelectedRevision(gitRevision);
            }
            else
            {
                using (var frm = new FormDiffSmall(gitRevision))
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
            GitBlame blame = GitModule.Current.Blame(_fileName, commit + "^", line + ",+1");
            if (blame.Headers.Count > 0)
            {
                commit = blame.Headers[0].CommitGuid;
                var gitRevision = new GitRevision(commit) { ParentGuids = new[] { commit + "^" } };
                if (_revGrid != null)
                {
                    _revGrid.SetSelectedRevision(gitRevision);
                }
                else
                {
                    using (var frm = new FormDiffSmall(gitRevision))
                        frm.ShowDialog(this);
                }
            }
        }

        private void showChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string commit = GetBlameCommit();
            if (commit == null)
                return;
            var gitRevision = new GitRevision(commit) { ParentGuids = new[] { commit + "^" } };
            using (var frm = new FormDiffSmall(gitRevision))
                frm.ShowDialog(this);
        }
    }
}
