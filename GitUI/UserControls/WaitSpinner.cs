using System.Drawing.Drawing2D;
using GitExtUtils.GitUI;
using Timer = System.Windows.Forms.Timer;

namespace GitUI.UserControls
{
    public sealed class WaitSpinner : Control
    {
        private readonly int _dotCount = 11;
        private readonly int _dotRadius = DpiUtil.Scale(8);
        private readonly int _circleRadius = DpiUtil.Scale(18);

        private readonly Color _color = SystemColors.ControlDarkDark;
        private readonly Timer _timer;
        private readonly IReadOnlyList<Brush> _brushes;
        private readonly (float sin, float cos)[] _angles;

        private bool _isAnimating;
        private PointF _centre;
        private int _progress;

        public bool IsAnimating
        {
            get => _isAnimating;
            set
            {
                _isAnimating = value;

                if (_isAnimating)
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
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);

            _brushes = GetBrushes();
            _angles = GetAngles();
            UpdateCentre();

            _timer = new Timer { Interval = 1000 / 30 }; // 30 fps
            _timer.Tick += delegate
            {
                _progress = (_progress + 1) % _dotCount;
                Invalidate();
            };
            IsAnimating = true;

            Resize += delegate { UpdateCentre(); };

            return;

            void UpdateCentre()
            {
                _centre = new PointF(Width / 2, (Height / 2) - 1);
            }

            IReadOnlyList<Brush> GetBrushes()
            {
                int alphaDelta = (byte.MaxValue / 2) / _dotCount;

                Brush[] brushes = new Brush[_dotCount];
                int alpha = 0;

                for (int i = 0; i < _dotCount; i++)
                {
                    if (alpha > 255)
                    {
                        alpha = 255;
                    }

                    brushes[i] = new SolidBrush(Color.FromArgb((byte)alpha, _color));
                    alpha += alphaDelta;
                }

                return brushes;
            }

            (float sin, float cos)[] GetAngles()
            {
                double angleDelta = Math.PI * 2.0 / _dotCount;

                (float sin, float cos)[] angles = new (float sin, float cos)[_dotCount];

                double angle = 0.0;

                for (int i = 0; i < _dotCount; i++)
                {
                    angles[i] = (
                        sin: (float)Math.Sin(angle),
                        cos: (float)Math.Cos(angle));
                    angle += angleDelta;
                }

                return angles;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            int p = _progress;
            for (int i = 0; i < _dotCount; i++)
            {
                p %= _dotCount;
                ref (float sin, float cos) angle = ref _angles[p];
                e.Graphics.FillEllipse(
                    _brushes[i],
                    (_centre.X - (_dotRadius / 2f)) + (_circleRadius * angle.cos),
                    (_centre.Y - (_dotRadius / 2f)) + (_circleRadius * angle.sin),
                    _dotRadius,
                    _dotRadius);
                p++;
            }

            base.OnPaint(e);
        }
    }
}
