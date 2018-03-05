using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Statistics;
using GitUIPluginInterfaces;

namespace GitImpact
{
    public class ImpactControl : UserControl
    {
        private const int BlockWidth = 60;
        private const int TransitionWidth = 50;

        private const int LinesFontSize = 10;
        private const int WeekFontSize = 8;


        private readonly object _dataLock = new object();

        private ImpactLoader _impactLoader;

        // <Author, <Commits, Added Lines, Deleted Lines>>
        private Dictionary<string, ImpactLoader.DataPoint> _authors;
        // <First weekday of commit date, <Author, <Commits, Added Lines, Deleted Lines>>>
        private SortedDictionary<DateTime, Dictionary<string, ImpactLoader.DataPoint>> _impact;

        // List of authors that determines the drawing order
        private List<string> _authorStack;
        // The paths for each author
        private Dictionary<string, GraphicsPath> _paths;
        // The brush for each author
        private Dictionary<string, SolidBrush> _brushes;
        // The changed-lines-labels for each author
        private Dictionary<string, List<Tuple<PointF, int>>> _lineLabels;
        // The week-labels
        private List<Tuple<PointF, DateTime>> _weekLabels;

        private HScrollBar _scrollBar;

        public ImpactControl()
        {
            Clear();

            InitializeComponent();

            // Set DoubleBuffer flag for flicker-free drawing
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            MouseWheel += ImpactControl_MouseWheel;
        }

        public void Init(IGitModule Module)
        {
            _impactLoader = new ImpactLoader(Module);
            _impactLoader.RespectMailmap = true; // respect the .mailmap file
            _impactLoader.Updated += OnImpactUpdate;
        }

        private void Clear()
        {
            lock (_dataLock)
            {
                _authors = new Dictionary<string, ImpactLoader.DataPoint>();
                _impact = new SortedDictionary<DateTime, Dictionary<string, ImpactLoader.DataPoint>>();

                _authorStack = new List<string>();
                _paths = new Dictionary<string, GraphicsPath>();
                _brushes = new Dictionary<string, SolidBrush>();
                _lineLabels = new Dictionary<string, List<Tuple<PointF, int>>>();
                _weekLabels = new List<Tuple<PointF, DateTime>>();
            }
        }

        public void Stop()
        {
            _impactLoader?.Stop();
        }

        void ImpactControl_MouseWheel(object sender, MouseEventArgs e)
        {
            this._scrollBar.Value = Math.Min(this._scrollBar.Maximum, Math.Max(this._scrollBar.Minimum, this._scrollBar.Value + e.Delta));
            // Redraw when we've scrolled
            Invalidate();
        }

        void OnImpactUpdate(object sender, ImpactLoader.CommitEventArgs e)
        {
            var commit = e.Commit;

            lock (_dataLock)
            {
                // UPDATE IMPACT

                // If week does not exist yet in the impact dictionary
                if (!_impact.ContainsKey(commit.week))
                    // Create it
                    _impact.Add(commit.week, new Dictionary<string, ImpactLoader.DataPoint>());

                // If author does not exist yet for this week in the impact dictionary
                if (!_impact[commit.week].ContainsKey(commit.author))
                    // Create it
                    _impact[commit.week].Add(commit.author, commit.data);
                else
                    // Otherwise just add the changes
                    _impact[commit.week][commit.author] += commit.data;

                // UPDATE AUTHORS

                // If author does not exist yet in the authors dictionary
                if (!_authors.ContainsKey(commit.author))
                    // Create it
                    _authors.Add(commit.author, commit.data);
                else
                    // Otherwise just add the changes
                    _authors[commit.author] += commit.data;

                // Add authors to intermediate weeks where they didn't create commits
                ImpactLoader.AddIntermediateEmptyWeeks(ref _impact, _authors);

                // UPDATE AUTHORSTACK

                // If author does not exist yet in the author_stack
                if (!_authorStack.Contains(commit.author))
                    // Add it to the front (drawn first)
                    _authorStack.Insert(0, commit.author);
            }

            UpdatePathsAndLabels();
            Invalidate();
        }

        public void UpdateData()
        {
            if (_impactLoader != null)
            {
                _impactLoader.ShowSubmodules = _showSubmodules;
                _impactLoader.Execute();
            }
        }

        private bool _showSubmodules;
        [DefaultValue(false)]
        public bool ShowSubmodules
        {
            get => _showSubmodules;
            set
            {
                _showSubmodules = value;
                Stop();
                Clear();
                UpdateData();
            }
        }

        private void InitializeComponent()
        {
            this._scrollBar = new System.Windows.Forms.HScrollBar();
            this.SuspendLayout();
            //
            // scrollBar
            //
            this._scrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._scrollBar.LargeChange = 0;
            this._scrollBar.Location = new System.Drawing.Point(0, 133);
            this._scrollBar.Maximum = 0;
            this._scrollBar.Name = "_scrollBar";
            this._scrollBar.Size = new System.Drawing.Size(150, 17);
            this._scrollBar.SmallChange = 0;
            this._scrollBar.TabIndex = 0;
            this._scrollBar.Scroll += this.OnScroll;
            //
            // ImpactControl
            //
            this.Controls.Add(this._scrollBar);
            this.Name = "ImpactControl";
            this.Paint += this.OnPaint;
            this.Resize += this.OnResize;
            this.ResumeLayout(false);

        }

        private int GetGraphWidth()
        {
            lock (_dataLock)
            {
                return Math.Max(0, _impact.Count * (BlockWidth + TransitionWidth) - TransitionWidth);
            }
        }

        private void UpdateScrollbar()
        {
            lock (_dataLock)
            {
                int RightValue = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange - _scrollBar.Value);

                _scrollBar.Minimum = 0;
                _scrollBar.Maximum = (int)(Math.Max(0, GetGraphWidth() - ClientSize.Width) * 1.1);
                _scrollBar.SmallChange = _scrollBar.Maximum / 22;
                _scrollBar.LargeChange = _scrollBar.Maximum / 11;

                _scrollBar.Value = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange - RightValue);
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            // White background
            e.Graphics.Clear(Color.White);
            UpdateScrollbar();
            lock (_dataLock)
            {
                // Nothing to draw
                if (_impact.Count == 0)
                    return;

                // Activate AntiAliasing
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // "Scroll" to the right position
                e.Graphics.TranslateTransform(-_scrollBar.Value, 0);

                // Draw paths in order of the author_stack
                // Default: person with least number of changed lines first, others on top
                foreach (var author in _authorStack)
                {
                    if (_brushes.ContainsKey(author) && _paths.ContainsKey(author))
                        e.Graphics.FillPath(_brushes[author], _paths[author]);
                }

                // Draw black border around selected author
                string selectedAuthor = _authorStack[_authorStack.Count - 1];
                if (_brushes.ContainsKey(selectedAuthor) && _paths.ContainsKey(selectedAuthor))
                    e.Graphics.DrawPath(new Pen(Color.Black, 2), _paths[selectedAuthor]);

                foreach (var author in _authorStack)
                    DrawAuthorLinesLabels(e.Graphics, author);
            }

            DrawWeekLabels(e.Graphics);
        }

        private void DrawAuthorLinesLabels(Graphics g, string author)
        {
            lock (_dataLock)
            {
                if (!_lineLabels.ContainsKey(author))
                    return;

                using (Font font = new Font("Arial", LinesFontSize))
                {
                    Brush brush = Brushes.White;

                    foreach (var label in _lineLabels[author])
                    {
                        SizeF sz = g.MeasureString(label.Item2.ToString(), font);
                        PointF pt = new PointF(label.Item1.X - sz.Width / 2, label.Item1.Y - sz.Height / 2);
                        g.DrawString(label.Item2.ToString(), font, brush, pt);
                    }
                }
            }
        }

        private void DrawWeekLabels(Graphics g)
        {
            lock (_dataLock)
            {
                using (Font font = new Font("Arial", WeekFontSize))
                {
                    Brush brush = Brushes.Gray;

                    foreach (var label in _weekLabels)
                    {
                        SizeF sz = g.MeasureString(label.Item2.ToString("dd. MMM yy"), font);
                        PointF pt = new PointF(label.Item1.X - sz.Width / 2, label.Item1.Y + sz.Height / 2);
                        g.DrawString(label.Item2.ToString("dd. MMM yy"), font, brush, pt);
                    }
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            UpdatePathsAndLabels();
            UpdateScrollbar();
            Invalidate();
        }

        private void UpdatePathsAndLabels()
        {
            int h_max = 0;
            int x = 0;
            var author_points_dict = new Dictionary<string, List<Tuple<Rectangle, int>>>();

            lock (_dataLock)
            {
                // Clear previous week labels
                _weekLabels.Clear();

                // Iterate through weeks
                foreach (var week in _impact)
                {
                    int y = 0;

                    // Iterate through authors
                    foreach (var pair in (from entry in week.Value orderby entry.Value.ChangedLines descending select entry))
                    {
                        string author = pair.Key;

                        // Calculate week-author-rectangle
                        int height = Math.Max(1, (int)Math.Round(Math.Pow(Math.Log(pair.Value.ChangedLines), 1.5) * 4));
                        Rectangle rc = new Rectangle(x, y, BlockWidth, height);

                        // Add rectangle to temporary list
                        if (!author_points_dict.ContainsKey(author))
                            author_points_dict.Add(author, new List<Tuple<Rectangle, int>>());

                        author_points_dict[author].Add(Tuple.Create(rc, pair.Value.ChangedLines));

                        // Create a new random brush for the author if none exists yet
                        if (!_brushes.ContainsKey(author))
                        {
                            int partLength = author.Length / 3;
                            _brushes.Add(author, new SolidBrush(Color.FromArgb(GenerateIntFromString(author.Substring(0, partLength)) % 255, GenerateIntFromString(author.Substring(partLength, partLength)) % 255, GenerateIntFromString(author.Substring(partLength)) % 255)));
                        }

                        // Increase y for next block
                        y += rc.Height + 2;
                    }

                    // Remember total height of largest week
                    h_max = Math.Max(h_max, y);

                    // Add week date label
                    _weekLabels.Add(Tuple.Create(new PointF(x + BlockWidth / 2, y), week.Key));

                    // Increase x for next week
                    x += BlockWidth + TransitionWidth;
                }

                // Pre-calculate height scale factor
                double height_factor = 0.9 * (float)Height / (float)h_max;

                // Scale week label coordinates
                for (int i = 0; i < _weekLabels.Count; i++)
                    _weekLabels[i] = Tuple.Create(new PointF(_weekLabels[i].Item1.X, _weekLabels[i].Item1.Y * (float)height_factor),
                                                  _weekLabels[i].Item2);

                // Clear previous paths
                _paths.Clear();

                // Clear previous labels
                _lineLabels.Clear();

                // Add points to each author's GraphicsPath
                foreach (var author_points in author_points_dict)
                {
                    string author = author_points.Key;

                    // Scale heights
                    for (int i = 0; i < author_points.Value.Count; i++)
                    {
                        author_points.Value[i] = Tuple.Create(
                            new Rectangle(author_points.Value[i].Item1.Left, (int)(author_points.Value[i].Item1.Top * height_factor),
                                          author_points.Value[i].Item1.Width, Math.Max(1, (int)(author_points.Value[i].Item1.Height * height_factor))),
                                          author_points.Value[i].Item2);

                        // Add lines-changed-labels
                        if (!_lineLabels.ContainsKey(author))
                            _lineLabels.Add(author, new List<Tuple<PointF, int>>());

                        if (author_points.Value[i].Item1.Height > LinesFontSize * 1.5)
                            _lineLabels[author].Add(Tuple.Create(new PointF(author_points.Value[i].Item1.Left + BlockWidth / 2,
                                author_points.Value[i].Item1.Top + author_points.Value[i].Item1.Height / 2),
                                author_points.Value[i].Item2));
                    }

                    _paths.Add(author, new GraphicsPath());

                    // Left border
                    _paths[author].AddLine(author_points.Value[0].Item1.Left, author_points.Value[0].Item1.Bottom,
                                          author_points.Value[0].Item1.Left, author_points.Value[0].Item1.Top);

                    // Top borders
                    for (int i = 0; i < author_points.Value.Count; i++)
                    {
                        _paths[author].AddLine(author_points.Value[i].Item1.Left, author_points.Value[i].Item1.Top,
                                              author_points.Value[i].Item1.Right, author_points.Value[i].Item1.Top);

                        if (i < author_points.Value.Count - 1)
                            _paths[author].AddBezier(author_points.Value[i].Item1.Right, author_points.Value[i].Item1.Top,
                                                    author_points.Value[i].Item1.Right + TransitionWidth / 2, author_points.Value[i].Item1.Top,
                                                    author_points.Value[i].Item1.Right + TransitionWidth / 2, author_points.Value[i + 1].Item1.Top,
                                                    author_points.Value[i + 1].Item1.Left, author_points.Value[i + 1].Item1.Top);
                    }

                    // Right border
                    _paths[author].AddLine(author_points.Value[author_points.Value.Count - 1].Item1.Right,
                                          author_points.Value[author_points.Value.Count - 1].Item1.Top,
                                          author_points.Value[author_points.Value.Count - 1].Item1.Right,
                                          author_points.Value[author_points.Value.Count - 1].Item1.Bottom);

                    // Bottom borders
                    for (int i = author_points.Value.Count - 1; i >= 0; i--)
                    {
                        _paths[author].AddLine(author_points.Value[i].Item1.Right, author_points.Value[i].Item1.Bottom,
                                              author_points.Value[i].Item1.Left, author_points.Value[i].Item1.Bottom);

                        if (i > 0)
                            _paths[author].AddBezier(author_points.Value[i].Item1.Left, author_points.Value[i].Item1.Bottom,
                                                    author_points.Value[i].Item1.Left - TransitionWidth / 2, author_points.Value[i].Item1.Bottom,
                                                    author_points.Value[i].Item1.Left - TransitionWidth / 2, author_points.Value[i - 1].Item1.Bottom,
                                                    author_points.Value[i - 1].Item1.Right, author_points.Value[i - 1].Item1.Bottom);
                    }
                }
            }
        }

        private int GenerateIntFromString(string text)
        {
            return text.Sum(c => (int)c);
        }

        /// <summary>
        /// Determines if the given coordinates are belonging to any author
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns>Name of the author</returns>
        public string GetAuthorByScreenPosition(int x, int y)
        {
            lock (_dataLock)
            {
                foreach (var author in _authorStack.Reverse<string>())
                    if (_paths.ContainsKey(author) && _paths[author].IsVisible(x + _scrollBar.Value, y))
                        return author;
            }
            return "";
        }

        /// <summary>
        /// Pushes the author to the top of the author_stack
        /// </summary>
        /// <param name="author">Name of the author</param>
        public bool SelectAuthor(string author)
        {
            lock (_dataLock)
            {
                if (!_authorStack.Contains(author))
                    return false;

                // Remove author from the stack
                _authorStack.Remove(author);
                // and add it again at the end
                _authorStack.Add(author);
            }

            return true;
        }

        /// <summary>
        /// Returns the selected author
        /// </summary>
        public string GetSelectedAuthor()
        {
            if (_authorStack.Count == 0)
                return string.Empty;

            lock (_dataLock)
            {
                return _authorStack.Last();
            }
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            // Redraw when we've scrolled
            Invalidate();
        }

        public Color GetAuthorColor(string author)
        {
            lock (_dataLock)
            {
                if (_brushes.ContainsKey(author))
                    return _brushes[author].Color;
            }
            return Color.Transparent;
        }

        [Browsable(false)]
        public List<string> Authors { get { lock (_dataLock) return _authorStack; } }

        public ImpactLoader.DataPoint GetAuthorInfo(string author)
        {
            lock (_dataLock)
            {
                if (_authors.ContainsKey(author))
                    return _authors[author];

                return new ImpactLoader.DataPoint(0, 0, 0);
            }
        }
    }
}
