using System.Drawing;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    internal static class RevisionGridUtils
    {
        public static Rectangle GetCellRectangle(DataGridViewCellPaintingEventArgs e)
        {
            var rect = new Rectangle(e.CellBounds.Left, e.CellBounds.Top + 4, e.CellBounds.Width,
                e.CellBounds.Height);
            return rect;
        }

        /// <summary>
        /// Draws the text and returns true if the text has been truncated.
        /// </summary>
        public static bool DrawColumnTextTruncated(Graphics gc, string text, Font font, Color color, Rectangle bounds)
        {
            int width = TextRenderer.MeasureText(gc, text, font, bounds.Size, TextFormatFlags.NoPrefix).Width;
            TextRenderer.DrawText(gc, text, font, bounds, color, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            return width > bounds.Width;
        }
    }
}
