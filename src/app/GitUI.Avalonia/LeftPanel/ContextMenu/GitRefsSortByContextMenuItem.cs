using Avalonia.Controls;
using GitCommands;
using GitCommands.Utils;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel.ContextMenu;

internal sealed class GitRefsSortByContextMenuItem : MenuItem
{
    private readonly Action _onSortByChanged;

    public GitRefsSortByContextMenuItem(Action onSortByChanged)
    {
        _onSortByChanged = onSortByChanged;

        Icon = new Image { Source = Images.SortBy };
        Header = TranslatedStrings.SortBy;

        foreach ((string text, GitRefsSortBy value) option in EnumHelper.GetValues<GitRefsSortBy>().Select(e => (Text: e.GetDescription(), Value: e)))
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
        GitRefsSortBy currentSort = AppSettings.RefsSortBy;
        foreach (MenuItem item in Items.OfType<MenuItem>())
        {
            item.IsChecked = currentSort.Equals(item.Tag);
        }
    }

    private void Item_Click(object? sender, EventArgs e)
    {
        if (sender is MenuItem item)
        {
            GitRefsSortBy sortingType = (GitRefsSortBy)item.Tag!;
            AppSettings.RefsSortBy = sortingType;

            _onSortByChanged?.Invoke();
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly GitRefsSortByContextMenuItem _contextMenuItem;

        public TestAccessor(GitRefsSortByContextMenuItem menuitem)
        {
            _contextMenuItem = menuitem;
        }

        public readonly void RaiseDropDownOpening() => _contextMenuItem.RequerySortingMethod();
    }
}
