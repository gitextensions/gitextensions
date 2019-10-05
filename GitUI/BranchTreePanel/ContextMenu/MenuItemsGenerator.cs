using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using GitExtUtils.GitUI.Theming;
using GitUI.BranchTreePanel.Interfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel.ContextMenu
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
            var itemsIndex = new Dictionary<MenuItemKey, ToolStripItem>();
            _items.Value.ForEach(r => itemsIndex.Add(r.Key, r.Item));
            return itemsIndex;
        }

        private List<ToolStripItemWithKey> CreateItems()
        {
            var items = new List<ToolStripItemWithKey>();
            Generate(items);
            return items;
        }

        public MenuItemsStrings Strings { get; } = new MenuItemsStrings();

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

        public bool TryGetMenuItem(MenuItemKey key, out ToolStripItem item)
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
                node => (node as IGitRefActions).Checkout(), Strings.Checkout, GetTooltip(MenuItemKey.GitRefCheckout), Properties.Images.BranchCheckout)
                .WithKey(MenuItemKey.GitRefCheckout);

            yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
               node => (node as IGitRefActions).Merge(), Strings.Merge, GetTooltip(MenuItemKey.GitRefMerge), Properties.Images.Merge)
               .WithKey(MenuItemKey.GitRefMerge);

            yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                node => (node as IGitRefActions).Rebase(), Strings.Rebase, GetTooltip(MenuItemKey.GitRefRebase), Properties.Images.Rebase)
                .WithKey(MenuItemKey.GitRefRebase);

            yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                node => (node as IGitRefActions).CreateBranch(), Strings.CreateBranch, GetTooltip(MenuItemKey.GitRefCreateBranch), Properties.Images.Branch.AdaptLightness())
                .WithKey(MenuItemKey.GitRefCreateBranch);

            yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                node => (node as IGitRefActions).Reset(), Strings.Reset, GetTooltip(MenuItemKey.GitRefReset), Properties.Images.ResetCurrentBranchToHere)
                .WithKey(MenuItemKey.GitRefReset);

            yield return new ToolStripSeparator().WithKey(MenuItemKey.GitRefActionsSeparator);
        }

        private IEnumerable<ToolStripItemWithKey> CreateRename()
        {
            if (Implements<ICanRename>())
            {
                yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                    node => (node as ICanRename).Rename(), Strings.Rename, GetTooltip(MenuItemKey.Rename), Properties.Images.Renamed.AdaptLightness())
                    .WithKey(MenuItemKey.Rename);
            }
        }

        private IEnumerable<ToolStripItemWithKey> CreateDelete()
        {
            if (Implements<ICanDelete>())
            {
                yield return _menuItemFactory.CreateMenuItem<ToolStripMenuItem, TNode>(
                    node => (node as ICanDelete).Delete(), Strings.Delete, GetTooltip(MenuItemKey.Delete), Properties.Images.BranchDelete)
                    .WithKey(MenuItemKey.Delete);
            }
        }

        protected TranslationString GetTooltip(MenuItemKey key)
        {
            return Strings.Tooltips[key];
        }

        private bool Implements<TInterface>()
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
