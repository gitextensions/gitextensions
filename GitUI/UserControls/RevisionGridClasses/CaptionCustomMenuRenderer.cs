using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGridClasses
{
    /// <summary>
    /// no mouse over effect for disabled menu items, if the Tag is "caption"
    /// </summary>
    internal class CaptionCustomMenuRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Tag as string != "caption")
            {
                base.OnRenderMenuItemBackground(e);
                return;
            }

            if (!e.Item.Enabled)
            {
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }
    }
}
