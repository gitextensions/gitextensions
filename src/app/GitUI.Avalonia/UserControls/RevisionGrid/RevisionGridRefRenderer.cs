using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using DrawingColor = System.Drawing.Color;
using MediaColor = Avalonia.Media.Color;
using Point = Avalonia.Point;
using Size = Avalonia.Size;
using ThemingColorHelper = GitExtUtils.GitUI.Theming.ColorHelper;

namespace GitUI.UserControls.RevisionGrid;

/// <summary>
///  Creates and renders revision-grid ref labels with the same edge shapes and head
///  indicators as the WinForms <c>RevisionGridRefRenderer</c>.
/// </summary>
internal static class RevisionGridRefRenderer
{
    private const double MarginRight = 5;
    private static readonly double[] _dashPattern = [4, 4];
    private static readonly Point[] _arrowPoints = new Point[4];
    private static readonly DashStyle DashedLine = new(_dashPattern, 0);

    private static double PaddingTopBottom => 2;

    // Pixel radius for the rounded corners of ref label capsules.
    private static double RefLabelCornerRadius => 5;

    // Pixel width of the highlight frame drawn around a hovered ref label,
    // and the left-side offset used when drawing the nestled remote label.
    private static double RefLabelHighlightWidth => 1;

    private static double PointWidth(double height) => height / 2;

    private static double PaddingLeftRight(string name) => string.IsNullOrEmpty(name) ? 1 : 4;

    /// <summary>
    ///  Creates the ordered controls for a revision's refs, nesting a local branch with its
    ///  tracked remote when both point at the same commit.
    /// </summary>
    public static IReadOnlyList<Control> CreateLabels(IReadOnlyList<IGitRef> refs)
        => CreateLabels(
            refs,
            showTags: true,
            showRemoteBranches: true,
            fill: false,
            getVirtualRef: null,
            superprojectRefs: null);

    internal static IReadOnlyList<Control> CreateLabels(
        IReadOnlyList<IGitRef> refs,
        bool showTags,
        bool showRemoteBranches,
        bool fill,
        Func<IGitRef, IGitRef?>? getVirtualRef,
        IReadOnlySet<string>? superprojectRefs)
    {
        IReadOnlyList<IGitRef> sortedRefs = SortRefs(
            refs.Where(gitRef => (!gitRef.IsTag || showTags)
                && (!gitRef.IsRemote || showRemoteBranches)));
        Dictionary<string, IGitRef> trackedRemotes = BuildTrackedRemoteMap(sortedRefs);
        List<Control> labels = [];

        foreach (IGitRef gitRef in sortedRefs)
        {
            if (trackedRemotes.ContainsValue(gitRef))
            {
                continue;
            }

            if (gitRef.IsHead && trackedRemotes.TryGetValue(gitRef.Name, out IGitRef? remote))
            {
                RefLabelControl localLabel = CreateLabel(
                    gitRef,
                    gitRef.Name,
                    RefLabelShape.PointRight,
                    fill,
                    dashed: superprojectRefs?.Contains(gitRef.CompleteName) == true);
                RefLabelControl remoteLabel = CreateLabel(
                    remote,
                    remote.LocalName == gitRef.Name ? remote.Remote : remote.Name,
                    RefLabelShape.NotchLeft,
                    fill,
                    showHeadIndicator: false,
                    dashed: superprojectRefs?.Contains(remote.CompleteName) == true);
                labels.Add(new NestledRefLabelPanel(localLabel, remoteLabel));
                continue;
            }

            if (getVirtualRef?.Invoke(gitRef) is IGitRef virtualRef)
            {
                (RefLabelShape refShape, RefLabelShape virtualShape) = gitRef.IsRemote
                    ? (RefLabelShape.NotchRight, RefLabelShape.PointLeft)
                    : (RefLabelShape.PointRight, RefLabelShape.NotchLeft);
                labels.Add(new NestledRefLabelPanel(
                    CreateLabel(
                        gitRef,
                        gitRef.Name,
                        refShape,
                        fill,
                        dashed: superprojectRefs?.Contains(gitRef.CompleteName) == true),
                    CreateLabel(
                        virtualRef,
                        virtualRef.Name,
                        virtualShape,
                        fill,
                        showHeadIndicator: false,
                        dashed: true)));
                continue;
            }

            labels.Add(CreateLabel(
                gitRef,
                gitRef.Name,
                gitRef.IsTag ? RefLabelShape.PointLeft : RefLabelShape.Rect,
                fill,
                dashed: superprojectRefs?.Contains(gitRef.CompleteName) == true));
        }

        return labels;
    }

    /// <summary>
    ///  Arranges the tracked remote's notch over the local branch's point. This is the
    ///  layout equivalent of the WinForms renderer resetting its drawing offset.
    /// </summary>
    internal sealed class NestledRefLabelPanel : Panel
    {
        private readonly RefLabelControl _localLabel;
        private readonly RefLabelControl _remoteLabel;

        public NestledRefLabelPanel(
            RefLabelControl localLabel,
            RefLabelControl remoteLabel)
        {
            _localLabel = localLabel;
            _remoteLabel = remoteLabel;
            Children.Add(localLabel);
            Children.Add(remoteLabel);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _localLabel.Measure(availableSize);
            _remoteLabel.Measure(availableSize);
            double overlap = MarginRight + _remoteLabel.PointWidth - 1;
            return new Size(
                _localLabel.DesiredSize.Width + _remoteLabel.DesiredSize.Width - overlap,
                Math.Max(_localLabel.DesiredSize.Height, _remoteLabel.DesiredSize.Height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _localLabel.Arrange(new Rect(new Point(0, 0), _localLabel.DesiredSize));
            double remoteX = _localLabel.DesiredSize.Width
                - MarginRight
                - _remoteLabel.PointWidth
                + 1;
            _remoteLabel.Arrange(new Rect(
                new Point(remoteX, 0),
                _remoteLabel.DesiredSize));
            return finalSize;
        }
    }

    private static Dictionary<string, IGitRef> BuildTrackedRemoteMap(IReadOnlyList<IGitRef> refs)
    {
        IReadOnlyList<IGitRef> localBranches = [.. refs.Where(gitRef => gitRef.IsHead)];
        Dictionary<string, IGitRef> remoteByLocal = [];

        foreach (IGitRef remote in refs.Where(gitRef => gitRef.IsRemote))
        {
            foreach (IGitRef local in localBranches)
            {
                if (local.MergeWith == remote.LocalName
                    && local.TrackingRemote == remote.Remote)
                {
                    if (!remoteByLocal.TryAdd(local.LocalName, remote))
                    {
                        throw new InvalidOperationException(
                            $"Multiple remote refs claim they are tracked by local branch '{local.LocalName}'.");
                    }
                }
            }
        }

        return remoteByLocal;
    }

    internal static RefLabelControl CreateLabel(
        IGitRef gitRef,
        string label,
        RefLabelShape shape,
        bool fill,
        bool showHeadIndicator = true,
        bool dashed = false)
        => new(
            gitRef,
            label,
            GetBrushResourceKey(gitRef),
            showHeadIndicator && gitRef.IsSelected
                ? RefLabelIcon.Head
                : showHeadIndicator && gitRef.IsSelectedHeadMergeSource
                    ? RefLabelIcon.HeadMergeSource
                    : RefLabelIcon.None,
            shape,
            fill,
            dashed,
            GetRemoteRefBrush(gitRef))
        {
            FontWeight = gitRef.IsSelected ? FontWeight.Bold : FontWeight.Normal,
            VerticalAlignment = VerticalAlignment.Center,
        };

    internal static RefLabelControl CreateSpecialLabel(
        string label,
        RefLabelIcon icon,
        bool dashed = true)
        => new(
            gitRef: null,
            label,
            "GitExtensionsOtherRefBrush",
            icon,
            RefLabelShape.Rect,
            fill: false,
            dashed);

    private static IBrush? GetRemoteRefBrush(IGitRef gitRef)
    {
        IGitModule? module = gitRef.Module;
        if (!gitRef.IsRemote
            || string.IsNullOrEmpty(gitRef.Remote)
            || module?.GetRemoteColors() is not { } remoteColors
            || !remoteColors.TryGetValue(gitRef.Remote, out DrawingColor color))
        {
            return null;
        }

        return new SolidColorBrush(MediaColor.FromArgb(color.A, color.R, color.G, color.B));
    }

    private static string GetBrushResourceKey(IGitRef gitRef)
    {
        if (gitRef.IsTag)
        {
            return "GitExtensionsTagRefBrush";
        }

        if (gitRef.IsHead)
        {
            return "GitExtensionsBranchRefBrush";
        }

        if (gitRef.IsRemote)
        {
            return "GitExtensionsRemoteBranchRefBrush";
        }

        return "GitExtensionsOtherRefBrush";
    }

    private static IReadOnlyList<IGitRef> SortRefs(IEnumerable<IGitRef> refs)
    {
        List<IGitRef> sortedRefs = [.. refs];
        sortedRefs.Sort(CompareRefs);
        return sortedRefs;

        static int CompareRefs(IGitRef left, IGitRef right)
        {
            int result = GetRank(left).CompareTo(GetRank(right));
            return result == 0
                ? string.Compare(left.Name, right.Name, StringComparison.Ordinal)
                : result;
        }

        static int GetRank(IGitRef gitRef)
        {
            if (gitRef.IsBisect)
            {
                return 0;
            }

            if (gitRef.IsSelected)
            {
                return 1;
            }

            if (gitRef.IsSelectedHeadMergeSource)
            {
                return 2;
            }

            if (gitRef.IsHead)
            {
                return 3;
            }

            if (gitRef.IsRemote)
            {
                return 4;
            }

            return 5;
        }
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose left edge is a concave '>' notch
    ///  that exactly fits the convex point tip of a preceding capsule.
    /// </summary>
    private static StreamGeometry CreateNotchLeftRoundRectPath(Rect bounds, double radius, double pointWidth)
    {
        double left = bounds.Left;
        double top = bounds.Top;
        double right = bounds.Right;
        double bottom = bounds.Bottom;
        double midY = bounds.Center.Y;

        // The notch corners are at the leftmost pixels; the notch tip is indented by pointWidth.
        return CreatePath(path =>
        {
            path.BeginFigure(new Point(left, top), isFilled: true);
            path.LineTo(new Point(left + pointWidth, midY)); // top notch corner → indented tip
            path.LineTo(new Point(left, bottom)); // indented tip → bottom notch corner
            path.LineTo(new Point(right - radius, bottom));
            path.QuadraticBezierTo(new Point(right, bottom), new Point(right, bottom - radius)); // bottom-right arc
            path.LineTo(new Point(right, top + radius));
            path.QuadraticBezierTo(new Point(right, top), new Point(right - radius, top)); // top-right arc
        });
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose right edge is a concave '&lt;' notch
    ///  that exactly fits the convex point tip of a following capsule.
    /// </summary>
    private static StreamGeometry CreateNotchRightRoundRectPath(Rect bounds, double radius, double pointWidth)
    {
        double left = bounds.Left;
        double top = bounds.Top;
        double right = bounds.Right;
        double bottom = bounds.Bottom;
        double midY = bounds.Center.Y;

        // The notch corners are at the rightmost pixels; the notch tip is indented by pointWidth.
        return CreatePath(path =>
        {
            path.BeginFigure(new Point(left + radius, top), isFilled: true); // top-left arc
            path.LineTo(new Point(right, top));
            path.LineTo(new Point(right - pointWidth, midY)); // top notch corner → indented tip
            path.LineTo(new Point(right, bottom)); // indented tip → bottom notch corner
            AddBottomAndLeft(path, left, bottom, top, radius); // bottom-left arc
        });
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose left edge is a convex '&lt;' point
    ///  that protrudes leftward, so it visually connects to a nestled preceding label.
    /// </summary>
    private static StreamGeometry CreatePointLeftRoundRectPath(Rect bounds, double radius, double pointWidth)
    {
        double left = bounds.Left;
        double top = bounds.Top;
        double right = bounds.Right;
        double bottom = bounds.Bottom;
        double midY = bounds.Center.Y;

        // The point tip is at the leftmost pixel; the top/bottom corners step back by pointWidth.
        return CreatePath(path =>
        {
            path.BeginFigure(new Point(left, midY), isFilled: true); // tip → top-left corner
            path.LineTo(new Point(left + pointWidth, top));
            path.LineTo(new Point(right - radius, top));
            AddRight(path, right, top, bottom, radius); // top-right arc, bottom-right arc
            path.LineTo(new Point(left + pointWidth, bottom)); // bottom-left corner → tip
        });
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose right edge is a convex '&gt;' point
    ///  instead of a rounded cap, so it visually connects to a nestled following label.
    /// </summary>
    private static StreamGeometry CreatePointRightRoundRectPath(Rect bounds, double radius, double pointWidth)
    {
        double left = bounds.Left;
        double top = bounds.Top;
        double right = bounds.Right;
        double bottom = bounds.Bottom;
        double midY = bounds.Center.Y;

        // The point tip is at the rightmost pixel; the top/bottom corners step back by pointWidth.
        return CreatePath(path =>
        {
            path.BeginFigure(new Point(left + radius, top), isFilled: true); // top-left arc
            path.LineTo(new Point(right - pointWidth, top));
            path.LineTo(new Point(right, midY)); // top-right corner → tip
            path.LineTo(new Point(right - pointWidth, bottom)); // tip → bottom-right corner
            AddBottomAndLeft(path, left, bottom, top, radius); // bottom-left arc
        });
    }

    private static StreamGeometry CreateRoundRectPath(Rect bounds, double radius)
        => CreatePath(path =>
        {
            path.BeginFigure(new Point(bounds.Left + radius, bounds.Top), isFilled: true);
            path.LineTo(new Point(bounds.Right - radius, bounds.Top));
            AddRight(path, bounds.Right, bounds.Top, bounds.Bottom, radius);
            AddBottomAndLeft(path, bounds.Left, bounds.Bottom, bounds.Top, radius);
        });

    private static StreamGeometry CreatePath(Action<StreamGeometryContext> draw)
    {
        StreamGeometry geometry = new();
        using StreamGeometryContext path = geometry.Open();
        draw(path);
        path.EndFigure(isClosed: true);
        return geometry;
    }

    private static void AddRight(
        StreamGeometryContext path,
        double right,
        double top,
        double bottom,
        double radius)
    {
        path.QuadraticBezierTo(
            new Point(right, top),
            new Point(right, top + radius));
        path.LineTo(new Point(right, bottom - radius));
        path.QuadraticBezierTo(
            new Point(right, bottom),
            new Point(right - radius, bottom));
    }

    private static void AddBottomAndLeft(
        StreamGeometryContext path,
        double left,
        double bottom,
        double top,
        double radius)
    {
        path.LineTo(new Point(left + radius, bottom));
        path.QuadraticBezierTo(
            new Point(left, bottom),
            new Point(left, bottom - radius));
        path.LineTo(new Point(left, top + radius));
        path.QuadraticBezierTo(
            new Point(left, top),
            new Point(left + radius, top));
    }

    private static void DrawArrow(
        DrawingContext context,
        IBrush brush,
        Rect bounds,
        double xOffset,
        bool filled)
    {
        double x = bounds.X + xOffset + 4;
        double y = bounds.Y + 3;
        double height = bounds.Height - 6;
        double width = height / 2;
        _arrowPoints[0] = new Point(x, y);
        _arrowPoints[1] = new Point(x + width, y + (height / 2));
        _arrowPoints[2] = new Point(x, y + height);
        _arrowPoints[3] = new Point(x, y);
        StreamGeometry arrow = new();
        using (StreamGeometryContext path = arrow.Open())
        {
            path.BeginFigure(_arrowPoints[0], isFilled: filled);
            path.LineTo(_arrowPoints[1]);
            path.LineTo(_arrowPoints[2]);
            path.EndFigure(isClosed: true);
        }

        context.DrawGeometry(filled ? brush : null, filled ? null : new Pen(brush, 1), arrow);
    }

    private static RefLabelIcon GetEffectiveIcon(RefLabelIcon icon)
        => icon is RefLabelIcon.Head or RefLabelIcon.HeadMergeSource
            ? icon
            : RefLabelIcon.None;

    /// <summary>
    ///  One custom-drawn ref label. Keeping the WinForms shape vocabulary here avoids
    ///  encoding mutually dependent point/notch geometry in generic Border styles.
    /// </summary>
    internal sealed class RefLabelControl : TemplatedControl
    {
        private readonly string _brushResourceKey;
        private readonly IBrush? _refBrush;
        private double _backgroundHeight;
        private bool _isHighlighted;
        private double _labelWidth;
        private FormattedText? _formattedText;

        public RefLabelControl(
            IGitRef? gitRef,
            string label,
            string brushResourceKey,
            RefLabelIcon icon,
            RefLabelShape shape,
            bool fill,
            bool dashed,
            IBrush? refBrush = null)
        {
            GitRef = gitRef;
            Label = label;
            _brushResourceKey = brushResourceKey;
            _refBrush = refBrush;
            Icon = icon;
            Shape = shape;
            Fill = fill;
            IsDashed = dashed;
            ActualThemeVariantChanged += (_, _) => InvalidateVisual();
        }

        public IGitRef? GitRef { get; }

        public string Label { get; private set; }

        public void AppendLabel(string suffix)
        {
            Label += suffix;
            InvalidateMeasure();
            InvalidateVisual();
        }

        public RefLabelIcon Icon { get; }

        public RefLabelShape Shape { get; }

        public bool Fill { get; }

        public bool IsDashed { get; }

        public bool IsRowSelected { get; set; }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted == value)
                {
                    return;
                }

                _isHighlighted = value;
                InvalidateVisual();
            }
        }

        public double PointWidth => RevisionGridRefRenderer.PointWidth(_backgroundHeight);

        public IBrush RefBrush => _refBrush ?? GetResourceBrush(_brushResourceKey, Brushes.Gray);

        public IBrush CapsuleBackgroundBrush
            => GetResourceBrush(
                AvaloniaThemeResources.KnownColorPrefix + System.Drawing.KnownColor.Window + "Brush",
                GetResourceBrush("GitExtensionsPanelBackgroundBrush", Brushes.White));

        internal IBrush TextBrush => new SolidColorBrush(
            ToMediaColor(ThemingColorHelper.Lerp(ToDrawingColor(RefBrush), DrawingColor.Black, 0.25F)));

        internal IBrush OutlineBrush => new SolidColorBrush(
            ToMediaColor(ThemingColorHelper.Lerp(
                ToDrawingColor(RefBrush),
                ToDrawingColor(CapsuleBackgroundBrush),
                Fill ? 0.83F : 0.5F)));

        protected override Size MeasureOverride(Size availableSize)
        {
            _formattedText = CreateFormattedText(Brushes.Black);
            _backgroundHeight = Math.Ceiling(_formattedText.Height) + (PaddingTopBottom * 2) - 1;
            double rowHeight = RevisionGridControl.GetRowHeight(this);
            RefLabelIcon effectiveIcon = GetEffectiveIcon(Icon);
            double iconWidth = effectiveIcon == RefLabelIcon.None ? 0 : rowHeight / 2;
            double extraWidth = Shape switch
            {
                RefLabelShape.NotchLeft or RefLabelShape.NotchRight => PointWidth,
                RefLabelShape.PointLeft or RefLabelShape.PointRight => PointWidth / 2,
                _ => 0,
            };

            _labelWidth = Math.Ceiling(_formattedText.Width)
                + iconWidth
                + (PaddingLeftRight(Label) * 2)
                + extraWidth
                - 1;

            return new Size(_labelWidth + MarginRight, rowHeight);
        }

        public override void Render(DrawingContext context)
        {
            if (_formattedText is null || _labelWidth <= 0 || _backgroundHeight <= 0)
            {
                return;
            }

            IBrush refBrush = RefBrush;
            RefLabelIcon effectiveIcon = GetEffectiveIcon(Icon);
            double top = (Bounds.Height - _backgroundHeight) / 2;
            Rect capsuleBounds = new(0.5, top + 0.5, Math.Max(0, _labelWidth - 1), _backgroundHeight - 1);
            StreamGeometry geometry = CreateGeometry(capsuleBounds, Shape, PointWidth);
            DrawingColor drawingRefColor = ToDrawingColor(refBrush);
            DrawingColor windowColor = ToDrawingColor(CapsuleBackgroundBrush);
            IBrush? background = Fill
                ? new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops =
                    [
                        new GradientStop(
                            ToMediaColor(ThemingColorHelper.Lerp(drawingRefColor, windowColor, 0.92F)),
                            0),
                        new GradientStop(
                            ToMediaColor(ThemingColorHelper.Lerp(
                                ThemingColorHelper.Lerp(drawingRefColor, windowColor, 0.92F),
                                windowColor,
                                0.9F)),
                            1),
                    ],
                }
                : IsRowSelected || this.FindAncestorOfType<ListBoxItem>()?.IsSelected == true
                    ? CapsuleBackgroundBrush
                    : null;
            Pen outline = new(OutlineBrush, RefLabelHighlightWidth, IsDashed ? DashedLine : null);
            context.DrawGeometry(background, outline, geometry);
            if (IsHighlighted)
            {
                context.DrawGeometry(
                    null,
                    new Pen(refBrush, RefLabelHighlightWidth, IsDashed ? DashedLine : null),
                    geometry);
            }

            double iconXOffset = Shape is RefLabelShape.NotchLeft or RefLabelShape.PointLeft
                ? PointWidth
                : 0;
            if (effectiveIcon != RefLabelIcon.None)
            {
                DrawArrow(context, refBrush, capsuleBounds, iconXOffset, effectiveIcon == RefLabelIcon.Head);
            }

            double iconWidth = effectiveIcon == RefLabelIcon.None ? 0 : Bounds.Height / 2;
            double textX = iconXOffset
                + iconWidth
                + PaddingLeftRight(Label)
                - (Shape == RefLabelShape.PointLeft ? PointWidth / 2 : 0);
            FormattedText formattedText = CreateFormattedText(
                Fill
                    ? refBrush
                    : TextBrush);
            context.DrawText(formattedText, new Point(textX, capsuleBounds.Y + PaddingTopBottom - 1));
        }

        public bool Contains(Point point)
        {
            if (point.X < 0 || point.X >= Bounds.Width || point.Y < 0 || point.Y >= Bounds.Height)
            {
                return false;
            }

            double top = (Bounds.Height - _backgroundHeight) / 2;
            Rect capsuleBounds = new(0, top, Math.Max(0, _labelWidth), _backgroundHeight);
            return CreateGeometry(capsuleBounds, Shape, PointWidth).FillContains(point);
        }

        private FormattedText CreateFormattedText(IBrush foreground)
            => new(
                Label,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight),
                FontSize,
                foreground);

        private IBrush GetResourceBrush(string resourceKey, IBrush fallback)
            => Application.Current?.TryGetResource(
                    resourceKey,
                    ActualThemeVariant,
                    out object? resource) == true
                && resource is IBrush brush
                ? brush
                : fallback;

        private static DrawingColor ToDrawingColor(IBrush brush)
            => brush is ISolidColorBrush solid
                ? DrawingColor.FromArgb(solid.Color.A, solid.Color.R, solid.Color.G, solid.Color.B)
                : DrawingColor.Gray;

        private static MediaColor ToMediaColor(DrawingColor color)
            => AvaloniaThemeResources.ToMediaColor(color);

        private static StreamGeometry CreateGeometry(
            Rect bounds,
            RefLabelShape labelShape,
            double pointWidth)
        {
            double radius = RefLabelCornerRadius / 2;
            return labelShape switch
            {
                RefLabelShape.NotchLeft => CreateNotchLeftRoundRectPath(bounds, radius, pointWidth),
                RefLabelShape.NotchRight => CreateNotchRightRoundRectPath(bounds, radius, pointWidth),
                RefLabelShape.PointLeft => CreatePointLeftRoundRectPath(bounds, radius, pointWidth),
                RefLabelShape.PointRight => CreatePointRightRoundRectPath(bounds, radius, pointWidth),
                RefLabelShape.Rect => CreateRoundRectPath(bounds, radius),
                _ => throw new ArgumentOutOfRangeException(nameof(labelShape), labelShape, null),
            };
        }
    }
}
