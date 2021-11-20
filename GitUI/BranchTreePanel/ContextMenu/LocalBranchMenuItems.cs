using GitUI.BranchTreePanel.Interfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
{
    internal class LocalBranchMenuItems<TNode> : MenuItemsGenerator<TNode>
        where TNode : class, INode
    {
        /// <summary> Keys of menu items only applying to inactive (i.e. not the current) branches.</summary>
        internal static MenuItemKey[] InactiveBranchFilterKeys =
                new[]
                {
                    MenuItemKey.GitRefCheckout,
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
    }

    public class LocalBranchMenuItemsStrings : Translate
    {
        internal readonly TranslationString DeleteTooltip = new("Delete the branch, which must be fully merged in its upstream branch or in HEAD");

        public void ApplyTo(MenuItemsStrings strings)
        {
            new BranchMenuItemsStrings().ApplyTo(strings);
            strings.Tooltips[MenuItemKey.Delete] = DeleteTooltip;
        }
    }
}
