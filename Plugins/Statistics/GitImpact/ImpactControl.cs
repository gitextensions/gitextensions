using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands.Statistics;

namespace GitImpact
{
    public class ImpactControl : UserControl
    {
        private const int block_width = 50;
        private const int transition_width = 50;

        // <Author, <Commits, Added Lines, Deleted Lines>>
        private Dictionary<string, Impact.DataPoint> authors;
        // <First weekday of commit date, <Author, <Commits, Added Lines, Deleted Lines>>>
        private SortedDictionary<DateTime, Dictionary<string, Impact.DataPoint>> impact;

        private List<string> author_stack;
        private Dictionary<string, GraphicsPath> paths;
        private Dictionary<string, Brush> brushes;
        private HScrollBar ScollBar;

        public ImpactControl()
        {
            authors = new Dictionary<string, Impact.DataPoint>();
            impact = new SortedDictionary<DateTime, Dictionary<string, Impact.DataPoint>>();

            author_stack = new List<string>();
            paths = new Dictionary<string, GraphicsPath>();
            brushes = new Dictionary<string, Brush>();

            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        public void UpdateData()
        {
            impact = Impact.GetImpact();
            authors = Impact.GetAuthors(impact);
            author_stack = new List<string>(from entry in authors orderby entry.Value.ChangedLines select entry.Key);
            Impact.AddIntermediateEmptyWeeks(ref impact, authors);

            UpdateWidth();
            UpdatePaths();
        }

        private void InitializeComponent()
        {
            this.ScollBar = new System.Windows.Forms.HScrollBar();
            this.SuspendLayout();
            // 
            // ScollBar
            // 
            this.ScollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ScollBar.LargeChange = 1000;
            this.ScollBar.Location = new System.Drawing.Point(0, 133);
            this.ScollBar.Maximum = 5000;
            this.ScollBar.Name = "ScollBar";
            this.ScollBar.Size = new System.Drawing.Size(150, 17);
            this.ScollBar.SmallChange = 100;
            this.ScollBar.TabIndex = 0;
            this.ScollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            // 
            // ImpactControl
            // 
            this.Controls.Add(this.ScollBar);
            this.Name = "ImpactControl";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.Resize += new System.EventHandler(this.OnResize);
            this.ResumeLayout(false);

        }

        public int GetGraphWidth()
        {
            return Math.Max(0, impact.Count * (block_width + transition_width) - transition_width);
        }

        private void UpdateWidth()
        {
            ScollBar.Minimum = 0;
            ScollBar.Maximum = GetGraphWidth();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            // White background
            e.Graphics.Clear(Color.White);

            // Nothing to draw
            if (impact.Count == 0)
                return;

            // Activate AntiAliasing
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // "Scroll" to the right position
            e.Graphics.TranslateTransform(-ScollBar.Value, 0);

            // Draw paths in order of the author_stack
            // Default: person with least number of changed lines first, others on top
            foreach (var author in author_stack)
                e.Graphics.FillPath(brushes[author], paths[author]);
        }

        private void OnResize(object sender, EventArgs e)
        {
            UpdatePaths();
            UpdateWidth();
            Invalidate();
        }

        private void UpdatePaths()
        {
            Random rnd = new Random();

            int h_max = 0;

            // Calculate points
            int x = 0;
            Dictionary<string, List<Rectangle>> author_points_dict = new Dictionary<string, List<Rectangle>>();
            foreach (var week in impact)
            {
                int y = 0;
                foreach (var pair in (from entry in week.Value orderby entry.Value.ChangedLines descending select entry))
                {
                    string author = pair.Key;

                    int height = Math.Max(1, (int)Math.Round(Math.Log(pair.Value.ChangedLines) * 5));
                    Rectangle rc = new Rectangle(x, y, block_width, height);

                    if (!author_points_dict.ContainsKey(author))
                        author_points_dict.Add(author, new List<Rectangle>());

                    author_points_dict[author].Add(rc);

                    if (!brushes.ContainsKey(author))
                        brushes.Add(author, new SolidBrush(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255))));

                    y += rc.Height + 2;
                }

                h_max = Math.Max(h_max, y);

                x += block_width + transition_width;
            }

            double height_factor = 0.9 * (float)Height / (float)h_max;

            // Add points to the GraphicsPath
            paths.Clear();
            foreach (var author_points in author_points_dict)
            {
                string author = author_points.Key;

                for (int i = 0; i < author_points.Value.Count; i++)
                {
                    author_points.Value[i] =
                        new Rectangle(author_points.Value[i].Left, (int)(author_points.Value[i].Top * height_factor),
                                      author_points.Value[i].Width, Math.Max(1, (int)(author_points.Value[i].Height * height_factor)));
                }

                paths.Add(author, new GraphicsPath());

                // Left border
                paths[author].AddLine(author_points.Value[0].Left, author_points.Value[0].Bottom,
                                      author_points.Value[0].Left, author_points.Value[0].Top);

                // Top borders
                for (int i = 0; i < author_points.Value.Count; i++)
                {
                    paths[author].AddLine(author_points.Value[i].Left, author_points.Value[i].Top,
                                          author_points.Value[i].Right, author_points.Value[i].Top);

                    if (i < author_points.Value.Count - 1)
                        paths[author].AddBezier(author_points.Value[i].Right, author_points.Value[i].Top,
                                                author_points.Value[i].Right + transition_width / 2, author_points.Value[i].Top,
                                                author_points.Value[i].Right + transition_width / 2, author_points.Value[i + 1].Top,
                                                author_points.Value[i + 1].Left, author_points.Value[i + 1].Top);
                }

                // Right border
                paths[author].AddLine(author_points.Value[author_points.Value.Count - 1].Right, 
                                      author_points.Value[author_points.Value.Count - 1].Top,
                                      author_points.Value[author_points.Value.Count - 1].Right, 
                                      author_points.Value[author_points.Value.Count - 1].Bottom);

                // Bottom borders
                for (int i = author_points.Value.Count - 1; i >= 0; i--)
                {
                    paths[author].AddLine(author_points.Value[i].Right, author_points.Value[i].Bottom,
                                          author_points.Value[i].Left, author_points.Value[i].Bottom);

                    if (i > 0)
                        paths[author].AddBezier(author_points.Value[i].Left, author_points.Value[i].Bottom,
                                                author_points.Value[i].Left - transition_width / 2, author_points.Value[i].Bottom,
                                                author_points.Value[i].Left - transition_width / 2, author_points.Value[i - 1].Bottom,
                                                author_points.Value[i - 1].Right, author_points.Value[i - 1].Bottom);
                }
            }
        }

        private string GetAuthorByScreenPosition(int x, int y)
        {
            foreach (var author in author_stack.Reverse<string>())
            {
                if (paths[author].IsVisible(x + ScollBar.Value, y))
                    return author;
            }

            return "";
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            string author = GetAuthorByScreenPosition(e.X, e.Y);
            if (!string.IsNullOrEmpty(author))
            {
                SelectAuthor(author);
                Invalidate();
            }
        }

        private void SelectAuthor(string author)
        {
            // Remove author from the stack
            author_stack.Remove(author);
            // and add it again at the end
            author_stack.Add(author);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            string author = GetAuthorByScreenPosition(e.X, e.Y);
            if (!string.IsNullOrEmpty(author))
                MessageBox.Show(author);
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }
    }
}
