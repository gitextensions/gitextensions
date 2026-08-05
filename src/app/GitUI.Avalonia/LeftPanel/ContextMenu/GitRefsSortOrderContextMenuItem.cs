using Avalonia.Controls;
using GitCommands;
using GitCommands.Utils;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel.ContextMenu;

internal sealed class GitRefsSortOrderContextMenuItem : MenuItem
{
    internal const string MenuItemName = "GitRefsSortOrderContextMenuItem";
    private readonly Action _onSortOrderChanged;

    public GitRefsSortOrderContextMenuItem(Action onSortOrderChanged)
    {
        _onSortOrderChanged = onSortOrderChanged;

        Icon = new Image { Source = Images.SortBy };
        Header = TranslatedStrings.SortOrder;
        Name = MenuItemName;

        foreach ((string text, GitRefsSortOrder value) option in EnumHelper.GetValues<GitRefsSortOrder>().Select(e => (Text: e.GetDescription(), Value: e)))
        {
            MenuItem item = new()
            {
                Header = option.text,
                Icon = null,
                Tag = option.value,
                ToggleType = MenuItemToggleType.Radio,
            };

            item.Click += Item_Click;
            Items.Add(item);
        }

        SubmenuOpened += (s, e) => RequerySortingMethod();
        RequerySortingMethod();
    }

    private void RequerySortingMethod()
    {
        GitRefsSortOrder currentSort = AppSettings.RefsSortOrder;
        foreach (MenuItem item in Items.OfType<MenuItem>())
        {
            item.IsChecked = currentSort.Equals(item.Tag);
        }
    }

    private void Item_Click(object? sender, EventArgs e)
    {
        if (sender is MenuItem item)
        {
            GitRefsSortOrder sortingType = (GitRefsSortOrder)item.Tag!;
            AppSettings.RefsSortOrder = sortingType;

            _onSortOrderChanged?.Invoke();
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly GitRefsSortOrderContextMenuItem _contextMenuItem;

        public TestAccessor(GitRefsSortOrderContextMenuItem menuitem)
        {
            _contextMenuItem = menuitem;
        }

        public readonly void RaiseDropDownOpening() => _contextMenuItem.RequerySortingMethod();
    }
}
