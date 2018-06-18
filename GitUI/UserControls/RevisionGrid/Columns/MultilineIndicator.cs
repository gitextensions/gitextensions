using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal readonly struct MultilineIndicator
    {
        private static readonly Brush _indicatorForeBrush = new SolidBrush(Color.FromArgb(128, Color.Black));
        private static readonly Brush _indicatorBackBrush = new SolidBrush(Color.FromArgb(32, Color.Black));

        private const int DotCount = 3;

        private static readonly int paddingX = DpiUtil.Scale(4);
        private static readonly int paddingTop = DpiUtil.Scale(5);
        private static readonly int paddingBottom = DpiUtil.Scale(4);
        private static readonly int marginX = DpiUtil.Scale(4);
        private static readonly int dotSize = DpiUtil.Scale(2);
        private static readonly int dotSpacing = DpiUtil.Scale(2);

        private readonly DataGridViewCellPaintingEventArgs _e;
        private readonly int _indicatorReservedWidth;
        private readonly int _indicatorRectHeight;
        private readonly int _indicatorRectWidth;
        private readonly bool _isMultiline;

        public Rectangle RemainingCellBounds => _isMultiline ? _e.CellBounds.ReduceRight(_indicatorReservedWidth) : _e.CellBounds;

        public MultilineIndicator(DataGridViewCellPaintingEventArgs e, GitRevision revision)
        {
            _e = e;

            _isMultiline = revision.HasMultiLineMessage;

            if (_isMultiline)
            {
                _indicatorRectWidth = paddingX + paddingX + (DotCount * (dotSize + dotSpacing)) - dotSpacing;
                _indicatorReservedWidth = _indicatorRectWidth + marginX + marginX;
                _indicatorRectHeight = dotSize + paddingTop + paddingBottom;

                if (e.CellBounds.Width < 2 * _indicatorReservedWidth)
                {
                    _isMultiline = false;
                }
            }
            else
            {
                _indicatorRectWidth = 0;
                _indicatorRectHeight = 0;
                _indicatorReservedWidth = 0;
            }
        }

        public void Render()
        {
            if (!_isMultiline)
            {
                return;
            }

            var indicatorRect = new Rectangle(
                _e.CellBounds.Right - marginX - _indicatorRectWidth,
                _e.CellBounds.Y + ((_e.CellBounds.Height - _indicatorRectHeight) / 2),
                _indicatorRectWidth,
                _indicatorRectHeight);

            _e.Graphics.FillRectangle(_indicatorBackBrush, indicatorRect);

            var x = indicatorRect.X + paddingX;
            var y = indicatorRect.Y + paddingTop;

            for (var i = 0; i < DotCount; i++)
            {
                _e.Graphics.FillRectangle(_indicatorForeBrush, x, y, dotSize, dotSize);
                x += dotSize + dotSpacing;
            }
        }
    }
}