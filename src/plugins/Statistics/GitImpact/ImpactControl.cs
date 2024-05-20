using System.ComponentModel;
using System.Drawing.Drawing2D;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;

namespace GitExtensions.Plugins.GitImpact
{
    public partial class ImpactControl : UserControl
    {
        private static readonly int BlockWidth = DpiUtil.Scale(60);
        private static readonly int BlockHalfWidth = BlockWidth / 2;
        private static readonly int TransitionWidth = DpiUtil.Scale(50);
        private static readonly int TransitionHalfWidth = TransitionWidth / 2;

        private const int LinesFontSize = 10;
        private const int WeekFontSize = 8;

        private readonly object _dataLock = new();

        private ImpactLoader? _impactLoader;

        // <Author, <Commits, Added Lines, Deleted Lines>>
        private readonly Dictionary<string, ImpactLoader.DataPoint> _authors = [];

        // <First weekday of commit date, <Author, <Commits, Added Lines, Deleted Lines>>>
        private SortedDictionary<DateOnly, Dictionary<string, ImpactLoader.DataPoint>> _impact = [];

        // List of authors that determines the drawing order
        private readonly List<string> _authorStack = [];

        // The paths for each author
        private readonly Dictionary<string, GraphicsPath> _paths = [];

        // The brush for each author
        private readonly Dictionary<string, SolidBrush> _brushes = [];

        // The changed-lines-labels for each author
        private readonly Dictionary<string, List<(PointF point, string changeCount)>> _lineLabels = [];

        // The week-labels
        private readonly List<(PointF point, string date)> _weekLabels = [];

        private readonly Font _weekFont = new("Arial", WeekFontSize);
        private readonly Brush _weekBrush = Brushes.Gray;
        private readonly Font _linesFont = new("Arial", LinesFontSize);
        private readonly Brush _linesBrush = Brushes.White;
        private readonly Pen _selectedAuthorPen = new(SystemColors.WindowText, 2);

        public string SelectedAuthor { get; private set; } = string.Empty;

        public ImpactControl()
        {
            Clear();

            InitializeComponent();

            // Set DoubleBuffer flag for flicker-free drawing
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            MouseWheel += ImpactControl_MouseWheel;
            Disposed += ImpactControl_Disposed;
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
                _authors.Clear();
                _impact.Clear();

                _authorStack.Clear();
                ClearPaths();
                ClearBrushes();
                _lineLabels.Clear();
                _weekLabels.Clear();
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

        private void OnImpactUpdate(IList<ImpactLoader.Commit> commits)
        {
            lock (_dataLock)
            {
                foreach (ImpactLoader.Commit commit in commits)
                {
                    // UPDATE IMPACT

                    // If week does not exist yet in the impact dictionary
                    if (!_impact.TryGetValue(commit.Week, out Dictionary<string, ImpactLoader.DataPoint> weekData))
                    {
                        // Create it
                        _impact.Add(commit.Week, weekData = []);
                    }

                    // If author does not exist yet for this week in the impact dictionary
                    if (!weekData.TryGetValue(commit.Author, out ImpactLoader.DataPoint authorWeekData))
                    {
                        // Create it
                        weekData.Add(commit.Author, commit.Data);
                    }
                    else
                    {
                        // Otherwise just add the changes
                        weekData[commit.Author] = authorWeekData + commit.Data;
                    }

                    // UPDATE AUTHORS

                    // If author does not exist yet in the authors dictionary
                    if (!_authors.TryGetValue(commit.Author, out ImpactLoader.DataPoint authorData))
                    {
                        // Create it
                        _authors.Add(commit.Author, commit.Data);
                    }
                    else
                    {
                        // Otherwise just add the changes
                        _authors[commit.Author] = authorData + commit.Data;
                    }

                    // UPDATE AUTHOR STACK

                    // If author does not exist yet in the author_stack
                    if (!_authorStack.Contains(commit.Author))
                    {
                        // Add it to the front (drawn first)
                        _authorStack.Insert(0, commit.Author);
                    }
                }

                // Add authors to intermediate weeks where they didn't create commits
                ImpactLoader.AddIntermediateEmptyWeeks(ref _impact, _authors.Keys);
            }

            UpdatePathsAndLabels();
            Invalidate();
        }

        public void UpdateData()
        {
            if (_impactLoader is not null)
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

        private int GetGraphWidth() => Math.Max(0, (_impact.Count * (BlockWidth + TransitionWidth)) - TransitionWidth);

        private void UpdateScrollbar()
        {
            int rightValue = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange - _scrollBar.Value);

            _scrollBar.Minimum = 0;
            _scrollBar.Maximum = (int)(Math.Max(0, GetGraphWidth() - ClientSize.Width) * 1.1);
            _scrollBar.SmallChange = _scrollBar.Maximum / 22;
            _scrollBar.LargeChange = _scrollBar.Maximum / 11;

            _scrollBar.Value = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange - rightValue);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            // White background
            e.Graphics.Clear(SystemColors.Window);
            UpdateScrollbar();

            // Nothing to draw
            if (_impact.Count == 0)
            {
                // Show this cursor until we get some results painted
                UseWaitCursor = true;
                return;
            }

            // Now we have results, don't show waiting cursor
            UseWaitCursor = false;

            // Activate AntiAliasing
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // "Scroll" to the right position
            e.Graphics.TranslateTransform(-_scrollBar.Value, 0);

            lock (_dataLock)
            {
                // Draw paths in order of the author_stack
                // Default: person with least number of changed lines first, others on top
                foreach (string author in _authorStack)
                {
                    if (author == SelectedAuthor)
                    {
                        continue;
                    }

                    DrawAuthorContribution(author);
                }

                // Draw selected author data
                DrawAuthorContribution(SelectedAuthor);
                if (_paths.TryGetValue(SelectedAuthor, out GraphicsPath? selectedAuthorPath))
                {
                    e.Graphics.DrawPath(_selectedAuthorPen, selectedAuthorPath);
                }

                foreach (string author in _authorStack)
                {
                    DrawAuthorLinesLabels(author);
                }

                void DrawAuthorContribution(string author)
                {
                    if (_brushes.TryGetValue(author, out SolidBrush authorBrush) && _paths.TryGetValue(author, out GraphicsPath? authorPath))
                    {
                        e.Graphics.FillPath(authorBrush, authorPath);
                    }
                }
            }

            DrawWeekLabels();

            void DrawAuthorLinesLabels(string author)
            {
                if (!_lineLabels.TryGetValue(author, out List<(PointF position, string changeCount)> authorData))
                {
                    return;
                }

                foreach ((PointF position, string changeCount) in authorData)
                {
                    e.Graphics.DrawString(changeCount, _linesFont, _linesBrush, position);
                }
            }

            void DrawWeekLabels()
            {
                foreach ((PointF point, string date) in _weekLabels)
                {
                    e.Graphics.DrawString(date, _weekFont, _weekBrush, point);
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
            Dictionary<string, List<(Rectangle, int changeCount)>> author_points_dict = [];

            using Graphics g = CreateGraphics();

            lock (_dataLock)
            {
                // Clear previous week labels
                _weekLabels.Clear();

                // Iterate through weeks
                foreach ((DateOnly weekDate, Dictionary<string, ImpactLoader.DataPoint> dataByAuthor) in _impact)
                {
                    int y = 0;

                    // Iterate through authors
                    foreach ((string author, ImpactLoader.DataPoint data) in from entry in dataByAuthor orderby entry.Value.ChangedLines descending select entry)
                    {
                        // Calculate week-author-rectangle
                        int height = Math.Max(1, (int)Math.Round(Math.Pow(Math.Log(data.ChangedLines), 1.5) * 4));
                        Rectangle rc = new(x, y, BlockWidth, height);

                        // Add rectangle to temporary list
                        if (!author_points_dict.ContainsKey(author))
                        {
                            author_points_dict.Add(author, new List<(Rectangle, int)>());
                        }

                        author_points_dict[author].Add((rc, data.ChangedLines));

                        // Create a new random brush for the author if none exists yet
                        if (!_brushes.ContainsKey(author))
                        {
                            Color color = Color.FromArgb((int)(author.GetHashCode() | 0xFF000000));
                            _brushes.Add(author, new SolidBrush(color));
                        }

                        // Increase y for next block
                        y += rc.Height + 2;
                    }

                    // Remember total height of largest week
                    h_max = Math.Max(h_max, y);

                    // Add week date label
                    string formatedWeekDate = weekDate.ToShortDateString();

                    _weekLabels.Add((new PointF(x + BlockHalfWidth, y), formatedWeekDate));

                    // Increase x for next week
                    x += BlockWidth + TransitionWidth;
                }

                // Pre-calculate height scale factor
                double height_factor = 0.9 * Height / h_max;

                // Scale week label coordinates
                for (int i = 0; i < _weekLabels.Count; i++)
                {
                    (PointF point, string formatedWeekDate) = _weekLabels[i];

                    PointF adjustedPoint = new(point.X, point.Y * (float)height_factor);

                    SizeF sz = g.MeasureString(formatedWeekDate, _weekFont);
                    PointF centeredAdjustedPoint = new(adjustedPoint.X - (sz.Width / 2), adjustedPoint.Y + (sz.Height / 2));

                    _weekLabels[i] = (centeredAdjustedPoint, formatedWeekDate);
                }

                // Clear previous paths
                ClearPaths();

                // Clear previous labels
                _lineLabels.Clear();

                // Add points to each author's GraphicsPath
                foreach ((string author, List<(Rectangle, int changeCount)> points) in author_points_dict)
                {
                    // Scale heights
                    for (int i = 0; i < points.Count; i++)
                    {
                        (Rectangle unscaledRect, int changeCount) = points[i];

                        Rectangle rect = new(unscaledRect.Left, (int)(unscaledRect.Top * height_factor),
                            unscaledRect.Width, Math.Max(1, (int)(unscaledRect.Height * height_factor)));

                        points[i] = (rect, changeCount);

                        // Add lines-changed-labels
                        if (!_lineLabels.TryGetValue(author, out List<(PointF point, string changeCount)> authorLineLabels))
                        {
                            _lineLabels.Add(author, authorLineLabels = new List<(PointF, string)>());
                        }

                        if (rect.Height > LinesFontSize * 1.5)
                        {
                            PointF adjustedPoint = new(rect.Left + BlockHalfWidth, rect.Top + (rect.Height / 2));

                            string changeCountText = changeCount.ToString();
                            SizeF sz = g.MeasureString(changeCountText, _linesFont);
                            PointF centeredAdjustedPosition = new(adjustedPoint.X - (sz.Width / 2), adjustedPoint.Y - (sz.Height / 2));

                            authorLineLabels.Add((centeredAdjustedPosition, changeCountText));
                        }
                    }

                    // Will be disposed when ClearPaths() is called
                    GraphicsPath authorGraphicsPath = new();
                    _paths.Add(author, authorGraphicsPath);

                    (Rectangle firstRect, int _) = points[0];

                    // Left border
                    authorGraphicsPath.AddLine(firstRect.Left, firstRect.Bottom,
                                           firstRect.Left, firstRect.Top);

                    // Top borders
                    for (int i = 0; i < points.Count; i++)
                    {
                        (Rectangle rect, int _) = points[i];

                        authorGraphicsPath.AddLine(rect.Left, rect.Top,
                                                   rect.Right, rect.Top);

                        if (i < points.Count - 1)
                        {
                            (Rectangle nextRect, int _) = points[i + 1];

                            authorGraphicsPath.AddBezier(rect.Right, rect.Top,
                                                         rect.Right + TransitionHalfWidth, rect.Top,
                                                         rect.Right + TransitionHalfWidth, nextRect.Top,
                                                         nextRect.Left, nextRect.Top);
                        }
                    }

                    (Rectangle lastRect, int _) = points[^1];

                    // Right border
                    authorGraphicsPath.AddLine(lastRect.Right, lastRect.Top,
                                               lastRect.Right, lastRect.Bottom);

                    // Bottom borders
                    for (int i = points.Count - 1; i >= 0; i--)
                    {
                        (Rectangle rect, int _) = points[i];

                        authorGraphicsPath.AddLine(rect.Right, rect.Bottom,
                                                   rect.Left, rect.Bottom);

                        if (i > 0)
                        {
                            (Rectangle prevRect, int _) = points[i - 1];

                            authorGraphicsPath.AddBezier(rect.Left, rect.Bottom,
                                                     rect.Left - TransitionHalfWidth, rect.Bottom,
                                                     rect.Left - TransitionHalfWidth, prevRect.Bottom,
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
        /// <returns>true if author has changed and graph should be redrawn
        /// false, otherwise</returns>
        public bool TrySetAuthorByScreenPosition(int x, int y)
        {
            lock (_dataLock)
            {
                for (int i = _authorStack.Count - 1; i >= 0; i--)
                {
                    string author = _authorStack[i];
                    if (_paths.TryGetValue(author, out GraphicsPath authorGraphicsPath)
                        && authorGraphicsPath.IsVisible(x + _scrollBar.Value, y))
                    {
                        if (SelectedAuthor != author)
                        {
                            SelectedAuthor = author;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
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
                if (_brushes.TryGetValue(author, out SolidBrush? brush))
                {
                    return brush.Color;
                }
            }

            return Color.Transparent;
        }

        [Browsable(false)]
        public List<string> Authors => _authorStack;

        public ImpactLoader.DataPoint GetAuthorInfo(string author)
        {
            lock (_dataLock)
            {
                if (_authors.TryGetValue(author, out ImpactLoader.DataPoint info))
                {
                    return info;
                }

                return new ImpactLoader.DataPoint(0, 0, 0);
            }
        }

        private void ImpactControl_Disposed(object? sender, EventArgs e)
        {
            lock (_dataLock)
            {
                _impactLoader?.Dispose();

                ClearBrushes();
                ClearPaths();
            }
        }

        private void ClearBrushes()
        {
            foreach (SolidBrush brush in _brushes.Values)
            {
                brush.Dispose();
            }

            _brushes.Clear();
        }

        private void ClearPaths()
        {
            foreach (GraphicsPath path in _paths.Values)
            {
                path.Dispose();
            }

            _paths.Clear();
        }
    }
}
