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
        private List<Image?>? _avatars;
        private readonly Color _backgroundColor;
        private List<GitBlameEntry>? _blameLines;
        private readonly Dictionary<int, SolidBrush> _brushs = [];
        private bool _isVisible = true;

        public BlameAuthorMargin(TextArea textArea) : base(textArea)
        {
            _backgroundColor = SystemColors.Window;
        }

        public int LineHeight => textArea.TextView.FontHeight;
        public override int Width => LineHeight + AgeBucketMarkerWidth + DpiUtil.Scale(2);

        public override bool IsVisible => _isVisible;

        public void Initialize(IEnumerable<GitBlameEntry> blameLines)
        {
            _blameLines = blameLines.ToList();
            _avatars = _blameLines.Select(a => a.Avatar).ToList();

            // Update the resolution otherwise the image is not drawn at the good size :(
            foreach (Image avatar in _avatars)
            {
                if (avatar is Bitmap bitmapAvatar)
                {
                    bitmapAvatar.SetResolution(DpiUtil.DpiX, DpiUtil.DpiY);
                }
            }

            // Build brushes
            foreach (GitBlameEntry blameLine in _blameLines)
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
            if (rect.Width <= 0 || rect.Height <= 0 || _blameLines?.Count is not > 0)
            {
                return;
            }

            g.Clear(_backgroundColor);

            if (_avatars?.Count is not > 0)
            {
                return;
            }

            int verticalOffset = textArea.VirtualTop.Y;
            int lineStart = verticalOffset / LineHeight;
            int negativeOffset = (lineStart * LineHeight) - verticalOffset;
            int lineCount = (int)Math.Ceiling((double)(rect.Height - negativeOffset) / LineHeight);

            for (int i = 0; i < lineCount; i++)
            {
                if (lineStart + i >= _avatars.Count)
                {
                    break;
                }

                int y = negativeOffset + (i * LineHeight);
                g.FillRectangle(_brushs[_blameLines[lineStart + i].AgeBucketIndex], 0, y, AgeBucketMarkerWidth, LineHeight);

                if (_avatars[lineStart + i] is not null)
                {
                    g.DrawImage(_avatars[lineStart + i], new Point(AgeBucketMarkerWidth, y));
                }
            }

            base.Paint(g, rect);
        }
    }
}
