using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using Microsoft;

namespace GitUI.CommandsDialogs;

/// <summary>
/// Add MenuCommands as menus to the FormBrowse main menu.
/// This class is intended to have NO dependency to FormBrowse
///   (if needed this kind of code should be done in FormBrowseMenuCommands).
/// </summary>
public class FormBrowseMenus : ITranslate
{
    /// <summary>
    /// The menu to which we will be adding RevisionGrid command menus.
    /// </summary>
    private readonly ToolStrip _mainMenuStrip;

    /// <summary>
    /// The context menu that be shown to allow toggle visibility of toolbars in <see cref="FormBrowse"/>.
    /// </summary>
    private readonly ContextMenuStrip _toolStripContextMenu = new();
    private string? _clickedToolbarName;
    private ToolStrip? _clickedToolStrip;
    private ToolStripItem? _clickedItem;

    private List<MenuCommand>? _navigateMenuCommands;
    private List<MenuCommand>? _viewMenuCommands;

    private ToolStripMenuItem? _navigateToolStripMenuItem;
    private ToolStripMenuItem? _viewToolStripMenuItem;
    private ToolStripMenuItem? _toolbarsMenuItem;

    // Store toolbars for dynamic refresh
    private List<ToolStrip> _registeredToolbars = [];

    // we have to remember which items we registered with the menucommands because other
    // location (RevisionGrid) can register items too!
    private readonly List<ToolStripMenuItem> _itemsRegisteredWithMenuCommand = [];

    public FormBrowseMenus(ToolStrip menuStrip)
    {
        _mainMenuStrip = menuStrip;

        CreateMenuItems();
        Translate();

        _toolStripContextMenu.Opening += (s, e) => RefreshContextMenu();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _navigateToolStripMenuItem?.Dispose();
            _viewToolStripMenuItem?.Dispose();
            _toolbarsMenuItem?.Dispose();
        }
    }

    public void Translate()
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);
    }

    public virtual void AddTranslationItems(ITranslation translation)
    {
        TranslationUtils.AddTranslationItemsFromList("FormBrowse", translation, GetAdditionalMainMenuItemsForTranslation());
    }

    public virtual void TranslateItems(ITranslation translation)
    {
        TranslationUtils.TranslateItemsFromList("FormBrowse", translation, GetAdditionalMainMenuItemsForTranslation());
    }

    /// <summary>
    /// Creates menu items for each toolbar supplied in <paramref name="toolStrips"/>. These menus will
    /// be surfaced in <see cref="_mainMenuStrip"/>, and <see cref="_toolStripContextMenu"/>,
    /// and will allow to toggle visibility of the toolbars.
    /// </summary>
    /// <param name="toolStrips">The list of toobars to toggle visibility for.</param>
    public void CreateToolbarsMenus(params ToolStripEx[] toolStrips)
    {
        Validates.NotNull(_toolbarsMenuItem);

        // Store toolbars for dynamic refresh
        _registeredToolbars = toolStrips.ToList<ToolStrip>();

        foreach (ToolStrip toolStrip in toolStrips)
        {
            DebugHelpers.Assert(!string.IsNullOrEmpty(toolStrip.Text), "Toolstrip must specify its name via Text property.");

            _toolbarsMenuItem.DropDownItems.Add(CreateItem(toolStrip));
        }

        // Add separator and Customize menu item
        _toolbarsMenuItem.DropDownItems.Add(new ToolStripSeparator { Name = "toolbarsCustomizeSeparator" });
        ToolStripMenuItem customizeItem = new("Customize toolbar...")
        {
            Name = "customizeToolbarsMenuItem"
        };
        customizeItem.Click += (s, e) =>
        {
            if (_mainMenuStrip.FindForm() is FormBrowse formBrowse)
            {
                FormSettings.ShowSettingsDialog(
                    formBrowse.UICommands,
                    formBrowse,
                    ToolbarsSettingsPage.GetPageReference());
            }
        };
        _toolbarsMenuItem.DropDownItems.Add(customizeItem);
    }

    /// <summary>Refreshes the toolbars menu to reflect dynamically created toolbars.</summary>
    /// <param name="dynamicToolbars">Optional dictionary of dynamically created toolbars to add.</param>
    public void RefreshToolbarsMenu(Dictionary<string, ToolStrip>? dynamicToolbars = null)
    {
        Validates.NotNull(_toolbarsMenuItem);

        // Add dynamic toolbars to registered list if provided
        if (dynamicToolbars != null)
        {
            foreach (ToolStrip toolStrip in dynamicToolbars.Values.Where(ts => !_registeredToolbars.Contains(ts)))
            {
                _registeredToolbars.Add(toolStrip);
            }
        }

        // Remove disposed or invalid toolbars from registered list
        _registeredToolbars.RemoveAll(ts => ts.IsDisposed || string.IsNullOrEmpty(ts.Text));

        // Clear all items except separator and Customize
        List<ToolStripItem> itemsToRemove = new();
        foreach (ToolStripItem item in _toolbarsMenuItem.DropDownItems)
        {
            if (item is not ToolStripSeparator && item.Name != "customizeToolbarsMenuItem")
            {
                itemsToRemove.Add(item);
            }
        }

        foreach (ToolStripItem item in itemsToRemove)
        {
            _toolbarsMenuItem.DropDownItems.Remove(item);
            item.Dispose();
        }

        // Re-add all registered toolbars to main menu
        int insertIndex = 0;
        foreach (ToolStrip toolStrip in _registeredToolbars)
        {
            DebugHelpers.Assert(!string.IsNullOrEmpty(toolStrip.Text), "Toolstrip must specify its name via Text property.");

            // Only show Custom toolbars if they have at least one item
            if (toolStrip.Text.StartsWith("Custom ") && toolStrip.Items.Count == 0)
            {
                continue;
            }

            ToolStripItem menuItem = CreateItem(toolStrip);
            _toolbarsMenuItem.DropDownItems.Insert(insertIndex++, menuItem);
        }

        // Refresh context menu with latest toolbars
        RefreshContextMenu();
    }

    // Refreshes the context menu with currently registered toolbars.
    private void RefreshContextMenu()
    {
        // Remove disposed or invalid toolbars from registered list
        // This is needed because RefreshContextMenu can be called directly from Opening event
        _registeredToolbars.RemoveAll(ts => ts.IsDisposed || string.IsNullOrEmpty(ts.Text));

        // Re-read the menu font on every open so a change applied in
        // Settings > Appearance > Fonts takes effect without restarting.
        _toolStripContextMenu.Font = AppSettings.MenuFont;

        // Clear context menu
        _toolStripContextMenu.Items.Clear();

        // Add only the Customize menu item to context menu (no toolbar visibility toggles)
        ToolStripMenuItem customizeContextItem = new("Customize toolbar...")
        {
            Name = "customizeToolbarsContextMenuItem",
            Image = GitUI.Properties.Images.Settings
        };
        string? toolbarName = _clickedToolbarName;
        customizeContextItem.Click += (s, e) =>
        {
            if (_mainMenuStrip.FindForm() is FormBrowse formBrowse)
            {
                FormSettings.ShowSettingsDialog(
                    formBrowse.UICommands,
                    formBrowse,
                    ToolbarsSettingsPage.GetPageReference(toolbarName));
            }
        };
        _toolStripContextMenu.Items.Add(customizeContextItem);

        // When the right-click happened on top of a concrete toolbar item, offer to remove
        // just that item from its toolbar. Placed below "Customize toolbar...". Captured in
        // locals so the handler is not affected by a later right-click changing the fields.
        ToolStrip? clickedToolStrip = _clickedToolStrip;
        ToolStripItem? clickedItem = _clickedItem;
        if (clickedToolStrip is not null && clickedItem is not null)
        {
            _toolStripContextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem removeItem = new("Remove this")
            {
                Name = "removeToolbarItemContextMenuItem"
            };
            removeItem.Click += (s, e) =>
            {
                if (_mainMenuStrip.FindForm() is FormBrowse formBrowse)
                {
                    formBrowse.RemoveToolbarItemAndSave(clickedToolStrip, clickedItem);
                }
            };
            _toolStripContextMenu.Items.Add(removeItem);
        }
    }

    private ToolStripItem CreateItem(ToolStrip senderToolStrip)
    {
        ToolStripMenuItem toolStripItem = new(senderToolStrip.Text)
        {
            Checked = senderToolStrip.Visible,
            CheckOnClick = true,
            Tag = senderToolStrip
        };

        CreateToolStripSubMenus(senderToolStrip, toolStripItem);

        toolStripItem.Click += (s, e) =>
        {
            senderToolStrip.Visible = !senderToolStrip.Visible;

            // Persist the new visibility so that ReorganizeToolbars reads an up-to-date config
            // and the state is preserved across restarts. Unchecking only hides the toolbar; it must
            // not delete it.
            ToolbarLayoutConfig? config = AppSettings.ToolbarLayout;
            if (config != null)
            {
                bool changed = false;

                ToolbarMetadata? meta = config.ToolbarsVisibility?.FirstOrDefault(t => t.Name == senderToolStrip.Text);
                if (meta != null)
                {
                    meta.Visible = senderToolStrip.Visible;
                    changed = true;
                }

                // Keep the parallel CustomToolbars list in sync: custom toolbars are recreated from
                // their CustomToolbars metadata at startup, so its Visible flag must match.
                CustomToolbarMetadata? customMeta = config.CustomToolbars?.FirstOrDefault(c => c.Name == senderToolStrip.Text);
                if (customMeta != null)
                {
                    customMeta.Visible = senderToolStrip.Visible;
                    changed = true;
                }

                if (changed)
                {
                    AppSettings.ToolbarLayout = config;
                    AppSettings.SettingsContainer.Save();
                }
            }

            if (_mainMenuStrip.FindForm() is FormBrowse formBrowse)
            {
                formBrowse.ReorganizeToolbars();
            }
        };

        return toolStripItem;
    }

    private static void CreateToolStripSubMenus(ToolStrip senderToolStrip, ToolStripMenuItem toolStripItem)
    {
        // The submenu lists the toolbar's actions for reference only. Enabling/disabling actions
        // is handled exclusively by the toolbar customization window (Settings - Toolbars), so these
        // entries are read-only: no checkbox toggling, no persistence.
        string? currentGroup = null;
        toolStripItem.DropDown.SuspendLayout();
        foreach (ToolStripItem toolbarItem in senderToolStrip.Items)
        {
            if (toolbarItem is ToolStripSeparator)
            {
                toolStripItem.DropDownItems.Add(new ToolStripSeparator());
                continue;
            }

            if (BelongToAGroup(toolbarItem, out string groupName))
            {
                // Collapse consecutive items of the same group into a single entry.
                if (currentGroup == groupName)
                {
                    continue;
                }

                currentGroup = groupName;
            }

            // Get display text for the menu item
            string displayText = GetToolbarItemDisplayText(toolbarItem);

            // No checkbox (no Checked / CheckOnClick) and no Click handler: the entry stays
            // enabled and normally rendered, but the left margin can no longer toggle the action.
            ToolStripMenuItem menuToolbarItem = new(displayText)
            {
                Tag = toolbarItem,
                Image = toolbarItem.Image
            };

            toolStripItem.DropDownItems.Add(menuToolbarItem);
        }

        toolStripItem.DropDown.ResumeLayout();

        return;

        static bool BelongToAGroup(ToolStripItem toolbarItem, out string groupName)
        {
            const string groupPrefix = "ToolBar_group:";
            if (toolbarItem.Tag is string group && group.StartsWith(groupPrefix))
            {
                groupName = group;
                return true;
            }

            groupName = string.Empty;
            return false;
        }
    }

    // Gets a display text for a toolbar item, using ToolTipText, Text, or Name.
    // Handles special cases for items with dynamic text.
    private static string GetToolbarItemDisplayText(ToolStripItem item)
    {
        if (item is null)
        {
            return "Unknown";
        }

        if (item is ToolStripSeparator)
        {
            return "--- separator ---";
        }

        // Expanding spacers are ToolStripLabels named "_SPACER_<n>" whose Text is placeholder
        // whitespace, so give them a friendly label instead of falling back to the raw Name.
        if (item.Name is not null && item.Name.StartsWith("_SPACER_", StringComparison.Ordinal))
        {
            return "--- expanding spacer ---";
        }

        // Check for items with dynamic text that should use a friendly name
        if (!string.IsNullOrWhiteSpace(item.Name))
        {
            // Items with dynamic text
            if (item.Name == "_NO_TRANSLATE_WorkingDir")
            {
                return "Change working directory";
            }

            if (item.Name == "toolStripButtonPush")
            {
                return "Push";
            }

            if (item.Name == "branchSelect")
            {
                return "Select branch";
            }
        }

        // Prefer Text (the action's name) so the customize list shows the action name rather than a
        // verbose tooltip (e.g. the merge-base or quick-search buttons carry a long descriptive
        // ToolTipText). Many toolbar buttons are image-only with an empty Text, so fall back to
        // ToolTipText for those.
        // Keep only the first line: some buttons (e.g. the working-directory split button) carry a
        // multi-line tooltip, which would otherwise force a tall menu row and, since a drop-down menu
        // sizes every row to its tallest item, stretch the whole submenu.
        if (!string.IsNullOrWhiteSpace(item.Text))
        {
            return FirstLine(item.Text).Replace("&", "");
        }

        if (!string.IsNullOrWhiteSpace(item.ToolTipText))
        {
            // Strip a trailing keyboard shortcut hint like " (F5)" or " (Ctrl+Alt+C)" so the
            // submenu shows the action name only, without the shortcut.
            return StripShortcutHint(FirstLine(item.ToolTipText)).Replace("&", "");
        }

        // Fallback to Name
        if (!string.IsNullOrWhiteSpace(item.Name))
        {
            return item.Name;
        }

        return $"[{item.GetType().Name}]";

        static string FirstLine(string text)
        {
            int newLine = text.IndexOfAny(['\r', '\n']);
            return (newLine < 0 ? text : text[..newLine]).Trim();
        }

        static string StripShortcutHint(string text)
            => System.Text.RegularExpressions.Regex.Replace(text, @"\s*\([^)]*\)\s*$", string.Empty, System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)).Trim();
    }

    public void ShowToolStripContextMenu(Point point, string? clickedToolbarName = null, ToolStrip? clickedToolStrip = null, ToolStripItem? clickedItem = null)
    {
        _clickedToolbarName = clickedToolbarName;
        _clickedToolStrip = clickedToolStrip;

        // Separators and expanding spacers are layout-only; "Remove this" targets real actions.
        _clickedItem = clickedItem is null or ToolStripSeparator ? null : clickedItem;

        _toolStripContextMenu.Show(point);
    }

    public void ResetMenuCommandSets()
    {
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

        IList<MenuCommand>? selectedMenuCommands = null; // make that more clear

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
        }

        Validates.NotNull(selectedMenuCommands);

        selectedMenuCommands.AddAll(menuCommands);
    }

    /// <summary>
    /// Inserts "Navigate" and "View" menus after the <paramref name="insertAfterMenuItem"/>.
    /// </summary>
    public void InsertRevisionGridMainMenuItems(ToolStripItem insertAfterMenuItem)
    {
        RemoveRevisionGridMainMenuItems();

        Validates.NotNull(_navigateToolStripMenuItem);
        Validates.NotNull(_navigateMenuCommands);
        Validates.NotNull(_viewToolStripMenuItem);
        Validates.NotNull(_viewMenuCommands);
        Validates.NotNull(_toolbarsMenuItem);

        SetDropDownItems(_navigateToolStripMenuItem, _navigateMenuCommands);
        SetDropDownItems(_viewToolStripMenuItem, _viewMenuCommands);

        _mainMenuStrip.Items.Insert(_mainMenuStrip.Items.IndexOf(insertAfterMenuItem) + 1, _navigateToolStripMenuItem);
        _mainMenuStrip.Items.Insert(_mainMenuStrip.Items.IndexOf(_navigateToolStripMenuItem) + 1, _viewToolStripMenuItem);

        // We're a bit lying here - "Toolbars" is not a RevisionGrid menu item,
        // however it is the logical place to add it to the "View" menu
        if (_toolbarsMenuItem.DropDownItems.Count > 0)
        {
            _viewToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            _viewToolStripMenuItem.DropDownItems.Add(_toolbarsMenuItem);
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
        _navigateToolStripMenuItem ??= new ToolStripMenuItem
        {
            Name = "navigateToolStripMenuItem",
            Text = "&Navigate"
        };

        _viewToolStripMenuItem ??= new ToolStripMenuItem
        {
            Name = "viewToolStripMenuItem",
            Text = "&View"
        };

        if (_toolbarsMenuItem is null)
        {
            _toolbarsMenuItem = new ToolStripMenuItem
            {
                Name = "toolbarsMenuItem",
                Text = "Toolbars",
            };
            _toolbarsMenuItem.DropDownOpening += (s, e) => RefreshToolbarsMenuItemCheckedState(_toolbarsMenuItem.DropDownItems);
        }
    }

    private IEnumerable<(string name, object item)> GetAdditionalMainMenuItemsForTranslation()
    {
        foreach (ToolStripMenuItem? menuItem in new[] { _navigateToolStripMenuItem, _viewToolStripMenuItem, _toolbarsMenuItem })
        {
            Validates.NotNull(menuItem);

            yield return (menuItem.Name!, menuItem);
        }
    }

    private void SetDropDownItems(ToolStripMenuItem toolStripMenuItemTarget, IEnumerable<MenuCommand> menuCommands)
    {
        toolStripMenuItemTarget.DropDownItems.Clear();

        List<ToolStripItem> toolStripItems = [];
        foreach (MenuCommand menuCommand in menuCommands)
        {
            ToolStripItem toolStripItem = MenuCommand.CreateToolStripItem(menuCommand);
            toolStripItems.Add(toolStripItem);

            if (toolStripItem is ToolStripMenuItem toolStripMenuItem)
            {
                menuCommand.RegisterMenuItem(toolStripMenuItem);
                _itemsRegisteredWithMenuCommand.Add(toolStripMenuItem);
            }
        }

        toolStripMenuItemTarget.DropDownItems.AddRange([.. toolStripItems]);
    }

    // clear is important to avoid mem leaks of event handlers
    // TODO: is everything cleared correct or are there leftover references?
    //       is this relevant here at all?
    //         see also ResetMenuCommandSets()?
    public void RemoveRevisionGridMainMenuItems()
    {
        _mainMenuStrip.Items.Remove(_navigateToolStripMenuItem!);
        _mainMenuStrip.Items.Remove(_viewToolStripMenuItem!);

        // don't forget to clear old associated menu items
        if (_itemsRegisteredWithMenuCommand is not null)
        {
            _navigateMenuCommands?.ForEach(mc => mc.UnregisterMenuItems(_itemsRegisteredWithMenuCommand));

            _viewMenuCommands?.ForEach(mc => mc.UnregisterMenuItems(_itemsRegisteredWithMenuCommand));

            _itemsRegisteredWithMenuCommand.Clear();
        }
    }

    public void OnMenuCommandsPropertyChanged()
    {
        IEnumerable<MenuCommand> menuCommands = GetNavigateAndViewMenuCommands();

        foreach (MenuCommand menuCommand in menuCommands)
        {
            menuCommand.SetCheckForRegisteredMenuItems();
            menuCommand.UpdateMenuItemsShortcutKeyDisplayString();
        }
    }

    private IEnumerable<MenuCommand> GetNavigateAndViewMenuCommands()
    {
        if (_navigateMenuCommands is null && _viewMenuCommands is null)
        {
            return [];
        }
        else if (_navigateMenuCommands is not null && _viewMenuCommands is not null)
        {
            return _navigateMenuCommands.Concat(_viewMenuCommands);
        }
        else
        {
            throw new ApplicationException("this case is not allowed");
        }
    }

    private static void RefreshToolbarsMenuItemCheckedState(ToolStripItemCollection toolStripItems)
    {
        foreach (ToolStripItem toolStripItem in toolStripItems)
        {
            if (toolStripItem is ToolStripMenuItem item && item.Tag is ToolStrip toolStrip)
            {
                item.Checked = toolStrip.Visible;
            }
        }
    }
}

public enum MainMenuItem
{
    NavigateMenu,
    ViewMenu
}
