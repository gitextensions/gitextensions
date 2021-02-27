using System.Windows.Forms;

namespace GitExtUtils.GitUI
{
    public interface IMenuItemBackgroundFilter
    {
        bool ShouldRenderMenuItemBackground(ToolStripItemRenderEventArgs e);
    }
}
