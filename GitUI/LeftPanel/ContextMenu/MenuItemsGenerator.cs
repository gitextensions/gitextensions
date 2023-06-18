using System.Collections;
using System.Diagnostics.CodeAnalysis;
using GitExtUtils.GitUI.Theming;
using GitUI.LeftPanel.Interfaces;
using ResourceManager;

namespace GitUI.LeftPanel.ContextMenu
{
    internal class MenuItemsGenerator<TNode> : IMenuItemsGenerator<TNode>
        where TNode : class, INode
    {
        private readonly IMenuItemFactory _menuItemFactory;
        private readonly Lazy<Dictionary<MenuItemKey, ToolStripItem>> _itemsIndex;
        private readonly Lazy<List<ToolStripItemWithKey>> _items;

        public MenuItemsGenerator(IMenuItemFactory menuItemFactory)
        {
            _menuItemFactory = menuItemFactory;
            _items = new Lazy<List<ToolStripItemWithKey>>(() => CreateItems());
            _itemsIndex = new Lazy<Dictionary<MenuItemKey, ToolStripItem>>(() => CreateItemsIndex());
        }

        private Dictionary<MenuItemKey, ToolStripItem> CreateItemsIndex()
        {
            Dictionary<MenuItemKey, ToolStripItem> itemsIndex = new();
            _items.Value.ForEach(r => itemsIndex.Add(r.Key, r.Item));
            return itemsIndex;
        }

        private List<ToolStripItemWithKey> CreateItems()
        {
            List<ToolStripItemWithKey> items = new();
            Generate(items);
            return items;
        }

        public MenuItemsStrings Strings { get; } = new();

        private void Generate(List<ToolStripItemWithKey> items)
        {
            items.AddRange(CreateGitRefItems());
            items.AddRange(CreateRename());
            items.AddRange(CreateDelete());
        }

        #region IEnumerable
        public IEnumerator<ToolStripItemWithKey> GetEnumerator()
        {
            return _items.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public bool TryGetMenuItem(MenuItemKey key, [NotNullWhen(true)] out ToolStripItem? item)
        {
            return _itemsIndex.Value.TryGetValue(key, out item);
        }

        private IEnumerable<ToolStripItemWithKey> CreateGitRefItems()
        {
            if (!Implements<IGitRefActions>())
            {
                yield break;
            }

            yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                node => ((IGitRefActions)node).Checkout(), Strings.Checkout, GetTooltip(MenuItemKey.GitRefCheckout), Properties.Images.BranchCheckout)
                .WithKey(MenuItemKey.GitRefCheckout);

            yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
               node => ((IGitRefActions)node).Merge(), Strings.Merge, GetTooltip(MenuItemKey.GitRefMerge), Properties.Images.Merge)
               .WithKey(MenuItemKey.GitRefMerge);

            yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                node => ((IGitRefActions)node).Rebase(), Strings.Rebase, GetTooltip(MenuItemKey.GitRefRebase), Properties.Images.Rebase)
                .WithKey(MenuItemKey.GitRefRebase);

            yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                node => ((IGitRefActions)node).CreateBranch(), Strings.CreateBranch, GetTooltip(MenuItemKey.GitRefCreateBranch), Properties.Images.Branch.AdaptLightness())
                .WithKey(MenuItemKey.GitRefCreateBranch);

            yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                node => ((IGitRefActions)node).Reset(), Strings.Reset, GetTooltip(MenuItemKey.GitRefReset), Properties.Images.ResetCurrentBranchToHere)
                .WithKey(MenuItemKey.GitRefReset);

            yield return new ToolStripSeparator().WithKey(MenuItemKey.GitRefActionsSeparator);
        }

        private IEnumerable<ToolStripItemWithKey> CreateRename()
        {
            if (Implements<ICanRename>())
            {
                ToolStripMenuItem item = _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                    node => ((ICanRename)node).Rename(), Strings.Rename, GetTooltip(MenuItemKey.Rename), Properties.Images.Renamed.AdaptLightness());
                yield return item.WithKey(MenuItemKey.Rename);
            }
        }

        private IEnumerable<ToolStripItemWithKey> CreateDelete()
        {
            if (Implements<ICanDelete>())
            {
                ToolStripMenuItem item = _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                    node => ((ICanDelete)node).Delete(), Strings.Delete, GetTooltip(MenuItemKey.Delete), Properties.Images.BranchDelete);
                yield return item.WithKey(MenuItemKey.Delete);
            }
        }

        protected TranslationString GetTooltip(MenuItemKey key)
        {
            return Strings.Tooltips[key];
        }

        private static bool Implements<TInterface>()
        {
            return typeof(TInterface).IsAssignableFrom(typeof(TNode));
        }
    }

    internal static class ToolStripButtonExtensions
    {
        public static ToolStripItemWithKey WithKey(this ToolStripItem item, MenuItemKey key)
        {
            return new ToolStripItemWithKey(key, item);
        }
    }
}
