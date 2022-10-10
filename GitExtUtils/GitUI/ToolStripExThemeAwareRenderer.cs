using System.Runtime.CompilerServices;
using GitExtUtils.GitUI.Theming;

namespace GitUI
{
    internal sealed class ToolStripExThemeAwareRenderer : ToolStripExProfessionalRenderer
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
    }
}
