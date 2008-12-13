using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class RevisionGrid : UserControl
    {
        public RevisionGrid()
        {
            InitializeComponent();
            RefreshRevisions();
            Revisions.SelectionChanged += new EventHandler(Revisions_SelectionChanged);
        }

        public event EventHandler SelectionChanged;

        void Revisions_SelectionChanged(object sender, EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        public List<GitRevision> GetRevisions()
        {
            List<GitRevision> retval = new List<GitRevision>();
            foreach (DataGridViewRow row in Revisions.SelectedRows)
            {
                if (row.DataBoundItem is GitRevision)
                {
                    retval.Add((GitRevision)row.DataBoundItem);
                }
            }
            return retval;
        }


        protected Bitmap graphImage;

        public void RefreshRevisions()
        {
            string currentCheckout = GitCommands.GitCommands.GetCurrentCheckout();

            List<GitRevision> revisions = GitCommands.GitCommands.GitRevisionGraph();

            {
                Revisions.DataSource = revisions;
                Revisions.CellPainting += new DataGridViewCellPaintingEventHandler(Revisions_CellPainting);

                int height = Revisions.RowTemplate.Height;
                int width = 8;
                int y = -height;

                graphImage = new Bitmap(1000, (revisions.Count * height) + 50, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                Graphics graph = Graphics.FromImage(graphImage);
                //graph.Clear(Color.White);

                string lastlastLine = "";
                string lastLine = "";
                string currentLine = "";

                for (int r = 0; r < revisions.Count; r++)
                {
                    GitRevision revision = revisions[r];

                    GitRevision prevRevision = null;
                    GitRevision nextRevision = null;

                    if (r > 0)
                        prevRevision = revisions[r - 1];
                    if (revisions.Count > r + 1)
                        nextRevision = revisions[r + 1];

                    y += height;
                    int nLine = 0;

                    char[] calc = new char[100];

                    for (int x = 0; x < 100; x++)
                    {
                        calc[x] = '|';
                    }

                    for (int n = 0; n < revision.GraphLines.Count+1; n++)
                    {
                        string nextLine = "";

                        if (n < revision.GraphLines.Count)
                        {
                            nextLine = revision.GraphLines[n];
                        }
                        else
                        {
                            if (nextRevision != null)
                                nextLine = nextRevision.GraphLines[0];
                        }


                        nLine++;

                        int x = 0;
                        for (int nc = 0; nc < currentLine.Length; nc++)
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
                                    graph.FillEllipse(new SolidBrush(Color.Blue), hcenter - 4, vcenter - 4, 8, 8);
                                else
                                    graph.FillEllipse(new SolidBrush(Color.Red), hcenter - 3, vcenter - 3, 6, 6);

                                if (/*r == 0 &&*/ nextRevision != null && nextRevision.GraphLines[0].Length > nc && (nextRevision.GraphLines[0][nc] == '|' || nextRevision.GraphLines[0][nc] == '*'))
                                {
                                    if (r == 0)
                                        graph.DrawLine(new Pen(Color.Red), hcenter, vcenter, hcenter, bottom);
                                    else
                                        if (nextLine != null && nextLine.Length > nc && nextLine[nc] == '|')
                                            graph.DrawLine(new Pen(Color.Red), hcenter, vcenter, hcenter, bottom+(height/2));
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
                                        if (nextLine.Length > nc+1 && nextLine[nc+1] == '|' || nextLine.Length <= nc+1)
                                            graph.DrawLine(new Pen(Color.Red), left - (width / 2), vcenter, left - (width / 2), bottom + (height / 2));
                                    }
                                }
                                else
                                {
                                    if ((nextLine.Length > nc + 2 && nextLine[nc + 2] != '\\') || nextLine.Length <= nc + 2)
                                    {
                                        //draw: 
                                        //      \
                                        graph.DrawLine(new Pen(Color.Red), right, bottom, right + (width / 2), bottom + (height / 2));
                                    }
                                    if (nc - 2 >= 0 && lastLine.Length > (nc - 2) && lastLine[nc - 2] == '\\')
                                    {
                                        //draw: _
                                        graph.DrawLine(new Pen(Color.Red), left - width, bottom, right, bottom);
                                    }
                                    else
                                    {
                                        // draw: \_
                                        graph.DrawLine(new Pen(Color.Red), left - (width / 2), vcenter, left, bottom);
                                        graph.DrawLine(new Pen(Color.Red), left, bottom, right, bottom);
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
                                        if (nextLine.Length > nc-1 && nextLine[nc-1] == '|' || nextLine.Length <= nc-1)
                                            graph.DrawLine(new Pen(Color.Red), left - (width / 2), vcenter, left - (width / 2), bottom + (height / 2));
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
                                        graph.DrawLine(new Pen(Color.Red), right, bottom, right + (width / 2), bottom - (height / 2));
                                    }
                                    if (nc - 2 >= 0 && nextLine.Length > (nc - 2) && nextLine[nc - 2] == '/')
                                    {
                                        //draw: _
                                        //      
                                        graph.DrawLine(new Pen(Color.Red), left - width, bottom, right, bottom);
                                    }
                                    else
                                    {
                                        //draw:  _
                                        //      /
                                        graph.DrawLine(new Pen(Color.Red), left - (width / 2), bottom + (height / 2), left, bottom);
                                        graph.DrawLine(new Pen(Color.Red), left, bottom, right, bottom);
                                    }
                                }
                            }

                            if (n == revision.GraphLines.Count - 1)
                            {
                                char prevChar = ' ';
                                char currentChar = calc[nc];
                                char nextChar = ' ';

                                if (prevRevision != null && prevRevision.GraphLines[prevRevision.GraphLines.Count - 1].Length > nc)
                                    prevChar = prevRevision.GraphLines[prevRevision.GraphLines.Count - 1][nc];

                                if (nextRevision != null && nextRevision.GraphLines[0].Length > nc)
                                    nextChar = nextRevision.GraphLines[0][nc];

                                if ((prevChar == '|' && currentChar == '|') || (prevChar == '|' && currentChar == '*'))
                                {
                                    graph.DrawLine(new Pen(Color.Red), hcenter, top + (height / 2), hcenter, vcenter + (height / 2));
                                }
                                if ((nextChar == '|' && currentChar == '|') || (nextChar == '*' && currentChar == '|'))
                                {
                                    graph.DrawLine(new Pen(Color.Red), hcenter, vcenter + (height / 2), hcenter, bottom + (height / 2));
                                }

                            }
                        }
                        lastlastLine = lastLine;
                        lastLine = currentLine;
                        currentLine = nextLine;
                    }
                }
            }
        }

        void Revisions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0 && (e.State & DataGridViewElementStates.Visible) != 0)
            {
                //e.Graphics.DrawImage(graphImage, e.CellBounds.X, e.CellBounds.Y, e.CellBounds.Width, e.CellBounds.Y + e.CellBounds.Height);
                
                if ((e.State & DataGridViewElementStates.Selected) != 0)
                    e.Graphics.FillRectangle(new SolidBrush(Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor), e.CellBounds);
                else
                    e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);

                e.Graphics.DrawImage(graphImage, e.CellBounds, new Rectangle(0, e.RowIndex * Revisions.RowTemplate.Height, e.CellBounds.Width, Revisions.RowTemplate.Height), GraphicsUnit.Pixel);
                e.Handled = true;
            }
        }
    }
}
