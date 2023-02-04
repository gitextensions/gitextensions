using GitCommands;
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
        internal readonly TranslationString Checkout = new("Chec&kout remote branch...");
        internal readonly TranslationString Rebase = new("&Rebase current branch on this remote branch...");
        internal readonly TranslationString Delete = new("&Delete remote branch...");
        internal readonly TranslationString DeleteTooltip = new("Delete the branch from the remote");

        public RemoteBranchMenuItemsStrings()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        public void ApplyTo(MenuItemsStrings strings)
        {
            new BranchMenuItemsStrings().ApplyTo(strings);
            strings.Checkout = Checkout;
            strings.Rebase = Rebase;
            strings.Delete = Delete;
            strings.Tooltips[MenuItemKey.Delete] = DeleteTooltip;
        }
    }
}
