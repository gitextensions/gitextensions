using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog;

public sealed partial class SettingsTreeViewUserControl : UserControl
{
    private readonly Dictionary<SettingsPageReference, TreeViewItem> _pages2NodeMap = [];
    private readonly Dictionary<TreeViewItem, TreeViewItem?> _parents = [];
    private readonly List<ISettingsPage> _settingsPages = [];
    private bool _isSelectionChangeTriggeredByGoto;
    private List<TreeViewItem> _nodesFoundByTextBox = [];

    public SettingsTreeViewUserControl()
    {
        InitializeComponent();
        textBoxFind.PlaceholderText = TranslatedStrings.SettingsTypeToFind;
        textBoxFind.TextChanged += textBoxFind_TextChanged;
        textBoxFind.KeyDown += textBoxFind_KeyDown;
        treeView1.SelectionChanged += treeView1_SelectionChanged;
    }

    public event EventHandler<SettingsPageSelectedEventArgs>? SettingsPageSelected;

    public IEnumerable<ISettingsPage> SettingsPages => _settingsPages;

    public void AddSettingsPage(ISettingsPage page, SettingsPageReference? parentPageReference, IImage? icon, bool asRoot = false)
    {
        TreeViewItem node;
        if (parentPageReference is null)
        {
            node = AddPage(treeView1.Items, page, icon, parent: null);
        }
        else if (asRoot)
        {
            if (!_pages2NodeMap.TryGetValue(parentPageReference, out node!))
            {
                throw new ArgumentException("You have to add parent page first: " + parentPageReference);
            }
        }
        else
        {
            if (!_pages2NodeMap.TryGetValue(parentPageReference, out TreeViewItem? parent))
            {
                throw new ArgumentException("You have to add parent page first: " + parentPageReference);
            }

            node = AddPage(parent.Items, page, icon, parent);
        }

        node.Tag = page;
        _pages2NodeMap.Add(page.PageReference, node);
        _settingsPages.Add(page);
    }

    public void GotoPage(SettingsPageReference? settingsPageReference)
    {
        TreeViewItem? node = settingsPageReference is null
            ? treeView1.Items.OfType<TreeViewItem>().FirstOrDefault()
            : _pages2NodeMap.GetValueOrDefault(settingsPageReference);
        node ??= treeView1.Items.OfType<TreeViewItem>().FirstOrDefault();
        if (node is null)
        {
            return;
        }

        for (TreeViewItem? parent = _parents.GetValueOrDefault(node); parent is not null; parent = _parents.GetValueOrDefault(parent))
        {
            parent.IsExpanded = true;
        }

        _isSelectionChangeTriggeredByGoto = true;
        try
        {
            node.IsSelected = true;
            treeView1.SelectedItem = node;
            node.IsExpanded = true;
            FireSettingsPageSelectedEvent(node);
            node.BringIntoView();
        }
        finally
        {
            _isSelectionChangeTriggeredByGoto = false;
        }
    }

    private TreeViewItem AddPage(ItemCollection items, ISettingsPage page, IImage? icon, TreeViewItem? parent)
    {
        StackPanel header = new()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 3,
            Children =
            {
                new Image { Width = 16, Height = 16, Source = icon, IsVisible = icon is not null },
                new TextBlock { Text = page.GetTitle(), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center },
            },
        };
        TreeViewItem node = new() { Header = header };
        items.Add(node);
        _parents.Add(node, parent);
        return node;
    }

    private void treeView1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_isSelectionChangeTriggeredByGoto && treeView1.SelectedItem is TreeViewItem node)
        {
            FireSettingsPageSelectedEvent(node);
        }
    }

    private void FireSettingsPageSelectedEvent(TreeViewItem node)
    {
        if (node.Tag is not ISettingsPage page)
        {
            return;
        }

        if (page.GuiControl is null && node.Items.OfType<TreeViewItem>().FirstOrDefault() is TreeViewItem firstChild)
        {
            firstChild.IsSelected = true;
            treeView1.SelectedItem = firstChild;
            if (_isSelectionChangeTriggeredByGoto)
            {
                FireSettingsPageSelectedEvent(firstChild);
            }

            return;
        }

        SettingsPageSelected?.Invoke(this, new SettingsPageSelectedEventArgs(page, _isSelectionChangeTriggeredByGoto));
    }

    private void textBoxFind_TextChanged(object? sender, TextChangedEventArgs e)
    {
        foreach (TreeViewItem node in _pages2NodeMap.Values.Distinct())
        {
            node.Classes.Remove("settings-search-match");
        }

        _nodesFoundByTextBox = [];
        if (string.IsNullOrWhiteSpace(textBoxFind.Text))
        {
            return;
        }

        string searchFor = textBoxFind.Text.Trim();
        string[] keywords = searchFor.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (TreeViewItem node in _pages2NodeMap.Values.Distinct())
        {
            if (node.Tag is not ISettingsPage page)
            {
                continue;
            }

            bool titleMatches = page.GetTitle().Contains(searchFor, StringComparison.InvariantCultureIgnoreCase);
            bool keywordsMatch = keywords.All(keyword => page.GetSearchKeywords()
                .Any(candidate => candidate.Contains(keyword, StringComparison.InvariantCultureIgnoreCase)));
            if (titleMatches || keywordsMatch)
            {
                node.Classes.Add("settings-search-match");
                _nodesFoundByTextBox.Add(node);
                for (TreeViewItem? parent = _parents.GetValueOrDefault(node); parent is not null; parent = _parents.GetValueOrDefault(parent))
                {
                    parent.IsExpanded = true;
                }
            }
        }
    }

    private void textBoxFind_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || _nodesFoundByTextBox.Count == 0)
        {
            return;
        }

        int currentIndex = treeView1.SelectedItem is TreeViewItem selected
            ? _nodesFoundByTextBox.IndexOf(selected)
            : -1;
        TreeViewItem next = _nodesFoundByTextBox[(currentIndex + 1) % _nodesFoundByTextBox.Count];
        next.IsSelected = true;
        treeView1.SelectedItem = next;
        next.BringIntoView();
        e.Handled = true;
    }
}

public class SettingsPageSelectedEventArgs(ISettingsPage settingsPage, bool isTriggeredByGoto) : EventArgs
{
    public ISettingsPage SettingsPage { get; } = settingsPage;

    public bool IsTriggeredByGoto { get; } = isTriggeredByGoto;
}
