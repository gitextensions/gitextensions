using System.Windows.Forms;

namespace GitUI.Browsing
{
    public interface IUserScriptMenuBuilder
    {
        /// <summary>
        /// Build and insert 'Run script' tool
        /// </summary>
        /// <param name="tool">Top tool</param>
        void Build(ToolStrip tool);

        /// <summary>
        /// Build and insert 'Run script' menu
        /// </summary>
        /// <param name="contextMenu">Context menu</param>
        void Build(ContextMenuStrip contextMenu);
    }
}
