using System.Collections.Generic;
using System.Linq;
using GitUI.BranchTreePanel.Interfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
{
    internal class LocalBranchMenuItems<TNode> : MenuItemsGenerator<TNode>
        where TNode : class, INode
    {
        private static MenuItemKey[] _inactiveBranchFilterKeys =
                new[]
                {
                    MenuItemKey.GitRefCheckout,
                    MenuItemKey.GitRefMerge,
                    MenuItemKey.GitRefMerge,
                    MenuItemKey.GitRefRebase,
                    MenuItemKey.GitRefReset,
                    MenuItemKey.GitRefActionsSeparator,
                    MenuItemKey.Delete
                };

        public LocalBranchMenuItems(IMenuItemFactory menuItemFactory) : base(menuItemFactory)
        {
            new LocalBranchMenuItemsStrings().ApplyTo(Strings);
        }

        /// <summary>
        /// Filter menu entries that depend on the branch being active or not
        /// </summary>
        public IEnumerable<ToolStripItemWithKey> GetInactiveBranchItems()
        {
            return this.Where(t => _inactiveBranchFilterKeys.Contains(t.Key));
        }
    }

    public class LocalBranchMenuItemsStrings : Translate
    {
        internal readonly TranslationString DeleteTooltip = new TranslationString("Delete the branch, which must be fully merged in its upstream branch or in HEAD");

        public void ApplyTo(MenuItemsStrings strings)
        {
            new BranchMenuItemsStrings().ApplyTo(strings);
            strings.Tooltips[MenuItemKey.Delete] = DeleteTooltip;
        }
    }
}
