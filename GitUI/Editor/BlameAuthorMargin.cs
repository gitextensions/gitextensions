using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using ICSharpCode.TextEditor;

namespace GitUI.Editor
{
    /// <summary>
    /// This class display avatars in the gutter in a blame control.
    /// </summary>
    public class BlameAuthorMargin : AbstractMargin
    {
        private static readonly int AgeBucketMarkerWidth = Convert.ToInt32(4 * DpiUtil.ScaleX);
        private List<Image> _avatars;
        private readonly int _lineHeight;
        private readonly Color _backgroundColor;
        private List<GitBlameEntry> _blameLines;
        private readonly Dictionary<int, SolidBrush> _brushs = new Dictionary<int, SolidBrush>();
        private bool _isVisible = true;

        public BlameAuthorMargin(TextArea textArea) : base(textArea)
        {
            _lineHeight = GetFontHeight(textArea.Font);
            _backgroundColor = SystemColors.Window;
            Width = _lineHeight + AgeBucketMarkerWidth + DpiUtil.Scale(2);
        }

        public override int Width { get; }
        public override bool IsVisible => _isVisible;

        private static int GetFontHeight(Font font)
        {
            var max = Math.Max(
                TextRenderer.MeasureText("_", font).Height,
                (int)Math.Ceiling(font.GetHeight()));

            return max + 1;
        }

        public void Initialize(IEnumerable<GitBlameEntry> blameLines)
        {
            _blameLines = blameLines.ToList();
            _avatars = _blameLines.Select(a => a.Avatar).ToList();

            // Update the resolution otherwise the image is not drawn at the good size :(
            foreach (var avatar in _avatars)
            {
                if (avatar is Bitmap bitmapAvatar)
                {
                    bitmapAvatar.SetResolution(DpiUtil.DpiX, DpiUtil.DpiY);
                }
            }

            // Build brushes
            foreach (var blameLine in _blameLines)
            {
                if (!_brushs.ContainsKey(blameLine.AgeBucketIndex))
                {
                    _brushs.Add(blameLine.AgeBucketIndex, new SolidBrush(blameLine.AgeBucketColor));
                }
            }
        }

        public void SetVisiblity(bool isVisible)
        {
            _isVisible = isVisible;
        }

        public override void Paint(Graphics g, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0 || _blameLines == null || _blameLines.Count == 0)
            {
                return;
            }

            g.Clear(_backgroundColor);

            if (_avatars == null || _avatars.Count == 0)
            {
                return;
            }

            var verticalOffset = textArea.VirtualTop.Y;
            var lineStart = verticalOffset / _lineHeight;
            var negativeOffset = (lineStart * _lineHeight) - verticalOffset;
            var lineCount = (int)Math.Ceiling((double)(rect.Height - negativeOffset) / _lineHeight);

            for (int i = 0; i < lineCount; i++)
            {
                if (lineStart + i >= _avatars.Count)
                {
                    break;
                }

                int y = negativeOffset + (i * _lineHeight);
                g.FillRectangle(_brushs[_blameLines[lineStart + i].AgeBucketIndex], 0, y, AgeBucketMarkerWidth, _lineHeight);

                if (_avatars[lineStart + i] != null)
                {
                    g.DrawImage(_avatars[lineStart + i], new Point(AgeBucketMarkerWidth, y));
                }
            }

            base.Paint(g, rect);
        }
    }
}
