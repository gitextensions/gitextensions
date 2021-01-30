using System.Collections.Generic;
using GitUI.BranchTreePanel.Interfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
{
    public class MenuItemsStrings : Translate
    {
        // the name of the members act as keys for the generated entries in the translation files (xlf)

        /// <see cref="IGitRefActions"/>
        internal readonly TranslationString Checkout = new("Checkout");
        internal readonly TranslationString Merge = new("&Merge");
        internal readonly TranslationString Rebase = new("&Rebase");
        internal readonly TranslationString CreateBranch = new("Create &Branch...");
        internal readonly TranslationString Reset = new("Re&set");

        /// <see cref="ICanRename"/>
        internal readonly TranslationString Rename = new("Rename");

        /// <see cref="ICanDelete"/>
        internal readonly TranslationString Delete = new("Delete");

        internal Dictionary<MenuItemKey, TranslationString> Tooltips { get; } = new Dictionary<MenuItemKey, TranslationString>();
    }

    public class BranchMenuItemsStrings : Translate
    {
        internal readonly TranslationString CheckoutTooltip = new("Checkout this branch");
        internal readonly TranslationString MergeTooltip = new("Merge this branch into current branch");
        internal readonly TranslationString CreateTooltip = new("Create a local branch from this branch");
        internal readonly TranslationString RebaseTooltip = new("Rebase current branch to this branch");
        internal readonly TranslationString ResetTooltip = new("Reset current branch to here");
        internal readonly TranslationString RenameTooltip = new("Rename this branch");

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
