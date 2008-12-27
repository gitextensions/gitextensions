using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.Drawing.Drawing2D;

namespace GitUI
{
    public partial class RevisionGrid : UserControl
    {
        public RevisionGrid()
        {
            base.InitLayout();
            InitializeComponent();

            NormalFont = Revisions.Font;
            HeadFont = new Font(NormalFont, FontStyle.Bold);
            //RefreshRevisions();
            Revisions.CellPainting += new DataGridViewCellPaintingEventHandler(Revisions_CellPainting);
            Revisions.SizeChanged += new EventHandler(Revisions_SizeChanged);
        }

        void Revisions_SizeChanged(object sender, EventArgs e)
        {
            LoadRevisions();
            ScrollTimer.Enabled = false;
            ScrollTimer.Stop();
            ScrollTimer.Enabled = true;
            ScrollTimer.Start();
        }

        ~RevisionGrid()
        {
            if (revisionGraphCommand != null)
                revisionGraphCommand.Kill();

            if (gitCountCommitsCommand != null)
                gitCountCommitsCommand.Kill();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            RefreshRevisions();
        }

        public Font NormalFont { get; set; }
        public Font HeadFont { get; set; }

        public event EventHandler SelectionChanged;

        public void SetSelectedRevision(GitRevision revision)
        {
            /*
            Revisions.ClearSelection();

            if (revision != null)
                {
                    foreach (DataGridViewRow row in Revisions.Rows)
                    {
                        if (((GitRevision)row.DataBoundItem).Guid == revision.Guid)
                            row.Selected = true;
                    }
                }
            Revisions.Select();*/
        }

        void Revisions_SelectionChanged(object sender, EventArgs e)
        {
            SelecctionTimer.Enabled = false;
            SelecctionTimer.Stop();
            SelecctionTimer.Enabled = true;
            SelecctionTimer.Start();
        }

        public List<GitRevision> GetRevisions()
        {
            List<GitRevision> retval = new List<GitRevision>();
            if (RevisionList != null)
            foreach (DataGridViewRow row in Revisions.SelectedRows)
            {
                if (RevisionList.Count > row.Index) 
                    retval.Add(RevisionList[row.Index]);
            }
            return retval;
        }


        protected Bitmap graphImage;

        GitCommands.GitCommands gitCountCommitsCommand = null;// = new GitCommands.GitCommands();
        GitCommands.RevisionGraph revisionGraphCommand = null;//new RevisionGraph();
        private bool ScrollBarSet;

        public void RefreshRevisions()
        {
            try
            {
                Error.Visible = false;

                if (!GitCommands.Settings.ValidWorkingDir())
                {
                    Revisions.RowCount = 0;
                    Revisions.ScrollBars = ScrollBars.None;

                    Loading.Visible = false;
                    return;
                }

                if (revisionGraphCommand != null)
                {
                    revisionGraphCommand.Kill();
                }

                if (gitCountCommitsCommand != null)
                {
                    gitCountCommitsCommand.Kill();
                }

                LastRevision = 0;
                ScrollBarSet = false;
                Revisions.ClearSelection();
                Revisions.VirtualMode = true;
                //Revisions.ScrollBars = ScrollBars.None;
                Revisions.RowCount = Math.Max(Revisions.DisplayedRowCount(true), GitCommands.Settings.MaxCommits);
                    
                currentCheckout = GitCommands.GitCommands.GetCurrentCheckout();

                InternalRefresh();
            }
            catch (Exception exception)
            {
                Error.Visible = true;
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetRowCount()
        {
            int count;
            if (int.TryParse(gitCountCommitsCommand.Output.ToString(), out count))
            {
                ScrollBarSet = true;
                Revisions.ScrollBars = ScrollBars.None;
                Revisions.RowCount = count;
                Revisions.ScrollBars = ScrollBars.Vertical;
            }
        }

        private void gitCountCommitsCommand_Exited(object sender, EventArgs e)
        {
            if (Revisions.InvokeRequired)
            {
                DoneCallback d = new DoneCallback(SetRowCount);
                this.Invoke(d, new object[] { });
            }
        }

        private void InternalRefresh()
        {
            int numberOfVisibleRows = Revisions.DisplayedRowCount(true) + 1;
            int firstVisibleRow = Revisions.FirstDisplayedScrollingRowIndex;

            if (numberOfVisibleRows < 1)
                numberOfVisibleRows = 20;

            if (LastRevision >= Math.Min(Revisions.RowCount, firstVisibleRow + numberOfVisibleRows))
            {
                return;
            }

            LastRevision = Math.Min(Revisions.RowCount, Math.Max(firstVisibleRow + numberOfVisibleRows, Math.Max(GitCommands.Settings.MaxCommits, LastRevision * 2)));

            Revisions.Enabled = false;
            Loading.Visible = true;
            revisionGraphCommand = new RevisionGraph();
            revisionGraphCommand.Exited += new EventHandler(gitGetCommitsCommand_Exited);
            revisionGraphCommand.LimitRevisions = LastRevision;
            revisionGraphCommand.Execute();
        }

        void gitGetCommitsCommand_Exited(object sender, EventArgs e)
        {
            RevisionList = revisionGraphCommand.Revisions;

            if (Revisions.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                DoneCallback d = new DoneCallback(LoadRevisions);
                this.Invoke(d, new object[] { });
            }
        }

        public string currentCheckout { get; set; }
        public List<GitRevision> RevisionList;
        private int LastRevision = 0;

        private void LoadRevisions()
        {
            if (RevisionList == null || RevisionList.Count == 0)
                return;

            if (!ScrollBarSet)
            {
                ScrollBarSet = true;
                Revisions.SuspendLayout();
                Revisions.ScrollBars = ScrollBars.None;
                Revisions.RowCount = RevisionList.Count;
                Revisions.ScrollBars = ScrollBars.Vertical;
                
                if (RevisionList.Count >= GitCommands.Settings.MaxCommits)
                {
                    gitCountCommitsCommand = new GitCommands.GitCommands();
                    gitCountCommitsCommand.CmdStartProcess(Settings.GitDir + "C:\\Windows\\System32\\cmd.exe", "/c \"git.exe rev-list --all --abbrev-commit | wc -l\"");
                    gitCountCommitsCommand.Exited += new EventHandler(gitCountCommitsCommand_Exited);
                }
                Revisions.ResumeLayout();
            }

            Revisions.SelectionChanged -= new EventHandler(Revisions_SelectionChanged);

            DrawVisibleGraphPart();

            Loading.Visible = false;
            Revisions.Enabled = true;
            Revisions.Focus();
            Revisions.SelectionChanged += new EventHandler(Revisions_SelectionChanged);
        }

        private bool skipFirst = false;
        private void DrawVisibleGraphPart()
        {
            int height = Revisions.RowTemplate.Height;
            int width = 8;
            int y = -height;
            int numberOfVisibleRows = Revisions.DisplayedRowCount(true);
            int firstVisibleRow = Revisions.FirstDisplayedScrollingRowIndex;
            numberOfVisibleRows = Math.Min(/*20*/numberOfVisibleRows, RevisionList.Count);
            if (firstVisibleRow < 1)
            {
                skipFirst = false;
                firstVisibleRow = 0;
            }
            else
            {
                skipFirst = true;
                firstVisibleRow -= 1;
            }

            if (graphImage!=null)
                graphImage.Dispose();

            graphImage = new Bitmap(1000, (numberOfVisibleRows * height) + 50, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);//System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Graphics graph = Graphics.FromImage(graphImage);

            string lastlastLine = "";
            string lastLine = "";
            string currentLine = "";

            Pen linePen = new Pen(Color.Red, 2);
            SolidBrush blueBrush = new SolidBrush(Color.Blue);
            SolidBrush redBrush = new SolidBrush(Color.Red);
            char[] calc = new char[100];

            int nLine = 0;

            GitRevision revision = null;

            GitRevision prevRevision = null;
            GitRevision nextRevision = null;
            
            for (int r = firstVisibleRow; r < Math.Min(RevisionList.Count, firstVisibleRow + numberOfVisibleRows); r++)
            {
                revision = RevisionList[r];

                if (r > 0)
                    prevRevision = RevisionList[r - 1];
                else
                    prevRevision = null;

                if (RevisionList.Count > r + 1)
                    nextRevision = RevisionList[r + 1];
                else
                    nextRevision = null;

                y += height;

                for (int x = 0; x < 100; x++)
                {
                    calc[x] = '|';
                }

                for (int n = 0; n < revision.GraphLines.Count + 1; n++)
                {
                    string nextLine;

                    if (n < revision.GraphLines.Count)
                    {
                        nextLine = revision.GraphLines[n];
                    }
                    else
                    {
                        if (nextRevision != null)
                            nextLine = nextRevision.GraphLines[0];
                        else
                            nextLine = "";
                    }


                    nLine++;

                    int x = 0;
                    for (int nc = 0; nc < currentLine.Length && nc < 100; nc++)
                    {

                        x += width;

                        char c = currentLine[nc];
                        int top = y;
                        int bottom = y + height;
                        int left = x;
                        int right = x + width;
                        int hcenter = x + (width / 2);
                        int vcenter = y + (height / 2);

                        if (c == '*')
                        {
                            if (revision.Guid == currentCheckout)
                                graph.FillEllipse(blueBrush, hcenter - 5, vcenter - 5, 9, 9);
                            else
                                graph.FillEllipse(redBrush, hcenter - 4, vcenter - 4, 7, 7);

                            if (nextRevision != null && nextRevision.GraphLines[0].Length > nc && (nextRevision.GraphLines[0][nc] == '|' || nextRevision.GraphLines[0][nc] == '*'))
                            {
                                if (r == 0)
                                    graph.DrawLine(linePen, hcenter, vcenter, hcenter, bottom + 1);
                                else
                                    if (nextLine != null && nextLine.Length > nc && nextLine[nc] == '|')
                                        graph.DrawLine(linePen, hcenter, vcenter, hcenter, bottom + (height / 2) + 1);
                            }
                        }
                        if (c != '|' && c != '*')
                        {
                            calc[nc] = ' ';
                        }
                        if (c == '\\' && nc % 2 == 1)
                        {
                            if ((nextLine.Length > nc && nextLine[nc] == '/' || nextLine.Length <= nc) ||
                                (lastLine.Length > nc && lastLine[nc] == '/' || lastLine.Length <= nc))
                            {
                                if (lastLine.Length > nc && lastLine[nc] == '/' || lastLine.Length <= nc)
                                {
                                    if (nextLine.Length > nc + 1 && nextLine[nc + 1] == '|' || nextLine.Length <= nc + 1)
                                        graph.DrawLine(linePen, left - (width / 2), vcenter, left - (width / 2), bottom + (height / 2) + 1);
                                }
                            }
                            else
                            {
                                if ((nextLine.Length > nc + 2 && nextLine[nc + 2] != '\\') || nextLine.Length <= nc + 2)
                                {
                                    //draw: 
                                    //      \
                                    graph.DrawLine(linePen, right, bottom, right + (width / 2), bottom + (height / 2));
                                }
                                if (nc - 2 >= 0 && lastLine.Length > (nc - 2) && lastLine[nc - 2] == '\\')
                                {
                                    //draw: _
                                    graph.DrawLine(linePen, left - width, bottom, right + 1, bottom);
                                }
                                else
                                {
                                    // draw: \_
                                    graph.DrawLine(linePen, left - (width / 2), vcenter, left, bottom);
                                    graph.DrawLine(linePen, left, bottom, right + 1, bottom);
                                }
                            }
                        }
                        if (c == '/' && nc % 2 == 1)
                        {
                            if ((nextLine.Length > nc && nextLine[nc] == '\\' || nextLine.Length <= nc) ||
                                (lastLine.Length > nc && lastLine[nc] == '\\' || lastLine.Length <= nc))
                            {
                                if (lastLine.Length > nc && lastLine[nc] == '\\' || lastLine.Length <= nc)
                                {
                                    if (nextLine.Length > nc - 1 && nextLine[nc - 1] == '|' || nextLine.Length <= nc - 1)
                                        graph.DrawLine(linePen, left - (width / 2), vcenter, left - (width / 2), bottom + (height / 2) + 1);
                                }
                            }
                            else
                            {



                                if ((lastLine.Length > nc + 2 && lastLine[nc + 2] != '/' || lastLine.Length <= nc + 2) ||
                                    (lastLine.Length > nc + 2 && lastLine[nc + 2] == '/' &&
                                     lastlastLine.Length > nc + 2 && lastlastLine[nc + 2] == '\\'))
                                {
                                    //draw: /
                                    //      
                                    graph.DrawLine(linePen, right, bottom, right + (width / 2), bottom - (height / 2));
                                }
                                if (nc - 2 >= 0 && nextLine.Length > (nc - 2) && nextLine[nc - 2] == '/')
                                {
                                    //draw: _
                                    //      
                                    graph.DrawLine(linePen, left - width, bottom, right + 1, bottom);
                                }
                                else
                                {
                                    //draw:  _
                                    //      /
                                    graph.DrawLine(linePen, left - (width / 2), bottom + (height / 2), left, bottom);
                                    graph.DrawLine(linePen, left, bottom, right + 1, bottom);
                                }
                            }
                        }

                        if (n == revision.GraphLines.Count - 1)
                        {
                            char prevChar = ' ';
                            char currentChar = calc[nc];
                            char nextChar = ' ';

                            if (prevRevision != null && prevRevision.GraphLines.Count > 0 && prevRevision.GraphLines[prevRevision.GraphLines.Count - 1].Length > nc)
                                prevChar = prevRevision.GraphLines[prevRevision.GraphLines.Count - 1][nc];

                            if (nextRevision != null && nextRevision.GraphLines[0].Length > nc)
                                nextChar = nextRevision.GraphLines[0][nc];

                            if ((prevChar == '|' && currentChar == '|') || (prevChar == '|' && currentChar == '*'))
                            {
                                graph.DrawLine(linePen, hcenter, top + (height / 2), hcenter, vcenter + (height / 2) + 1);
                            }
                            if ((nextChar == '|' && currentChar == '|') || (nextChar == '*' && currentChar == '|'))
                            {
                                graph.DrawLine(linePen, hcenter, vcenter + (height / 2), hcenter, bottom + (height / 2) + 1);
                            }

                        }
                    }
                    lastlastLine = lastLine;
                    lastLine = currentLine;
                    currentLine = nextLine;
                }
            }
        }

        void Revisions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.State & DataGridViewElementStates.Visible) != 0)
            {
                if (RevisionList != null && RevisionList.Count > e.RowIndex)
                {
                    GitRevision revision = (GitRevision)RevisionList[e.RowIndex];

                    e.Handled = true;

                    if ((e.State & DataGridViewElementStates.Selected) != 0)
                        //e.Graphics.FillRectangle(new SolidBrush(Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor), e.CellBounds);
                        e.Graphics.FillRectangle(new LinearGradientBrush(e.CellBounds, Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor, Color.LightBlue, 90, false), e.CellBounds);
                    else
                        e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);

                    if (e.ColumnIndex == 0)
                    {
                        int top = ((e.RowIndex - Revisions.FirstDisplayedScrollingRowIndex) * Revisions.RowTemplate.Height);
                        if (skipFirst)
                            top += Revisions.RowTemplate.Height;

                        e.Graphics.DrawImage(graphImage, e.CellBounds, new Rectangle(0, top/* e.RowIndex * Revisions.RowTemplate.Height*/, e.CellBounds.Width, Revisions.RowTemplate.Height), GraphicsUnit.Pixel);
                    }
                    else
                        if (e.ColumnIndex == 1)
                        {

                            float offset = 0;
                            foreach (GitHead h in revision.Heads)
                            {
                                SolidBrush brush = new SolidBrush(h.IsTag == true ? Color.DarkBlue : h.IsHead ? Color.DarkRed : h.IsRemote ? Color.Green : Color.Gray);

                                e.Graphics.DrawString("[" + h.Name + "] ", HeadFont, brush, new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));

                                offset += e.Graphics.MeasureString("[" + h.Name + "] ", HeadFont).Width;
                            }
                            string text = (string)revision.Message;
                            e.Graphics.DrawString(text, NormalFont, new SolidBrush(Color.Black), new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));
                        }
                        else
                            if (e.ColumnIndex == 2)
                            {
                                string text = (string)revision.Author;
                                e.Graphics.DrawString(text, NormalFont, new SolidBrush(Color.Black), new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                            }
                            else
                                if (e.ColumnIndex == 3)
                                {
                                    string text = (string)revision.Date;
                                    e.Graphics.DrawString(text, NormalFont, new SolidBrush(Color.Black), new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                                }
                }
            }

        }

        private void Revisions_DoubleClick(object sender, EventArgs e)
        {
            List<GitRevision> r = GetRevisions();
            if (r.Count > 0)
            {
                FormDiffSmall form = new FormDiffSmall();
                form.SetRevision(r[0]);
                form.ShowDialog();
            }
            else
                new FormDiff().ShowDialog();
        }

        private void Revisions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        protected override void InitLayout()
        {
        }

        private void RevisionGrid_Load(object sender, EventArgs e)
        {

        }

        private void Revisions_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                LoadRevisions();
                ScrollTimer.Enabled = false;
                ScrollTimer.Stop();
                ScrollTimer.Enabled = true;
                ScrollTimer.Start();
            }
        }

        private void SelecctionTimer_Tick(object sender, EventArgs e)
        {
            SelecctionTimer.Enabled = false;
            SelecctionTimer.Stop();
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            ScrollTimer.Enabled = false;
            ScrollTimer.Stop();
            Revisions.InvalidateColumn(0);
            InternalRefresh();
        }
    }
}
