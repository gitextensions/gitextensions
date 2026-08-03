using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MediaColor = Avalonia.Media.Color;
using MediaPoint = Avalonia.Point;

namespace GitUI.UserControls;

public sealed class WaitSpinner : Control
{
    private readonly int _dotCount = 11;
    private readonly double _dotRadius = 8;
    private readonly double _circleRadius = 18;

    private readonly DispatcherTimer _timer;
    private readonly (double sin, double cos)[] _angles;

    private bool _isAnimating;
    private int _progress;

    public new bool IsAnimating
    {
        get => _isAnimating;
        set
        {
            _isAnimating = value;

            if (_isAnimating && VisualRoot is not null)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }
    }

    public WaitSpinner()
    {
        _angles = GetAngles();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000d / 30d) }; // 30 fps
        _timer.Tick += delegate
        {
            _progress = (_progress + 1) % _dotCount;
            InvalidateVisual();
        };
        IsAnimating = true;

        AttachedToVisualTree += (_, _) =>
        {
            if (IsAnimating)
            {
                _timer.Start();
            }
        };
        DetachedFromVisualTree += (_, _) => _timer.Stop();

        return;

        (double sin, double cos)[] GetAngles()
        {
            double angleDelta = Math.PI * 2.0 / _dotCount;

            (double sin, double cos)[] angles = new (double sin, double cos)[_dotCount];

            double angle = 0.0;

            for (int i = 0; i < _dotCount; i++)
            {
                angles[i] = (
                    sin: Math.Sin(angle),
                    cos: Math.Cos(angle));
                angle += angleDelta;
            }

            return angles;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        MediaColor color = ResolveColor();
        MediaPoint centre = new(Bounds.Width / 2, (Bounds.Height / 2) - 1);
        int alphaDelta = (byte.MaxValue / 2) / _dotCount;
        int alpha = 0;
        int p = _progress;
        for (int i = 0; i < _dotCount; i++)
        {
            p %= _dotCount;
            (double sin, double cos) angle = _angles[p];
            byte currentAlpha = (byte)Math.Min(byte.MaxValue, alpha);
            SolidColorBrush brush = new(MediaColor.FromArgb(currentAlpha, color.R, color.G, color.B));
            context.DrawEllipse(
                brush,
                null,
                new MediaPoint(
                    centre.X + (_circleRadius * angle.cos),
                    centre.Y + (_circleRadius * angle.sin)),
                _dotRadius / 2,
                _dotRadius / 2);
            alpha += alphaDelta;
            p++;
        }
    }

    private MediaColor ResolveColor()
    {
        if (this.TryFindResource("GitExtensionsKnownColorControlDarkDarkBrush", ActualThemeVariant, out object? value)
            && value is ISolidColorBrush brush)
        {
            return brush.Color;
        }

        return Colors.DimGray;
    }

    // parity-scaffolding: Freezes animation at a deterministic frame for render tests and paired captures.
    internal void SetProgressForCapture(int progress)
    {
        _progress = Math.Abs(progress) % _dotCount;
        InvalidateVisual();
    }
}
