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

        public static void DrawColumnText(Graphics gc, string text, Font font, Color color, Rectangle bounds)
        {
            TextRenderer.DrawText(gc, text, font, bounds, color,
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}
