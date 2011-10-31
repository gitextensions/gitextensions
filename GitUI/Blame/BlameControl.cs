using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.Blame
{
    public partial class BlameControl : GitExtensionsControl
    {
        private GitBlame _blame;
        private string _lastRevision;
        private RevisionGrid _revGrid;

        public BlameControl()
        {
            InitializeComponent();
            Translate();

            BlameCommitter.IsReadOnly = true;
            BlameCommitter.EnableScrollBars(false);
            BlameCommitter.ShowLineNumbers = false;
            BlameCommitter.DisableFocusControlOnHover = true;
            BlameCommitter.ScrollPosChanged += BlameCommitter_ScrollPosChanged;
            BlameCommitter.MouseMove += new MouseEventHandler(BlameCommitter_MouseMove);
            BlameCommitter.MouseLeave += new EventHandler(BlameCommitter_MouseLeave);

            BlameFile.IsReadOnly = true;
            BlameFile.ScrollPosChanged += BlameFile_ScrollPosChanged;
            BlameFile.SelectedLineChanged += BlameFile_SelectedLineChanged;
            BlameFile.RequestDiffView += ActiveTextAreaControlDoubleClick;
            BlameFile.MouseMove += new MouseEventHandler(BlameFile_MouseMove);
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
            if (selectedLine >= _blame.Lines.Count)
                return;

            var newRevision = _blame.Lines[selectedLine].CommitGuid;

            if (_lastRevision == newRevision)
                return;

            _lastRevision = newRevision;
            commitInfo.SetRevision(_lastRevision);
        }

        bool bChangeScrollPosition = false;

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

        public void LoadBlame(string guid, string fileName, RevisionGrid revGrid)
        {
            var scrollpos = BlameFile.ScrollPos;

            var blameCommitter = new StringBuilder();
            var blameFile = new StringBuilder();
            _revGrid = revGrid;

            _blame = Settings.Module.Blame(fileName, guid);

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
                    blameCommitter.AppendLine((blameHeader.Author + " - " + blameHeader.AuthorTime.ToString() + " - " + blameHeader.FileName + new string(' ', 100)).Trim(new char[] { '\r', '\n' }));
                }
                if (blameLine.LineText == null )
                    blameFile.AppendLine("");
                else
                    blameFile.AppendLine(blameLine.LineText.Trim(new char[] { '\r', '\n' }));
                }

            BlameCommitter.ViewText("committer.txt", blameCommitter.ToString());
            BlameFile.ViewText(fileName, blameFile.ToString());
            BlameFile.ScrollPos = scrollpos;

            BlameFile_SelectedLineChanged(null, 0);
        }

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            if (_revGrid != null)
            {
                _revGrid.SetSelectedRevision(new GitRevision(_lastRevision) { ParentGuids = new[] { _lastRevision + "^" } });
            }
            else
            {
                var frm = new FormDiffSmall();
                frm.SetRevision(_lastRevision);
                frm.ShowDialog();
            }
        }
    }
}
