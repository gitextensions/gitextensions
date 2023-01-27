using GitCommands;
using GitUI.BranchTreePanel.Interfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
{
    internal class LocalBranchMenuItems<TNode> : MenuItemsGenerator<TNode>
        where TNode : class, INode
    {
        /// <summary>Keys of local branch menu items applying to the currently checked out branch.
        /// See <see cref="LocalBranchNode.IsCurrent"/> and <see cref="MenuItemsGenerator{TNode}"/>.</summary>
        internal static MenuItemKey[] CurrentBranchItemKeys = new[] { MenuItemKey.GitRefCreateBranch, MenuItemKey.Rename };

        public LocalBranchMenuItems(IMenuItemFactory menuItemFactory) : base(menuItemFactory)
        {
            new LocalBranchMenuItemsStrings().ApplyTo(Strings);
        }
    }

    public class LocalBranchMenuItemsStrings : Translate
    {
        internal readonly TranslationString DeleteTooltip = new("Delete the branch, which must be fully merged in its upstream branch or in HEAD");

        public LocalBranchMenuItemsStrings()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        public void ApplyTo(MenuItemsStrings strings)
        {
            new BranchMenuItemsStrings().ApplyTo(strings);
            strings.Tooltips[MenuItemKey.Delete] = DeleteTooltip;
        }
    }
}
