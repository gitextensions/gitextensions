using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GitExtUtils.GitUI.Theming
{
    internal class TabControlPaintContext
    {
        private readonly Point _mouseCursor;
        private readonly Graphics _graphics;
        private readonly Rectangle _clipRectangle;
        private readonly Color _parentBackColor;
        private readonly int _selectedIndex;
        private readonly int _tabCount;
        private readonly Size _imageSize;
        private readonly Font _font;
        private readonly bool _enabled;
        private readonly Image[] _tabImages;
        private readonly Rectangle[] _tabRects;
        private readonly string[] _tabTexts;
        private readonly Color[] _tabBackColors;
        private readonly Size _size;
        private readonly bool _failed;

        private static int ImagePadding { get; } = DpiUtil.Scale(6);
        private static int SelectedTabPadding { get; } = DpiUtil.Scale(2);
        private static int BorderWidth { get; } = DpiUtil.Scale(1);

        public TabControlPaintContext(TabControl tabs, PaintEventArgs e)
        {
            _mouseCursor = tabs.PointToClient(Cursor.Position);
            _graphics = e.Graphics;
            _clipRectangle = e.ClipRectangle;
            _size = tabs.Size;
            _parentBackColor = tabs.Parent.BackColor;
            _selectedIndex = tabs.SelectedIndex;
            _tabCount = tabs.TabCount;
            _font = tabs.Font;
            _imageSize = tabs.ImageList?.ImageSize ?? Size.Empty;
            _enabled = tabs.Enabled;

            try
            {
                _tabTexts = Enumerable.Range(0, _tabCount)
                    .Select(i => tabs.TabPages[i].Text)
                    .ToArray();
                _tabImages = Enumerable.Range(0, _tabCount)
                    .Select(i => GetTabImage(tabs, i))
                    .ToArray();
                _tabRects = Enumerable.Range(0, _tabCount)
                    .Select(tabs.GetTabRect)
                    .ToArray();
                _tabBackColors = Enumerable.Range(0, _tabCount)
                    .Select(i => tabs.TabPages[i].BackColor)
                    .ToArray();
            }
            catch (ArgumentOutOfRangeException)
            {
                // Workaround probable bug in .NET framework, known example:
                // tabCtrl.GetTabRect[tabCtrl.SelectedIndex] may throw ArgumentOutOfRangeException
                // https://github.com/gitextensions/gitextensions/pull/7213#issuecomment-554760531
                _failed = true;
            }
        }

        public void Paint()
        {
            if (_failed)
            {
                return;
            }

            using (var canvasBrush = new SolidBrush(_parentBackColor))
            {
                _graphics.FillRectangle(canvasBrush, _clipRectangle);
            }

            RenderSelectedPageBackground();

            IEnumerable<int> pageIndices;
            if (_selectedIndex.IsWithin(0, _tabCount))
            {
                // render tabs in pyramid order with selected on top
                pageIndices = Enumerable.Range(0, _selectedIndex)
                    .Concat(Enumerable.Range(_selectedIndex, _tabCount - _selectedIndex).Reverse());
            }
            else
            {
                pageIndices = Enumerable.Range(0, _tabCount);
            }

            foreach (var index in pageIndices)
            {
                RenderTabBackground(index);
                RenderTabImage(index);
                RenderTabText(index, _tabImages[index] != null);
            }
        }

        private void RenderTabBackground(int index)
        {
            using (var borderPen = GetBorderPen())
            {
                var outerRect = GetOuterTabRect(index);
                _graphics.FillRectangle(GetBackgroundBrush(index), outerRect);

                var points = new List<Point>(4);
                if (index <= _selectedIndex)
                {
                    points.Add(new Point(outerRect.Left, outerRect.Bottom - 1));
                }

                points.Add(new Point(outerRect.Left, outerRect.Top));
                points.Add(new Point(outerRect.Right - 1, outerRect.Top));

                if (index >= _selectedIndex)
                {
                    points.Add(new Point(outerRect.Right - 1, outerRect.Bottom - 1));
                }

                _graphics.DrawLines(borderPen, points.ToArray());
            }
        }

        private void RenderTabImage(int index)
        {
            var image = _tabImages[index];
            if (image == null)
            {
                return;
            }

            var imgRect = GetTabImageRect(index);
            _graphics.DrawImage(image, imgRect);
        }

        private Rectangle GetTabImageRect(int index)
        {
            var innerRect = _tabRects[index];
            int imgHeight = _imageSize.Height;
            var imgRect = new Rectangle(
                new Point(innerRect.X + ImagePadding,
                    innerRect.Y + ((innerRect.Height - imgHeight) / 2)),
                _imageSize);

            if (index == _selectedIndex)
            {
                imgRect.Offset(0, -SelectedTabPadding);
            }

            return imgRect;
        }

        private static Image GetTabImage(TabControl tabs, int index)
        {
            var images = tabs.ImageList?.Images;
            if (images == null)
            {
                return null;
            }

            var page = tabs.TabPages[index];
            if (!string.IsNullOrEmpty(page.ImageKey))
            {
                return images[page.ImageKey];
            }

            if (page.ImageIndex.IsWithin(0, images.Count))
            {
                return images[page.ImageIndex];
            }

            return null;
        }

        private void RenderTabText(int index, bool hasImage)
        {
            if (string.IsNullOrEmpty(_tabTexts[index]))
            {
                return;
            }

            var textRect = GetTabTextRect(index, hasImage);

            const TextFormatFlags format =
                TextFormatFlags.NoClipping |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.HorizontalCenter;

            var textColor = _enabled
                ? index == _selectedIndex
                    ? SystemColors.WindowText
                    : SystemColors.ControlText
                : SystemColors.GrayText;

            TextRenderer.DrawText(_graphics, _tabTexts[index], _font, textRect, textColor, format);
        }

        private Rectangle GetTabTextRect(int index, bool hasImage)
        {
            var innerRect = _tabRects[index];
            Rectangle textRect;
            if (hasImage)
            {
                int deltaWidth = _imageSize.Width + ImagePadding;
                textRect = new Rectangle(
                    innerRect.X + deltaWidth,
                    innerRect.Y,
                    innerRect.Width - deltaWidth,
                    innerRect.Height);
            }
            else
            {
                textRect = innerRect;
            }

            if (index == _selectedIndex)
            {
                textRect.Offset(0, -SelectedTabPadding);
            }

            return textRect;
        }

        private Rectangle GetOuterTabRect(int index)
        {
            var innerRect = _tabRects[index];

            if (index == _selectedIndex)
            {
                return Rectangle.FromLTRB(
                    innerRect.Left - SelectedTabPadding,
                    innerRect.Top - SelectedTabPadding,
                    innerRect.Right + SelectedTabPadding,
                    innerRect.Bottom + 1); // +1 to overlap tabs bottom line
            }

            return Rectangle.FromLTRB(
                innerRect.Left,
                innerRect.Top + 1,
                innerRect.Right,
                innerRect.Bottom);
        }

        private void RenderSelectedPageBackground()
        {
            if (!_selectedIndex.IsWithin(0, _tabCount))
            {
                return;
            }

            var tabRect = _tabRects[_selectedIndex];
            var pageRect = Rectangle.FromLTRB(0, tabRect.Bottom, _size.Width - 1,
                _size.Height - 1);

            if (!_clipRectangle.IntersectsWith(pageRect))
            {
                return;
            }

            _graphics.FillRectangle(GetBackgroundBrush(_selectedIndex), pageRect);
            using (var borderPen = GetBorderPen())
            {
                _graphics.DrawRectangle(borderPen, pageRect);
            }
        }

        private Brush GetBackgroundBrush(int index)
        {
            if (index == _selectedIndex)
            {
                return _tabBackColors[index] == Color.Transparent
                    ? SystemBrushes.Window
                    : new SolidBrush(_tabBackColors[index]);
            }

            bool isHighlighted = _tabRects[index].Contains(_mouseCursor);

            return isHighlighted
                ? SystemBrushes.ControlLightLight
                : SystemBrushes.Control;
        }

        private Pen GetBorderPen() =>
            new Pen(SystemBrushes.ControlDark, BorderWidth);
    }
}
