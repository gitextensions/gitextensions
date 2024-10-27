using GitUI.LeftPanel.Interfaces;

namespace GitUI.LeftPanel.ContextMenu
{
    internal class FavoritesBranchMenuItems<TNode> : MenuItemsGenerator<TNode>
        where TNode : class, INode
    {
        public FavoritesBranchMenuItems(IMenuItemFactory menuItemFactory) : base(menuItemFactory)
        {
            new FavoriteBranchMenuItemsStrings().ApplyTo(Strings);
        }
    }
}
