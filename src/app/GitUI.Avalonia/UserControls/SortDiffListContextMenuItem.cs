using Avalonia.Controls;
using GitCommands;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.UserControls;

public class SortDiffListContextMenuItem : MenuItem
{
    private readonly TranslationString _filePathSortText = new("File &path - tree");
    private readonly TranslationString _filePathFlatSortText = new("&File path - flat");
    private readonly TranslationString _fileExtensionSortText = new("File &extension - tree");
    private readonly TranslationString _fileExtensionFlatSortText = new("File e&xtension - flat");
    private readonly TranslationString _fileStatusSortText = new("File &status - tree");
    private readonly TranslationString _fileStatusFlatSortText = new("File s&tatus - flat");
    private readonly IDiffListSortService _sortService;
    private readonly MenuItem[] _allItems;

    public SortDiffListContextMenuItem(IDiffListSortService sortService)
    {
        _sortService = sortService ?? throw new ArgumentNullException(nameof(sortService));
        Header = TranslatedStrings.SortGroupBy;
        Icon = new Image { Source = Images.SortBy };

        _allItems =
        [
            CreateItem(_filePathSortText.Text, DiffListSortType.FilePath),
            CreateItem(_filePathFlatSortText.Text, DiffListSortType.FilePathFlat),
            CreateItem(_fileExtensionSortText.Text, DiffListSortType.FileExtension),
            CreateItem(_fileExtensionFlatSortText.Text, DiffListSortType.FileExtensionFlat),
            CreateItem(_fileStatusSortText.Text, DiffListSortType.FileStatus),
            CreateItem(_fileStatusFlatSortText.Text, DiffListSortType.FileStatusFlat),
        ];

        foreach (MenuItem item in AllItems())
        {
            item.Click += Item_Click;
            Items.Add(item);
        }

        SubmenuOpened += (_, _) => RequerySortingMethod();
        RequerySortingMethod();

        static MenuItem CreateItem(string text, DiffListSortType sortType)
            => new()
            {
                Header = text,
                Tag = sortType,
                ToggleType = MenuItemToggleType.Radio,
            };
    }

    private IReadOnlyList<MenuItem> AllItems()
        => _allItems;

    private void RequerySortingMethod()
    {
        DiffListSortType currentSort = _sortService.DiffListSorting;
        foreach (MenuItem item in AllItems())
        {
            item.IsChecked = currentSort.Equals(item.Tag);
        }
    }

    private void Item_Click(object? sender, EventArgs e)
    {
        MenuItem item = (MenuItem)sender!;
        _sortService.DiffListSorting = (DiffListSortType)item.Tag!;
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(SortDiffListContextMenuItem contextMenuItem)
    {
        public void RaiseDropDownOpening()
            => contextMenuItem.RequerySortingMethod();

        public IReadOnlyList<MenuItem> Items => contextMenuItem.AllItems();
    }
}
