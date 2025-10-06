using GitCommands;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.UserControls;

public class SortDiffListContextMenuItem : ToolStripMenuItem
{
    private readonly TranslationString _filePathSortText = new("File &path - tree");
    private readonly TranslationString _filePathFlatSortText = new("&File path - flat");
    private readonly TranslationString _fileExtensionSortText = new("File &extension - tree");
    private readonly TranslationString _fileExtensionFlatSortText = new("File e&xtension - flat");
    private readonly TranslationString _fileStatusSortText = new("File &status - tree");
    private readonly TranslationString _fileStatusFlatSortText = new("File s&tatus - flat");
    private readonly IDiffListSortService _sortService;
    private readonly ToolStripMenuItem[] _allItems;

    public SortDiffListContextMenuItem(IDiffListSortService sortService)
    {
        _sortService = sortService ?? throw new ArgumentNullException(nameof(sortService));
        Image = Images.SortBy;
        Text = TranslatedStrings.SortGroupBy;

        _allItems =
        [
            new ToolStripMenuItem()
            {
                Text = _filePathSortText.Text,
                ShowShortcutKeys = true,
                Image = null,
                Tag = DiffListSortType.FilePath
            },
            new ToolStripMenuItem()
            {
                Text = _filePathFlatSortText.Text,
                ShowShortcutKeys = true,
                Image = null,
                Tag = DiffListSortType.FilePathFlat
            },
            new ToolStripMenuItem()
            {
                Text = _fileExtensionSortText.Text,
                ShowShortcutKeys = true,
                Image = null,
                Tag = DiffListSortType.FileExtension
            },
            new ToolStripMenuItem()
            {
                Text = _fileExtensionFlatSortText.Text,
                ShowShortcutKeys = true,
                Image = null,
                Tag = DiffListSortType.FileExtensionFlat
            },
            new ToolStripMenuItem()
            {
                Text = _fileStatusSortText.Text,
                ShowShortcutKeys = true,
                Image = null,
                Tag = DiffListSortType.FileStatus
            },
            new ToolStripMenuItem()
            {
                Text = _fileStatusFlatSortText.Text,
                ShowShortcutKeys = true,
                Image = null,
                Tag = DiffListSortType.FileStatusFlat
            }
        ];

        foreach (ToolStripMenuItem item in AllItems())
        {
            item.Click += Item_Click;
            DropDownItems.Add(item);
        }

        DropDownOpening += (s, e) => RequerySortingMethod();
        RequerySortingMethod();
    }

    private IReadOnlyList<ToolStripMenuItem> AllItems()
    {
        return _allItems;
    }

    private void RequerySortingMethod()
    {
        DiffListSortType currentSort = _sortService.DiffListSorting;
        foreach (ToolStripMenuItem item in AllItems())
        {
            item.Checked = currentSort.Equals(item.Tag);
        }
    }

    private void Item_Click(object sender, EventArgs e)
    {
        ToolStripMenuItem item = (ToolStripMenuItem)sender;
        DiffListSortType sortingType = (DiffListSortType)item.Tag;
        _sortService.DiffListSorting = sortingType;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal struct TestAccessor
    {
        private readonly SortDiffListContextMenuItem _contextMenuItem;

        public TestAccessor(SortDiffListContextMenuItem menuitem)
        {
            _contextMenuItem = menuitem;
        }

        public void RaiseDropDownOpening() => _contextMenuItem.RequerySortingMethod();
    }
}
