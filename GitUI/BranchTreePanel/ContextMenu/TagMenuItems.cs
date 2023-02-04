using GitCommands;
using GitUI.BranchTreePanel.Interfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
{
    internal class TagMenuItems<TNode> : MenuItemsGenerator<TNode>
        where TNode : class, INode
    {
        public TagMenuItems(IMenuItemFactory menuItemFactory) : base(menuItemFactory)
        {
            new TagMenuItemsStrings().ApplyTo(Strings);
        }
    }

    public class TagMenuItemsStrings : Translate
    {
        internal readonly TranslationString Checkout = new("Chec&kout tag revision...");
        internal readonly TranslationString Rebase = new("&Rebase current branch on this tag revision...");
        internal readonly TranslationString Delete = new("&Delete tag...");

        internal readonly TranslationString CheckoutTooltip = new("Checkout this tag revision");
        internal readonly TranslationString CreateTooltip = new("Create a local branch from this tag");
        internal readonly TranslationString MergeTooltip = new("Merge this tag into current branch");
        internal readonly TranslationString RebaseTooltip = new("Rebase current branch on this tag");
        internal readonly TranslationString ResetTooltip = new("Reset current branch to here");
        internal readonly TranslationString DeleteTooltip = new("Delete this tag");

        public TagMenuItemsStrings()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        public void ApplyTo(MenuItemsStrings strings)
        {
            strings.Checkout = Checkout;
            strings.Rebase = Rebase;
            strings.Delete = Delete;
            strings.Tooltips[MenuItemKey.GitRefCheckout] = CheckoutTooltip;
            strings.Tooltips[MenuItemKey.GitRefCreateBranch] = CreateTooltip;
            strings.Tooltips[MenuItemKey.GitRefMerge] = MergeTooltip;
            strings.Tooltips[MenuItemKey.GitRefRebase] = RebaseTooltip;
            strings.Tooltips[MenuItemKey.GitRefReset] = ResetTooltip;
            strings.Tooltips[MenuItemKey.Delete] = DeleteTooltip;
        }
    }
}
