using System.Collections.Generic;
using GitUI.BranchTreePanel.Interfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
{
    public class MenuItemsStrings : Translate
    {
        // the name of the members act as keys for the generated entries in the translation files (xlf)

        /// <see cref="IGitRefActions"/>
        internal readonly TranslationString Checkout = new TranslationString("Checkout");
        internal readonly TranslationString Merge = new TranslationString("&Merge");
        internal readonly TranslationString Rebase = new TranslationString("&Rebase");
        internal readonly TranslationString CreateBranch = new TranslationString("Create &Branch...");
        internal readonly TranslationString Reset = new TranslationString("Re&set");

        /// <see cref="ICanRename"/>
        internal readonly TranslationString Rename = new TranslationString("Rename");

        /// <see cref="ICanDelete"/>
        internal readonly TranslationString Delete = new TranslationString("Delete");

        internal Dictionary<MenuItemKey, TranslationString> Tooltips { get; } = new Dictionary<MenuItemKey, TranslationString>();
    }

    public class BranchMenuItemsStrings : Translate
    {
        internal readonly TranslationString CheckoutTooltip = new TranslationString("Checkout this branch");
        internal readonly TranslationString MergeTooltip = new TranslationString("Merge this branch into current branch");
        internal readonly TranslationString CreateTooltip = new TranslationString("Create a local branch from this branch");
        internal readonly TranslationString RebaseTooltip = new TranslationString("Rebase current branch to this branch");
        internal readonly TranslationString ResetTooltip = new TranslationString("Reset current branch to here");
        internal readonly TranslationString RenameTooltip = new TranslationString("Rename this branch");

        public void ApplyTo(MenuItemsStrings strings)
        {
            strings.Tooltips[MenuItemKey.GitRefCheckout] = CheckoutTooltip;
            strings.Tooltips[MenuItemKey.GitRefCreateBranch] = CreateTooltip;
            strings.Tooltips[MenuItemKey.GitRefMerge] = MergeTooltip;
            strings.Tooltips[MenuItemKey.GitRefRebase] = RebaseTooltip;
            strings.Tooltips[MenuItemKey.GitRefReset] = ResetTooltip;
            strings.Tooltips[MenuItemKey.Rename] = RenameTooltip;
        }
    }
}
