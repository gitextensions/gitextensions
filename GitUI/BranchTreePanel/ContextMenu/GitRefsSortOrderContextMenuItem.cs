using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel.ContextMenu
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

            foreach (var option in EnumHelper.GetValues<GitRefsSortOrder>().Select(e => (Text: e.GetDescription(), Value: e)))
            {
                ToolStripMenuItem item = new()
                {
                    Text = option.Text,
                    Image = null,
                    Tag = option.Value
                };

                item.Click += Item_Click;
                DropDownItems.Add(item);
            }

            DropDownOpening += (s, e) => RequerySortingMethod();
            RequerySortingMethod();
        }

        private void RequerySortingMethod()
        {
            var currentSort = AppSettings.RefsSortOrder;
            foreach (ToolStripMenuItem item in DropDownItems)
            {
                item.Checked = currentSort.Equals(item.Tag);
            }
        }

        private void Item_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                var sortingType = (GitRefsSortOrder)item.Tag;
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
