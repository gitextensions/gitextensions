using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;

namespace GitUI.CommandsDialogs.SettingsDialog;

public sealed partial class SettingsTreeViewUserControl : UserControl
{
    private bool _isSelectionChangeTriggeredByGoto;
    private List<TreeViewItem> _nodesFoundByTextBox = [];
    private readonly Dictionary<SettingsPageReference, TreeViewItem> _pages2NodeMap = [];

    // Avalonia TreeViewItem has no parent-node property, so preserve that relationship explicitly.
    private readonly Dictionary<TreeViewItem, TreeViewItem?> _parents = [];
    private readonly List<ISettingsPage> _settingsPages = [];

    public event EventHandler<SettingsPageSelectedEventArgs>? SettingsPageSelected;
    public IEnumerable<ISettingsPage> SettingsPages => _settingsPages;

    public SettingsTreeViewUserControl()
    {
        InitializeComponent();

        textBoxFind.PlaceholderText = TranslatedStrings.SettingsTypeToFind;
        textBoxFind.TextChanged += textBoxFind_TextChanged;
        textBoxFind.KeyDown += textBoxFind_KeyUp;
        treeView1.SelectionChanged += treeView1_AfterSelect;

        // Scale ImageSize and images scale automatically
        // Avalonia requires automation properties to be projected explicitly for this standalone control.
        InputAccessibility.Apply(this);
    }

    /// <summary>Add page to settings tree.</summary>
    /// <param name="page">The settings page to add.</param>
    /// <param name="parentPageReference">An already added settings page to be a parent in the tree.</param>
    /// <param name="icon">An icon to display in tree node.</param>
    /// <param name="asRoot">only one page can be set as the root page (for the GitExt and Plugin root node).</param>
    public void AddSettingsPage(ISettingsPage page, SettingsPageReference? parentPageReference, IImage? icon, bool asRoot = false)
    {
        TreeViewItem node;
        if (parentPageReference is null)
        {
            // add one of the root nodes (e. g. "Git Extensions" or "Plugins"
            node = AddPage(treeView1.Items, page, icon, parent: null);
        }
        else
        {
            if (asRoot)
            {
                // e. g. to set the Checklist on the "Git Extensions" node
                if (!_pages2NodeMap.TryGetValue(parentPageReference, out node!))
                {
                    throw new ArgumentException("You have to add parent page first: " + parentPageReference);
                }
            }
            else
            {
                if (!_pages2NodeMap.TryGetValue(parentPageReference, out TreeViewItem? parentNode))
                {
                    throw new ArgumentException("You have to add parent page first: " + parentPageReference);
                }

                node = AddPage(parentNode.Items, page, icon, parentNode);
            }
        }

        node.Tag = page;
        _pages2NodeMap.Add(page.PageReference, node);
        _settingsPages.Add(page);
    }

    // Avalonia uses an item collection and a composed icon/text header in place of TreeNodeCollection.
    private TreeViewItem AddPage(ItemCollection treeNodeCollection, ISettingsPage page, IImage? icon, TreeViewItem? parent)
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
        treeNodeCollection.Add(node);
        _parents.Add(node, parent);
        return node;
    }

    // Avalonia exposes SelectionChanged rather than WinForms AfterSelect.
    private void treeView1_AfterSelect(object? sender, SelectionChangedEventArgs e)
    {
        if (!_isSelectionChangeTriggeredByGoto && treeView1.SelectedItem is TreeViewItem node)
        {
            FireSettingsPageSelectedEvent(node);
        }
    }

    private void FireSettingsPageSelectedEvent(TreeViewItem node)
    {
        if (SettingsPageSelected is not null)
        {
            if (node.Tag is not ISettingsPage page)
            {
                return;
            }

            if (page.GuiControl is null)
            {
                TreeViewItem? firstSubNode = node.Items.OfType<TreeViewItem>().FirstOrDefault();
                if (firstSubNode is not null)
                {
                    firstSubNode.IsSelected = true;
                    treeView1.SelectedItem = firstSubNode;
                    return;
                }
            }

            SettingsPageSelected?.Invoke(this, new SettingsPageSelectedEventArgs(page, _isSelectionChangeTriggeredByGoto));
        }
    }

    private void textBoxFind_TextChanged(object? sender, TextChangedEventArgs e)
    {
        _nodesFoundByTextBox.Clear();

        if (string.IsNullOrWhiteSpace(textBoxFind.Text))
        {
            ResetAllNodeHighlighting();
            return;
        }

        string searchFor = textBoxFind.Text.ToLowerInvariant();
        string[] andKeywords = searchFor.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (TreeViewItem node in _pages2NodeMap.Values.Distinct())
        {
            if (node.Tag is not ISettingsPage settingsPage)
            {
                continue;
            }

            // search for title
            if (settingsPage.GetTitle().Contains(searchFor, StringComparison.InvariantCultureIgnoreCase))
            {
                _nodesFoundByTextBox.Add(node);
                continue;
            }

            // search for keywords (space combines as 'and')
            if (andKeywords.All(keyword => settingsPage.GetSearchKeywords().Any(k => k.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))))
            {
                // only part of a keyword must match to have a match
                if (!_nodesFoundByTextBox.Contains(node))
                {
                    _nodesFoundByTextBox.Add(node);
                }
            }
        }

        ResetAllNodeHighlighting();

        foreach (TreeViewItem node in _nodesFoundByTextBox)
        {
            HighlightNode(node, true);
            for (TreeViewItem? parent = _parents.GetValueOrDefault(node); parent is not null; parent = _parents.GetValueOrDefault(parent))
            {
                // if visible: when searching, if the selected node is valid, it will still have grey background
                parent.IsExpanded = true;
            }

            node.BringIntoView();
        }
    }

    /// <summary>Highlights a <see cref="TreeViewItem"/> or returns it to the default colors.</summary>
    private static void HighlightNode(TreeViewItem treeNode, bool highlight)
    {
        if (highlight)
        {
            treeNode.Classes.Add("settings-search-match");
        }
        else
        {
            treeNode.Classes.Remove("settings-search-match");
        }
    }

    private void ResetAllNodeHighlighting()
    {
        foreach (TreeViewItem node in _pages2NodeMap.Values.Distinct())
        {
            HighlightNode(node, false);
        }
    }

    public void GotoPage(SettingsPageReference? settingsPageReference)
    {
        TreeViewItem? node;
        if (settingsPageReference is null)
        {
            node = treeView1.Items.OfType<TreeViewItem>().FirstOrDefault();
        }
        else
        {
            _pages2NodeMap.TryGetValue(settingsPageReference, out node);
        }

        if (node is not null)
        {
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
    }

    // Avalonia exposes KeyDown rather than WinForms KeyUp.
    private void textBoxFind_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // TODO: how to avoid the windows sound when pressing ENTER?
            e.Handled = true;

            // each enter key press selects next highlighted node (cycle)
            int indexOfSelectedNode = treeView1.SelectedItem is TreeViewItem selected
                ? _nodesFoundByTextBox.IndexOf(selected)
                : -1;
            if (indexOfSelectedNode == -1 || indexOfSelectedNode + 1 == _nodesFoundByTextBox.Count)
            {
                TreeViewItem? firstFoundNode = _nodesFoundByTextBox.FirstOrDefault();
                if (firstFoundNode is not null)
                {
                    firstFoundNode.IsSelected = true;
                    treeView1.SelectedItem = firstFoundNode;
                    firstFoundNode.BringIntoView();
                }
            }
            else
            {
                TreeViewItem nextNode = _nodesFoundByTextBox[indexOfSelectedNode + 1];
                nextNode.IsSelected = true;
                treeView1.SelectedItem = nextNode;
                nextNode.BringIntoView();
            }
        }
    }
}

public class SettingsPageSelectedEventArgs : EventArgs
{
    public ISettingsPage SettingsPage { get; }
    public bool IsTriggeredByGoto { get; }

    public SettingsPageSelectedEventArgs(ISettingsPage settingsPage, bool isTriggeredByGoto)
    {
        SettingsPage = settingsPage;
        IsTriggeredByGoto = isTriggeredByGoto;
    }
}
