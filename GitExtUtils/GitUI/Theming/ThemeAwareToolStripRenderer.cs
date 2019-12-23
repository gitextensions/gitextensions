using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace GitExtUtils.GitUI.Theming
{
    public class ThemeAwareToolStripRenderer : ToolStripProfessionalRenderer
    {
        private static readonly ConditionalWeakTable<Bitmap, Bitmap> AdaptedImagesCache =
            new ConditionalWeakTable<Bitmap, Bitmap>();

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
