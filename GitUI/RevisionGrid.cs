using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.Drawing.Drawing2D;
using System.IO;

namespace GitUI
{
    public partial class RevisionGrid : UserControl
    {
        public event EventHandler ChangedCurrentBranch;

        public virtual void OnChangedCurrentBranch()
        {
            if (ChangedCurrentBranch != null)
                ChangedCurrentBranch(this, null);
        }


        IndexWatcher indexWatcher = new IndexWatcher();

        public RevisionGrid()
        {
            base.InitLayout();
            InitializeComponent();

            NormalFont = Revisions.Font;
            HeadFont = new Font(NormalFont, FontStyle.Bold);
            //RefreshRevisions();
            Revisions.CellPainting += new DataGridViewCellPaintingEventHandler(Revisions_CellPainting);
            Revisions.SizeChanged += new EventHandler(Revisions_SizeChanged);
            
            showRevisionGraphToolStripMenuItem.Checked = Settings.ShowRevisionGraph;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;

            SetShowBranches();
            filter = "";
        }

        public string LogParam = "HEAD --all --boundary";

        private string filter;
        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
            }
        }

        public string FormatQuickFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return "";
            else
                return " --regexp-ignore-case --grep=\"" + filter + "\" --committer=\"" + filter + "\" --author=\"" + filter + "\" ";
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
            if (Revisions.SelectedRows.Count > 0)
                LastRow = Revisions.SelectedRows[0].Index;

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
            if (indexWatcher.IndexChanged)
                ForceRefreshRevisions();
        }

        public int LastScrollPos = 0;
        public List<int> LastSelectedRows = new List<int>();

        public void ForceRefreshRevisions()
        {
            try
            {
                LastScrollPos = Revisions.FirstDisplayedScrollingRowIndex;
                LastSelectedRows.Clear();

                foreach (DataGridViewRow row in Revisions.SelectedRows)
                {
                    LastSelectedRows.Add(row.Index);
                }

                if (Settings.ShowRevisionGraph)
                    Revisions.Columns[0].Width = 150;
                else
                    Revisions.Columns[0].Width = 0;


                Error.Visible = false;
                NoCommits.Visible = false;
                NoGit.Visible = false;
                Revisions.Visible = true;

                if (!GitCommands.Settings.ValidWorkingDir())
                {
                    Revisions.RowCount = 0;
                    Revisions.ScrollBars = ScrollBars.None;
                    Revisions.Visible = false;

                    NoCommits.Visible = true;
                    NoGit.Visible = true;
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
                Revisions.RowCount = 0;
                Revisions.RowCount = Math.Max(Revisions.DisplayedRowCount(true), GitCommands.Settings.MaxCommits);

                currentCheckout = GitCommands.GitCommands.GetCurrentCheckout();
                ScrollBarSet = false;
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
            Error.Visible = false;
            NoCommits.Visible = false;
            NoGit.Visible = false;
            Revisions.Visible = true;

            if (!GitCommands.Settings.ValidWorkingDir())
            {
                Revisions.RowCount = 0;
                Revisions.ScrollBars = ScrollBars.None;
                Revisions.Visible = false;

                NoCommits.Visible = true;
                NoGit.Visible = true;
                Loading.Visible = false;
                return;
            }

            int numberOfVisibleRows = Revisions.DisplayedRowCount(true) + 1;
            int firstVisibleRow = Revisions.FirstDisplayedScrollingRowIndex;

            if (numberOfVisibleRows < 1)
                numberOfVisibleRows = 20;

            if (LastRevision >= Math.Min(Revisions.RowCount, firstVisibleRow + numberOfVisibleRows))
            {
                return;
            }

            LastRevision = Math.Min(Revisions.RowCount, Math.Max(LastScrollPos + numberOfVisibleRows, Math.Max(firstVisibleRow + numberOfVisibleRows, Math.Max(GitCommands.Settings.MaxCommits, LastRevision * 2))));

            Revisions.Enabled = false;
            Loading.Visible = true;
            indexWatcher.Reset();
            revisionGraphCommand = new RevisionGraph();

            revisionGraphCommand.LogParam = LogParam + Filter;
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
            if (RevisionList == null)
            {
                return;
            }

            if (RevisionList != null && (RevisionList.Count == 0 && string.IsNullOrEmpty(Filter)))
            {
                Loading.Visible = false;
                NoCommits.Visible = true;
                Revisions.Visible = false;
                return;
            }
            Revisions.SuspendLayout();

            if (!ScrollBarSet)
            {
                ScrollBarSet = true;
                Revisions.ScrollBars = ScrollBars.None;
                Revisions.RowCount = RevisionList.Count;
                Revisions.ScrollBars = ScrollBars.Vertical;
                
                if (RevisionList.Count >= GitCommands.Settings.MaxCommits)
                {
                    string grep = "";
                    if (!string.IsNullOrEmpty(Filter))
                        grep = " --grep=\"" + Filter + "\" ";

                    gitCountCommitsCommand = new GitCommands.GitCommands();
                    gitCountCommitsCommand.CmdStartProcess("cmd.exe", "/c \"\"" + Settings.GitDir + "git.cmd\" rev-list " + grep + LogParam + " | \"" + Settings.GitBinDir + "wc.exe\" -l\"");
                    gitCountCommitsCommand.Exited += new EventHandler(gitCountCommitsCommand_Exited);
                }
                
            }

            Revisions.SelectionChanged -= new EventHandler(Revisions_SelectionChanged);

            if (LastScrollPos > 0 && Revisions.RowCount > LastScrollPos)
            {
                Revisions.FirstDisplayedScrollingRowIndex = LastScrollPos;
                LastScrollPos = -1;
            }

            if (LastSelectedRows.Count > 0)
            {
                Revisions.ClearSelection();

                if (Revisions.Rows.Count > LastSelectedRows[0])
                    Revisions.CurrentCell = Revisions.Rows[LastSelectedRows[0]].Cells[0];

                foreach (int row in LastSelectedRows)
                {
                    if (Revisions.Rows.Count > row)
                    {
                        Revisions.Rows[row].Selected = true;
                    }
                }
                LastSelectedRows.Clear();
            }

            DrawVisibleGraphPart();

            Loading.Visible = false;
            Revisions.Enabled = true;
            Revisions.Focus();
            Revisions.SelectionChanged += new EventHandler(Revisions_SelectionChanged);

            Revisions.ResumeLayout();
        }

        private bool skipFirst = false;
        private void DrawVisibleGraphPart()
        {
            int height = Revisions.RowTemplate.Height;
            int width = 6;
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

            Pen linePen = new Pen(Color.Red, 1);
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
                                    graph.DrawLine(linePen, hcenter, vcenter, hcenter, bottom);
                                else
                                    if (nextLine != null && nextLine.Length > nc && nextLine[nc] == '|')
                                        graph.DrawLine(linePen, hcenter, vcenter, hcenter, bottom + (height / 2));
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
                                        graph.DrawLine(linePen, left - (width / 2), vcenter, left - (width / 2), bottom + (height / 2));
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
                                    graph.DrawLine(linePen, left - width, bottom, right, bottom);
                                }
                                else
                                {
                                    // draw: \_
                                    graph.DrawLine(linePen, left - (width / 2), vcenter, left, bottom);
                                    graph.DrawLine(linePen, left, bottom, right, bottom);
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
                                        graph.DrawLine(linePen, left - (width / 2), vcenter, left - (width / 2), bottom + (height / 2));
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
                                    graph.DrawLine(linePen, left - width, bottom, right, bottom);
                                }
                                else
                                {
                                    //draw:  _
                                    //      /
                                    graph.DrawLine(linePen, left - (width / 2), bottom + (height / 2), left, bottom);
                                    graph.DrawLine(linePen, left, bottom, right, bottom);
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
                                graph.DrawLine(linePen, hcenter, top + (height / 2), hcenter, vcenter + (height / 2));
                            }
                            if ((nextChar == '|' && currentChar == '|') || (nextChar == '*' && currentChar == '|'))
                            {
                                graph.DrawLine(linePen, hcenter, vcenter + (height / 2), hcenter, bottom + (height / 2));
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
                                if ((h.IsRemote && !ShowRemoteBranches.Checked) == false)
                                {
                                    SolidBrush brush = new SolidBrush(h.IsTag == true ? Color.DarkBlue : h.IsHead ? Color.DarkRed : h.IsRemote ? Color.Green : Color.Gray);

                                    e.Graphics.DrawString("[" + h.Name + "] ", HeadFont, brush, new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));

                                    offset += e.Graphics.MeasureString("[" + h.Name + "] ", HeadFont).Width;
                                }
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
                GitUICommands.Instance.StartCompareRevisionsDialog();
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

        private void createTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RevisionList.Count > LastRow && LastRow >= 0)
            {
                FormTagSmall frm = new FormTagSmall();
                frm.Revision = (GitRevision)RevisionList[LastRow];
                frm.ShowDialog();
                RefreshRevisions();

            }
        }

        private void resetCurrentBranchToHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RevisionList.Count > LastRow && LastRow >= 0)
            {
                FormResetCurrentBranch frm = new FormResetCurrentBranch((GitRevision)RevisionList[LastRow]);
                frm.ShowDialog();
                RefreshRevisions();
            }
        }

        private void createNewBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RevisionList.Count > LastRow && LastRow >= 0)
            {
                FormBranchSmall frm = new FormBranchSmall();
                frm.Revision = (GitRevision)RevisionList[LastRow];
                frm.ShowDialog();
                RefreshRevisions();
                OnChangedCurrentBranch();
            }

        }

        private void Revisions_CellContextMenuStripChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        public int LastRow { get; set; }

        private void Revisions_MouseClick(object sender, MouseEventArgs e)
        {
            System.Drawing.Point pt = Revisions.PointToClient(Cursor.Position);
            DataGridView.HitTestInfo hti = Revisions.HitTest(pt.X, pt.Y);
            LastRow = hti.RowIndex;
        }

        private void Revisions_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Point pt = Revisions.PointToClient(Cursor.Position);
                DataGridView.HitTestInfo hti = Revisions.HitTest(pt.X, pt.Y);
                LastRow = hti.RowIndex;
                Revisions.ClearSelection();
                if (LastRow >= 0 && Revisions.Rows.Count > LastRow)
                    Revisions.Rows[LastRow].Selected = true;
            }
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartAddFilesDialog();
        }

        private void Commit_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartCommitDialog();
            RefreshRevisions();
        }

        private void GitIgnore_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartEditGitIgnoreDialog();
        }


        private void ShowRemoteBranches_Click(object sender, EventArgs e)
        {
            ShowRemoteBranches.Checked = !ShowRemoteBranches.Checked;
            Revisions.Invalidate();
        }

        private void showAllBranchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.ShowAllBranches = !showAllBranchesToolStripMenuItem.Checked;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        private void SetShowBranches()
        {
            showAllBranchesToolStripMenuItem.Checked = Settings.ShowAllBranches;
            if (Settings.ShowAllBranches)
                LogParam = "HEAD --all --boundary";
            else
                LogParam = "HEAD";

        }

        private void revertCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RevisionList.Count > LastRow && LastRow >= 0)
            {
                FormRevertCommitSmall frm = new FormRevertCommitSmall((GitRevision)RevisionList[LastRow]);
                frm.ShowDialog();
                RefreshRevisions();
            }
        }


        private void showRevisionGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.ShowRevisionGraph = !showRevisionGraphToolStripMenuItem.Checked;
            showRevisionGraphToolStripMenuItem.Checked = Settings.ShowRevisionGraph;
            this.ForceRefreshRevisions();
        }

        private FormRevisionFilter RevisionFilter = null;
        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RevisionFilter == null)
                RevisionFilter  = new FormRevisionFilter();

            RevisionFilter.ShowDialog();
            filter = RevisionFilter.GetFilter();
            ForceRefreshRevisions();
        }

        private void Revisions_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
                filterToolStripMenuItem_Click(null, null);
        }

        private void CreateTag_Opening(object sender, CancelEventArgs e)
        {
            if (RevisionList.Count < LastRow || LastRow < 0)
                return;

            GitRevision revision = RevisionList[LastRow] as GitRevision;

            ToolStripDropDown tagDropDown = new ToolStripDropDown();
            ToolStripDropDown branchDropDown = new ToolStripDropDown();
            ToolStripDropDown checkoutBranchDropDown = new ToolStripDropDown();

            foreach (GitHead head in revision.Heads)
            {
                if (head.IsTag)
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += new EventHandler(toolStripItem_Click);
                    tagDropDown.Items.Add(toolStripItem);
                } else
                if (head.IsHead && !head.IsRemote)
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += new EventHandler(toolStripItem_ClickBranch);
                    branchDropDown.Items.Add(toolStripItem);

                    toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click +=new EventHandler(toolStripItem_ClickCheckoutBranch);
                    checkoutBranchDropDown.Items.Add(toolStripItem);
                }
            }

            deleteTagToolStripMenuItem.DropDown = tagDropDown;
            deleteTagToolStripMenuItem.Visible = tagDropDown.Items.Count > 0;

            deleteBranchToolStripMenuItem.DropDown = branchDropDown;
            deleteBranchToolStripMenuItem.Visible = branchDropDown.Items.Count > 0;

            checkoutBranchToolStripMenuItem.DropDown = checkoutBranchDropDown;
            checkoutBranchToolStripMenuItem.Visible = checkoutBranchDropDown.Items.Count > 0;
        }

        void toolStripItem_Click(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            new FormProcess(GitCommands.GitCommands.DeleteTagCmd(toolStripItem.Text));
            ForceRefreshRevisions();
        }

        void toolStripItem_ClickBranch(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartDeleteBranchDialog();

            ForceRefreshRevisions();
        }

        void toolStripItem_ClickCheckoutBranch(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            new FormProcess("checkout \"" + toolStripItem.Text + "\"");

            ForceRefreshRevisions();
            OnChangedCurrentBranch();
        }
        private void deleteTagToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void checkoutRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RevisionList.Count > LastRow && LastRow >= 0)
            {
                if (MessageBox.Show("Are you sure to checkout the selected revision", "Checkout revision", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    new FormProcess("checkout \"" + RevisionList[LastRow].Guid + "\"");
                    ForceRefreshRevisions();
                    OnChangedCurrentBranch();
                }
            }
        }

        private void orderRevisionsByDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.OrderRevisionByDate = !orderRevisionsByDateToolStripMenuItem.Checked;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;
            this.ForceRefreshRevisions();
        }

        private void deleteBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void checkoutBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}
