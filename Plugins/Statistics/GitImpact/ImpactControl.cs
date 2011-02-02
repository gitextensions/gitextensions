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

        // List of authors that determines the drawing order
        private List<string> author_stack;
        // The paths for each author
        private Dictionary<string, GraphicsPath> paths;
        // The brush for each author
        private Dictionary<string, SolidBrush> brushes;

        private TimeSpan time_data;
        private TimeSpan time_process;

        private HScrollBar scrollBar;

        public ImpactControl()
        {
            authors = new Dictionary<string, Impact.DataPoint>();
            impact = new SortedDictionary<DateTime, Dictionary<string, Impact.DataPoint>>();

            author_stack = new List<string>();
            paths = new Dictionary<string, GraphicsPath>();
            brushes = new Dictionary<string, SolidBrush>();

            time_data = new TimeSpan();
            time_process = new TimeSpan();

            InitializeComponent();

            // Set DoubleBuffer flag for flicker-free drawing
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        public TimeSpan TimeData { get { return time_data; } }
        public TimeSpan TimeProcess { get { return time_process; } }

        public void UpdateData()
        {
            // Start time measurement
            DateTime start = DateTime.Now;

            // Gather impact information
            impact = Impact.GetImpact();

            DateTime gathered = DateTime.Now;

            // Gather author informations
            authors = Impact.GetAuthors(impact);
            // Create author stack (authors with most changed lines get drawn on top)
            author_stack = new List<string>(from entry in authors orderby entry.Value.ChangedLines select entry.Key);
            // Add authors to intermediate weeks where they didn't create commits
            Impact.AddIntermediateEmptyWeeks(ref impact, authors);

            UpdateScrollbar();
            UpdatePaths();

            // Stop time measurement
            DateTime end = DateTime.Now;

            // Calculate data gathering and processing times
            time_data = gathered - start;
            time_process = end - gathered;
        }

        private void InitializeComponent()
        {
            this.scrollBar = new System.Windows.Forms.HScrollBar();
            this.SuspendLayout();
            // 
            // scrollBar
            // 
            this.scrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.scrollBar.LargeChange = 1000;
            this.scrollBar.Location = new System.Drawing.Point(0, 133);
            this.scrollBar.Maximum = 5000;
            this.scrollBar.Name = "scrollBar";
            this.scrollBar.Size = new System.Drawing.Size(150, 17);
            this.scrollBar.SmallChange = 100;
            this.scrollBar.TabIndex = 0;
            this.scrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            // 
            // ImpactControl
            // 
            this.Controls.Add(this.scrollBar);
            this.Name = "ImpactControl";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.Resize += new System.EventHandler(this.OnResize);
            this.ResumeLayout(false);

        }

        private int GetGraphWidth()
        {
            return Math.Max(0, impact.Count * (block_width + transition_width) - transition_width);
        }

        private void UpdateScrollbar()
        {
            bool ScrollBarAtMaximum = (scrollBar.Value == scrollBar.Maximum);
            
            scrollBar.Minimum = 0;
            scrollBar.Maximum = Math.Max(0, GetGraphWidth() - ClientSize.Width);
            scrollBar.SmallChange = scrollBar.Maximum / 10;
            scrollBar.LargeChange = scrollBar.Maximum / 20;
            
            if (ScrollBarAtMaximum)
                this.scrollBar.Value = scrollBar.Maximum;

            scrollBar.Maximum += scrollBar.LargeChange;
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
            e.Graphics.TranslateTransform(-scrollBar.Value, 0);

            // Draw paths in order of the author_stack
            // Default: person with least number of changed lines first, others on top
            foreach (var author in author_stack)
                e.Graphics.FillPath(brushes[author], paths[author]);
        }

        private void OnResize(object sender, EventArgs e)
        {
            UpdatePaths();
            UpdateScrollbar();
            Invalidate();
        }

        private void UpdatePaths()
        {
            // Randomizer for the user colors
            Random rnd = new Random();

            int h_max = 0;
            int x = 0;
            Dictionary<string, List<Rectangle>> author_points_dict = new Dictionary<string, List<Rectangle>>();

            // Iterate through weeks
            foreach (var week in impact)
            {
                int y = 0;

                // Iterate through authors
                foreach (var pair in (from entry in week.Value orderby entry.Value.ChangedLines descending select entry))
                {
                    string author = pair.Key;

                    // Calculate week-author-rectangle
                    int height = Math.Max(1, (int)Math.Round(Math.Log(pair.Value.ChangedLines) * 5));
                    Rectangle rc = new Rectangle(x, y, block_width, height);

                    // Add rectangle to temporary list
                    if (!author_points_dict.ContainsKey(author))
                        author_points_dict.Add(author, new List<Rectangle>());

                    author_points_dict[author].Add(rc);

                    // Create a new random brush for the author if none exists yet
                    if (!brushes.ContainsKey(author))
                        brushes.Add(author, new SolidBrush(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255))));

                    // Increase y for next block
                    y += rc.Height + 2;
                }

                // Remember total height of largest week
                h_max = Math.Max(h_max, y);

                // Increase x for next week
                x += block_width + transition_width;
            }

            // Pre-calculate height scale factor
            double height_factor = 0.9 * (float)Height / (float)h_max;

            // Clear previous paths
            paths.Clear();

            // Add points to each author's GraphicsPath
            foreach (var author_points in author_points_dict)
            {
                string author = author_points.Key;

                // Scale heights
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

        /// <summary>
        /// Determines if the given coordinates are belonging to any author
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns>Name of the author</returns>
        public string GetAuthorByScreenPosition(int x, int y)
        {
            foreach (var author in author_stack.Reverse<string>())
                if (paths[author].IsVisible(x + scrollBar.Value, y))
                    return author;

            return "";
        }

        /// <summary>
        /// Pushes the author to the top of the author_stack
        /// </summary>
        /// <param name="author">Name of the author</param>
        public bool SelectAuthor(string author)
        {
            if (!author_stack.Contains(author))
                return false;

            // Remove author from the stack
            author_stack.Remove(author);
            // and add it again at the end
            author_stack.Add(author);

            return true;
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            // Redraw when we've scrolled
            Invalidate();
        }
    }
}
