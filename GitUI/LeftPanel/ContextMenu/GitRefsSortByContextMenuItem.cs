using GitCommands;
using GitCommands.Utils;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel.ContextMenu
{
    internal class GitRefsSortByContextMenuItem : ToolStripMenuItem
    {
        private readonly Action _onSortByChanged;

        public GitRefsSortByContextMenuItem(Action onSortByChanged)
        {
            _onSortByChanged = onSortByChanged;

            Image = Images.SortBy;
            Text = TranslatedStrings.SortBy;

            foreach ((string text, GitRefsSortBy value) option in EnumHelper.GetValues<GitRefsSortBy>().Select(e => (Text: e.GetDescription(), Value: e)))
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
            GitRefsSortBy currentSort = AppSettings.RefsSortBy;
            foreach (ToolStripMenuItem item in DropDownItems)
            {
                item.Checked = currentSort.Equals(item.Tag);
            }
        }

        private void Item_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                GitRefsSortBy sortingType = (GitRefsSortBy)item.Tag;
                AppSettings.RefsSortBy = sortingType;

                _onSortByChanged?.Invoke();
            }
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal struct TestAccessor
        {
            private readonly GitRefsSortByContextMenuItem _contextMenuItem;

            public TestAccessor(GitRefsSortByContextMenuItem menuitem)
            {
                _contextMenuItem = menuitem;
            }

            public void RaiseDropDownOpening() => _contextMenuItem.RequerySortingMethod();
        }
    }
}
