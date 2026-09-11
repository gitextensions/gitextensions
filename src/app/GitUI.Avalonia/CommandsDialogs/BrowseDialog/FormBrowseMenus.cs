using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using ResourceManager;

namespace GitUI.CommandsDialogs;

/// <summary>
/// Adds the revision-grid Navigate and View command sets to the Browse main menu.
/// </summary>
/// <remarks>
/// The revision grid remains the sole command/state owner; this class only creates and
/// synchronizes additional menu-item presentations.
/// </remarks>
internal sealed class FormBrowseMenus : ITranslate, IDisposable
{
    /// <summary>
    /// The menu to which we will be adding RevisionGrid command menus.
    /// </summary>
    private readonly Menu _mainMenuStrip;
    private readonly ContextMenu _toolStripContextMenu = new();
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
    private readonly MenuItem _toolbarsMenuItem = new()
    {
        Name = "toolbarsMenuItem",
        Header = "_Toolbars",
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

        _viewToolStripMenuItem.Items.Add(new Separator());
        _viewToolStripMenuItem.Items.Add(_toolbarsMenuItem);

        _navigateToolStripMenuItem.SubmenuOpened += MainMenuItem_SubmenuOpened;
        _viewToolStripMenuItem.SubmenuOpened += MainMenuItem_SubmenuOpened;
        _toolStripContextMenu.Opening += ToolStripContextMenu_Opening;
    }

    internal MenuItem NavigateMenuItem => _navigateToolStripMenuItem;

    internal MenuItem ViewMenuItem => _viewToolStripMenuItem;

    internal ContextMenu ToolStripContextMenu => _toolStripContextMenu;

    public void AddTranslationItems(ITranslation translation)
    {
        translation.AddTranslationItem(nameof(FormBrowse), "navigateToolStripMenuItem", "Text", "&Navigate");
        translation.AddTranslationItem(nameof(FormBrowse), "viewToolStripMenuItem", "Text", "&View");
        translation.AddTranslationItem(nameof(FormBrowse), "toolbarsMenuItem", "Text", "Toolbars");
    }

    public void TranslateItems(ITranslation translation)
    {
        _navigateToolStripMenuItem.Header = Translate("navigateToolStripMenuItem", "&Navigate");
        _viewToolStripMenuItem.Header = Translate("viewToolStripMenuItem", "&View");
        _toolbarsMenuItem.Header = Translate("toolbarsMenuItem", "Toolbars");
        OnMenuCommandsPropertyChanged();

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

    public void CreateToolbarsMenus(params (Control ToolStrip, string Text)[] toolStrips)
    {
        foreach ((Control toolStrip, string text) in toolStrips)
        {
            _toolStripContextMenu.Items.Add(CreateItem(toolStrip, text));
            _toolbarsMenuItem.Items.Add(CreateItem(toolStrip, text));
        }

        static MenuItem CreateItem(Control toolStrip, string text)
        {
            MenuItem item = new()
            {
                Header = text,
                IsChecked = toolStrip.IsVisible,
                Tag = toolStrip,
                ToggleType = MenuItemToggleType.CheckBox,
            };
            CreateToolStripSubMenus(toolStrip, item);
            toolStrip.PropertyChanged += (_, e) =>
            {
                if (e.Property == Visual.IsVisibleProperty)
                {
                    item.IsChecked = toolStrip.IsVisible;
                }
            };
            item.Click += (_, _) => toolStrip.IsVisible = !toolStrip.IsVisible;
            return item;
        }
    }

    private static void CreateToolStripSubMenus(Control senderToolStrip, MenuItem toolStripItem)
    {
        IEnumerable<Control> controls = senderToolStrip is Panel panel
            ? panel.Children
            : senderToolStrip.GetLogicalDescendants()
                .OfType<Control>()
                .Where(control => control.TemplatedParent is null && !string.IsNullOrEmpty(control.Name));
        foreach (Control toolbarItem in controls.Where(IncludeToolbarItem))
        {
            if (toolbarItem.Classes.Contains("gitextensions-toolbar-separator"))
            {
                toolStripItem.Items.Add(new Separator());
                continue;
            }

            string text = toolbarItem.Name switch
            {
                "menuCommitInfoPosition" => "Commit info position",
                "userShell" => ToolTip.GetTip(toolbarItem)?.ToString()?.Split('\u00a0')[0] ?? "bash",
                _ => ToolTip.GetTip(toolbarItem)?.ToString()
                     ?? (toolbarItem as ContentControl)?.Content?.ToString()
                     ?? toolbarItem.Name
                     ?? string.Empty,
            };
            MenuItem menuToolbarItem = new()
            {
                Header = text,
                IsChecked = toolbarItem.IsVisible,
                Tag = toolbarItem,
                ToggleType = MenuItemToggleType.CheckBox,
            };
            toolbarItem.PropertyChanged += (_, e) =>
            {
                if (e.Property == Visual.IsVisibleProperty)
                {
                    menuToolbarItem.IsChecked = toolbarItem.IsVisible;
                }
            };
            menuToolbarItem.Click += (_, _) => toolbarItem.IsVisible = !toolbarItem.IsVisible;
            toolStripItem.Items.Add(menuToolbarItem);
        }

        return;

        bool IncludeToolbarItem(Control control)
        {
            if (senderToolStrip.Name != "ToolStripFilters")
            {
                return true;
            }

            // The source groups each label/editor/drop-down cluster behind its first item.
            return control.Name is not "tscboBranchFilter" and not "tsddbtnBranchFilter"
                and not "tstxtRevisionFilter" and not "tsddbtnRevisionFilter";
        }
    }

    internal void RefreshItems()
    {
        _revisionGrid.RefreshMainMenuState();
        SynchronizeItems(includeVisibility: true);
    }

    internal void OnMenuCommandsPropertyChanged()
    {
        // Visibility remains owned by the explicit menu-opening refresh, avoiding transient cloned-menu expansion.
        SynchronizeItems(includeVisibility: false);
    }

    private void SynchronizeItems(bool includeVisibility)
    {
        Dictionary<string, MenuCommand> menuCommands = _revisionGrid.MenuCommands.NavigateMenuCommands
            .Concat(_revisionGrid.MenuCommands.ViewMenuCommands)
            .Where(command => !command.IsSeparator && command.Name is not null)
            .ToDictionary(command => command.Name!, StringComparer.Ordinal);
        foreach ((MenuItem source, MenuItem target) in _sourceItems)
        {
            if (source.Tag is string tag
                && menuCommands.TryGetValue(tag, out MenuCommand? command)
                && command?.Text is string commandText)
            {
                source.Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(commandText);
            }

            target.Header = source.Header;
            target.InputGesture = source.InputGesture;
            target.ToggleType = source.ToggleType;
            target.IsChecked = source.IsChecked;
            target.IsEnabled = source.IsEnabled;
            if (includeVisibility)
            {
                target.IsVisible = source.IsVisible;
            }

            ToolTip.SetTip(target, ToolTip.GetTip(source));
        }
    }

    public void Dispose()
    {
        _navigateToolStripMenuItem.SubmenuOpened -= MainMenuItem_SubmenuOpened;
        _viewToolStripMenuItem.SubmenuOpened -= MainMenuItem_SubmenuOpened;
        _toolStripContextMenu.Opening -= ToolStripContextMenu_Opening;
        _mainMenuStrip.Items.Remove(_navigateToolStripMenuItem);
        _mainMenuStrip.Items.Remove(_viewToolStripMenuItem);
        _toolStripContextMenu.Close();
    }

    private void MainMenuItem_SubmenuOpened(object? sender, EventArgs e)
    {
        RefreshItems();
        RefreshToolbarsMenuItemCheckedState(_toolbarsMenuItem.Items);
    }

    private void ToolStripContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        => RefreshToolbarsMenuItemCheckedState(_toolStripContextMenu.Items);

    private static void RefreshToolbarsMenuItemCheckedState(IEnumerable<object?> items)
    {
        foreach (MenuItem item in items.OfType<MenuItem>())
        {
            if (item.Tag is Control toolStrip)
            {
                item.IsChecked = toolStrip.IsVisible;
            }
        }
    }

    private void CopyItems(MenuItem sourceParent, MenuItem targetParent)
    {
        foreach (object? item in sourceParent.Items)
        {
            if (item is Separator sourceSeparator)
            {
                // Avalonia clones the reusable RevisionGrid menu, so its measured ToolStrip metrics must follow the clone.
                targetParent.Items.Add(new Separator
                {
                    Width = sourceSeparator.Width,
                    MinWidth = sourceSeparator.MinWidth,
                    HorizontalAlignment = sourceSeparator.HorizontalAlignment,
                });
                continue;
            }

            if (item is not MenuItem source)
            {
                continue;
            }

            MenuItem target = new()
            {
                Name = source.Name,
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
                Width = source.Width,
                MinWidth = source.MinWidth,
                HorizontalAlignment = source.HorizontalAlignment,
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
