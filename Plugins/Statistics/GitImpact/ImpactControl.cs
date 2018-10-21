using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Statistics;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;

namespace GitImpact
{
    public class ImpactControl : UserControl
    {
        private static readonly int BlockWidth = DpiUtil.Scale(60);
        private static readonly int TransitionWidth = DpiUtil.Scale(50);

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
        private Dictionary<string, List<(PointF point, int size)>> _lineLabels;

        // The week-labels
        private List<(PointF point, DateTime date)> _weekLabels;

        private HScrollBar _scrollBar;

        public ImpactControl()
        {
            Clear();

            InitializeComponent();

            // Set DoubleBuffer flag for flicker-free drawing
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            MouseWheel += ImpactControl_MouseWheel;
        }

        public void Init(IGitModule module)
        {
            _impactLoader = new ImpactLoader(module)
            {
                // respect the .mailmap file
                RespectMailmap = true
            };

            _impactLoader.CommitLoaded += OnImpactUpdate;
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
                _lineLabels = new Dictionary<string, List<(PointF, int)>>();
                _weekLabels = new List<(PointF, DateTime)>();
            }
        }

        public void Stop()
        {
            _impactLoader?.Stop();
        }

        private void ImpactControl_MouseWheel(object sender, MouseEventArgs e)
        {
            _scrollBar.Value = Math.Min(_scrollBar.Maximum, Math.Max(_scrollBar.Minimum, _scrollBar.Value + e.Delta));

            // Redraw when we've scrolled
            Invalidate();
        }

        private void OnImpactUpdate(ImpactLoader.Commit commit)
        {
            lock (_dataLock)
            {
                // UPDATE IMPACT

                // If week does not exist yet in the impact dictionary
                if (!_impact.ContainsKey(commit.Week))
                {
                    // Create it
                    _impact.Add(commit.Week, new Dictionary<string, ImpactLoader.DataPoint>());
                }

                // If author does not exist yet for this week in the impact dictionary
                if (!_impact[commit.Week].ContainsKey(commit.Author))
                {
                    // Create it
                    _impact[commit.Week].Add(commit.Author, commit.Data);
                }
                else
                {
                    // Otherwise just add the changes
                    _impact[commit.Week][commit.Author] += commit.Data;
                }

                // UPDATE AUTHORS

                // If author does not exist yet in the authors dictionary
                if (!_authors.ContainsKey(commit.Author))
                {
                    // Create it
                    _authors.Add(commit.Author, commit.Data);
                }
                else
                {
                    // Otherwise just add the changes
                    _authors[commit.Author] += commit.Data;
                }

                // Add authors to intermediate weeks where they didn't create commits
                ImpactLoader.AddIntermediateEmptyWeeks(ref _impact, _authors.Keys);

                // UPDATE AUTHOR STACK

                // If author does not exist yet in the author_stack
                if (!_authorStack.Contains(commit.Author))
                {
                    // Add it to the front (drawn first)
                    _authorStack.Insert(0, commit.Author);
                }
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
            SuspendLayout();

            _scrollBar = new HScrollBar
            {
                Dock = DockStyle.Bottom,
                LargeChange = 0,
                Location = new Point(0, 133),
                Maximum = 0,
                Name = "_scrollBar",
                SmallChange = 0,
                TabIndex = 0
            };
            _scrollBar.Scroll += OnScroll;

            Controls.Add(_scrollBar);
            Name = "ImpactControl";
            Paint += OnPaint;
            Resize += OnResize;
            ResumeLayout(false);
        }

        private int GetGraphWidth()
        {
            lock (_dataLock)
            {
                return Math.Max(0, (_impact.Count * (BlockWidth + TransitionWidth)) - TransitionWidth);
            }
        }

        private void UpdateScrollbar()
        {
            lock (_dataLock)
            {
                int rightValue = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange - _scrollBar.Value);

                _scrollBar.Minimum = 0;
                _scrollBar.Maximum = (int)(Math.Max(0, GetGraphWidth() - ClientSize.Width) * 1.1);
                _scrollBar.SmallChange = _scrollBar.Maximum / 22;
                _scrollBar.LargeChange = _scrollBar.Maximum / 11;

                _scrollBar.Value = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange - rightValue);
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
                {
                    return;
                }

                // Activate AntiAliasing
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // "Scroll" to the right position
                e.Graphics.TranslateTransform(-_scrollBar.Value, 0);

                // Draw paths in order of the author_stack
                // Default: person with least number of changed lines first, others on top
                foreach (var author in _authorStack)
                {
                    if (_brushes.ContainsKey(author) && _paths.ContainsKey(author))
                    {
                        e.Graphics.FillPath(_brushes[author], _paths[author]);
                    }
                }

                // Draw black border around selected author
                string selectedAuthor = _authorStack[_authorStack.Count - 1];
                if (_brushes.ContainsKey(selectedAuthor) && _paths.ContainsKey(selectedAuthor))
                {
                    e.Graphics.DrawPath(new Pen(Color.Black, 2), _paths[selectedAuthor]);
                }

                foreach (var author in _authorStack)
                {
                    DrawAuthorLinesLabels(e.Graphics, author);
                }
            }

            DrawWeekLabels(e.Graphics);
        }

        private void DrawAuthorLinesLabels(Graphics g, string author)
        {
            lock (_dataLock)
            {
                if (!_lineLabels.ContainsKey(author))
                {
                    return;
                }

                using (var font = new Font("Arial", LinesFontSize))
                {
                    Brush brush = Brushes.White;

                    foreach (var (point, size) in _lineLabels[author])
                    {
                        SizeF sz = g.MeasureString(size.ToString(), font);
                        var pt = new PointF(point.X - (sz.Width / 2), point.Y - (sz.Height / 2));
                        g.DrawString(size.ToString(), font, brush, pt);
                    }
                }
            }
        }

        private void DrawWeekLabels(Graphics g)
        {
            lock (_dataLock)
            {
                using (var font = new Font("Arial", WeekFontSize))
                {
                    Brush brush = Brushes.Gray;

                    foreach (var (point, date) in _weekLabels)
                    {
                        SizeF sz = g.MeasureString(date.ToString("dd. MMM yy"), font);
                        var pt = new PointF(point.X - (sz.Width / 2), point.Y + (sz.Height / 2));
                        g.DrawString(date.ToString("dd. MMM yy"), font, brush, pt);
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
            var author_points_dict = new Dictionary<string, List<(Rectangle, int changeCount)>>();

            lock (_dataLock)
            {
                // Clear previous week labels
                _weekLabels.Clear();

                // Iterate through weeks
                foreach (var (weekDate, dataByAuthor) in _impact)
                {
                    int y = 0;

                    // Iterate through authors
                    foreach (var (author, data) in from entry in dataByAuthor orderby entry.Value.ChangedLines descending select entry)
                    {
                        // Calculate week-author-rectangle
                        int height = Math.Max(1, (int)Math.Round(Math.Pow(Math.Log(data.ChangedLines), 1.5) * 4));
                        var rc = new Rectangle(x, y, BlockWidth, height);

                        // Add rectangle to temporary list
                        if (!author_points_dict.ContainsKey(author))
                        {
                            author_points_dict.Add(author, new List<(Rectangle, int)>());
                        }

                        author_points_dict[author].Add((rc, data.ChangedLines));

                        // Create a new random brush for the author if none exists yet
                        if (!_brushes.ContainsKey(author))
                        {
                            var color = Color.FromArgb((int)(author.GetHashCode() | 0xFF000000));
                            _brushes.Add(author, new SolidBrush(color));
                        }

                        // Increase y for next block
                        y += rc.Height + 2;
                    }

                    // Remember total height of largest week
                    h_max = Math.Max(h_max, y);

                    // Add week date label
                    _weekLabels.Add((new PointF(x + (BlockWidth / 2f), y), weekDate));

                    // Increase x for next week
                    x += BlockWidth + TransitionWidth;
                }

                // Pre-calculate height scale factor
                double height_factor = 0.9 * Height / h_max;

                // Scale week label coordinates
                for (int i = 0; i < _weekLabels.Count; i++)
                {
                    var (point, date) = _weekLabels[i];

                    var adjustedPoint = new PointF(point.X, point.Y * (float)height_factor);

                    _weekLabels[i] = (adjustedPoint, date);
                }

                // Clear previous paths
                _paths.Clear();

                // Clear previous labels
                _lineLabels.Clear();

                // Add points to each author's GraphicsPath
                foreach (var (author, points) in author_points_dict)
                {
                    // Scale heights
                    for (int i = 0; i < points.Count; i++)
                    {
                        var (unscaledRect, num) = points[i];

                        var rect = new Rectangle(unscaledRect.Left, (int)(unscaledRect.Top * height_factor),
                            unscaledRect.Width, Math.Max(1, (int)(unscaledRect.Height * height_factor)));

                        points[i] = (rect, num);

                        // Add lines-changed-labels
                        if (!_lineLabels.ContainsKey(author))
                        {
                            _lineLabels.Add(author, new List<(PointF, int)>());
                        }

                        if (rect.Height > LinesFontSize * 1.5)
                        {
                            var adjustedPoint = new PointF(rect.Left + (BlockWidth / 2), rect.Top + (rect.Height / 2));

                            _lineLabels[author].Add((adjustedPoint, num));
                        }
                    }

                    _paths.Add(author, new GraphicsPath());

                    var (firstRect, _) = points[0];

                    // Left border
                    _paths[author].AddLine(firstRect.Left, firstRect.Bottom,
                                           firstRect.Left, firstRect.Top);

                    // Top borders
                    for (int i = 0; i < points.Count; i++)
                    {
                        var (rect, _) = points[i];

                        _paths[author].AddLine(rect.Left, rect.Top,
                                               rect.Right, rect.Top);

                        if (i < points.Count - 1)
                        {
                            var (nextRect, _) = points[i + 1];

                            _paths[author].AddBezier(rect.Right, rect.Top,
                                                     rect.Right + (TransitionWidth / 2), rect.Top,
                                                     rect.Right + (TransitionWidth / 2), nextRect.Top,
                                                     nextRect.Left, nextRect.Top);
                        }
                    }

                    var (lastRect, _) = points[points.Count - 1];

                    // Right border
                    _paths[author].AddLine(lastRect.Right,
                                           lastRect.Top,
                                           lastRect.Right,
                                           lastRect.Bottom);

                    // Bottom borders
                    for (int i = points.Count - 1; i >= 0; i--)
                    {
                        var (rect, _) = points[i];

                        _paths[author].AddLine(rect.Right, rect.Bottom,
                                               rect.Left, rect.Bottom);

                        if (i > 0)
                        {
                            var (prevRect, _) = points[i - 1];

                            _paths[author].AddBezier(rect.Left, rect.Bottom,
                                                     rect.Left - (TransitionWidth / 2), rect.Bottom,
                                                     rect.Left - (TransitionWidth / 2), prevRect.Bottom,
                                                     prevRect.Right, prevRect.Bottom);
                        }
                    }
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
            lock (_dataLock)
            {
                foreach (var author in _authorStack.Reverse<string>())
                {
                    if (_paths.ContainsKey(author) && _paths[author].IsVisible(x + _scrollBar.Value, y))
                    {
                        return author;
                    }
                }
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
                {
                    return false;
                }

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
            {
                return string.Empty;
            }

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
                {
                    return _brushes[author].Color;
                }
            }

            return Color.Transparent;
        }

        [Browsable(false)]
        public List<string> Authors
        {
            get
            {
                lock (_dataLock)
                {
                    return _authorStack;
                }
            }
        }

        public ImpactLoader.DataPoint GetAuthorInfo(string author)
        {
            lock (_dataLock)
            {
                if (_authors.ContainsKey(author))
                {
                    return _authors[author];
                }

                return new ImpactLoader.DataPoint(0, 0, 0);
            }
        }
    }
}
