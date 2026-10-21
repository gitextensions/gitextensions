using System.Drawing.Drawing2D;

namespace GitUI;

// Helper that renders the drop-down arrow scaled proportionally to the toolbar's icon size.
// WinForms draws the arrow at a fixed size (sized for 16px icons), so on larger icons it
// looks disproportionately small. This scales the arrow to match <see cref="ToolStrip.ImageScalingSize"/>.
internal static class ToolStripArrowScaler
{
    // The icon size (in pixels) the framework's default arrow is designed for.
    private const int BaselineIconSize = 16;

    // Invokes <paramref name="renderBase"/> with the graphics scaled around the arrow's center so
    // the arrow grows proportionally to the toolbar icon size. Falls back to the base rendering
    // when no scaling is needed.
    public static void RenderScaledArrow(ToolStripArrowRenderEventArgs e, Action<ToolStripArrowRenderEventArgs> renderBase)
    {
        float scale = GetScale(e.Item?.Owner);
        if (scale <= 1f)
        {
            renderBase(e);
            return;
        }

        Graphics g = e.Graphics;
        Rectangle rect = e.ArrowRectangle;
        float centerX = rect.Left + (rect.Width / 2f);
        float centerY = rect.Top + (rect.Height / 2f);

        GraphicsState state = g.Save();
        try
        {
            g.TranslateTransform(centerX, centerY);
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-centerX, -centerY);
            renderBase(e);
        }
        finally
        {
            g.Restore(state);
        }
    }

    // Returns <see langword="true"/> when <paramref name="item"/>'s owning toolbar uses icons larger
    // than the baseline, i.e. when its drop-down arrow should be scaled up.
    public static bool NeedsScaling(ToolStripItem? item) => GetScale(item?.Owner) > 1f;

    private static float GetScale(ToolStrip? toolStrip)
    {
        int width = toolStrip?.ImageScalingSize.Width ?? 0;
        return width > BaselineIconSize ? (float)width / BaselineIconSize : 1f;
    }
}
