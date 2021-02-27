using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using GitExtUtils.GitUI.Theming;

namespace GitUI
{
    internal sealed class ToolStripExThemeAwareRenderer : ToolStripProfessionalRenderer
    {
        private static readonly ConditionalWeakTable<Bitmap, Bitmap> AdaptedImagesCache = new();

        public ToolStripExThemeAwareRenderer()
        {
            RoundedEdges = false;
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var image = (Bitmap)e.Image;
            if (!AdaptedImagesCache.TryGetValue(image, out var adapted))
            {
                adapted = image.AdaptLightness();
                AdaptedImagesCache.Add(image, adapted);
            }

            base.OnRenderItemCheck(new ToolStripItemImageRenderEventArgs(
                e.Graphics, e.Item, adapted, e.ImageRectangle));
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.ToolStrip.GetMenuItemBackgroundFilter()?.ShouldRenderMenuItemBackground(e) != false)
            {
                base.OnRenderMenuItemBackground(e);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is not IToolStripEx { DrawBorder: false })
            {
                // render border
                base.OnRenderToolStripBorder(e);
            }
        }
    }
}
