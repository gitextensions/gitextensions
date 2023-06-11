using System.Diagnostics.CodeAnalysis;
using GitUI.LeftPanel.Interfaces;

namespace GitUI.LeftPanel.ContextMenu
{
    /// <summary>
    /// Builds context menu actions for a <see cref="INode"/> depending on declared interfaces.
    /// </summary>
    public interface IMenuItemsGenerator<TNode> : IEnumerable<ToolStripItemWithKey>
        where TNode : class, INode
    {
        bool TryGetMenuItem(MenuItemKey key, [NotNullWhen(true)] out ToolStripItem? item);
    }

    public class ToolStripItemWithKey
    {
        public ToolStripItemWithKey(MenuItemKey key, ToolStripItem item)
        {
            Key = key;
            Item = item;
        }

        public MenuItemKey Key { get; }
        public ToolStripItem Item { get; }
    }
}
