using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.UserControls
{
    public class SortDiffListContextMenuItem : ToolStripMenuItem
    {
        private readonly TranslationString _sortByText = new TranslationString("&Sort by...");
        private readonly TranslationString _filePathSortText = new TranslationString("File &Path");
        private readonly TranslationString _fileExtensionSortText = new TranslationString("File &Extension");
        private readonly TranslationString _fileStatusSortText = new TranslationString("File &Status");
        private readonly IDiffListSortService _sortService;
        private readonly ToolStripMenuItem _filePathSortItem;
        private readonly ToolStripMenuItem _fileExtensionSortItem;
        private readonly ToolStripMenuItem _fileStatusSortItem;
        private readonly ToolStripMenuItem[] _allItems;

        public SortDiffListContextMenuItem(IDiffListSortService sortService)
        {
            _sortService = sortService ?? throw new ArgumentNullException(nameof(sortService));
            Image = Images.SortBy;
            Text = _sortByText.Text;

            _filePathSortItem = new ToolStripMenuItem()
            {
                Text = _filePathSortText.Text,
                ShowShortcutKeys = true,
                Image = null,
                Tag = DiffListSortType.FilePath
            };

            _fileExtensionSortItem = new ToolStripMenuItem()
            {
                Text = _fileExtensionSortText.Text,
                ShowShortcutKeys = true,
                Image = null,
                Tag = DiffListSortType.FileExtension
            };

            _fileStatusSortItem = new ToolStripMenuItem()
            {
                Text = _fileStatusSortText.Text,
                ShowShortcutKeys = true,
                Image = null,
                Tag = DiffListSortType.FileStatus
            };

            _allItems = new[] { _filePathSortItem, _fileExtensionSortItem, _fileStatusSortItem, };

            foreach (var item in AllItems())
            {
                item.Click += Item_Click;
                DropDownItems.Add(item);
            }

            DropDownOpening += (s, e) => RequerySortingMethod();
            RequerySortingMethod();
        }

        internal TestAccessor GetTestAccessor() => new TestAccessor(this);

        private IReadOnlyList<ToolStripMenuItem> AllItems()
        {
            return _allItems;
        }

        private void RequerySortingMethod()
        {
            var currentSort = _sortService.DiffListSorting;
            foreach (var item in AllItems())
            {
                item.Checked = currentSort.Equals(item.Tag);
            }
        }

        private void Item_Click(object sender, EventArgs e)
        {
            var menuitme = sender as ToolStripMenuItem;
            var sortingType = (DiffListSortType)menuitme.Tag;
            _sortService.DiffListSorting = sortingType;
        }

        public struct TestAccessor
        {
            private readonly SortDiffListContextMenuItem _contextMenuItem;

            public TestAccessor(SortDiffListContextMenuItem menuitem)
            {
                _contextMenuItem = menuitem;
            }

            public void SimulateOpeningEvent() => _contextMenuItem.RequerySortingMethod();
        }
    }
}
