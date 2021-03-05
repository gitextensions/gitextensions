using System;
using System.Windows.Forms;

namespace GitCommands
{
    public class CustomDiffMergeTool
    {
        public CustomDiffMergeTool(ToolStripMenuItem menuItem, EventHandler click)
        {
            MenuItem = menuItem;
            Click = click;
        }

        public ToolStripMenuItem MenuItem { get; }
        public EventHandler Click { get; }
    }
}
