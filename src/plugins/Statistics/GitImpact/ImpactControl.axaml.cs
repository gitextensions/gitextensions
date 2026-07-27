using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using AvaloniaApplication = Avalonia.Application;
using AvaloniaFontFamily = Avalonia.Media.FontFamily;
using AvaloniaPoint = Avalonia.Point;
using MediaColor = Avalonia.Media.Color;

namespace GitExtensions.Plugins.GitImpact;

public partial class ImpactControl : UserControl, IDisposable
{
    private const double BlockWidth = 60;
    private const double BlockHalfWidth = BlockWidth / 2;
    private const double TransitionWidth = 50;
    private const double TransitionHalfWidth = TransitionWidth / 2;
    private const double LinesFontSize = 10 * 96 / 72;
    private const double WeekFontSize = 8 * 96 / 72;
    private const double ScrollBarHeight = 16;
    private const double WheelScrollDistance = 120;
    private const double DarkBackgroundLuminanceThreshold = 0.35;
    private const double DarkAuthorLuminanceThreshold = 0.25;
    private const double DarkAuthorLighteningFactor = 0.35;

    private readonly Lock _dataLock = new();

    private ImpactLoader? _impactLoader;

    // <Author, <Commits, Added Lines, Deleted Lines>>
    private readonly Dictionary<string, ImpactLoader.DataPoint> _authors = [];

    // <First weekday of commit date, <Author, <Commits, Added Lines, Deleted Lines>>>
    private SortedDictionary<DateOnly, Dictionary<string, ImpactLoader.DataPoint>> _impact = [];

    // List of authors that determines the drawing order
    private readonly List<string> _authorStack = [];

    // The paths for each author
    private readonly Dictionary<string, StreamGeometry> _paths = [];

    // The brush for each author
    private readonly Dictionary<string, SolidColorBrush> _brushes = [];

    // The changed-lines-labels for each author
    private readonly Dictionary<string, List<(AvaloniaPoint point, string changeCount)>> _lineLabels = [];

    // The week-labels
    private readonly List<(AvaloniaPoint point, string date)> _weekLabels = [];

    private readonly AvaloniaFontFamily _fontFamily = new("Arial");
    private bool _disposed;

    public string SelectedAuthor { get; private set; } = string.Empty;

    public event EventHandler? Invalidated;

    public ImpactControl()
    {
        Clear();

        InitializeComponent();

        _scrollBar.Scroll += OnScroll;
        PointerWheelChanged += ImpactControl_PointerWheelChanged;
        SizeChanged += OnSizeChanged;
    }

    public void Init(IGitModule module)
    {
        _impactLoader?.Dispose();
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
            _brushes.Clear();
            _lineLabels.Clear();
            _weekLabels.Clear();
        }
    }

    public void Stop()
    {
        _impactLoader?.Stop();
    }

    private void ImpactControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        _scrollBar.Value = Math.Min(
            _scrollBar.Maximum,
            Math.Max(_scrollBar.Minimum, _scrollBar.Value + (e.Delta.Y * WheelScrollDistance)));

        // Redraw when we've scrolled
        InvalidateVisual();
        e.Handled = true;
    }

    private void OnImpactUpdate(IList<ImpactLoader.Commit> commits)
    {
        lock (_dataLock)
        {
            foreach (ImpactLoader.Commit commit in commits)
            {
                // UPDATE IMPACT

                // If week does not exist yet in the impact dictionary
                if (!_impact.TryGetValue(commit.Week, out Dictionary<string, ImpactLoader.DataPoint>? weekData))
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

    private double GetGraphWidth() => Math.Max(0, (_impact.Count * (BlockWidth + TransitionWidth)) - TransitionWidth);

    private void UpdateScrollbar()
    {
        double rightValue = Math.Max(0, _scrollBar.Maximum - _scrollBar.LargeChange - _scrollBar.Value);

        _scrollBar.Minimum = 0;
        _scrollBar.Maximum = Math.Max(0, GetGraphWidth() - Bounds.Width) * 1.1;
        _scrollBar.SmallChange = _scrollBar.Maximum / 22;
        _scrollBar.LargeChange = _scrollBar.Maximum / 11;

        _scrollBar.Value = Math.Clamp(
            _scrollBar.Maximum - _scrollBar.LargeChange - rightValue,
            _scrollBar.Minimum,
            _scrollBar.Maximum);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double graphHeight = GetGraphHeight();
        IBrush background = GetResourceBrush("GitExtensionsPanelBackgroundBrush", Brushes.White);
        context.FillRectangle(background, new Rect(0, 0, Bounds.Width, graphHeight));
        UpdateScrollbar();

        // Nothing to draw
        if (_impact.Count == 0)
        {
            // Show this cursor until we get some results painted
            Cursor = new Cursor(StandardCursorType.Wait);
            return;
        }

        // Now we have results, don't show waiting cursor
        Cursor = Cursor.Default;

        using (context.PushClip(new Rect(0, 0, Bounds.Width, graphHeight)))
        using (context.PushTransform(Matrix.CreateTranslation(-_scrollBar.Value, 0)))
        {
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
                if (_paths.TryGetValue(SelectedAuthor, out StreamGeometry? selectedAuthorPath))
                {
                    context.DrawGeometry(
                        null,
                        new Pen(GetResourceBrush("GitExtensionsWindowTextBrush", Brushes.Black), 2),
                        selectedAuthorPath);
                }

                foreach (string author in _authorStack)
                {
                    DrawAuthorLinesLabels(author);
                }

                DrawWeekLabels();
            }
        }

        void DrawAuthorContribution(string author)
        {
            if (_brushes.TryGetValue(author, out SolidColorBrush? authorBrush)
                && _paths.TryGetValue(author, out StreamGeometry? authorPath))
            {
                context.DrawGeometry(authorBrush, null, authorPath);
            }
        }

        void DrawAuthorLinesLabels(string author)
        {
            if (!_lineLabels.TryGetValue(author, out List<(AvaloniaPoint position, string changeCount)>? authorData))
            {
                return;
            }

            foreach ((AvaloniaPoint position, string changeCount) in authorData)
            {
                context.DrawText(CreateFormattedText(changeCount, LinesFontSize, Brushes.White), position);
            }
        }

        void DrawWeekLabels()
        {
            foreach ((AvaloniaPoint point, string date) in _weekLabels)
            {
                context.DrawText(CreateFormattedText(date, WeekFontSize, Brushes.Gray), point);
            }
        }
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdatePathsAndLabels();
        UpdateScrollbar();
        InvalidateVisual();
    }

    private void UpdatePathsAndLabels()
    {
        double maximumHeight = 0;
        double x = 0;
        Dictionary<string, List<(Rect rectangle, int changeCount)>> authorPoints = [];

        lock (_dataLock)
        {
            // Clear previous week labels
            _weekLabels.Clear();

            // Iterate through weeks
            foreach ((DateOnly weekDate, Dictionary<string, ImpactLoader.DataPoint> dataByAuthor) in _impact)
            {
                double y = 0;

                // Iterate through authors
                foreach ((string author, ImpactLoader.DataPoint data) in dataByAuthor.OrderByDescending(entry => entry.Value.ChangedLines))
                {
                    // Calculate week-author-rectangle
                    int changedLines = Math.Max(1, data.ChangedLines);
                    double height = Math.Max(1, Math.Round(Math.Pow(Math.Log(changedLines), 1.5) * 4));
                    Rect rectangle = new(x, y, BlockWidth, height);

                    // Add rectangle to temporary list
                    if (!authorPoints.TryGetValue(author, out List<(Rect rectangle, int changeCount)>? rectangles))
                    {
                        rectangles = [];
                        authorPoints.Add(author, rectangles);
                    }

                    rectangles.Add((rectangle, data.ChangedLines));

                    // Create a new random brush for the author if none exists yet
                    if (!_brushes.ContainsKey(author))
                    {
                        int colorValue = author.GetHashCode() | unchecked((int)0xFF000000);
                        MediaColor color = AdaptAuthorColor(MediaColor.FromUInt32(unchecked((uint)colorValue)));
                        _brushes.Add(author, new SolidColorBrush(color));
                    }

                    // Increase y for next block
                    y += rectangle.Height + 2;
                }

                // Remember total height of largest week
                maximumHeight = Math.Max(maximumHeight, y);

                // Add week date label
                string formattedWeekDate = weekDate.ToShortDateString();

                _weekLabels.Add((new AvaloniaPoint(x + BlockHalfWidth, y), formattedWeekDate));

                // Increase x for next week
                x += BlockWidth + TransitionWidth;
            }

            // Pre-calculate height scale factor
            double heightFactor = maximumHeight > 0 ? 0.9 * GetGraphHeight() / maximumHeight : 1.0;

            // Scale week label coordinates
            for (int i = 0; i < _weekLabels.Count; i++)
            {
                (AvaloniaPoint point, string formattedWeekDate) = _weekLabels[i];

                AvaloniaPoint adjustedPoint = new(point.X, point.Y * heightFactor);

                FormattedText formattedText = CreateFormattedText(formattedWeekDate, WeekFontSize, Brushes.Gray);
                AvaloniaPoint centeredAdjustedPoint = new(
                    adjustedPoint.X - (formattedText.Width / 2),
                    adjustedPoint.Y + (formattedText.Height / 2));

                _weekLabels[i] = (centeredAdjustedPoint, formattedWeekDate);
            }

            // Clear previous paths
            ClearPaths();

            // Clear previous labels
            _lineLabels.Clear();

            // Add points to each author's path
            foreach ((string author, List<(Rect rectangle, int changeCount)> points) in authorPoints)
            {
                // Scale heights
                for (int i = 0; i < points.Count; i++)
                {
                    (Rect unscaledRect, int changeCount) = points[i];

                    Rect rectangle = new(
                        unscaledRect.Left,
                        unscaledRect.Top * heightFactor,
                        unscaledRect.Width,
                        Math.Max(1, unscaledRect.Height * heightFactor));

                    points[i] = (rectangle, changeCount);

                    // Add lines-changed-labels
                    if (!_lineLabels.TryGetValue(author, out List<(AvaloniaPoint point, string changeCount)>? authorLineLabels))
                    {
                        _lineLabels.Add(author, authorLineLabels = []);
                    }

                    if (rectangle.Height > LinesFontSize * 1.5)
                    {
                        AvaloniaPoint adjustedPoint = new(
                            rectangle.Left + BlockHalfWidth,
                            rectangle.Top + (rectangle.Height / 2));

                        string changeCountText = changeCount.ToString(CultureInfo.CurrentCulture);
                        FormattedText formattedText = CreateFormattedText(changeCountText, LinesFontSize, Brushes.White);
                        AvaloniaPoint centeredAdjustedPosition = new(
                            adjustedPoint.X - (formattedText.Width / 2),
                            adjustedPoint.Y - (formattedText.Height / 2));

                        authorLineLabels.Add((centeredAdjustedPosition, changeCountText));
                    }
                }

                StreamGeometry authorPath = CreateAuthorPath(points);
                _paths.Add(author, authorPath);
            }
        }
    }

    private static StreamGeometry CreateAuthorPath(List<(Rect rectangle, int changeCount)> points)
    {
        StreamGeometry authorPath = new();
        using StreamGeometryContext path = authorPath.Open();

        (Rect firstRect, int _) = points[0];

        // Left border
        path.BeginFigure(new AvaloniaPoint(firstRect.Left, firstRect.Bottom), isFilled: true);
        path.LineTo(new AvaloniaPoint(firstRect.Left, firstRect.Top));

        // Top borders
        for (int i = 0; i < points.Count; i++)
        {
            (Rect rectangle, int _) = points[i];

            path.LineTo(new AvaloniaPoint(rectangle.Right, rectangle.Top));

            if (i < points.Count - 1)
            {
                (Rect nextRect, int _) = points[i + 1];

                path.CubicBezierTo(
                    new AvaloniaPoint(rectangle.Right + TransitionHalfWidth, rectangle.Top),
                    new AvaloniaPoint(rectangle.Right + TransitionHalfWidth, nextRect.Top),
                    new AvaloniaPoint(nextRect.Left, nextRect.Top));
            }
        }

        (Rect lastRect, int _) = points[^1];

        // Right border
        path.LineTo(new AvaloniaPoint(lastRect.Right, lastRect.Bottom));

        // Bottom borders
        for (int i = points.Count - 1; i >= 0; i--)
        {
            (Rect rectangle, int _) = points[i];

            path.LineTo(new AvaloniaPoint(rectangle.Left, rectangle.Bottom));

            if (i > 0)
            {
                (Rect previousRect, int _) = points[i - 1];

                path.CubicBezierTo(
                    new AvaloniaPoint(rectangle.Left - TransitionHalfWidth, rectangle.Bottom),
                    new AvaloniaPoint(rectangle.Left - TransitionHalfWidth, previousRect.Bottom),
                    new AvaloniaPoint(previousRect.Right, previousRect.Bottom));
            }
        }

        path.EndFigure(isClosed: true);
        return authorPath;
    }

    /// <summary>
    /// Determines if the given coordinates are belonging to any author.
    /// </summary>
    /// <param name="x">x coordinate.</param>
    /// <param name="y">y coordinate.</param>
    /// <returns>
    /// <see langword="true"/> if author has changed and graph should be redrawn;
    /// <see langword="false"/>, otherwise.
    /// </returns>
    public bool TrySetAuthorByScreenPosition(int x, int y)
    {
        lock (_dataLock)
        {
            for (int i = _authorStack.Count - 1; i >= 0; i--)
            {
                string author = _authorStack[i];
                if (_paths.TryGetValue(author, out StreamGeometry? authorPath)
                    && authorPath.FillContains(new AvaloniaPoint(x + _scrollBar.Value, y)))
                {
                    if (SelectedAuthor != author)
                    {
                        SelectedAuthor = author;
                        return true;
                    }

                    return false;
                }
            }
        }

        return false;
    }

    private void OnScroll(object? sender, ScrollEventArgs e)
    {
        // Redraw when we've scrolled
        InvalidateVisual();
    }

    public MediaColor GetAuthorColor(string author)
    {
        lock (_dataLock)
        {
            if (_brushes.TryGetValue(author, out SolidColorBrush? brush))
            {
                return brush.Color;
            }
        }

        return Colors.Transparent;
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

    public void Invalidate()
    {
        InvalidateVisual();
        Invalidated?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ImpactLoader? impactLoader;
        lock (_dataLock)
        {
            impactLoader = _impactLoader;
            _impactLoader = null;
        }

        impactLoader?.Dispose();
        lock (_dataLock)
        {
            _brushes.Clear();
            ClearPaths();
        }
    }

    private void ClearPaths()
    {
        _paths.Clear();
    }

    private FormattedText CreateFormattedText(string text, double fontSize, IBrush brush)
        => new(
            text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface(_fontFamily),
            fontSize,
            brush);

    private double GetGraphHeight() => Math.Max(1, Bounds.Height - ScrollBarHeight);

    private IBrush GetResourceBrush(string key, IBrush fallback)
        => AvaloniaApplication.Current?.TryGetResource(key, ActualThemeVariant, out object? resource) == true
            && resource is IBrush brush
                ? brush
                : fallback;

    private MediaColor AdaptAuthorColor(MediaColor color)
    {
        IBrush backgroundBrush = GetResourceBrush("GitExtensionsPanelBackgroundBrush", Brushes.White);
        if (backgroundBrush is not ISolidColorBrush solidBackground
            || GetLuminance(solidBackground.Color) >= DarkBackgroundLuminanceThreshold
            || GetLuminance(color) >= DarkAuthorLuminanceThreshold)
        {
            return color;
        }

        return MediaColor.FromArgb(
            color.A,
            (byte)(color.R + ((255 - color.R) * DarkAuthorLighteningFactor)),
            (byte)(color.G + ((255 - color.G) * DarkAuthorLighteningFactor)),
            (byte)(color.B + ((255 - color.B) * DarkAuthorLighteningFactor)));

        static double GetLuminance(MediaColor value)
            => ((0.2126 * value.R) + (0.7152 * value.G) + (0.0722 * value.B)) / 255;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ImpactControl control)
    {
        public int PathCount => control._paths.Count;
        public double GraphWidth => control.GetGraphWidth();

        public void AddCommits(IList<ImpactLoader.Commit> commits)
            => control.OnImpactUpdate(commits);

        public AvaloniaPoint GetAuthorHitPoint(string author)
            => control._paths[author].Bounds.Center;
    }
}
