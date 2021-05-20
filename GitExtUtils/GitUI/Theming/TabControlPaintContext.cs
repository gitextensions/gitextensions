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
        private readonly Image?[] _tabImages;
        private readonly Rectangle[] _tabRects;
        private readonly string[] _tabTexts;
        private readonly Size _size;
        private readonly bool _failed;

        private static readonly int ImagePadding = DpiUtil.Scale(6);

        // DPI 100% - 175%: 2; DPI 200%: 4
        // so that when leftmost tab is selected, its border matches the border of tab control
        private static readonly int SelectedTabPadding = 2 * (int)Math.Floor(DpiUtil.ScaleX);
        private const int BorderWidth = 1;

        public TabControlPaintContext(TabControl tabs, PaintEventArgs e)
        {
            _mouseCursor = tabs.PointToClient(Cursor.Position);
            _graphics = e.Graphics;
            _clipRectangle = e.ClipRectangle;
            _size = tabs.Size;
            _parentBackColor = GetParentBackColor(tabs);
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
            }
            catch (ArgumentOutOfRangeException)
            {
                // Workaround probable bug in .NET framework, known example:
                // tabCtrl.GetTabRect[tabCtrl.SelectedIndex] may throw ArgumentOutOfRangeException
                // https://github.com/gitextensions/gitextensions/pull/7213#issuecomment-554760531
                _failed = true;

                // Set these to null explicitly to satisfy nullability checking. We will always verify
                // _failed before dereferencing these.
                _tabTexts = null!;
                _tabImages = null!;
                _tabRects = null!;
            }
        }

        public void Paint()
        {
            if (_failed)
            {
                return;
            }

            using SolidBrush canvasBrush = new(_parentBackColor);
            _graphics.FillRectangle(canvasBrush, _clipRectangle);

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
                RenderTabText(index, _tabImages[index] is not null);
            }
        }

        private void RenderTabBackground(int index)
        {
            using var borderPen = CreateBorderPen();
            var outerRect = GetOuterTabRect(index);
            _graphics.FillRectangle(GetBackgroundBrush(index), outerRect);

            List<Point> points = new(4);
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

        private void RenderTabImage(int index)
        {
            var image = _tabImages[index];
            if (image is null)
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
            Rectangle imgRect = new(
                new Point(innerRect.X + ImagePadding,
                    innerRect.Y + ((innerRect.Height - imgHeight) / 2)),
                _imageSize);

            if (index == _selectedIndex)
            {
                imgRect.Offset(0, -SelectedTabPadding);
            }

            return imgRect;
        }

        private static Image? GetTabImage(TabControl tabs, int index)
        {
            var images = tabs.ImageList?.Images;
            if (images is null)
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
            using var borderPen = CreateBorderPen();
            {
                _graphics.DrawRectangle(borderPen, pageRect);
            }
        }

        private Color GetParentBackColor(TabControl tabs)
        {
            var parent = tabs.Parent;
            while (parent is not null)
            {
                if (parent.BackColor != Color.Transparent)
                {
                    return parent.BackColor;
                }

                parent = parent.Parent;
            }

            return SystemColors.Window;
        }

        private Brush GetBackgroundBrush(int index)
        {
            if (index == _selectedIndex)
            {
                return SystemBrushes.Window;
            }

            bool isHighlighted = _tabRects[index].Contains(_mouseCursor);
            return isHighlighted
                ? new SolidBrush(ColorHelper.Lerp(SystemColors.Control, SystemColors.HotTrack, 64f / 255f))
                : SystemBrushes.Control;
        }

        private Pen CreateBorderPen() =>
            new(Color.LightGray.AdaptBackColor(), BorderWidth);
    }
}
