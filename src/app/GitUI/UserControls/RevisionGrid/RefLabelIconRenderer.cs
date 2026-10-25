using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using GitExtUtils.GitUI;

namespace GitUI.UserControls.RevisionGrid;

/// <summary>
///  Sizes, positions and draws the icon of a ref label.
/// </summary>
/// <remarks>
///  An instance computes its geometry once, in the constructor, from the <see cref="RefLabelIconMetrics"/> of a ref label.
///  Since those metrics are relative to the capsule and change only with the font, the DPI scaling or the row height,
///  instances are cached and shared by all labels with the same metrics.
///  The horizontal padding around the icon is applied uniformly by <see cref="RevisionGridRefRenderer"/>
///  and must not be added by an implementation.
/// </remarks>
internal abstract class RefLabelIconRenderer
{
    private static readonly ConcurrentDictionary<(RefLabelIcon Icon, RefLabelIconMetrics Metrics), RefLabelIconRenderer?> _renderers = [];

    /// <summary>
    ///  Gets the cached renderer for the given <paramref name="icon"/>, or <see langword="null"/> if no icon is to be drawn.
    /// </summary>
    public static RefLabelIconRenderer? Get(RefLabelIcon icon, in RefLabelIconMetrics metrics)
        => _renderers.GetOrAdd((icon, metrics), static key => Create(key.Icon, key.Metrics));

    private static RefLabelIconRenderer? Create(RefLabelIcon icon, in RefLabelIconMetrics metrics)
        => icon switch
        {
            RefLabelIcon.Head => new ArrowIconRenderer(metrics, filled: true),
            RefLabelIcon.HeadMergeSource => new ArrowIconRenderer(metrics, filled: false),
            RefLabelIcon.RemoteForced => new CloudIconRenderer(metrics),
            _ => null
        };

    /// <summary>
    ///  Gets the horizontal space occupied by the icon, excluding the uniform padding.
    /// </summary>
    public abstract int Width { get; }

    /// <summary>
    ///  Draws the icon with its left edge at <paramref name="x"/>, relative to the capsule top <paramref name="capsuleTop"/>.
    /// </summary>
    public abstract void Draw(Graphics graphics, int x, int capsuleTop, Color color);

    private sealed class ArrowIconRenderer : RefLabelIconRenderer
    {
        // Vertical inset of the arrow within the capsule, so that the arrow scales with the row height.
        private static int InsetY => DpiUtil.Scale(3);

        private readonly PointF[] _points = new PointF[4];
        private readonly bool _filled;
        private readonly int _height;
        private readonly int _offsetY;

        public ArrowIconRenderer(in RefLabelIconMetrics metrics, bool filled)
        {
            _filled = filled;
            _height = Math.Max(0, metrics.CapsuleHeight - (2 * InsetY));
            _offsetY = (metrics.CapsuleHeight - _height) / 2;
        }

        public override int Width => _height / 2;

        public override void Draw(Graphics graphics, int x, int capsuleTop, Color color)
        {
            ThreadHelper.AssertOnUIThread();

            int y = capsuleTop + _offsetY;

            _points[0] = new PointF(x, y);
            _points[1] = new PointF(x + (_height / 2f), y + (_height / 2f));
            _points[2] = new PointF(x, y + _height);
            _points[3] = _points[0];

            if (_filled)
            {
                using SolidBrush brush = new(color);
                graphics.FillPolygon(brush, _points);
            }
            else
            {
                using Pen pen = new(color);
                graphics.DrawPolygon(pen, _points);
            }
        }
    }

    private sealed class CloudIconRenderer : RefLabelIconRenderer
    {
        // The flat bottom of the cloud outline, relative to the icon size.
        private const float bottomRatio = 0.760f;

        // The rightmost x-coordinates of the cloud outline, relative to the icon size.
        // The outline does not span the full [0, 1] range, so the width is trimmed from the reported Width.
        private const float usedWidth = 0.834f;

        // Pixel width of the pen used to trace the cloud outline.
        private static float PenWidth => DpiUtil.ScaleX;

        private readonly int _size;
        private readonly int _offsetY;

        public CloudIconRenderer(in RefLabelIconMetrics metrics)
        {
            // The cloud glyph is slightly larger than the text height so that it appears balanced next to the text,
            // and it is positioned such that its flat bottom sits on the text baseline.
            // The pen is centered on the path, so the visible bottom edge of the stroke is half a pen width lower.
            _size = metrics.TextHeight + 1;
            _offsetY = metrics.TextBaselineOffset - (int)Math.Round(bottomRatio * _size) - (int)Math.Ceiling(PenWidth / 2);
        }

        public override int Width => (int)Math.Round(usedWidth * _size);

        public override void Draw(Graphics graphics, int x, int capsuleTop, Color color)
        {
            using Pen pen = new(color, PenWidth) { LineJoin = LineJoin.Round };
            using GraphicsPath path = new();

            float s = _size;
            int y = capsuleTop + _offsetY;

            // Bezier curves traced from a cloud SVG (72x72 viewBox), normalized to 0..1 and shifted left
            // so the visible glyph starts exactly at x, matching the arrow icon.
            // Three bumps: left (small), middle (medium), right (large).

            // Left bump, descending left side
            path.AddBezier(
                x + (0.138f * s), y + (0.420f * s),
                x + (0.056f * s), y + (0.439f * s),
                x + (0.000f * s), y + (0.510f * s),
                x + (0.000f * s), y + (0.596f * s));

            // Bottom-left rounded corner
            path.AddBezier(
                x + (0.000f * s), y + (0.596f * s),
                x + (0.000f * s), y + (0.687f * s),
                x + (0.063f * s), y + (bottomRatio * s),
                x + (0.141f * s), y + (bottomRatio * s));

            // Flat bottom
            path.AddLine(
                x + (0.141f * s), y + (bottomRatio * s),
                x + (0.679f * s), y + (bottomRatio * s));

            // Bottom-right rounded corner
            path.AddBezier(
                x + (0.679f * s), y + (bottomRatio * s),
                x + (0.764f * s), y + (bottomRatio * s),
                x + (usedWidth * s), y + (0.682f * s),
                x + (usedWidth * s), y + (0.586f * s));

            // Right bump, ascending right side
            path.AddBezier(
                x + (usedWidth * s), y + (0.586f * s),
                x + (0.834f * s), y + (0.494f * s),
                x + (0.770f * s), y + (0.419f * s),
                x + (0.690f * s), y + (0.413f * s));

            // Right bump top arc
            path.AddBezier(
                x + (0.690f * s), y + (0.413f * s),
                x + (0.660f * s), y + (0.309f * s),
                x + (0.577f * s), y + (0.240f * s),
                x + (0.478f * s), y + (0.240f * s));

            // Right bump descending to middle valley
            path.AddBezier(
                x + (0.478f * s), y + (0.240f * s),
                x + (0.415f * s), y + (0.240f * s),
                x + (0.358f * s), y + (0.269f * s),
                x + (0.321f * s), y + (0.315f * s));

            // Middle valley across to left bump top
            path.AddLine(
                x + (0.321f * s), y + (0.315f * s),
                x + (0.260f * s), y + (0.310f * s));

            // Left bump top arc, back to start
            path.AddBezier(
                x + (0.260f * s), y + (0.310f * s),
                x + (0.197f * s), y + (0.310f * s),
                x + (0.145f * s), y + (0.358f * s),
                x + (0.138f * s), y + (0.420f * s));

            SmoothingMode oldMode = graphics.SmoothingMode;
            try
            {
                if (pen.Width == 1f)
                {
                    graphics.SmoothingMode = SmoothingMode.None;
                }

                graphics.DrawPath(pen, path);
            }
            finally
            {
                graphics.SmoothingMode = oldMode;
            }
        }
    }
}
