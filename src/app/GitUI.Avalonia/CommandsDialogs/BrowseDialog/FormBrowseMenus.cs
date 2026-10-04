using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.UserControls;
using Microsoft;
using ResourceManager;
using Point = Avalonia.Point;
using SourceControls = GitUI.Compat.WinFormsControls;

namespace GitUI.CommandsDialogs;

/// <summary>
/// Add MenuCommands as menus to the FormBrowse main menu.
/// This class is intended to have NO dependency to FormBrowse
///   (if needed this kind of code should be done in FormBrowseMenuCommands).
/// </summary>
internal sealed class FormBrowseMenus : ITranslate, IDisposable
{
    /// <summary>
    /// The menu to which we will be adding RevisionGrid command menus.
    /// </summary>
    private readonly Menu _mainMenuStrip;

    /// <summary>
    /// The context menu that be shown to allow toggle visibility of toolbars in <see cref="FormBrowse"/>.
    /// </summary>
    private readonly ContextMenu _toolStripContextMenu = new();
    private List<MenuCommand>? _navigateMenuCommands;
    private List<MenuCommand>? _viewMenuCommands;

    private SourceControls.ToolStripMenuItem? _navigateToolStripMenuItem;
    private SourceControls.ToolStripMenuItem? _viewToolStripMenuItem;
    private MenuItem? _toolbarsMenuItem;

    // we have to remember which items we registered with the menucommands because other
    // location (RevisionGrid) can register items too!
    private readonly List<MenuItem> _itemsRegisteredWithMenuCommand = [];

    public FormBrowseMenus(Menu menuStrip)
    {
        _mainMenuStrip = menuStrip;

        CreateMenuItems();
        Translate();

        _navigateToolStripMenuItem!.SubmenuOpened += MainMenuItem_SubmenuOpened;
        _viewToolStripMenuItem!.SubmenuOpened += MainMenuItem_SubmenuOpened;
        _toolbarsMenuItem!.SubmenuOpened += (_, _) => RefreshToolbarsMenuItemCheckedState(_toolbarsMenuItem.Items);
        _toolStripContextMenu.Opening += (_, _) => RefreshToolbarsMenuItemCheckedState(_toolStripContextMenu.Items);
    }

    public void Dispose()
    {
        _navigateToolStripMenuItem?.SubmenuOpened -= MainMenuItem_SubmenuOpened;
        _viewToolStripMenuItem?.SubmenuOpened -= MainMenuItem_SubmenuOpened;
        RemoveRevisionGridMainMenuItems();
        _toolStripContextMenu.Close();
    }

    public void Translate()
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);
    }

    internal MenuItem NavigateMenuItem => _navigateToolStripMenuItem!;

    internal MenuItem ViewMenuItem => _viewToolStripMenuItem!;

    internal ContextMenu ToolStripContextMenu => _toolStripContextMenu;

    public void AddTranslationItems(ITranslation translation)
    {
        foreach ((string name, object _) in GetAdditionalMainMenuItemsForTranslation())
        {
            translation.AddTranslationItem(nameof(FormBrowse), name, "Text", GetSourceText(name));
        }
    }

    public void TranslateItems(ITranslation translation)
    {
        foreach ((string name, object item) in GetAdditionalMainMenuItemsForTranslation())
        {
            ((MenuItem)item).Header = Translate(name, GetSourceText(name));
        }

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

    private static string GetSourceText(string name)
        => name switch
        {
            "navigateToolStripMenuItem" => "&Navigate",
            "viewToolStripMenuItem" => "&View",
            "toolbarsMenuItem" => "Toolbars",
            _ => string.Empty,
        };

    internal void SetVisible(bool visible)
    {
        _navigateToolStripMenuItem!.IsVisible = visible;
        _viewToolStripMenuItem!.IsVisible = visible;
    }

    /// <summary>
    /// Creates menu items for each toolbar supplied in <paramref name="toolStrips"/>. These menus will
    /// be surfaced in <see cref="_mainMenuStrip"/>, and <see cref="_toolStripContextMenu"/>,
    /// and will allow to toggle visibility of the toolbars.
    /// </summary>
    /// <param name="toolStrips">The list of toobars to toggle visibility for.</param>
    public void CreateToolbarsMenus(params (Control ToolStrip, string Text)[] toolStrips)
    {
        Validates.NotNull(_toolbarsMenuItem);

        foreach ((Control toolStrip, string text) in toolStrips)
        {
            _toolStripContextMenu.Items.Add(CreateItem(toolStrip, text));
            _toolbarsMenuItem.Items.Add(CreateItem(toolStrip, text));
        }

        static MenuItem CreateItem(Control toolStrip, string text)
        {
            MenuItem toolStripItem = new()
            {
                Header = text,
                IsChecked = toolStrip.IsVisible,
                Tag = toolStrip,
                ToggleType = MenuItemToggleType.CheckBox,

                // Cancel closing menu to allow selecting/unselecting multiple items.
                StaysOpenOnClick = true,
            };
            AutomationProperties.SetName(toolStripItem, text);
            CreateToolStripSubMenus(toolStrip, toolStripItem);
            toolStrip.PropertyChanged += (_, e) =>
            {
                if (e.Property == Visual.IsVisibleProperty)
                {
                    toolStripItem.IsChecked = toolStrip.IsVisible;
                }
            };
            toolStripItem.Click += (_, _) => toolStrip.IsVisible = !toolStrip.IsVisible;
            return toolStripItem;
        }
    }

    private static void CreateToolStripSubMenus(Control senderToolStrip, MenuItem toolStripItem)
    {
        const string toolbarSettingsPrefix = "formbrowse_toolbar_visibility_";
        string? currentGroup = null;
        IReadOnlyList<Control> controls = GetToolbarItems(senderToolStrip);
        foreach (Control toolbarItem in controls)
        {
            if (toolbarItem.Classes.Contains("gitextensions-toolbar-separator"))
            {
                toolStripItem.Items.Add(new Separator());
                continue;
            }

            bool belongToAGroup = BelongToAGroup(toolbarItem, out string groupName);
            string key;
            if (belongToAGroup)
            {
                if (currentGroup == groupName)
                {
                    toolbarItem.IsVisible = LoadVisibilitySetting(groupName);
                    continue;
                }

                currentGroup = groupName;
                key = groupName;
            }
            else
            {
                key = toolbarItem.Name ?? string.Empty;
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
            bool visible = LoadVisibilitySetting(key, IsVisibleByDefault(key));

            // Worktree availability is loaded asynchronously with the left-panel model. Keep
            // its configured menu state without exposing the button before that load finishes.
            if (key != "toolStripWorktrees")
            {
                toolbarItem.IsVisible = visible;
            }

            MenuItem menuToolbarItem = new()
            {
                Header = text,
                IsChecked = visible,
                Tag = toolbarItem,
                ToggleType = MenuItemToggleType.CheckBox,
                StaysOpenOnClick = true,
            };
            AutomationProperties.SetName(
                menuToolbarItem,
                AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(text));
            menuToolbarItem.Click += (_, _) =>
            {
                bool selectedVisibility = !toolbarItem.IsVisible;
                toolbarItem.IsVisible = selectedVisibility;
                menuToolbarItem.IsChecked = selectedVisibility;
                SaveVisibilitySetting(key, selectedVisibility, IsVisibleByDefault(key));
                if (belongToAGroup)
                {
                    foreach (Control item in controls)
                    {
                        if (item.Tag is string group && group == groupName)
                        {
                            item.IsVisible = selectedVisibility;
                        }
                    }
                }

                AdaptSeparatorsVisibility(senderToolStrip);
            };
            toolStripItem.Items.Add(menuToolbarItem);
        }

        AdaptSeparatorsVisibility(senderToolStrip);
        return;

        static bool IsVisibleByDefault(string buttonKey) => !buttonKey.Contains(FormBrowse.FetchPullToolbarShortcutsPrefix, StringComparison.Ordinal);
        static void SaveVisibilitySetting(string key, bool visible, bool defaultValue = true)
            => AppSettings.SetBool(toolbarSettingsPrefix + key, visible == defaultValue ? null : visible);
        static bool LoadVisibilitySetting(string key, bool defaultValue = true)
            => AppSettings.GetBool(toolbarSettingsPrefix + key, defaultValue);

        static bool BelongToAGroup(Control toolbarItem, out string groupName)
        {
            const string groupPrefix = "ToolBar_group:";
            if (toolbarItem.Tag is string group && group.StartsWith(groupPrefix, StringComparison.Ordinal))
            {
                groupName = group;
                return true;
            }

            groupName = string.Empty;
            return false;
        }
    }

    private static IReadOnlyList<Control> GetToolbarItems(Control toolStrip)
        => toolStrip switch
        {
            NativeToolStrip strip => strip.Items,
            FilterToolBar filters => filters.Strip.Items,
            Panel panel => panel.Children,
            ContentControl { Content: Panel panel } => panel.Children,
            _ => [],
        };

    private static void AdaptSeparatorsVisibility(Control senderToolStrip)
    {
        IReadOnlyList<Control> items = GetToolbarItems(senderToolStrip);

        // First pass: toolbar items from left to right
        bool shouldHideNextSeparator = true;
        foreach (Control item in items)
        {
            HandleCurrentItem(item);
        }

        // Second pass: toolbar items from right to left
        shouldHideNextSeparator = true;
        for (int i = items.Count - 1; i >= 0; i--)
        {
            Control item = items[i];
            if (item.Classes.Contains("gitextensions-toolbar-separator") && !item.IsVisible)
            {
                continue;
            }

            HandleCurrentItem(item);
        }

        void HandleCurrentItem(Control toolStripItem)
        {
            if (toolStripItem.Classes.Contains("gitextensions-toolbar-separator"))
            {
                toolStripItem.IsVisible = !shouldHideNextSeparator;
                shouldHideNextSeparator = true;
            }
            else
            {
                shouldHideNextSeparator &= !toolStripItem.IsVisible;
            }
        }
    }

    public void ShowToolStripContextMenu(Point point)
    {
        _toolStripContextMenu.Placement = PlacementMode.AnchorAndGravity;
        _toolStripContextMenu.PlacementRect = new Rect(point.X, point.Y, 1, 1);
        _toolStripContextMenu.Open(_mainMenuStrip);
    }

    public void ResetMenuCommandSets()
    {
        RemoveRevisionGridMainMenuItems();
        _navigateMenuCommands = null;
        _viewMenuCommands = null;
    }

    /// <summary>
    /// Appends the provided <paramref name="menuCommands"/> list to the commands menus specified by <paramref name="mainMenuItem"/>.
    /// </summary>
    /// <remarks>
    /// Each new command set will be automatically separated by a separator.
    /// </remarks>
    public void AddMenuCommandSet(MainMenuItem mainMenuItem, IEnumerable<MenuCommand> menuCommands)
    {
        // In the current implementation command menus are defined in the RevisionGrid control,
        // and added to the main menu of the FormBrowse for the ease of use
        List<MenuCommand> selectedMenuCommands; // make that more clear
        switch (mainMenuItem)
        {
            case MainMenuItem.NavigateMenu:
                if (_navigateMenuCommands is null)
                {
                    _navigateMenuCommands = [];
                }
                else
                {
                    _navigateMenuCommands.Add(MenuCommand.CreateSeparator());
                }

                selectedMenuCommands = _navigateMenuCommands;
                break;
            case MainMenuItem.ViewMenu:
                if (_viewMenuCommands is null)
                {
                    _viewMenuCommands = [];
                }
                else
                {
                    _viewMenuCommands.Add(MenuCommand.CreateSeparator());
                }

                selectedMenuCommands = _viewMenuCommands;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mainMenuItem));
        }

        selectedMenuCommands.AddRange(menuCommands);
    }

    /// <summary>
    /// Inserts "Navigate" and "View" menus after the <paramref name="insertAfterMenuItem"/>.
    /// </summary>
    public void InsertRevisionGridMainMenuItems(MenuItem insertAfterMenuItem)
    {
        RemoveRevisionGridMainMenuItems();

        Validates.NotNull(_navigateToolStripMenuItem);
        Validates.NotNull(_navigateMenuCommands);
        Validates.NotNull(_viewToolStripMenuItem);
        Validates.NotNull(_viewMenuCommands);
        Validates.NotNull(_toolbarsMenuItem);

        SetDropDownItems(_navigateToolStripMenuItem, _navigateMenuCommands);
        SetDropDownItems(_viewToolStripMenuItem, _viewMenuCommands);

        int insertIndex = _mainMenuStrip.Items.IndexOf(insertAfterMenuItem) + 1;
        _mainMenuStrip.Items.Insert(insertIndex, _navigateToolStripMenuItem);
        _mainMenuStrip.Items.Insert(insertIndex + 1, _viewToolStripMenuItem);

        // We're a bit lying here - "Toolbars" is not a RevisionGrid menu item,
        // however it is the logical place to add it to the "View" menu
        if (_toolbarsMenuItem.Items.Count > 0)
        {
            _viewToolStripMenuItem.Items.Add(new Separator());
            _viewToolStripMenuItem.Items.Add(_toolbarsMenuItem);
        }

        // maybe set check marks on menu items
        OnMenuCommandsPropertyChanged();
    }

    /// <summary>
    /// Creates menu items to be added to the main menu of the <see cref="FormBrowse"/>
    /// (represented by <see cref="_mainMenuStrip"/>).
    /// </summary>
    /// <remarks>
    /// Call in ctor before translation.
    /// </remarks>
    private void CreateMenuItems()
    {
        _navigateToolStripMenuItem ??= new SourceControls.ToolStripMenuItem
        {
            Name = "navigateToolStripMenuItem",
            Header = "_Navigate",
            IsVisible = false,
        };

        _viewToolStripMenuItem ??= new SourceControls.ToolStripMenuItem
        {
            Name = "viewToolStripMenuItem",
            Header = "_View",
            IsVisible = false,
        };

        _toolbarsMenuItem ??= new MenuItem
        {
            Name = "toolbarsMenuItem",
            Header = "_Toolbars",
        };
    }

    private IEnumerable<(string name, object item)> GetAdditionalMainMenuItemsForTranslation()
    {
        foreach (MenuItem? menuItem in new[] { _navigateToolStripMenuItem, _viewToolStripMenuItem, _toolbarsMenuItem })
        {
            Validates.NotNull(menuItem);
            yield return (menuItem.Name!, menuItem);
        }
    }

    private void SetDropDownItems(MenuItem toolStripMenuItemTarget, IEnumerable<MenuCommand> menuCommands)
    {
        toolStripMenuItemTarget.Items.Clear();
        foreach (MenuCommand menuCommand in menuCommands)
        {
            Control toolStripItem = MenuCommand.CreateToolStripItem(menuCommand);
            if (toolStripItem is MenuItem toolStripMenuItem)
            {
                menuCommand.RegisterMenuItem(toolStripMenuItem);
                _itemsRegisteredWithMenuCommand.Add(toolStripMenuItem);
                toolStripMenuItem.Click += (_, _) => OnMenuCommandsPropertyChanged();
            }

            toolStripMenuItemTarget.Items.Add(toolStripItem);
        }
    }

    // clear is important to avoid mem leaks of event handlers
    // TODO: is everything cleared correct or are there leftover references?
    //       is this relevant here at all?
    //         see also ResetMenuCommandSets()?
    public void RemoveRevisionGridMainMenuItems()
    {
        _mainMenuStrip.Items.Remove(_navigateToolStripMenuItem);
        _mainMenuStrip.Items.Remove(_viewToolStripMenuItem);

        // don't forget to clear old associated menu items
        _navigateMenuCommands?.ForEach(mc => mc.UnregisterMenuItems(_itemsRegisteredWithMenuCommand));
        _viewMenuCommands?.ForEach(mc => mc.UnregisterMenuItems(_itemsRegisteredWithMenuCommand));
        _itemsRegisteredWithMenuCommand.Clear();
        _navigateToolStripMenuItem?.Items.Clear();
        _viewToolStripMenuItem?.Items.Clear();
    }

    public void OnMenuCommandsPropertyChanged()
    {
        foreach (MenuCommand menuCommand in GetNavigateAndViewMenuCommands())
        {
            menuCommand.SetCheckForRegisteredMenuItems();
            menuCommand.UpdateMenuItemsShortcutKeyDisplayString();
            menuCommand.UpdateMenuItemsText();
        }
    }

    private IEnumerable<MenuCommand> GetNavigateAndViewMenuCommands()
    {
        if (_navigateMenuCommands is null && _viewMenuCommands is null)
        {
            return [];
        }

        if (_navigateMenuCommands is not null && _viewMenuCommands is not null)
        {
            return _navigateMenuCommands.Concat(_viewMenuCommands);
        }

        throw new ApplicationException("this case is not allowed");
    }

    private void MainMenuItem_SubmenuOpened(object? sender, EventArgs e)
    {
        OnMenuCommandsPropertyChanged();
        RefreshToolbarsMenuItemCheckedState(_toolbarsMenuItem!.Items);
    }

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
}

internal enum MainMenuItem
{
    NavigateMenu,
    ViewMenu
}
