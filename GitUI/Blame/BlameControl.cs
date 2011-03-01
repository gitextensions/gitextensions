using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.Drawing;

namespace GitUI.Blame
{
    public partial class BlameControl : UserControl
    {
        private GitBlame _blame;
        private string _lastRevision;

        public BlameControl()
        {
            InitializeComponent();

            BlameCommitter.IsReadOnly = true;
            BlameCommitter.EnableScrollBars(false);
            BlameCommitter.ShowLineNumbers = false;
            BlameFile.ScrollPosChanged += BlameCommitter_ScrollPosChanged;
            BlameFile.IsReadOnly = true;
            BlameFile.SelectedLineChanged += BlameFile_SelectedLineChanged;

            BlameFile.RequestDiffView += ActiveTextAreaControlDoubleClick;

            BlameFile.MouseMove += new MouseEventHandler(BlameFile_MouseMove);
            BlameFile.MouseLeave += new EventHandler(BlameFile_MouseLeave);
            //BlameFile.MouseHover
        }

        void BlameFile_MouseLeave(object sender, EventArgs e)
        {
            blameTooltip.Hide(this);
        }

        int lastTooltipX = -100;
        int lastTooltipY = -100;
        string lastTooltip = "";
        void BlameFile_MouseMove(object sender, MouseEventArgs e)
        {
            if (_blame == null)
                return;

            int line = BlameFile.GetLineFromVisualPosY(e.Y);

            if (line >= _blame.Lines.Count)
                return;

            GitBlameHeader blameHeader = _blame.FindHeaderForCommitGuid(_blame.Lines[line].CommitGuid);

            string tooltipText = blameHeader.ToString();

            if (lastTooltip != tooltipText)
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
            }

            int newTooltipX = e.X + splitContainer2.SplitterDistance + 20;
            int newTooltipY = e.Y + splitContainer1.SplitterDistance + 20;

            if (lastTooltip != tooltipText || Math.Abs(lastTooltipX - newTooltipX) > 5 || Math.Abs(lastTooltipY - newTooltipY) > 5)
            {
                lastTooltip = tooltipText;
                lastTooltipX = newTooltipX;
                lastTooltipY = newTooltipY;
                //blameTooltip.Show(tooltipText, this, newTooltipX, newTooltipY);
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

        void BlameCommitter_ScrollPosChanged(object sender, EventArgs e)
        {
            SyncBlameViews();
        }

        public void LoadBlame(string guid, string fileName)
        {
            var scrollpos = BlameFile.ScrollPos;

            var blameCommitter = new StringBuilder();
            var blameFile = new StringBuilder();

            _blame = GitCommandHelpers.Blame(fileName, guid);

            for (int i = 0; i < _blame.Lines.Count; i++ )
            {
                GitBlameLine blameLine = _blame.Lines[i];
                GitBlameHeader blameHeader = _blame.FindHeaderForCommitGuid(blameLine.CommitGuid);
                if (i > 0 && _blame.Lines[i-1].CommitGuid == blameLine.CommitGuid)
                {
                    blameCommitter.AppendLine(new string(' ', 200));
                } else
                {
                    blameCommitter.AppendLine(blameHeader.Author + " - " + blameHeader.AuthorTime.ToString() + " - " + blameHeader.FileName + new string(' ', 100));
                }
                blameFile.AppendLine(blameLine.LineText);
            }

            BlameCommitter.ViewText("committer.txt", blameCommitter.ToString());
            BlameFile.ViewText(fileName, blameFile.ToString());
            BlameFile.ScrollPos = scrollpos;

            BlameFile_SelectedLineChanged(null, 0);
        }

        private void SyncBlameViews()
        {
            BlameCommitter.ScrollPos = BlameFile.ScrollPos;
        }

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            var frm = new FormDiffSmall();
            frm.SetRevision(_lastRevision);
            frm.ShowDialog();
        }
    }
}
