using System.Windows.Forms;

namespace GitCommands
{
    public class CustomDiffMergeTool
    {
        public CustomDiffMergeTool(ToolStripMenuItem menuItem, System.EventHandler click)
        {
            MenuItem = menuItem;
            Click = click;
        }

        public ToolStripMenuItem MenuItem { get; set; }
        public System.EventHandler Click;
    }
}
