using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Columns;
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
    private const int MarginRight = 5;
    private static readonly double[] _dashPattern = [4, 4];
    private static readonly DashStyle DashedLine = new(_dashPattern, 0);

    // Cache for GetTextAscent, which is only invalidated by a font change.
    private static (FontFamily Family, FontStyle Style, FontWeight Weight, double Size)? _ascentFont;
    private static int _ascentTextHeight;
    private static int _ascent;

    private static int PaddingTopBottom => 2;

    // Uniform horizontal padding applied on both sides of any ref label icon.
    private static int IconPaddingLeftRight => 3;

    // Pixel radius for the rounded corners of ref label capsules.
    private static int RefLabelCornerRadius => 5;

    // Pixel width of the highlight frame drawn around a hovered ref label,
    // and the left-side offset used when drawing the nestled remote label.
    private static int RefLabelHighlightWidth => 1;

    private static int PointWidth(int height) => height / 2;

    private static int PaddingLeftRight(string name) => string.IsNullOrEmpty(name) ? 1 : 4;

    /// <summary>
    ///  Creates a closed path for a capsule whose left edge is a concave '>' notch
    ///  that exactly fits the convex point tip of a preceding capsule.
    /// </summary>
    private static StreamGeometry CreateNotchLeftRoundRectPath(Rect rect, int radius, int pointWidth)
    {
        double left = rect.X;
        double top = rect.Y;
        double right = rect.Right;
        double bottom = rect.Bottom;
        double midY = top + ((int)rect.Height / 2);
        double arcRadius = radius / 2d;
        double arcOffset = QuarterCircleControlOffset(arcRadius);

        // The notch corners are at the leftmost pixels; the notch tip is indented by pointWidth.
        return CreatePath(path =>
        {
            path.BeginFigure(new Point(left, top), isFilled: true);
            path.LineTo(new Point(left + pointWidth, midY)); // top notch corner → indented tip
            path.LineTo(new Point(left, bottom)); // indented tip → bottom notch corner
            path.LineTo(new Point(right - arcRadius, bottom));
            path.CubicBezierTo(new Point(right - arcRadius + arcOffset, bottom),
                new Point(right, bottom - arcRadius + arcOffset), new Point(right, bottom - arcRadius)); // bottom-right arc
            path.LineTo(new Point(right, top + arcRadius));
            path.CubicBezierTo(new Point(right, top + arcRadius - arcOffset),
                new Point(right - arcRadius + arcOffset, top), new Point(right - arcRadius, top)); // top-right arc
        });
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose right edge is a concave '&lt;' notch
    ///  that exactly fits the convex point tip of a following capsule.
    /// </summary>
    private static StreamGeometry CreateNotchRightRoundRectPath(Rect rect, int radius, int pointWidth)
    {
        double left = rect.X;
        double top = rect.Y;
        double right = rect.Right;
        double bottom = rect.Bottom;
        double midY = top + ((int)rect.Height / 2);
        double arcRadius = radius / 2d;
        double arcOffset = QuarterCircleControlOffset(arcRadius);

        // The notch corners are at the rightmost pixels; the notch tip is indented by pointWidth.
        return CreatePath(path =>
        {
            path.BeginFigure(new Point(left, top + arcRadius), isFilled: true);
            path.CubicBezierTo(new Point(left, top + arcRadius - arcOffset),
                new Point(left + arcRadius - arcOffset, top), new Point(left + arcRadius, top)); // top-left arc
            path.LineTo(new Point(right, top));
            path.LineTo(new Point(right - pointWidth, midY)); // top notch corner → indented tip
            path.LineTo(new Point(right, bottom)); // indented tip → bottom notch corner
            AddBottomAndLeft(path, left, bottom, top, arcRadius); // bottom-left arc
        });
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose left edge is a convex '&lt;' point
    ///  that protrudes leftward, so it visually connects to a nestled preceding label.
    /// </summary>
    private static StreamGeometry CreatePointLeftRoundRectPath(Rect rect, int radius, int pointWidth)
    {
        double left = rect.X;
        double top = rect.Y;
        double right = rect.Right;
        double bottom = rect.Bottom;
        double midY = top + ((int)rect.Height / 2);
        double arcRadius = radius / 2d;

        // The point tip is at the leftmost pixel; the top/bottom corners step back by pointWidth.
        return CreatePath(path =>
        {
            path.BeginFigure(new Point(left, midY), isFilled: true); // tip → top-left corner
            path.LineTo(new Point(left + pointWidth, top));
            path.LineTo(new Point(right - arcRadius, top));

            // top-right arc
            // bottom-right arc
            AddRight(path, right, top, bottom, arcRadius);
            path.LineTo(new Point(left + pointWidth, bottom)); // bottom-left corner → tip
        });
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose right edge is a convex '&gt;' point
    ///  instead of a rounded cap, so it visually connects to a nestled following label.
    /// </summary>
    private static StreamGeometry CreatePointRightRoundRectPath(Rect rect, int radius, int pointWidth)
    {
        double left = rect.X;
        double top = rect.Y;
        double right = rect.Right;
        double bottom = rect.Bottom;
        double midY = top + ((int)rect.Height / 2);
        double arcRadius = radius / 2d;
        double arcOffset = QuarterCircleControlOffset(arcRadius);

        // The point tip is at the rightmost pixel; the top/bottom corners step back by pointWidth.
        return CreatePath(path =>
        {
            path.BeginFigure(new Point(left, top + arcRadius), isFilled: true);
            path.CubicBezierTo(new Point(left, top + arcRadius - arcOffset),
                new Point(left + arcRadius - arcOffset, top), new Point(left + arcRadius, top)); // top-left arc
            path.LineTo(new Point(right - pointWidth, top));
            path.LineTo(new Point(right, midY)); // top-right corner → tip
            path.LineTo(new Point(right - pointWidth, bottom)); // tip → bottom-right corner
            AddBottomAndLeft(path, left, bottom, top, arcRadius); // bottom-left arc
        });
    }

    private static StreamGeometry CreateRoundRectPath(Rect rect, int radius)
        => CreatePath(path =>
        {
            double arcRadius = radius / 2d;
            double arcOffset = QuarterCircleControlOffset(arcRadius);
            path.BeginFigure(new Point(rect.Left, rect.Top + arcRadius), isFilled: true);
            path.CubicBezierTo(new Point(rect.Left, rect.Top + arcRadius - arcOffset),
                new Point(rect.Left + arcRadius - arcOffset, rect.Top), new Point(rect.Left + arcRadius, rect.Top));
            path.LineTo(new Point(rect.Right - arcRadius, rect.Top));
            AddRight(path, rect.Right, rect.Top, rect.Bottom, arcRadius);
            AddBottomAndLeft(path, rect.Left, rect.Bottom, rect.Top, arcRadius);
        });

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
        Func<IGitRef, (IGitRef GitRef, string Name)?>? getVirtualRef,
        IReadOnlySet<string>? superprojectRefs,
        Func<IGitRef, (string Label, string? HighlightedLabel)>? getLabel = null)
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
                    remote.Remote,
                    RefLabelShape.NotchLeft,
                    fill,
                    showHeadIndicator: false,
                    dashed: superprojectRefs?.Contains(remote.CompleteName) == true);
                labels.Add(new NestledRefLabelPanel(localLabel, remoteLabel));
                continue;
            }

            if (getVirtualRef?.Invoke(gitRef) is { } virtualRefData)
            {
                IGitRef virtualRef = virtualRefData.GitRef;
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
                        virtualRefData.Name,
                        virtualShape,
                        fill,
                        showHeadIndicator: false,
                        dashed: true,
                        fontWeight: virtualRef is NestledVirtualRef { TrackingBranchIsGone: true }
                            ? FontWeight.Bold
                            : FontWeight.Normal)));
                continue;
            }

            (string label, string? highlightedLabel) = getLabel?.Invoke(gitRef) ?? (gitRef.Name, null);
            labels.Add(CreateLabel(
                gitRef,
                label,
                gitRef.IsTag ? RefLabelShape.PointLeft : RefLabelShape.Rect,
                fill,
                dashed: superprojectRefs?.Contains(gitRef.CompleteName) == true,
                highlightedLabel: highlightedLabel));
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
        private readonly RefLabelHighlightOverlay _highlightOverlay;

        public NestledRefLabelPanel(
            RefLabelControl localLabel,
            RefLabelControl remoteLabel)
        {
            _localLabel = localLabel;
            _remoteLabel = remoteLabel;
            _highlightOverlay = new RefLabelHighlightOverlay(this);
            Children.Add(localLabel);
            Children.Add(remoteLabel);

            // Avalonia paints a parent's Render before its children. A visual-only,
            // noninteractive final sibling preserves the source's deferred pair frames.
            VisualChildren.Add(_highlightOverlay);
        }

        internal int PointWidth => _localLabel.FontPointWidth;

        internal void InvalidateHighlight() => _highlightOverlay.InvalidateVisual();

        protected override Size MeasureOverride(Size availableSize)
        {
            _localLabel.Measure(availableSize);
            double remoteX = Math.Max(0, _localLabel.GetCapsuleWidth(availableSize.Width)
                - PointWidth + RefLabelHighlightWidth);
            _remoteLabel.Measure(new Size(Math.Max(0, availableSize.Width - remoteX), availableSize.Height));
            _highlightOverlay.Measure(availableSize);
            return new Size(
                remoteX + _remoteLabel.DesiredSize.Width,
                Math.Max(_localLabel.DesiredSize.Height, _remoteLabel.DesiredSize.Height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // A row's first measure can use only the fill column's minimum before
            // its viewport is arranged. DesiredSize retains that clipped constraint;
            // DrawRefEx instead clips its measured text to the current bounds.
            _localLabel.Arrange(new Rect(0, 0,
                Math.Min(finalSize.Width, _localLabel.GetCapsuleWidth(finalSize.Width) + MarginRight), finalSize.Height));

            // The source resets offset from the actual clipped branch rectangle,
            // then clips the remote to the remaining cell width, not its ideal width.
            double remoteX = Math.Max(0, _localLabel.CapsuleBounds.Right - PointWidth + RefLabelHighlightWidth);
            double remainingWidth = Math.Max(0, finalSize.Width - remoteX);
            _remoteLabel.Arrange(new Rect(
                remoteX, 0,
                Math.Min(remainingWidth, _remoteLabel.GetCapsuleWidth(remainingWidth) + MarginRight),
                finalSize.Height));
            _highlightOverlay.Arrange(new Rect(finalSize));
            InvalidateHighlight();
            return finalSize;
        }

        private sealed class RefLabelHighlightOverlay : Control
        {
            private readonly NestledRefLabelPanel _owner;

            public RefLabelHighlightOverlay(NestledRefLabelPanel owner)
            {
                _owner = owner;
                IsHitTestVisible = false;
                Focusable = false;
            }

            public override void Render(DrawingContext context)
            {
                foreach (RefLabelControl label in new[] { _owner._localLabel, _owner._remoteLabel })
                {
                    using (context.PushTransform(Matrix.CreateTranslation(label.Bounds.X, label.Bounds.Y)))
                    {
                        label.DrawHighlight(context);
                    }
                }
            }
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
                if (local.IsTrackingRemote(remote))
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
        bool dashed = false,
        FontWeight? fontWeight = null,
        string? highlightedLabel = null,
        RefLabelIcon? icon = null)
        => new(
            gitRef,
            label,
            GetBrushResourceKey(gitRef),
            icon is { } explicitIcon
                ? GetEffectiveIcon(explicitIcon)
                : showHeadIndicator && gitRef.IsSelected
                    ? RefLabelIcon.Head
                    : showHeadIndicator && gitRef.IsSelectedHeadMergeSource
                        ? RefLabelIcon.HeadMergeSource
                        : RefLabelIcon.None,
            shape,
            fill,
            dashed,
            GetRemoteRefBrush(gitRef),
            highlightedLabel)
        {
            // Explicit Normal denotes the source NormalFont, which may itself be bold.
            // Its selected/forced BoldFont replaces configured Italic rather than adding Bold.
            FontWeight = fontWeight == FontWeight.Bold || (fontWeight is null && gitRef.IsSelected) || AppSettings.Font.Bold
                ? FontWeight.Bold : FontWeight.Normal,
            FontStyle = fontWeight != FontWeight.Bold && (fontWeight is not null || !gitRef.IsSelected) && AppSettings.Font.Italic
                ? FontStyle.Italic : FontStyle.Normal,
            VerticalAlignment = VerticalAlignment.Center,
        };

    /// <summary>
    ///  Draws a ref label and returns the retained label.
    /// </summary>
    /// <returns>The drawn ref label.</returns>
    public static RefLabelControl DrawRef(
        bool isRowSelected,
        IGitRef gitRef,
        string name,
        RefLabelIcon icon,
        RefLabelShape shape = RefLabelShape.Rect,
        bool dashedLine = false,
        bool fill = false,
        bool highlight = false)
    {
        (RefLabelControl label, Action? drawHighlight) = DrawRefEx(
            isRowSelected,
            gitRef,
            name,
            icon,
            shape,
            dashedLine,
            fill,
            highlight);
        drawHighlight?.Invoke();
        return label;
    }

    /// <summary>
    ///  Draws a ref label with the specified edge shape and returns the label and an optional deferred highlight action.
    /// </summary>
    /// <returns>
    ///  The drawn label,
    ///  and a deferred action that paints the highlight frame — or <see langword="null"/> when <paramref name="highlight"/> is <see langword="false"/>.
    ///  The caller is responsible for invoking the action at the appropriate time (typically after all adjacent labels are drawn).
    /// </returns>
    public static (RefLabelControl Label, Action? DrawHighlight) DrawRefEx(
        bool isRowSelected,
        IGitRef gitRef,
        string name,
        RefLabelIcon icon,
        RefLabelShape shape = RefLabelShape.Rect,
        bool dashedLine = false,
        bool fill = false,
        bool highlight = false)
    {
        RefLabelControl label = CreateLabel(
            gitRef,
            name,
            shape,
            fill,
            showHeadIndicator: icon != RefLabelIcon.None,
            dashed: dashedLine,
            icon: icon);
        DrawRefBackground(label, isRowSelected, highlight: false);
        return (label, highlight ? () => label.IsHighlighted = true : null);
    }

    private static void DrawRefBackground(RefLabelControl label, bool isRowSelected, bool highlight)
    {
        label.IsRowSelected = isRowSelected;
        label.IsHighlighted = highlight;
    }

    public static DrawingColor GetHeadColor(IGitRef gitRef)
    {
        if (gitRef.IsTag)
        {
            return AppColor.Tag.GetThemeColor();
        }

        if (gitRef.IsHead)
        {
            return AppColor.Branch.GetThemeColor();
        }

        if (gitRef.IsRemote)
        {
            return AppColor.RemoteBranch.GetThemeColor();
        }

        return AppColor.OtherTag.GetThemeColor();
    }

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

    private static StreamGeometry CreatePath(Action<StreamGeometryContext> draw)
    {
        StreamGeometry geometry = new();
        using StreamGeometryContext path = geometry.Open();
        path.SetFillRule(FillRule.EvenOdd);
        draw(path);
        path.EndFigure(isClosed: true);
        return geometry;
    }

    // GraphicsPath.AddArc stores circular quarters as cubic Beziers. The source
    // radius parameter is the arc rectangle's diameter, not a quadratic corner radius.
    private static double QuarterCircleControlOffset(double radius)
        => radius * (4d / 3) * Math.Tan(Math.PI / 8);

    private static void AddRight(
        StreamGeometryContext path,
        double right,
        double top,
        double bottom,
        double radius)
    {
        double arcOffset = QuarterCircleControlOffset(radius);
        path.CubicBezierTo(new Point(right - radius + arcOffset, top),
            new Point(right, top + radius - arcOffset), new Point(right, top + radius));
        path.LineTo(new Point(right, bottom - radius));
        path.CubicBezierTo(new Point(right, bottom - radius + arcOffset),
            new Point(right - radius + arcOffset, bottom), new Point(right - radius, bottom));
    }

    private static void AddBottomAndLeft(
        StreamGeometryContext path,
        double left,
        double bottom,
        double top,
        double radius)
    {
        double arcOffset = QuarterCircleControlOffset(radius);
        path.LineTo(new Point(left + radius, bottom));
        path.CubicBezierTo(new Point(left + radius - arcOffset, bottom),
            new Point(left, bottom - radius + arcOffset), new Point(left, bottom - radius));
        path.LineTo(new Point(left, top + radius));
    }

    /// <summary>
    ///  Computes the width of the text and icon of a ref label including their padding, excluding the shape-specific extra width.
    /// </summary>
    public static int GetContentWidth(TemplatedControl owner, string name, RefLabelIcon icon)
    {
        (Size textSize, _, _, _, int iconAreaWidth) = MeasureContent(owner, name, icon);
        return (int)textSize.Width + iconAreaWidth + (2 * PaddingLeftRight(name));
    }

    private static (Size TextSize, int BackgroundHeight, int TextOffsetY, RefLabelIconRenderer? IconRenderer, int IconAreaWidth) MeasureContent(
        TemplatedControl owner,
        string name,
        RefLabelIcon icon)
    {
        int paddingTopBottom = PaddingTopBottom;

        Size measuredText = WinFormsTextMeasurer.MeasureTextRendererNoPadding(owner, string.IsNullOrEmpty(name) ? " " : name);
        Size textSize = new(string.IsNullOrEmpty(name) ? 0 : Math.Ceiling(measuredText.Width), Math.Ceiling(measuredText.Height));

        int backgroundHeight = (int)textSize.Height + paddingTopBottom + paddingTopBottom - 1;
        int textOffsetY = paddingTopBottom - 1;

        // The metrics are relative to the capsule and therefore constant, so the icon renderers are cached and shared.
        RefLabelIconRenderer? iconRenderer = RefLabelIconRenderer.Get(
            icon,
            new RefLabelIconMetrics(backgroundHeight, (int)textSize.Height, textOffsetY + GetTextAscent(owner, (int)textSize.Height)));
        int iconAreaWidth = iconRenderer is null ? 0 : iconRenderer.Width + (2 * IconPaddingLeftRight);

        return (textSize, backgroundHeight, textOffsetY, iconRenderer, iconAreaWidth);
    }

    /// <summary>
    ///  Computes the distance from the top of the text to its baseline.
    /// </summary>
    /// <remarks>
    ///  The measured text height corresponds to the line spacing of the font, so the ascent can be scaled by the same ratio.
    ///  The result is cached because it only changes when the font changes.
    /// </remarks>
    private static int GetTextAscent(TemplatedControl owner, int textHeight)
    {
        var font = (owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontSize);
        if (_ascentFont == font && textHeight == _ascentTextHeight)
        {
            return _ascent;
        }

        // Framework constraint: the native text line supplies the baseline instead of GDI font metrics.
        using TextLayout text = new(" ", new Typeface(owner.FontFamily, owner.FontStyle, owner.FontWeight), owner.FontSize, Brushes.Black);
        _ascent = text.TextLines.Count > 0 ? (int)Math.Round(text.TextLines[0].Baseline) : textHeight;
        _ascentFont = font;
        _ascentTextHeight = textHeight;

        return _ascent;
    }

    private static RefLabelIcon GetEffectiveIcon(RefLabelIcon icon)
        => icon is RefLabelIcon.Head or RefLabelIcon.HeadMergeSource or RefLabelIcon.RemoteForced
            ? icon
            : RefLabelIcon.None;

    /// <summary>
    ///  Computes the point width for the given font, which is needed to calculate the ideal capsule
    ///  size for Notch and Point shapes.
    /// </summary>
    /// <remarks>
    ///  Computes the capsule's ideal height given the same font as <see cref="DrawRef"/> and <see cref="DrawRefEx"/>.
    ///  Does not account for clipping to available cell width.
    /// </remarks>
    public static int GetPointWidth(TemplatedControl owner)
    {
        int textHeight = (int)Math.Ceiling(WinFormsTextMeasurer.MeasureTextRendererNoPadding(owner, " ").Height);
        int backgroundHeight = textHeight + (PaddingTopBottom * 2) - 1;
        return PointWidth(backgroundHeight);
    }

    /// <summary>
    ///  One custom-drawn ref label. Keeping the WinForms shape vocabulary here avoids
    ///  encoding mutually dependent point/notch geometry in generic Border styles.
    /// </summary>
    internal sealed class RefLabelControl : TemplatedControl
    {
        private readonly string _brushResourceKey;
        private string? _highlightedLabel;
        private RefLabelIcon _icon;
        private readonly IBrush? _refBrush;
        private string _normalLabel;
        private int _backgroundHeight;
        private int _fontPointWidth;
        private bool _isHighlighted;
        private int _labelWidth;
        private double _literalTextWidth;
        private Size _textSize;
        private int _textOffsetY;
        private RefLabelIconRenderer? _iconRenderer;
        private int _iconAreaWidth;

        public RefLabelControl(
            IGitRef? gitRef,
            string label,
            string brushResourceKey,
            RefLabelIcon icon,
            RefLabelShape shape,
            bool fill,
            bool dashed,
            IBrush? refBrush = null,
            string? highlightedLabel = null)
        {
            GitRef = gitRef;
            _normalLabel = label;
            _highlightedLabel = highlightedLabel;
            Label = _normalLabel;
            _brushResourceKey = brushResourceKey;
            _refBrush = refBrush;
            _icon = icon;
            Shape = shape;
            Fill = fill;
            IsDashed = dashed;
            ActualThemeVariantChanged += (_, _) =>
            {
                InvalidateVisual();
                InvalidateHighlight();
            };
            ResourcesChanged += (_, _) =>
            {
                // A custom palette can change without changing the active theme variant.
                InvalidateVisual();
                InvalidateHighlight();
            };
        }

        public IGitRef? GitRef { get; }

        public string Label { get; private set; }

        public void AppendLabel(string suffix)
        {
            _normalLabel += suffix;
            Label = _isHighlighted && _highlightedLabel is not null
                ? _highlightedLabel + suffix
                : _normalLabel;
            InvalidateMeasure();
            InvalidateVisual();
            InvalidateHighlight();
        }

        public RefLabelIcon Icon => _isHighlighted && _icon == RefLabelIcon.RemoteForced ? RefLabelIcon.None : _icon;

        internal void ReplaceRemoteNameWithIcon(string name)
        {
            _highlightedLabel ??= _normalLabel;
            _normalLabel = name;
            Label = _isHighlighted ? _highlightedLabel : _normalLabel;
            _icon = RefLabelIcon.RemoteForced;
            InvalidateMeasure();
            InvalidateVisual();
        }

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
                Label = value && _highlightedLabel is not null
                    ? _highlightedLabel
                    : _normalLabel;
                InvalidateMeasure();
                InvalidateVisual();
                InvalidateHighlight();
            }
        }

        /// <summary>
        ///  Gets the source integer slant width derived from the capsule's text height.
        /// </summary>
        public int PointWidth => RevisionGridRefRenderer.PointWidth(_backgroundHeight);

        /// <summary>
        ///  Gets the source font-space slant width independently of the caption's line count.
        /// </summary>
        internal int FontPointWidth => _fontPointWidth;

        /// <summary>
        ///  Gets the source hit slant width, shared from the first label of a nestled pair.
        /// </summary>
        public int HitPointWidth => Parent is NestledRefLabelPanel panel ? panel.PointWidth : FontPointWidth;

        /// <summary>
        ///  Gets the unshrunk, source integer painted rectangle in label-local coordinates.
        /// </summary>
        public Rect CapsuleBounds => new(0, ((int)Bounds.Height - _backgroundHeight) / 2,
            GetCapsuleWidth(Bounds.Width), _backgroundHeight);

        internal int GetCapsuleWidth(double availableWidth)
            => (int)Math.Min(Math.Max(0, availableWidth), _labelWidth);

        internal TextTrimming GetTextTrimming(int availableTextWidth)
            => _literalTextWidth <= availableTextWidth
                ? TextTrimming.None
                : TextTrimming.CharacterEllipsis;

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
            // The source measures prefixes with NoPadding but paints the literal caption
            // with NoPrefix. Empty labels retain the font's space height and zero width.
            (_textSize, _backgroundHeight, _textOffsetY, _iconRenderer, _iconAreaWidth) = MeasureContent(this, Label, GetEffectiveIcon(Icon));
            _literalTextWidth = WinFormsTextMeasurer.MeasureSize(FontFamily, FontStyle, FontWeight,
                FontSize, Label, singleLine: false, useTextRendererPadding: false).Width;

            // Nestled hits share the first font's space height, not a multiline
            // caption's height. Cache it with layout rather than querying GDI per move.
            _fontPointWidth = GetPointWidth(this);
            int rowHeight = (int)RevisionGridControl.GetRowHeight(this);

            // For notch/point the rect must include the full/half notch/point area
            // so text can be positioned within the visible portion, matching the symmetric padding of other shapes.
            int extraWidth = Shape switch
            {
                RefLabelShape.NotchLeft or RefLabelShape.NotchRight => PointWidth,
                RefLabelShape.PointLeft or RefLabelShape.PointRight => PointWidth / 2,
                _ => 0,
            };

            // When an icon is present, its own right-side padding (part of iconAreaWidth) already provides most of the gap
            // before the text, so only a small extra nudge (instead of a second full paddingLeftRight) is added.
            int leadingPadding = _iconRenderer is null ? PaddingLeftRight(Label) : 1;

            _labelWidth = (int)_textSize.Width
                + _iconAreaWidth
                + leadingPadding + PaddingLeftRight(Label)
                + extraWidth
                - 1;

            InvalidateHighlight();
            return new Size(_labelWidth + MarginRight, rowHeight);
        }

        public override void Render(DrawingContext context)
        {
            if (_labelWidth <= 0 || _backgroundHeight <= 0)
            {
                return;
            }

            IBrush refBrush = RefBrush;
            RefLabelIcon effectiveIcon = GetEffectiveIcon(Icon);
            Rect capsuleBounds = CapsuleBounds;
            if (capsuleBounds.Width <= 0 || capsuleBounds.Height <= 0)
            {
                // It may happen, as observed in #5396
                return;
            }

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

            // frame
            Pen outline = new(OutlineBrush, RefLabelHighlightWidth, IsDashed ? DashedLine : null);
            context.DrawGeometry(background, outline, geometry);

            // For NotchLeft and PointLeft the point/notch occupies the left portion of the rect,
            // so the icon and text must be shifted right by pointWidth.
            int iconXOffset = Shape is RefLabelShape.NotchLeft or RefLabelShape.PointLeft
                ? PointWidth
                : effectiveIcon == RefLabelIcon.None ? 0 : 1;

            _iconRenderer?.Draw(context,
                (int)capsuleBounds.X + iconXOffset + IconPaddingLeftRight,
                (int)capsuleBounds.Y,
                (refBrush as ISolidColorBrush)?.Color ?? Colors.Gray);
            int paddingLeftRight = PaddingLeftRight(Label);

            // For PointLeft, offset by half pointWidth so text starts inside the point.
            int leadingPadding = _iconRenderer is null ? paddingLeftRight : 1;
            int textX = iconXOffset
                + _iconAreaWidth
                + leadingPadding
                - (Shape == RefLabelShape.PointLeft ? PointWidth / 2 : 0);
            int availableWidth = Math.Max(0, (int)Bounds.Width);
            int textWidth = Math.Clamp(Math.Min(availableWidth - paddingLeftRight - paddingLeftRight, (int)_textSize.Width),
                0, Math.Max(0, availableWidth - textX));
            Rect textBounds = new(textX, capsuleBounds.Y + _textOffsetY, textWidth, _textSize.Height);
            if (textWidth > 0 && _textSize.Height > 0)
            {
                // NoPrefix keeps the raw caption; EndEllipsis does not introduce word
                // wrapping or suppress explicit line breaks. The source adjusts vertical
                // alignment with integer halves and leaves oversized text at the top.
                // Native literal metrics decide whether ellipsis is needed, so a different
                // Avalonia glyph advance cannot abbreviate a caption that fits in GDI.
                using TextLayout textLayout = new(Label, new Typeface(FontFamily, FontStyle, FontWeight),
                    FontSize, Fill ? refBrush : TextBrush, textWrapping: TextWrapping.NoWrap,
                    textTrimming: GetTextTrimming(textWidth), flowDirection: FlowDirection.LeftToRight,
                    maxWidth: textWidth);
                int paintHeight = (int)Math.Ceiling(textLayout.Height);
                int textY = (int)textBounds.Y;
                if (paintHeight <= textBounds.Height)
                {
                    textY += ((int)textBounds.Height / 2) - (paintHeight / 2);
                }

                using (context.PushClip(textBounds))
                {
                    textLayout.Draw(context, new Point(textBounds.X, textY));
                }
            }

            if (Parent is not NestledRefLabelPanel)
            {
                DrawHighlight(context);
            }
        }

        public bool Contains(Point point)
        {
            if (point.X < 0 || point.X >= Bounds.Width || point.Y < 0 || point.Y >= Bounds.Height)
            {
                return false;
            }

            return new RefLabelHitInfo(CapsuleBounds, Shape, HitPointWidth, GitRef, null).Contains(point);
        }

        internal void DrawHighlight(DrawingContext context)
        {
            Rect capsuleBounds = CapsuleBounds;
            if (!IsHighlighted || capsuleBounds.Width <= 0 || capsuleBounds.Height <= 0)
            {
                return;
            }

            context.DrawGeometry(null,
                new Pen(RefBrush, RefLabelHighlightWidth, IsDashed ? DashedLine : null),
                CreateGeometry(capsuleBounds, Shape, PointWidth));
        }

        private void InvalidateHighlight()
        {
            if (Parent is NestledRefLabelPanel panel)
            {
                panel.InvalidateHighlight();
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == FontFamilyProperty || change.Property == FontSizeProperty
                || change.Property == FontStyleProperty || change.Property == FontWeightProperty)
            {
                InvalidateMeasure();
                InvalidateVisual();
                InvalidateHighlight();
            }
            else if (change.Property == BoundsProperty)
            {
                InvalidateHighlight();
            }
        }

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
            int pointWidth)
        {
            int radius = RefLabelCornerRadius;
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
