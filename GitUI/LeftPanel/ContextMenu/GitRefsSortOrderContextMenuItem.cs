using GitCommands;
using GitCommands.Utils;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel.ContextMenu
{
    internal class GitRefsSortOrderContextMenuItem : ToolStripMenuItem
    {
        internal const string MenuItemName = "GitRefsSortOrderContextMenuItem";
        private readonly Action _onSortOrderChanged;

        public GitRefsSortOrderContextMenuItem(Action onSortOrderChanged)
        {
            _onSortOrderChanged = onSortOrderChanged;

            Image = Images.SortBy;
            Text = TranslatedStrings.SortOrder;
            Name = MenuItemName;

            foreach ((string text, GitRefsSortOrder value) option in EnumHelper.GetValues<GitRefsSortOrder>().Select(e => (Text: e.GetDescription(), Value: e)))
            {
                ToolStripMenuItem item = new()
                {
                    Text = option.text,
                    Image = null,
                    Tag = option.value
                };

                item.Click += Item_Click;
                DropDownItems.Add(item);
            }

            DropDownOpening += (s, e) => RequerySortingMethod();
            RequerySortingMethod();
        }

        private void RequerySortingMethod()
        {
            GitRefsSortOrder currentSort = AppSettings.RefsSortOrder;
            foreach (ToolStripMenuItem item in DropDownItems)
            {
                item.Checked = currentSort.Equals(item.Tag);
            }
        }

        private void Item_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                GitRefsSortOrder sortingType = (GitRefsSortOrder)item.Tag;
                AppSettings.RefsSortOrder = sortingType;

                _onSortOrderChanged?.Invoke();
            }
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal struct TestAccessor
        {
            private readonly GitRefsSortOrderContextMenuItem _contextMenuItem;

            public TestAccessor(GitRefsSortOrderContextMenuItem menuitem)
            {
                _contextMenuItem = menuitem;
            }

            public void RaiseDropDownOpening() => _contextMenuItem.RequerySortingMethod();
        }
    }
}
