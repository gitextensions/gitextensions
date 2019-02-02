using GitUI.BranchTreePanel.Interfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
{
    internal class RemoteBranchMenuItems<TNode> : MenuItemsGenerator<TNode>
        where TNode : class, INode
    {
        public RemoteBranchMenuItems(IMenuItemFactory menuItemFactory) : base(menuItemFactory)
        {
            new RemoteBranchMenuItemsStrings().ApplyTo(Strings);
        }
    }

    public class RemoteBranchMenuItemsStrings : Translate
    {
        internal readonly TranslationString DeleteTooltip = new TranslationString("Delete the branch from the remote");

        public void ApplyTo(MenuItemsStrings strings)
        {
            new BranchMenuItemsStrings().ApplyTo(strings);
            strings.Tooltips[MenuItemKey.Delete] = DeleteTooltip;
        }
    }
}
