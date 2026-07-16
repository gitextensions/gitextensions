using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GitUI.UserControls.RevisionGrid;

/// <summary>
///  Creates and renders revision-grid ref labels with the same edge shapes and head
///  indicators as the WinForms <c>RevisionGridRefRenderer</c>.
/// </summary>
internal static class RevisionGridRefRenderer
{
    private const double CornerDiameter = 5;
    private const double MarginRight = 5;
    private const double PaddingLeftRight = 4;
    private const double PaddingTopBottom = 2;
    private const double RowHeight = 24;

    /// <summary>
    ///  Creates the ordered controls for a revision's refs, nesting a local branch with its
    ///  tracked remote when both point at the same commit.
    /// </summary>
    public static IReadOnlyList<Control> CreateLabels(IReadOnlyList<IGitRef> refs)
    {
        IReadOnlyList<IGitRef> sortedRefs = SortRefs(refs);
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
                RefLabelControl localLabel = CreateLabel(gitRef, gitRef.Name, RefLabelShape.PointRight);
                RefLabelControl remoteLabel = CreateLabel(
                    remote,
                    remote.LocalName == gitRef.Name ? remote.Remote : remote.Name,
                    RefLabelShape.NotchLeft,
                    showHeadIndicator: false);
                labels.Add(new NestledRefLabelPanel(localLabel, remoteLabel));
                continue;
            }

            labels.Add(CreateLabel(
                gitRef,
                gitRef.Name,
                gitRef.IsTag ? RefLabelShape.PointLeft : RefLabelShape.Rect));
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

    private static RefLabelControl CreateLabel(
        IGitRef gitRef,
        string label,
        RefLabelShape shape,
        bool showHeadIndicator = true)
        => new(
            label,
            GetBrushResourceKey(gitRef),
            showHeadIndicator && gitRef.IsSelected
                ? RefLabelIcon.Head
                : showHeadIndicator && gitRef.IsSelectedHeadMergeSource
                    ? RefLabelIcon.HeadMergeSource
                    : RefLabelIcon.None,
            shape)
        {
            FontWeight = gitRef.IsSelected ? FontWeight.Bold : FontWeight.Normal,
            VerticalAlignment = VerticalAlignment.Center,
        };

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
    ///  One custom-drawn ref label. Keeping the WinForms shape vocabulary here avoids
    ///  encoding mutually dependent point/notch geometry in generic Border styles.
    /// </summary>
    internal sealed class RefLabelControl : TemplatedControl
    {
        private readonly string _brushResourceKey;
        private double _backgroundHeight;
        private double _labelWidth;
        private FormattedText? _formattedText;

        public RefLabelControl(
            string label,
            string brushResourceKey,
            RefLabelIcon icon,
            RefLabelShape shape)
        {
            Label = label;
            _brushResourceKey = brushResourceKey;
            Icon = icon;
            Shape = shape;
            ActualThemeVariantChanged += (_, _) => InvalidateVisual();
        }

        public string Label { get; }

        public RefLabelIcon Icon { get; }

        public RefLabelShape Shape { get; }

        public double PointWidth => _backgroundHeight / 2;

        public IBrush RefBrush => GetResourceBrush(_brushResourceKey, Brushes.Gray);

        public IBrush CapsuleBackgroundBrush
            => GetResourceBrush("GitExtensionsRefLabelBackgroundBrush", Brushes.White);

        protected override Size MeasureOverride(Size availableSize)
        {
            _formattedText = CreateFormattedText(Brushes.Black);
            _backgroundHeight = Math.Ceiling(_formattedText.Height) + (PaddingTopBottom * 2) - 1;
            double iconWidth = Icon == RefLabelIcon.None ? 0 : RowHeight / 2;
            double extraWidth = Shape switch
            {
                RefLabelShape.NotchLeft or RefLabelShape.NotchRight => PointWidth,
                RefLabelShape.PointLeft or RefLabelShape.PointRight => PointWidth / 2,
                _ => 0,
            };

            _labelWidth = Math.Ceiling(_formattedText.Width)
                + iconWidth
                + (PaddingLeftRight * 2)
                + extraWidth
                - 1;

            return new Size(_labelWidth + MarginRight, RowHeight);
        }

        public override void Render(DrawingContext context)
        {
            if (_formattedText is null || _labelWidth <= 0 || _backgroundHeight <= 0)
            {
                return;
            }

            IBrush refBrush = RefBrush;
            double top = (Bounds.Height - _backgroundHeight) / 2;
            Rect capsuleBounds = new(0.5, top + 0.5, Math.Max(0, _labelWidth - 1), _backgroundHeight - 1);
            StreamGeometry geometry = CreateGeometry(capsuleBounds, Shape, PointWidth);
            context.DrawGeometry(CapsuleBackgroundBrush, new Pen(refBrush, 1), geometry);

            double iconXOffset = Shape is RefLabelShape.NotchLeft or RefLabelShape.PointLeft
                ? PointWidth
                : 0;
            if (Icon != RefLabelIcon.None)
            {
                DrawHeadIndicator(context, refBrush, capsuleBounds, iconXOffset, Icon == RefLabelIcon.Head);
            }

            double iconWidth = Icon == RefLabelIcon.None ? 0 : RowHeight / 2;
            double textX = iconXOffset
                + iconWidth
                + PaddingLeftRight
                - (Shape == RefLabelShape.PointLeft ? PointWidth / 2 : 0);
            FormattedText formattedText = CreateFormattedText(refBrush);
            context.DrawText(formattedText, new Point(textX, capsuleBounds.Y + PaddingTopBottom - 1));
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

        private static StreamGeometry CreateGeometry(
            Rect bounds,
            RefLabelShape labelShape,
            double pointWidth)
        {
            double left = bounds.Left;
            double top = bounds.Top;
            double right = bounds.Right;
            double bottom = bounds.Bottom;
            double middle = bounds.Center.Y;
            double radius = CornerDiameter / 2;
            StreamGeometry geometry = new();

            using (StreamGeometryContext path = geometry.Open())
            {
                switch (labelShape)
                {
                    case RefLabelShape.NotchLeft:
                        path.BeginFigure(new Point(left, top), isFilled: true);
                        path.LineTo(new Point(left + pointWidth, middle));
                        path.LineTo(new Point(left, bottom));
                        path.LineTo(new Point(right - radius, bottom));
                        path.QuadraticBezierTo(
                            new Point(right, bottom),
                            new Point(right, bottom - radius));
                        path.LineTo(new Point(right, top + radius));
                        path.QuadraticBezierTo(
                            new Point(right, top),
                            new Point(right - radius, top));
                        break;

                    case RefLabelShape.NotchRight:
                        path.BeginFigure(new Point(left + radius, top), isFilled: true);
                        path.LineTo(new Point(right, top));
                        path.LineTo(new Point(right - pointWidth, middle));
                        path.LineTo(new Point(right, bottom));
                        AddBottomAndLeft(path, left, bottom, top, radius);
                        break;

                    case RefLabelShape.PointLeft:
                        path.BeginFigure(new Point(left, middle), isFilled: true);
                        path.LineTo(new Point(left + pointWidth, top));
                        path.LineTo(new Point(right - radius, top));
                        AddRight(path, right, top, bottom, radius);
                        path.LineTo(new Point(left + pointWidth, bottom));
                        break;

                    case RefLabelShape.PointRight:
                        path.BeginFigure(new Point(left + radius, top), isFilled: true);
                        path.LineTo(new Point(right - pointWidth, top));
                        path.LineTo(new Point(right, middle));
                        path.LineTo(new Point(right - pointWidth, bottom));
                        AddBottomAndLeft(path, left, bottom, top, radius);
                        break;

                    case RefLabelShape.Rect:
                        path.BeginFigure(new Point(left + radius, top), isFilled: true);
                        path.LineTo(new Point(right - radius, top));
                        AddRight(path, right, top, bottom, radius);
                        AddBottomAndLeft(path, left, bottom, top, radius);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(labelShape), labelShape, null);
                }

                path.EndFigure(isClosed: true);
            }

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

        private static void DrawHeadIndicator(
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
            StreamGeometry arrow = new();

            using (StreamGeometryContext path = arrow.Open())
            {
                path.BeginFigure(new Point(x, y), isFilled: filled);
                path.LineTo(new Point(x + width, y + (height / 2)));
                path.LineTo(new Point(x, y + height));
                path.EndFigure(isClosed: true);
            }

            context.DrawGeometry(filled ? brush : null, filled ? null : new Pen(brush, 1), arrow);
        }
    }
}
