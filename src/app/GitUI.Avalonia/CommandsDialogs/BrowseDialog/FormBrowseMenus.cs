using Avalonia.Controls;
using Avalonia.Interactivity;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using ResourceManager;

namespace GitUI.CommandsDialogs;

/// <summary>
/// Adds the revision-grid Navigate and View command sets to the Browse main menu.
/// </summary>
/// <remarks>
/// This is the Avalonia twin of <c>CommandsDialogs/BrowseDialog/FormBrowseMenus.cs</c>.
/// The revision grid remains the sole command/state owner; this class only creates and
/// synchronizes additional menu-item presentations.
/// </remarks>
internal sealed class FormBrowseMenus : ITranslate, IDisposable
{
    private readonly Menu _mainMenuStrip;
    private readonly RevisionGridControl _revisionGrid;
    private readonly Dictionary<MenuItem, MenuItem> _sourceItems = [];
    private readonly MenuItem _navigateToolStripMenuItem = new()
    {
        Name = "navigateToolStripMenuItem",
        Header = "_Navigate",
        IsVisible = false,
    };
    private readonly MenuItem _viewToolStripMenuItem = new()
    {
        Name = "viewToolStripMenuItem",
        Header = "_View",
        IsVisible = false,
    };

    public FormBrowseMenus(Menu mainMenuStrip, RevisionGridControl revisionGrid, MenuItem insertAfterMenuItem)
    {
        _mainMenuStrip = mainMenuStrip;
        _revisionGrid = revisionGrid;

        CopyItems(revisionGrid.NavigateMenuItem, _navigateToolStripMenuItem);
        CopyItems(revisionGrid.ViewMenuItem, _viewToolStripMenuItem);

        int insertIndex = mainMenuStrip.Items.IndexOf(insertAfterMenuItem) + 1;
        mainMenuStrip.Items.Insert(insertIndex, _navigateToolStripMenuItem);
        mainMenuStrip.Items.Insert(insertIndex + 1, _viewToolStripMenuItem);

        _navigateToolStripMenuItem.SubmenuOpened += MainMenuItem_SubmenuOpened;
        _viewToolStripMenuItem.SubmenuOpened += MainMenuItem_SubmenuOpened;
    }

    internal MenuItem NavigateMenuItem => _navigateToolStripMenuItem;

    internal MenuItem ViewMenuItem => _viewToolStripMenuItem;

    public void AddTranslationItems(ITranslation translation)
    {
        translation.AddTranslationItem(nameof(FormBrowse), "navigateToolStripMenuItem", "Text", "&Navigate");
        translation.AddTranslationItem(nameof(FormBrowse), "viewToolStripMenuItem", "Text", "&View");
    }

    public void TranslateItems(ITranslation translation)
    {
        _navigateToolStripMenuItem.Header = Translate("navigateToolStripMenuItem", "&Navigate");
        _viewToolStripMenuItem.Header = Translate("viewToolStripMenuItem", "&View");
        RefreshItems();

        return;

        string Translate(string name, string source)
        {
            string translated = translation.TranslateItem(
                nameof(FormBrowse),
                name,
                "Text",
                () => source) ?? source;
            return AvaloniaTranslationUtils.ToAvaloniaMnemonics(translated);
        }
    }

    internal void SetVisible(bool visible)
    {
        _navigateToolStripMenuItem.IsVisible = visible;
        _viewToolStripMenuItem.IsVisible = visible;
    }

    internal void RefreshItems()
    {
        _revisionGrid.RefreshMainMenuState();
        foreach ((MenuItem source, MenuItem target) in _sourceItems)
        {
            target.Header = source.Header;
            target.InputGesture = source.InputGesture;
            target.ToggleType = source.ToggleType;
            target.IsChecked = source.IsChecked;
            target.IsEnabled = source.IsEnabled;
            target.IsVisible = source.IsVisible;
            ToolTip.SetTip(target, ToolTip.GetTip(source));
        }
    }

    public void Dispose()
    {
        _navigateToolStripMenuItem.SubmenuOpened -= MainMenuItem_SubmenuOpened;
        _viewToolStripMenuItem.SubmenuOpened -= MainMenuItem_SubmenuOpened;
        _mainMenuStrip.Items.Remove(_navigateToolStripMenuItem);
        _mainMenuStrip.Items.Remove(_viewToolStripMenuItem);
    }

    private void MainMenuItem_SubmenuOpened(object? sender, EventArgs e)
        => RefreshItems();

    private void CopyItems(MenuItem sourceParent, MenuItem targetParent)
    {
        foreach (object? item in sourceParent.Items)
        {
            if (item is Separator)
            {
                targetParent.Items.Add(new Separator());
                continue;
            }

            if (item is not MenuItem source)
            {
                continue;
            }

            MenuItem target = new()
            {
                Tag = source.Tag,
                Header = source.Header,
                Icon = CloneIcon(source.Icon),
                InputGesture = source.InputGesture,
                ToggleType = source.ToggleType,
                IsChecked = source.IsChecked,
                IsEnabled = source.IsEnabled,
                IsVisible = source.IsVisible,
                Focusable = source.Focusable,
                IsHitTestVisible = source.IsHitTestVisible,
            };
            foreach (string className in source.Classes.Where(className => !className.StartsWith(':')))
            {
                target.Classes.Add(className);
            }

            CopyItems(source, target);
            target.Click += (_, _) =>
            {
                source.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                RefreshItems();
            };
            _sourceItems.Add(source, target);
            targetParent.Items.Add(target);
        }
    }

    private static Image? CloneIcon(object? icon)
        => icon is Image image
            ? new Image
            {
                Width = image.Width,
                Height = image.Height,
                Source = image.Source,
                Stretch = image.Stretch,
            }
            : null;
}
