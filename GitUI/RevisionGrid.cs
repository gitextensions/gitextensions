using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using System.Drawing.Drawing2D;
using System.IO;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class RevisionGrid : GitExtensionsControl
    {
        TranslationString authorDate = new TranslationString("AuthorDate");
        TranslationString commitDate = new TranslationString("CommitDate");
        TranslationString graphCaption = new TranslationString("Graph");
        TranslationString messageCaption = new TranslationString("Message");
        TranslationString authorCaption = new TranslationString("Author");

        public event EventHandler ChangedCurrentBranch;

        public virtual void OnChangedCurrentBranch()
        {
            if (ChangedCurrentBranch != null)
                ChangedCurrentBranch(this, null);
        }

        private readonly SynchronizationContext syncContext;
        IndexWatcher indexWatcher = new IndexWatcher();

        public RevisionGrid()
        {
            syncContext = SynchronizationContext.Current;

            base.InitLayout();
            InitializeComponent(); Translate();
            Revisions.Columns[0].Width = 40;

            NormalFont = Revisions.Font;
            HeadFont = new Font(NormalFont, FontStyle.Bold);
            //RefreshRevisions();
            Revisions.CellPainting += new DataGridViewCellPaintingEventHandler(Revisions_CellPainting);
            Revisions.SizeChanged += new EventHandler(Revisions_SizeChanged);

            Revisions.KeyDown += new KeyEventHandler(Revisions_KeyDown);

            showRevisionGraphToolStripMenuItem.Checked = Settings.ShowRevisionGraph;
            showAuthorDateToolStripMenuItem.Checked = Settings.ShowAuthorDate;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;

            SetShowBranches();
            filter = "";
            quickSearchString = "";
            quickSearchTimer.Tick += new EventHandler(quickSearchTimer_Tick);
        }

        Label quickSearchLabel;
        private void ShowQuickSearchString()
        {
            if (quickSearchLabel == null)
            {
                quickSearchLabel = new Label();
                quickSearchLabel.Location = new Point(10, 10);
                quickSearchLabel.BorderStyle = BorderStyle.FixedSingle;
                quickSearchLabel.ForeColor = SystemColors.InfoText;
                quickSearchLabel.BackColor = SystemColors.Info;
                //quickSearchLabel.Size = new Size(200, 50);
                this.Controls.Add(quickSearchLabel);
            }

            quickSearchLabel.Visible = true;
            quickSearchLabel.BringToFront();
            quickSearchLabel.Text = quickSearchString;
            quickSearchLabel.AutoSize = true;
        }

        private void HideQuickSearchString()
        {
            if (quickSearchLabel != null)
                quickSearchLabel.Visible = false;
        }

        void quickSearchTimer_Tick(object sender, EventArgs e)
        {
            quickSearchTimer.Stop();
            quickSearchString = "";
            HideQuickSearchString();
        }

        private string quickSearchString;
        private string lastQuickSearchString = string.Empty;
        
        void Revisions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.Up)
            {
                int nextIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    nextIndex = Revisions.SelectedRows[0].Index - 1;

                FindNextMatch(nextIndex, lastQuickSearchString, true);
                e.Handled = true;
                return;
            }
            if (e.Alt && e.KeyCode == Keys.Down)
            {
                int nextIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    nextIndex = Revisions.SelectedRows[0].Index + 1;
                
                FindNextMatch(nextIndex, lastQuickSearchString, false);  
                e.Handled = true;
                return;  
            }
            char key = (char)e.KeyValue;
            if (char.IsLetterOrDigit(key) || char.IsNumber(key) || char.IsSeparator(key))
            {
                quickSearchTimer.Stop();
                quickSearchTimer.Interval = 700;
                quickSearchTimer.Start();

                quickSearchString = string.Concat(quickSearchString, (char)e.KeyValue).ToLower();

                int oldIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    oldIndex = Revisions.SelectedRows[0].Index;

                FindNextMatch(oldIndex, quickSearchString, false);
                lastQuickSearchString = quickSearchString;
                //System.Diagnostics.Debug.WriteLine(lastQuickSearchString);
                e.Handled = true;
                ShowQuickSearchString();
            }
            else
            {
                quickSearchString = "";
                HideQuickSearchString();
                return;
            }
        }

        private void FindNextMatch(int startIndex, string searchString, bool reverse)
        {
            if (RevisionList.Count == 0)
            {
                return;
            }
            
            Predicate<GitRevision> match = delegate(GitRevision r)
            {
                foreach (GitHead gitHead in r.Heads)
                {
                    if (gitHead.Name.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }

                //Make sure it only matches the start of a word
                string modifiedSearchString = " " + searchString;

                if ((" " + r.Author.ToLower()).Contains(modifiedSearchString))
                {
                    return true;
                }

                if ((" " + r.Message.ToLower()).Contains(modifiedSearchString))
                {
                    return true;
                }
                return false;
            };
            
            int index;
            
            if (reverse)
            {
                //Check for out of bounds roll over if required
                if (startIndex < 0 || startIndex >= RevisionList.Count)
                    startIndex = RevisionList.Count - 1;
                
                index = RevisionList.FindLastIndex(startIndex, match);
                
                if (index == -1)
                {
                    //We didn't find it so start searching from the bottom
                    int bottomIndex = RevisionList.Count - 1;
                    index = RevisionList.FindLastIndex(bottomIndex, bottomIndex - startIndex, match);
                }
            }
            else
            {
                //Check for out of bounds roll over if required
                if (startIndex < 0 || startIndex >= RevisionList.Count)
                    startIndex = 0;
                
                index = RevisionList.FindIndex(startIndex, match);

                if (index == -1)
                {
                    //We didn't find it so start searching from the top
                    index = RevisionList.FindIndex(0, startIndex, match);
                }
            }
            
            if (index > -1)
            {
                Revisions.ClearSelection();
                Revisions.Rows[index].Selected = true;

                Revisions.CurrentCell = Revisions.Rows[index].Cells[0];
            }
        }

        public string LogParam = "HEAD --all --boundary";

        private bool ContextMenuEnabled = true;
        public void DisableContextMenu()
        {
            Revisions.ContextMenuStrip = null;
            ContextMenuEnabled = false;
        }

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
            Revisions.ClearSelection();

            if (revision != null)
            {
                foreach (DataGridViewRow row in Revisions.Rows)
                {
                    if (((GitRevision)row.DataBoundItem).Guid == revision.Guid)
                        row.Selected = true;
                }
            }
            Revisions.Select();
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
            {
                ForceRefreshRevisions();
            }
        }

        public int LastScrollPos = 0;
        public List<int> LastSelectedRows = new List<int>();

        public void ForceRefreshRevisions()
        {
            try
            {
                initialLoad = true;
                graphWidth = 0;

                LastScrollPos = Revisions.FirstDisplayedScrollingRowIndex;
                LastSelectedRows.Clear();

                foreach (DataGridViewRow row in Revisions.SelectedRows)
                {
                    LastSelectedRows.Add(row.Index);
                }

                if (!Settings.ShowRevisionGraph)
                    //Revisions.Columns[0].Width = 1;
                    //else
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
            syncContext.Post(_ => SetRowCount(), null);
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
            syncContext.Post(_ => LoadRevisions(), null);
        }

        public string currentCheckout { get; set; }
        public List<GitRevision> RevisionList;
        private int LastRevision = 0;
        private bool initialLoad = true;

        private string GetDateHeaderText()
        {
            return Settings.ShowAuthorDate ? authorDate.Text : commitDate.Text;
        }

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

            Revisions.Columns[0].HeaderText = graphCaption.Text;
            Revisions.Columns[1].HeaderText = messageCaption.Text;
            Revisions.Columns[2].HeaderText = authorCaption.Text;
            Revisions.Columns[3].HeaderText = GetDateHeaderText();

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
                    gitCountCommitsCommand.CmdStartProcess("cmd.exe", "/c \"\"" + Settings.GitCommand + "\" rev-list " + grep + LogParam + " | \"" + Settings.GitBinDir + "wc\" -l\"");
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

            if (initialLoad)
            {
                initialLoad = false;
                SelecctionTimer.Enabled = false;
                SelecctionTimer.Stop();
                SelecctionTimer.Enabled = true;
                SelecctionTimer.Start();
            }
        }

        private bool skipFirst = false;
        private int graphWidth = 0;
        private void DrawVisibleGraphPart()
        {
            graphWidth = 0;
            int height = Revisions.RowTemplate.Height;
            int width = Settings.RevisionGraphWidth;
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

            if (graphImage != null)
                graphImage.Dispose();

            graphImage = new Bitmap(1000, (numberOfVisibleRows * height) + 50, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);//System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Graphics graph = Graphics.FromImage(graphImage);

            string lastlastLine = "";
            string lastLine = "";
            string currentLine = "";

            Pen linePen = new Pen(Settings.RevisionGraphColor, Settings.RevisionGraphThickness);
            SolidBrush blueBrush = new SolidBrush(Settings.RevisionGraphColorSelected);
            SolidBrush redBrush = new SolidBrush(Settings.RevisionGraphColor);
            char[] calc = new char[100];

            int nLine = 0;

            GitRevision revision = null;

            GitRevision prevRevision = null;
            GitRevision nextRevision = null;

            int lastVisibleRow = Math.Min(RevisionList.Count - 1, firstVisibleRow + numberOfVisibleRows);

            for (int r = firstVisibleRow; r <= lastVisibleRow; r++)
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
                            //                            if (nextChar != ' ')
                        }
                        graphWidth = Math.Max(graphWidth, Math.Max(40, (currentLine.Length * width) + width));
                        graphWidth = Math.Min((100 * width) + width, graphWidth);

                    }
                    lastlastLine = lastLine;
                    lastLine = currentLine;
                    currentLine = nextLine;
                }
            }

            Revisions.Columns[0].Width = graphWidth;
        }

        void Revisions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.State & DataGridViewElementStates.Visible) != 0)
            {
                if (RevisionList != null && RevisionList.Count > e.RowIndex)
                {
                    GitRevision revision = RevisionList[e.RowIndex];

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
                                    SolidBrush brush = new SolidBrush(h.IsTag == true ? Settings.TagColor : h.IsHead ? Settings.BranchColor : h.IsRemote ? Settings.RemoteBranchColor : Settings.OtherTagColor);

                                    e.Graphics.DrawString("[" + h.Name + "] ", HeadFont, brush, new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));

                                    offset += e.Graphics.MeasureString("[" + h.Name + "] ", HeadFont).Width;
                                }
                            }
                            string text = revision.Message;
                            e.Graphics.DrawString(text, NormalFont, new SolidBrush(Color.Black), new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));
                        }
                        else
                            if (e.ColumnIndex == 2)
                            {
                                string text = revision.Author;
                                e.Graphics.DrawString(text, NormalFont, new SolidBrush(Color.Black), new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                            }
                            else
                                if (e.ColumnIndex == 3)
                                {
                                    string text = Settings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate;
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
                RevisionFilter = new FormRevisionFilter();

            RevisionFilter.ShowDialog();
            filter = RevisionFilter.GetFilter();
            ForceRefreshRevisions();
        }

        private void Revisions_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F && ContextMenuEnabled)
                filterToolStripMenuItem_Click(null, null);
        }

        private void CreateTag_Opening(object sender, CancelEventArgs e)
        {
            if (RevisionList.Count < LastRow || LastRow < 0 || RevisionList.Count == 0)
                return;

            GitRevision revision = RevisionList[LastRow] as GitRevision;

            ToolStripDropDown tagDropDown = new ToolStripDropDown();
            ToolStripDropDown branchDropDown = new ToolStripDropDown();
            ToolStripDropDown checkoutBranchDropDown = new ToolStripDropDown();
            ToolStripDropDown mergeBranchDropDown = new ToolStripDropDown();
            ToolStripDropDown rebaseDropDown = new ToolStripDropDown();

            foreach (GitHead head in revision.Heads)
            {
                if (head.IsTag)
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += new EventHandler(toolStripItem_Click);
                    tagDropDown.Items.Add(toolStripItem);
                }
                else
                    if (head.IsHead || head.IsRemote)
                    {
                        ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                        toolStripItem.Click += new EventHandler(toolStripItem_ClickMergeBranch);
                        mergeBranchDropDown.Items.Add(toolStripItem);

                        toolStripItem = new ToolStripMenuItem(head.Name);
                        toolStripItem.Click += new EventHandler(toolStripItem_ClickRebaseBranch);
                        rebaseDropDown.Items.Add(toolStripItem);

                        if (head.IsHead && !head.IsRemote)
                        {
                            toolStripItem = new ToolStripMenuItem(head.Name);
                            toolStripItem.Click += new EventHandler(toolStripItem_ClickBranch);
                            branchDropDown.Items.Add(toolStripItem);

                            toolStripItem = new ToolStripMenuItem(head.Name);
                            toolStripItem.Click += new EventHandler(toolStripItem_ClickCheckoutBranch);
                            checkoutBranchDropDown.Items.Add(toolStripItem);
                        }

                    }
            }

            deleteTagToolStripMenuItem.DropDown = tagDropDown;
            deleteTagToolStripMenuItem.Visible = tagDropDown.Items.Count > 0;

            deleteBranchToolStripMenuItem.DropDown = branchDropDown;
            deleteBranchToolStripMenuItem.Visible = branchDropDown.Items.Count > 0;

            checkoutBranchToolStripMenuItem.DropDown = checkoutBranchDropDown;
            checkoutBranchToolStripMenuItem.Visible = checkoutBranchDropDown.Items.Count > 0;

            mergeBranchToolStripMenuItem.DropDown = mergeBranchDropDown;
            mergeBranchToolStripMenuItem.Visible = mergeBranchDropDown.Items.Count > 0;

            rebaseOnToolStripMenuItem.DropDown = rebaseDropDown;
            rebaseOnToolStripMenuItem.Visible = rebaseDropDown.Items.Count > 0;

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

            GitUICommands.Instance.StartDeleteBranchDialog(toolStripItem.Text);

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

        void toolStripItem_ClickMergeBranch(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartMergeBranchDialog(toolStripItem.Text);

            ForceRefreshRevisions();
        }

        void toolStripItem_ClickRebaseBranch(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartRebaseDialog(toolStripItem.Text);

            ForceRefreshRevisions();
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

        private void showAuthorDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.ShowAuthorDate = !showAuthorDateToolStripMenuItem.Checked;
            showAuthorDateToolStripMenuItem.Checked = Settings.ShowAuthorDate;
            this.ForceRefreshRevisions();
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
            if (GitUICommands.Instance.StartCheckoutBranchDialog())
                this.RefreshRevisions();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void cherryPickCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RevisionList.Count > LastRow && LastRow >= 0)
            {
                FormCherryPickCommitSmall frm = new FormCherryPickCommitSmall((GitRevision)RevisionList[LastRow]);
                frm.ShowDialog();
                RefreshRevisions();
            }

        }

        private void showRelativeDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.RelativeDate = !showRelativeDateToolStripMenuItem.Checked;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;
            this.ForceRefreshRevisions();
        }

    }
}
