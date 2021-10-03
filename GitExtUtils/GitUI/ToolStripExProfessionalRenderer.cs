using System.Windows.Forms;

namespace GitUI
{
    internal class ToolStripExProfessionalRenderer : ToolStripProfessionalRenderer
    {
        public ToolStripExProfessionalRenderer()
        {
            RoundedEdges = false;
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
