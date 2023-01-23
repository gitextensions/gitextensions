using GitCommands;
using GitUI.BranchTreePanel.Interfaces;
using ICSharpCode.TextEditor.Actions;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
{
    public class MenuItemsStrings : Translate
    {
        // the name of the members act as keys for the generated entries in the translation files (xlf)

        /// <see cref="IGitRefActions"/>
        internal TranslationString Checkout;
        internal TranslationString Rebase;
        internal readonly TranslationString Merge = new("&Merge into current branch...");
        internal readonly TranslationString CreateBranch = new("Create &branch...");
        internal readonly TranslationString Reset = new("Re&set current branch to here...");

        /// <see cref="ICanRename"/>
        internal TranslationString Rename = new("R&ename branch...");

        /// <see cref="ICanDelete"/>
        internal TranslationString Delete;

        internal Dictionary<MenuItemKey, TranslationString> Tooltips { get; } = new Dictionary<MenuItemKey, TranslationString>();

        public MenuItemsStrings()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }
    }

    public class BranchMenuItemsStrings : Translate
    {
        internal readonly TranslationString Checkout = new("Chec&kout branch...");
        internal readonly TranslationString Rebase = new("&Rebase current branch on this branch...");
        internal readonly TranslationString Delete = new("&Delete branch...");

        internal readonly TranslationString CheckoutTooltip = new("Checkout this branch");
        internal readonly TranslationString MergeTooltip = new("Merge this branch into current branch");
        internal readonly TranslationString CreateTooltip = new("Create a local branch from this branch");
        internal readonly TranslationString RebaseTooltip = new("Rebase current branch on this branch");
        internal readonly TranslationString ResetTooltip = new("Reset current branch to here");
        internal readonly TranslationString RenameTooltip = new("Rename this branch");

        public BranchMenuItemsStrings()
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
            strings.Tooltips[MenuItemKey.Rename] = RenameTooltip;
        }
    }
}
