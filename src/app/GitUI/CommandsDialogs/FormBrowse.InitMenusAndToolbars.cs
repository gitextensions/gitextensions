using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUI.Shells;
using GitUI.UserControls;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs;

partial class FormBrowse
{
    // This file is dedicated to init logic for FormBrowse menus and toolbars

    internal static readonly string FetchPullToolbarShortcutsPrefix = "pull_shortcut_";

    // Stable identifiers for the three built-in toolbars, used as settings keys and layout names.
    private const string StandardToolbarName = "Standard";
    private const string FiltersToolbarName = "Filters";
    private const string ScriptsToolbarName = "Scripts";

    // Control-name prefix shared by all dynamically created custom toolbars.
    private const string CustomToolbarNamePrefix = "ToolStripCustom";

    // Dictionary to store original toolbar items before any manipulation
    // This preserves event handlers and allows items to be found even after they've been moved
    private readonly Dictionary<string, ToolStripItem> _originalToolbarItems = new();

    public IReadOnlyDictionary<string, ToolStripItem> OriginalToolbarItems => _originalToolbarItems;

    // Snapshots of the default toolbar order, captured before ApplySavedToolbarLayout() runs.
    // Used by ToolbarsSettingsPage to populate the "Default Standard toolbar" and "Default Filters toolbar" categories.
    public IReadOnlyList<ToolStripItem> DefaultStandardToolbarSnapshot { get; private set; } = [];
    public IReadOnlyList<ToolStripItem> DefaultFiltersToolbarSnapshot { get; private set; } = [];

    [System.Diagnostics.Conditional("DEBUG")]
    private static void LogToolbar(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);

        try
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GitExtensions", "toolbar_debug.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Ignore file write errors
        }
    }

    private void InitMenusAndToolbars(string? revFilter, string? pathFilter)
    {
        commandsToolStripMenuItem.DropDownOpening += CommandsToolStripMenuItem_DropDownOpening;

        InitFilters(revFilter, pathFilter);

        // Subscribe existing built-in toolbars to the right-click context menu
        foreach (Control control in toolPanel.TopToolStripPanel.Controls)
        {
            if (control is ToolStrip ts)
            {
                SubscribeToolbarContextMenu(ts);
            }
        }

        // Subscribe dynamically added toolbars (custom toolbars created later)
        toolPanel.TopToolStripPanel.ControlAdded += (s, e) =>
        {
            if (e.Control is ToolStrip ts)
            {
                SubscribeToolbarContextMenu(ts);
            }
        };

        new ToolStripItem[]
        {
            recoverLostObjectsToolStripMenuItem, // Repository->Git maintenance->Recover lost objects
            branchSelect, // main toolbar
        }.ForEach(ColorHelper.AdaptImageLightness);

        pullToolStripMenuItem1.Tag = GitPullAction.None;
        mergeToolStripMenuItem.Tag = GitPullAction.Merge;
        rebaseToolStripMenuItem1.Tag = GitPullAction.Rebase;
        fetchToolStripMenuItem.Tag = GitPullAction.Fetch;
        fetchAllToolStripMenuItem.Tag = GitPullAction.FetchAll;
        fetchPruneAllToolStripMenuItem.Tag = GitPullAction.FetchPruneAll;

        Color toolForeColor = SystemColors.WindowText;
        BackColor = SystemColors.Window;
        ForeColor = toolForeColor;
        mainMenuStrip.ForeColor = toolForeColor;
        InitToolStripStyles(toolForeColor, Color.Transparent);

        UpdateCommitButtonAndGetBrush(status: null, AppSettings.ShowGitStatusInBrowseToolbar);

        FillNextPullActionAsDefaultToolStripMenuItems();
        RefreshDefaultPullAction();

        FillUserShells(defaultShell: BashShell.ShellName);

        // Store all original items BEFORE any manipulation
        StoreOriginalToolbarItems();

        InsertFetchPullShortcuts();

        toolStripButtonPull.DropDownOpening += (_, _) => UpdateFetchAllVisibility();

        LoadDynamicToolbarsFromConfig();

        ApplySavedToolbarLayout();

        ApplyInitialToolbarLayout();

        toolPanel.TopToolStripPanel.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                _formBrowseMenus.ShowToolStripContextMenu(Cursor.Position);
            }
        };
    }

    private void SubscribeToolbarContextMenu(ToolStrip ts)
    {
        ts.MouseClick += (s2, e2) =>
        {
            if (e2.Button == MouseButtons.Right)
            {
                _formBrowseMenus.ShowToolStripContextMenu(Cursor.Position, ts.Text);
            }
        };
    }

    private void InitToolStripStyles(Color toolForeColor, Color toolBackColor)
    {
        toolPanel.TopToolStripPanel.BackColor = toolBackColor;
        toolPanel.TopToolStripPanel.ForeColor = toolForeColor;

        mainMenuStrip.BackColor = toolBackColor;

        ToolStripMain.BackColor = toolBackColor;
        ToolStripMain.ForeColor = toolForeColor;

        ToolStripFilters.BackColor = toolBackColor;
        ToolStripFilters.ForeColor = toolForeColor;
        ToolStripFilters.InitToolStripStyles(toolForeColor, toolBackColor);

        ToolStripScripts.BackColor = toolBackColor;
        ToolStripScripts.ForeColor = toolForeColor;
    }

    private void InitFilters(string? revFilter, string? pathFilter)
    {
        // ToolStripFilters.RefreshRevisionFunction() is init in UICommands_PostRepositoryChanged

        if (!string.IsNullOrWhiteSpace(revFilter))
        {
            ToolStripFilters.SetRevisionFilter(revFilter);
        }

        if (!string.IsNullOrWhiteSpace(pathFilter))
        {
            SetPathFilter(pathFilter.QuoteNE());
        }
    }

    private void StoreOriginalToolbarItems()
    {
        // Store all toolbar items with their original references
        // This allows items to be found even after they've been moved to custom toolbars
        StoreItemsFromToolbar(ToolStripMain);
        StoreItemsFromToolbar(ToolStripFilters);
        StoreItemsFromToolbar(ToolStripScripts);

        // Capture default order snapshots before InsertFetchPullShortcuts and ApplySavedToolbarLayout run
        DefaultStandardToolbarSnapshot = ToolStripMain.Items.Cast<ToolStripItem>().ToList();
        DefaultFiltersToolbarSnapshot = ToolStripFilters.Items.Cast<ToolStripItem>().ToList();

        LogToolbar($"[StoreOriginalToolbarItems] Stored {_originalToolbarItems.Count} items");
    }

    private void StoreItemsFromToolbar(ToolStrip toolbar)
    {
        foreach (ToolStripItem item in toolbar.Items)
        {
            if (!string.IsNullOrWhiteSpace(item.Name) && !_originalToolbarItems.ContainsKey(item.Name))
            {
                _originalToolbarItems[item.Name] = item;
                LogToolbar($"[StoreOriginalToolbarItems] Stored: {item.Name} from {toolbar.Name}");
            }

            if (item is ToolStripDropDownItem dropDown)
            {
                foreach (ToolStripItem subItem in dropDown.DropDownItems)
                {
                    if (!string.IsNullOrWhiteSpace(subItem.Name) && !_originalToolbarItems.ContainsKey(subItem.Name))
                    {
                        _originalToolbarItems[subItem.Name] = subItem;
                        LogToolbar($"[StoreOriginalToolbarItems] Stored dropdown sub-item: {subItem.Name} from {item.Name}");
                    }
                }
            }
        }
    }

    private void LoadDynamicToolbarsFromConfig()
    {
        ToolbarLayoutConfig? config = AppSettings.ToolbarLayout;

        LogToolbar($"[LoadDynamicToolbarsFromConfig] Config is null: {config is null}");

        if (config is null || config.CustomToolbars is null || config.CustomToolbars.Count == 0)
        {
            LogToolbar("[LoadDynamicToolbarsFromConfig] No custom toolbars to load");
            return;
        }

        LogToolbar($"[LoadDynamicToolbarsFromConfig] Loading {config.CustomToolbars.Count} custom toolbars");

        foreach (CustomToolbarMetadata metadata in config.CustomToolbars.OrderBy(m => m.Index))
        {
            string controlName = $"{CustomToolbarNamePrefix}{new string(metadata.Name.Where(c => char.IsLetterOrDigit(c)).ToArray())}";

            // Guard against double-invocation of LoadDynamicToolbarsFromConfig and against
            // stale configs where two entries share the same sanitized control Name.
            if (toolPanel.TopToolStripPanel.Controls.OfType<ToolStrip>().Any(
                    ts => ts.Text == metadata.Name || ts.Name == controlName))
            {
                LogToolbar($"[LoadDynamicToolbarsFromConfig] Skipping duplicate toolbar: {metadata.Name}");
                continue;
            }

            LogToolbar($"[LoadDynamicToolbarsFromConfig] Creating toolbar: {metadata.Name}, Index: {metadata.Index}, Visible: {metadata.Visible}");

            ToolStripEx newToolStrip = new()
            {
                Name = controlName,
                Text = metadata.Name,
                Visible = metadata.Visible,

                // Match built-in toolbar properties for consistent appearance
                ClickThrough = true,
                Dock = DockStyle.None,
                DrawBorder = false,
                GripEnabled = false,
                GripStyle = ToolStripGripStyle.Visible,
                GripMargin = new System.Windows.Forms.Padding(0),
                Padding = new System.Windows.Forms.Padding(0),
                LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow,
                BackColor = ToolStripMain.BackColor,
                ForeColor = ToolStripMain.ForeColor
            };

            // Always add to panel (even if invisible) so toolbar can be found later for ReorganizeToolbars and toggle visibility
            toolPanel.TopToolStripPanel.Controls.Add(newToolStrip);
            LogToolbar($"[LoadDynamicToolbarsFromConfig] Added toolbar: {metadata.Name}, Visible: {metadata.Visible}");
        }
    }

    private void ApplyInitialToolbarLayout()
    {
        // Applies the saved toolbar layout at startup: positions every toolbar (built-in
        // and custom) on its configured Row at the configured OrderInRow, by joining each
        // one to the panel in ascending OrderInRow with a cumulative-X strategy. This works
        // around ToolStripPanel.Join not honoring the requested position when an earlier
        // toolbar grows wider than the next toolbar's Location.X.

        ToolbarLayoutConfig? config = AppSettings.ToolbarLayout;

        // With no saved layout there is nothing to apply: re-joining the built-in toolbars with a
        // cumulative-X strategy would discard the designer's single-row placement and can push a
        // toolbar onto a second row. Leave the original layout untouched for a default install.
        // Note: AppSettings.ToolbarLayout never returns null (it yields an empty config), so test
        // for emptiness rather than null.
        if (!HasSavedToolbarLayout(config))
        {
            LogToolbar("[ApplyInitialToolbarLayout] No saved layout, keep designer layout");
            return;
        }

        List<(ToolStrip ToolStrip, int Row, int OrderInRow)> layoutInfo = BuildToolbarLayoutInfo(config, "ApplyInitialToolbarLayout");
        JoinToolbarsByRowOrder(layoutInfo, "ApplyInitialToolbarLayout");
        LogToolbar($"[ApplyInitialToolbarLayout] COMPLETED - Applied layout with {layoutInfo.Count} toolbars");
    }

    private void ApplySavedToolbarLayout()
    {
        ToolbarLayoutConfig? config = AppSettings.ToolbarLayout;

        LogToolbar($"[ApplySavedToolbarLayout] Config is null: {config is null}");

        if (config is null)
        {
            LogToolbar("[ApplySavedToolbarLayout] No saved layout, use defaults");
            return;
        }

        bool hasItemLayout = config.Items is { Count: > 0 };
        LogToolbar($"[ApplySavedToolbarLayout] Applying layout with {config.Items?.Count ?? 0} items");

        if (config.ToolbarsVisibility != null && config.ToolbarsVisibility.Count > 0)
        {
            RestoreToolbarsVisibility(config);
        }

        // Tracks which item names have already been placed on some toolbar during this cycle.
        // If the same item name appears again on another toolbar, we clone instead of moving.
        HashSet<string> placedItemNames = new(StringComparer.Ordinal);

        if (hasItemLayout)
        {
            ApplyLayoutToBuiltInToolbars(config, placedItemNames);
        }

        if (config.CustomToolbars is not null)
        {
            ApplyLayoutToCustomToolbars(config, hasItemLayout, placedItemNames);
        }
    }

    private void RestoreToolbarsVisibility(ToolbarLayoutConfig config)
    {
        LogToolbar($"[ApplySavedToolbarLayout] Restoring visibility for {config.ToolbarsVisibility!.Count} toolbars");

        foreach (ToolbarMetadata metadata in config.ToolbarsVisibility)
        {
            ToolStrip? toolbar = metadata.Name switch
            {
                StandardToolbarName => ToolStripMain,
                FiltersToolbarName => ToolStripFilters,
                ScriptsToolbarName => ToolStripScripts,
                _ => toolPanel.TopToolStripPanel.Controls.Cast<Control>().OfType<ToolStrip>()
                    .FirstOrDefault(ts => ts.Text == metadata.Name)
            };

            if (toolbar != null)
            {
                toolbar.Visible = metadata.Visible;
                ApplyToolbarIconSize(toolbar, metadata.IconSize, AppSettings.ToolbarSyncIconTextWithSize);
                LogToolbar($"[ApplySavedToolbarLayout] Set {metadata.Name} visibility to {metadata.Visible}, IconSize to {metadata.IconSize}");
            }
        }
    }

    private void ApplyLayoutToBuiltInToolbars(ToolbarLayoutConfig config, HashSet<string> placedItemNames)
    {
        bool AllIconsShowText(string name) => config.ToolbarsVisibility?
            .FirstOrDefault(m => m.Name == name)?.AllIconsShowText ?? false;

        ApplyLayoutToToolStrip(ToolStripMain, StandardToolbarName, config, isCustomToolbar: false, allIconsShowText: AllIconsShowText(StandardToolbarName), placedItemNames);
        ApplyLayoutToToolStrip(ToolStripFilters, FiltersToolbarName, config, isCustomToolbar: false, allIconsShowText: AllIconsShowText(FiltersToolbarName), placedItemNames);
        ApplyLayoutToToolStrip(ToolStripScripts, ScriptsToolbarName, config, isCustomToolbar: false, allIconsShowText: AllIconsShowText(ScriptsToolbarName), placedItemNames);
    }

    private void ApplyLayoutToCustomToolbars(ToolbarLayoutConfig config, bool hasItemLayout, HashSet<string> placedItemNames)
    {
        LogToolbar($"[ApplySavedToolbarLayout] Processing {config.CustomToolbars!.Count} custom toolbars");

        foreach (CustomToolbarMetadata metadata in config.CustomToolbars)
        {
            ToolStrip? customToolStrip = toolPanel.TopToolStripPanel.Controls
                .Cast<Control>()
                .OfType<ToolStrip>()
                .FirstOrDefault(ts => ts.Text == metadata.Name);

            if (customToolStrip is not null)
            {
                LogToolbar($"[ApplySavedToolbarLayout] Applying layout to custom toolbar: {metadata.Name}, IconSize: {metadata.IconSize}");
                ApplyToolbarIconSize(customToolStrip, metadata.IconSize, AppSettings.ToolbarSyncIconTextWithSize);
                if (hasItemLayout)
                {
                    ApplyLayoutToToolStrip(customToolStrip, metadata.Name, config, isCustomToolbar: true, allIconsShowText: metadata.AllIconsShowText, placedItemNames);
                }
            }
            else
            {
                LogToolbar($"[ApplySavedToolbarLayout] ERROR: Custom toolbar not found: {metadata.Name}");
            }
        }

        Dictionary<string, ToolStrip> dynamicToolbars = toolPanel.TopToolStripPanel.Controls
            .OfType<ToolStrip>()
            .Where(toolStrip => toolStrip.Name.StartsWith(CustomToolbarNamePrefix))
            .ToDictionary(toolStrip => toolStrip.Name);

        foreach (ToolStrip toolStrip in dynamicToolbars.Values)
        {
            LogToolbar($"[ApplySavedToolbarLayout] Added {toolStrip.Name} ({toolStrip.Text}) with {toolStrip.Items.Count} items to dynamic toolbars");
        }

        if (dynamicToolbars.Count > 0)
        {
            LogToolbar($"[ApplySavedToolbarLayout] Refreshing toolbar menus with {dynamicToolbars.Count} dynamic toolbars");
            _formBrowseMenus.RefreshToolbarsMenu(dynamicToolbars);
        }
    }

    private void ApplyLayoutToToolStrip(ToolStrip toolStrip, string toolbarName, ToolbarLayoutConfig config, bool isCustomToolbar, bool allIconsShowText = false, HashSet<string>? placedItemNames = null)
    {
        if (toolStrip is null || config is null || config.Items is null)
        {
            return;
        }

        // Match items to their toolbar by name. The name is the authoritative, stable key
        // (a unique-name guard exists), unlike the former integer index which could diverge
        // between item and toolbar metadata after a delete/recreate or rename.
        List<ToolbarItemConfig> itemsForToolbar = config.Items
            .Where(ic => ic.ToolbarName == toolbarName)
            .OrderBy(ic => ic.Order)
            .ToList();

        LogToolbar($"[ApplyLayoutToToolStrip] Toolbar: {toolStrip.Name}, Name: {toolbarName}, IsCustom: {isCustomToolbar}, Items to apply: {itemsForToolbar.Count}");

        if (isCustomToolbar)
        {
            ApplyLayoutToCustomToolStrip(toolStrip, itemsForToolbar, allIconsShowText, placedItemNames);
        }
        else
        {
            ApplyLayoutToBuiltInToolStrip(toolStrip, itemsForToolbar, allIconsShowText, placedItemNames);
        }

        // If the toolbar has "all icons show text" set, override every real item to ImageAndText.
        if (allIconsShowText)
        {
            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (item is not ToolStripSeparator and not ToolStripLabel)
                {
                    ApplyItemDisplayStyle(item, showText: true);
                }
            }
        }

        LogToolbar($"[ApplyLayoutToToolStrip] Final item count in {toolStrip.Name}: {toolStrip.Items.Count}");
    }

    private void ApplyLayoutToCustomToolStrip(ToolStrip toolStrip, List<ToolbarItemConfig> itemsForToolbar, bool allIconsShowText, HashSet<string>? placedItemNames)
    {
        int insertIndex = 0;
        foreach (ToolbarItemConfig itemConfig in itemsForToolbar)
        {
            LogToolbar($"[ApplyLayoutToToolStrip] Processing item: {itemConfig.ItemName}, Order: {itemConfig.Order}");

            ToolStripItem? item = CreateItemForCustomConfig(itemConfig.ItemName);
            if (item != null)
            {
                InsertItemIntoCustomToolStrip(toolStrip, item, itemConfig, allIconsShowText, placedItemNames, ref insertIndex);
            }
            else
            {
                LogToolbar($"[ApplyLayoutToToolStrip] WARNING: Item {itemConfig.ItemName} not found!");
            }
        }
    }

    private ToolStripItem? CreateItemForCustomConfig(string itemName)
    {
        if (itemName.StartsWith("_SEPARATOR_"))
        {
            LogToolbar($"[ApplyLayoutToToolStrip] Created separator");
            return new ToolStripSeparator { Overflow = ToolStripItemOverflow.Never, Visible = true };
        }

        if (itemName.StartsWith("_SPACER_"))
        {
            LogToolbar($"[ApplyLayoutToToolStrip] Created spacer");
            return new ToolStripLabel
            {
                Name = itemName,
                AutoSize = true,
                Text = "     ",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Overflow = ToolStripItemOverflow.Never,
                Visible = true
            };
        }

        if (itemName.StartsWith("_LABEL_"))
        {
            ToolStripLabel label = CreateLabelItem(itemName);
            LogToolbar($"[ApplyLayoutToToolStrip] Created editable label: {label.Text}");
            return label;
        }

        ToolStripItem? item = FindItemInAllToolbars(itemName);
        LogToolbar($"[ApplyLayoutToToolStrip] FindItemInAllToolbars({itemName}) returned: {(item != null ? item.Name : "NULL")}");
        return item;
    }

    private void InsertItemIntoCustomToolStrip(ToolStrip toolStrip, ToolStripItem item, ToolbarItemConfig itemConfig, bool allIconsShowText, HashSet<string>? placedItemNames, ref int insertIndex)
    {
        if (item is ToolStripMenuItem menuItem)
        {
            item = ConvertMenuItemToButton(menuItem);
            string itemType = item switch
            {
                ToolStripSplitButton => "SplitButton",
                ToolStripDropDownButton => "DropDownButton",
                _ => "Button"
            };
            LogToolbar($"[ApplyLayoutToToolStrip] Converted {menuItem.Name} from MenuItem to {itemType}");
        }

        string trackingName = itemConfig.ItemName;
        if (placedItemNames != null && placedItemNames.Contains(trackingName) &&
            item is ToolStripButton or ToolStripSplitButton or ToolStripDropDownButton)
        {
            LogToolbar($"[ApplyLayoutToToolStrip] Cloning {item.Name} (already placed on another toolbar)");
            item = ToolbarItemConverter.CloneItem(item, wantsText: itemConfig.ShowText || allIconsShowText);
        }
        else if (item.Owner != null && item.Owner != toolStrip)
        {
            LogToolbar($"[ApplyLayoutToToolStrip] Removing {item.Name} from {item.Owner.Name}");
            item.Owner.Items.Remove(item);
        }

        int targetIndex = Math.Min(insertIndex, toolStrip.Items.Count);
        if (!toolStrip.Items.Contains(item))
        {
            toolStrip.Items.Insert(targetIndex, item);
            item.Visible = true;
            item.Overflow = ToolStripItemOverflow.Never;
            placedItemNames?.Add(trackingName);
            LogToolbar($"[ApplyLayoutToToolStrip] Inserted {item.Name} at index {targetIndex} in {toolStrip.Name}");
        }
        else
        {
            LogToolbar($"[ApplyLayoutToToolStrip] Item {item.Name} already in {toolStrip.Name}");
        }

        if (item is not ToolStripSeparator and not ToolStripLabel)
        {
            ApplyItemDisplayStyle(item, itemConfig.ShowText);
        }

        insertIndex++;
    }

    private void ApplyLayoutToBuiltInToolStrip(ToolStrip toolStrip, List<ToolbarItemConfig> itemsForToolbar, bool allIconsShowText, HashSet<string>? placedItemNames)
    {
        // For built-in toolbars: rebuild the item list from the config.
        // Separators may be stored either by their real name (e.g. "toolStripSeparator0")
        // or by the legacy placeholder scheme ("_SEPARATOR_N"). Both are handled below.
        Dictionary<string, ToolStripItem> byName = toolStrip.Items.Cast<ToolStripItem>()
            .Where(i => !string.IsNullOrWhiteSpace(i.Name))
            .ToDictionary(i => i.Name!, i => i);

        // Queue of the original separators in document order, used when a config entry
        // uses the legacy "_SEPARATOR_N" scheme and no named separator can be found.
        Queue<ToolStripSeparator> separatorPool = new(toolStrip.Items.OfType<ToolStripSeparator>());

        toolStrip.Items.Clear();

        foreach (ToolbarItemConfig itemConfig in itemsForToolbar)
        {
            ToolStripItem? namedItem = ResolveBuiltInItem(byName, itemConfig.ItemName);

            if (namedItem != null && !itemConfig.ItemName.StartsWith('_'))
            {
                AddResolvedBuiltInItem(toolStrip, ref namedItem, itemConfig, allIconsShowText, placedItemNames);
            }
            else
            {
                AddSpecialBuiltInItem(toolStrip, itemConfig.ItemName, separatorPool);
            }
        }
    }

    private ToolStripItem? ResolveBuiltInItem(Dictionary<string, ToolStripItem> byName, string itemName)
    {
        // Look up in the current toolbar first, then fall back to the global dictionary,
        // then fall back to FindItemInAllToolbars (which also searches menus and converts).
        if (byName.TryGetValue(itemName, out ToolStripItem? item))
        {
            return item;
        }

        if (_originalToolbarItems.TryGetValue(itemName, out item))
        {
            return item;
        }

        if (!itemName.StartsWith('_'))
        {
            return FindItemInAllToolbars(itemName);
        }

        return null;
    }

    private void AddResolvedBuiltInItem(ToolStrip toolStrip, ref ToolStripItem namedItem, ToolbarItemConfig itemConfig, bool allIconsShowText, HashSet<string>? placedItemNames)
    {
        // MenuItems must be converted to a ToolStripButton/SplitButton to render
        // and respond to clicks inside a ToolStrip.
        if (namedItem is ToolStripMenuItem menuItem)
        {
            namedItem = ConvertMenuItemToButton(menuItem);
            LogToolbar($"[ApplyLayoutToToolStrip] Converted {menuItem.Name} from MenuItem to {namedItem.GetType().Name}");
        }

        if (placedItemNames != null && placedItemNames.Contains(itemConfig.ItemName) &&
            namedItem is ToolStripButton or ToolStripSplitButton or ToolStripDropDownButton)
        {
            LogToolbar($"[ApplyLayoutToToolStrip] Cloning {namedItem.Name} (already placed on another toolbar)");
            namedItem = ToolbarItemConverter.CloneItem(namedItem, wantsText: itemConfig.ShowText || allIconsShowText);
        }
        else if (namedItem.Owner != null && namedItem.Owner != toolStrip)
        {
            namedItem.Owner.Items.Remove(namedItem);
        }

        toolStrip.Items.Add(namedItem);
        namedItem.Visible = true;
        namedItem.Overflow = ToolStripItemOverflow.Never;
        placedItemNames?.Add(itemConfig.ItemName);
        ApplyItemDisplayStyle(namedItem, itemConfig.ShowText);
        LogToolbar($"[ApplyLayoutToToolStrip] Added named item: {itemConfig.ItemName}");
    }

    private static void AddSpecialBuiltInItem(ToolStrip toolStrip, string itemName, Queue<ToolStripSeparator> separatorPool)
    {
        if (itemName.StartsWith("_SEPARATOR_"))
        {
            // Legacy placeholder — reuse a pooled separator or create a new one.
            ToolStripSeparator sep = separatorPool.Count > 0
                ? separatorPool.Dequeue()
                : new ToolStripSeparator();
            toolStrip.Items.Add(sep);
            LogToolbar($"[ApplyLayoutToToolStrip] Added separator for {itemName}");
        }
        else if (itemName.StartsWith("_SPACER_"))
        {
            ToolStripLabel spacer = new()
            {
                Name = itemName,
                AutoSize = true,
                Text = "     ",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Overflow = ToolStripItemOverflow.Never
            };
            toolStrip.Items.Add(spacer);
            LogToolbar($"[ApplyLayoutToToolStrip] Added spacer for {itemName}");
        }
        else if (itemName.StartsWith("_LABEL_"))
        {
            ToolStripLabel label = CreateLabelItem(itemName);
            toolStrip.Items.Add(label);
            LogToolbar($"[ApplyLayoutToToolStrip] Added editable label: {label.Text}");
        }
        else
        {
            LogToolbar($"[ApplyLayoutToToolStrip] WARNING: Item {itemName} not found, skipped");
        }
    }

    private static ToolStripLabel CreateLabelItem(string itemName)
    {
        // _LABEL_{encodedText}_{order}
        string encoded = itemName.Substring(7);
        int lastUnderscore = encoded.LastIndexOf('_');
        string labelText = lastUnderscore > 0
            ? Uri.UnescapeDataString(encoded.Substring(0, lastUnderscore))
            : Uri.UnescapeDataString(encoded);

        return new ToolStripLabel
        {
            Name = itemName,
            Text = labelText,
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            AutoSize = true,
            Overflow = ToolStripItemOverflow.Never,
            Visible = true
        };
    }

    // Wrap LogToolbar in an inline lambda. Calling [Conditional("DEBUG")] methods directly
    // via delegate is forbidden by the compiler (CS1618); the lambda body allows the
    // conditional call to be elided in Release while still producing a valid Action<string>.
    private ToolStripItem ConvertMenuItemToButton(ToolStripMenuItem menuItem)
        => ToolbarItemConverter.Convert(
            menuItem,
            ToolStripItemDisplayStyle.Image,
            _originalToolbarItems,
            msg => LogToolbar(msg));

    private ToolStripItem? FindItemInAllToolbars(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return null;
        }

        // First, try to find in the original items dictionary
        // This works even if items have been moved to custom toolbars
        if (_originalToolbarItems.TryGetValue(itemName, out ToolStripItem? originalItem))
        {
            LogToolbar($"[FindItemInAllToolbars] Found {itemName} in _originalToolbarItems");
            return originalItem;
        }

        LogToolbar($"[FindItemInAllToolbars] {itemName} NOT found in _originalToolbarItems, searching in toolbars...");

        ToolStripItem? item = ToolStripMain.Items.Cast<ToolStripItem>().FirstOrDefault(i => i.Name == itemName);
        if (item != null)
        {
            LogToolbar($"[FindItemInAllToolbars] Found {itemName} in ToolStripMain");
            return item;
        }

        item = ToolStripFilters.Items.Cast<ToolStripItem>().FirstOrDefault(i => i.Name == itemName);
        if (item != null)
        {
            LogToolbar($"[FindItemInAllToolbars] Found {itemName} in ToolStripFilters");
            return item;
        }

        item = ToolStripScripts.Items.Cast<ToolStripItem>().FirstOrDefault(i => i.Name == itemName);
        if (item != null)
        {
            LogToolbar($"[FindItemInAllToolbars] Found {itemName} in ToolStripScripts");
            return item;
        }

        LogToolbar($"[FindItemInAllToolbars] {itemName} not found in toolbars, searching in menus...");
        item = FindItemInMenus(mainMenuStrip, itemName);
        if (item != null)
        {
            LogToolbar($"[FindItemInAllToolbars] Found {itemName} in menus");
            return item;
        }

        ContextMenuStrip? revisionContextMenu = RevisionGridControl?.MainContextMenu;
        if (revisionContextMenu != null)
        {
            item = FindItemInContextMenuRecursive(revisionContextMenu, itemName);
            if (item != null)
            {
                LogToolbar($"[FindItemInAllToolbars] Found {itemName} in revision grid context menu");
                return item;
            }
        }

        ContextMenuStrip? repoObjectsTreeContextMenu = RepoObjectsTreeContextMenu;
        if (repoObjectsTreeContextMenu != null)
        {
            item = FindItemInContextMenuRecursive(repoObjectsTreeContextMenu, itemName);
            if (item != null)
            {
                LogToolbar($"[FindItemInAllToolbars] Found {itemName} in repo objects tree context menu");
                return item;
            }
        }

        LogToolbar($"[FindItemInAllToolbars] {itemName} NOT FOUND anywhere!");
        return null;
    }

    private static ToolStripItem? FindItemInMenus(ToolStrip menuStrip, string itemName)
    {
        if (menuStrip == null)
        {
            return null;
        }

        foreach (ToolStripItem menuItem in menuStrip.Items)
        {
            if (menuItem.Name == itemName)
            {
                return menuItem;
            }

            if (menuItem is ToolStripMenuItem menuItemWithSubItems && menuItemWithSubItems.DropDownItems.Count > 0)
            {
                ToolStripItem? found = FindItemInMenuRecursive(menuItemWithSubItems, itemName);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static ToolStripItem? FindItemInMenuRecursive(ToolStripMenuItem parentMenuItem, string itemName)
    {
        foreach (ToolStripItem item in parentMenuItem.DropDownItems)
        {
            if (item.Name == itemName)
            {
                return item;
            }

            if (item is ToolStripMenuItem subMenuItem && subMenuItem.DropDownItems.Count > 0)
            {
                ToolStripItem? found = FindItemInMenuRecursive(subMenuItem, itemName);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static ToolStripItem? FindItemInContextMenuRecursive(ContextMenuStrip contextMenu, string itemName)
    {
        foreach (ToolStripItem item in contextMenu.Items)
        {
            if (item.Name == itemName)
            {
                return item;
            }

            if (item is ToolStripMenuItem subMenuItem)
            {
                ToolStripItem? found = FindItemInMenuRecursive(subMenuItem, itemName);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    // Synchronizes Enabled state of toolbar buttons whose Tag is a ToolStripMenuItem
    // belonging to <paramref name="contextMenu"/>, so that buttons reflect the current
    // context (e.g. tree selection) without requiring the context menu to be opened.
    private void SyncToolbarButtonStatesFromContextMenu(ContextMenuStrip contextMenu)
    {
        // Build a fast lookup: menuItem instance → its current Enabled state
        Dictionary<ToolStripMenuItem, bool> enabledByItem = BuildEnabledStates(contextMenu.Items);

        foreach (Control control in toolPanel.TopToolStripPanel.Controls)
        {
            if (control is not ToolStrip toolStrip)
            {
                continue;
            }

            foreach (ToolStripItem toolStripItem in toolStrip.Items)
            {
                if (toolStripItem.Tag is ToolStripMenuItem linkedMenuItem &&
                    enabledByItem.TryGetValue(linkedMenuItem, out bool enabled))
                {
                    toolStripItem.Enabled = enabled;
                }
            }
        }

        static Dictionary<ToolStripMenuItem, bool> BuildEnabledStates(ToolStripItemCollection items)
        {
            Dictionary<ToolStripMenuItem, bool> result = new();
            CollectEnabledStates(items, result);
            return result;
        }

        static void CollectEnabledStates(ToolStripItemCollection items, Dictionary<ToolStripMenuItem, bool> result)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    result[menuItem] = menuItem.Enabled;
                    if (menuItem.DropDownItems.Count > 0)
                    {
                        CollectEnabledStates(menuItem.DropDownItems, result);
                    }
                }
            }
        }
    }

    private void UpdateWorktreeToolStripVisibility()
    {
        if (!Module.IsValidGitWorkingDir())
        {
            toolStripWorktrees.Visible = false;
            return;
        }

        ThreadHelper.FileAndForget(async () =>
        {
            IReadOnlyList<GitWorktree> worktrees = Module.GetWorktrees();

            await this.SwitchToMainThreadAsync();

            toolStripWorktrees.Visible = worktrees.Count > 1;
        });
    }

    // Sets DisplayStyle to ImageAndText or Image on a toolbar item.
    // When enabling text, fills item.Text from ToolTipText if empty so icon-only items
    // (e.g. RefreshButton which has no Text) actually render a visible label.
    // labelText: preferred text to display; falls back to item.ToolTipText (minus shortcut
    // hints) when empty. Pass the wrapper's DisplayName from ToolbarsSettingsPage to avoid
    // using a dynamic ToolTipText (e.g. tsbtnAdvancedFilter whose tooltip shows filter state).
    internal static void ApplyItemDisplayStyle(ToolStripItem item, bool showText, string? labelText = null)
    {
        // IPushLabelItem (ToolStripPushButton or ToolStripPushButtonClone) carries its own
        // ShowLabel/LabelText state — act directly on the item, no redirection to the original.
        if (item is IPushLabelItem pushItem)
        {
            // Prefer the caller-supplied labelText; fall back to the existing label on the item
            // (which may already be the translated "Push" string set at construction time).
            string resolvedLabel = !string.IsNullOrEmpty(labelText)
                ? labelText
                : pushItem.LabelText ?? (item as ToolStripPushButton)?._push.Text ?? "Push";

            // Set LabelText before ShowLabel so that when ShowLabel's setter calls
            // RefreshDisplayedText, _labelText already holds the correct new value.
            pushItem.LabelText = resolvedLabel;
            pushItem.ShowLabel = showText;
            return;
        }

        if (showText)
        {
            if (!string.IsNullOrEmpty(labelText))
            {
                item.Text = labelText;
            }
            else if (string.IsNullOrEmpty(item.Text) && !string.IsNullOrEmpty(item.ToolTipText))
            {
                // Strip keyboard shortcut hints like " (F5)" or " (Ctrl+P)" from the tooltip.
                item.Text = System.Text.RegularExpressions.Regex.Replace(
                    item.ToolTipText, @"\s*\([^)]*\)\s*$", string.Empty).Trim();
            }

            // Let the item resize itself to fit the text label.
            item.AutoSize = true;
            item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        }
        else
        {
            item.DisplayStyle = ToolStripItemDisplayStyle.Image;
        }
    }

    internal static void ApplyToolbarIconSize(ToolStrip toolStrip, int iconSize, bool syncTextWithSize = false)
    {
        // Guard against corrupt config: fall back to the default rather than producing invisible icons.
        int size = iconSize > 0 ? iconSize : 16;
        toolStrip.ImageScalingSize = new Size(size, size);

        // Widen the drop-down region of split buttons proportionally so the enlarged arrow
        // (scaled by ToolStripArrowScaler) fits inside it. 11px (DPI-scaled) is the framework's
        // baseline drop-down width for 16px icons; reset to that baseline when not enlarging.
        int baselineDropDownWidth = GitExtUtils.GitUI.DpiUtil.Scale(11);
        int dropDownWidth = size > 16
            ? (int)Math.Round(baselineDropDownWidth * (size / 16.0))
            : baselineDropDownWidth;
        foreach (ToolStripSplitButton splitButton in toolStrip.Items.OfType<ToolStripSplitButton>())
        {
            splitButton.DropDownButtonWidth = dropDownWidth;
        }

        // AppSettings.Font is the application font (Segoe UI 9 pt by default), applied to all
        // GitExtensionsFormBase instances and inherited by their child ToolStrips.
        Font baseFont = GitCommands.AppSettings.Font;
        if (syncTextWithSize)
        {
            // Scale the font proportionally: baseline is 16px icons = application font size.
            float scaledSize = baseFont.Size * size / 16f;
            if (Math.Abs(toolStrip.Font.Size - scaledSize) > 0.01f
                || toolStrip.Font.FontFamily.Name != baseFont.FontFamily.Name
                || toolStrip.Font.Style != baseFont.Style)
            {
                toolStrip.Font = new Font(baseFont.FontFamily, scaledSize, baseFont.Style);
            }
        }
        else
        {
            // Restore the application font when sync is disabled.
            if (Math.Abs(toolStrip.Font.Size - baseFont.Size) > 0.01f
                || toolStrip.Font.FontFamily.Name != baseFont.FontFamily.Name
                || toolStrip.Font.Style != baseFont.Style)
            {
                toolStrip.Font = baseFont;
            }
        }
    }

    public void FreezeToolbarsForAction(Action action)
    {
        ToolStripPanel topPanel = toolPanel.TopToolStripPanel;
        NativeMethods.SendMessageW(topPanel.Handle, NativeMethods.WM_SETREDRAW, NativeMethods.FALSE, IntPtr.Zero);
        toolPanel.SuspendLayout();
        try
        {
            action();
        }
        finally
        {
            toolPanel.ResumeLayout(performLayout: true);
            NativeMethods.SendMessageW(topPanel.Handle, NativeMethods.WM_SETREDRAW, NativeMethods.TRUE, IntPtr.Zero);
            topPanel.Refresh();
            ApplyToolbarFontScaling();
        }
    }

    public void ReorganizeToolbars() => FreezeToolbarsForAction(ReorganizeToolbarsCore);

    private void ApplyToolbarFontScaling()
    {
        ToolbarLayoutConfig? config = AppSettings.ToolbarLayout;
        bool syncText = AppSettings.ToolbarSyncIconTextWithSize;

        (string Name, ToolStrip Strip)[] builtIn =
        [
            (StandardToolbarName, ToolStripMain),
            (FiltersToolbarName,  ToolStripFilters),
            (ScriptsToolbarName,  ToolStripScripts),
        ];

        foreach ((string name, ToolStrip ts) in builtIn)
        {
            if (ts.IsDisposed)
            {
                continue;
            }

            ToolbarMetadata? meta = config?.ToolbarsVisibility?.FirstOrDefault(t => t.Name == name);
            int iconSize = meta?.IconSize ?? 16;
            ApplyToolbarIconSize(ts, iconSize, syncText);
        }

        foreach (ToolStrip customTs in toolPanel.TopToolStripPanel.Controls
            .OfType<ToolStrip>()
            .Where(ts => ts.Name.StartsWith(CustomToolbarNamePrefix) && !ts.IsDisposed && !string.IsNullOrEmpty(ts.Text)))
        {
            CustomToolbarMetadata? customMeta = config?.CustomToolbars?.FirstOrDefault(c => c.Name == customTs.Text);
            int iconSize = customMeta?.IconSize ?? 16;
            ApplyToolbarIconSize(customTs, iconSize, syncText);
        }
    }

    // Builds the (toolbar, row, orderInRow) list from the saved config.
    // Custom toolbars are discovered by enumerating the live panel's controls.
    // True only when the user actually saved a toolbar customization. AppSettings.ToolbarLayout
    // returns an empty (non-null) config for a default install, which must be treated as "no layout".
    private static bool HasSavedToolbarLayout(ToolbarLayoutConfig? config)
        => config is not null
            && ((config.Items?.Count > 0)
                || (config.CustomToolbars?.Count > 0)
                || (config.ToolbarsVisibility?.Count > 0));

    private List<(ToolStrip ToolStrip, int Row, int OrderInRow)> BuildToolbarLayoutInfo(ToolbarLayoutConfig? config, string logPrefix)
    {
        LogToolbar($"[{logPrefix}] START - Config is null: {config is null}");
        if (config?.ToolbarsVisibility != null)
        {
            LogToolbar($"[{logPrefix}] ToolbarsVisibility count: {config.ToolbarsVisibility.Count}");
            foreach (ToolbarMetadata meta in config.ToolbarsVisibility)
            {
                LogToolbar($"[{logPrefix}] Toolbar '{meta.Name}': Row={meta.Row}, OrderInRow={meta.OrderInRow}, Visible={meta.Visible}");
            }
        }

        List<ToolStrip> customToolStrips = toolPanel.TopToolStripPanel.Controls
            .OfType<ToolStrip>()
            .Where(ts => ts.Name.StartsWith(CustomToolbarNamePrefix) && !ts.IsDisposed && !string.IsNullOrEmpty(ts.Text))
            .ToList();

        Dictionary<string, ToolStrip> toolStripsByName = BuildToolStripNameMap(customToolStrips);

        List<(ToolStrip ToolStrip, int Row, int OrderInRow)> layoutInfo = new();

        foreach ((string name, ToolStrip toolStrip) in toolStripsByName)
        {
            if (toolStrip.IsDisposed)
            {
                continue;
            }

            int row = 0;
            int orderInRow = GetDefaultOrderInRow(name);

            ToolbarMetadata? metadata = config?.ToolbarsVisibility?.FirstOrDefault(t => t.Name == name);
            if (metadata != null)
            {
                row = metadata.Row;
                orderInRow = metadata.OrderInRow;
                LogToolbar($"[{logPrefix}] Found '{name}' in ToolbarsVisibility: Row={row}, OrderInRow={orderInRow}");
            }
            else
            {
                CustomToolbarMetadata? customMeta = config?.CustomToolbars?.FirstOrDefault(c => c.Name == name);
                if (customMeta != null)
                {
                    row = customMeta.Row;
                    orderInRow = customMeta.OrderInRow;
                    LogToolbar($"[{logPrefix}] Found '{name}' in CustomToolbars: Row={row}, OrderInRow={orderInRow}");
                }
                else
                {
                    LogToolbar($"[{logPrefix}] '{name}' NOT FOUND in config, using defaults: Row={row}, OrderInRow={orderInRow}");
                }
            }

            layoutInfo.Add((toolStrip, row, orderInRow));
        }

        return layoutInfo;

        static int GetDefaultOrderInRow(string toolbarName) => toolbarName switch
        {
            StandardToolbarName => 0,
            FiltersToolbarName => 1,
            ScriptsToolbarName => 2,
            _ => 99 // Custom toolbars at the end
        };
    }

    // Builds the toolbar name -> ToolStrip map. Extracted to a method so the returned
    // dictionary is opaque to static analysis, which otherwise wrongly infers it is always
    // empty and flags the subsequent enumeration (S4158).
    private Dictionary<string, ToolStrip> BuildToolStripNameMap(IEnumerable<ToolStrip> customToolStrips)
    {
        Dictionary<string, ToolStrip> result = new()
        {
            { StandardToolbarName, ToolStripMain },
            { FiltersToolbarName, ToolStripFilters },
            { ScriptsToolbarName, ToolStripScripts }
        };

        foreach (ToolStrip customTs in customToolStrips)
        {
            result[customTs.Text] = customTs;
        }

        return result;
    }

    // Clears the panel and re-joins each visible toolbar at its configured Row / OrderInRow.
    // Uses a cumulative-X strategy because ToolStripPanel.Join silently relocates toolbars when
    // the requested Location.X falls before the right edge of an earlier toolbar on the same row.
    //
    // After each row is laid out, the optional rowFinalizer callback receives the row index and
    // the list of toolbars added to that row (with their IsCustom flag) for any post-processing
    // (e.g. ReorganizeToolbarsCore uses it to stretch custom toolbars that sit alone on a row).
    private void JoinToolbarsByRowOrder(
        IReadOnlyList<(ToolStrip ToolStrip, int Row, int OrderInRow)> layoutInfo,
        string logPrefix,
        Func<ToolStrip, bool>? isCustomFn = null,
        Action<int, IReadOnlyList<(ToolStrip ToolStrip, bool IsCustom)>>? rowFinalizer = null)
    {
        toolPanel.TopToolStripPanel.Controls.Clear();

        var groupedByRow = layoutInfo
            .Where(li => li.ToolStrip.Visible)
            .GroupBy(li => li.Row)
            .OrderBy(g => g.Key);

        int hintY = 0;

        foreach (var rowGroup in groupedByRow)
        {
            int row = rowGroup.Key;
            var orderedAscending = rowGroup.OrderBy(li => li.OrderInRow).ToList();
            LogToolbar($"[{logPrefix}] Processing Row {row} at hintY={hintY} with {orderedAscending.Count} toolbars (added in ascending OrderInRow at X=cumulativeWidth)");

            RowJoinState rowState = new(hintY);
            foreach (var item in orderedAscending)
            {
                JoinSingleToolbarInRow(item.ToolStrip, item.OrderInRow, row, rowState, isCustomFn, logPrefix);
            }

            int fallbackHeight = ToolStripMain.Height > 0 ? ToolStripMain.Height : 25;
            hintY = rowState.RowStrips
                .Select(s => s.ToolStrip.Location.Y + s.ToolStrip.Height)
                .DefaultIfEmpty((rowState.ActualRowY ?? hintY) + fallbackHeight)
                .Max();

            rowFinalizer?.Invoke(row, rowState.RowStrips);

            LogToolbar($"[{logPrefix}] Row {row} final order: {string.Join(" | ", rowState.RowStrips.Select(s => s.ToolStrip).OrderBy(ts => ts.Location.X).Select(ts => $"{ts.Name}@X={ts.Location.X},Y={ts.Location.Y}"))}, hintY={hintY}");
        }

        toolPanel.TopToolStripPanel.PerformLayout();
        LogPanelDiagnostic(logPrefix);
    }

    // Accumulates the running state of a row while JoinToolbarsByRowOrder joins its toolbars
    // one by one: the cumulative X position for the next toolbar, the row's actual Y position
    // (locked once the first toolbar is joined), and the joined toolbars collected so far.
    // Grouping these three values that always travel together keeps JoinSingleToolbarInRow's
    // parameter count within SonarCloud's limit (S107) without splitting its logic further.
    private sealed class RowJoinState(int hintY)
    {
        public int CurrentX { get; set; }
        public int? ActualRowY { get; set; }
        public int HintY => hintY;
        public List<(ToolStrip ToolStrip, bool IsCustom)> RowStrips { get; } = new();
    }

    // Joins a single toolbar at its cumulative X position within the row, correcting for any
    // stale free space left by Controls.Clear() (see JoinToolbarsByRowOrder remarks). Updates
    // rowState with the toolbar's contribution and the cumulative X for the next toolbar.
    private void JoinSingleToolbarInRow(
        ToolStrip toolStrip,
        int orderInRow,
        int row,
        RowJoinState rowState,
        Func<ToolStrip, bool>? isCustomFn,
        string logPrefix)
    {
        int targetY = rowState.ActualRowY ?? rowState.HintY;
        LogToolbar($"[{logPrefix}] Joining '{toolStrip.Name}' at ({rowState.CurrentX}, {targetY}) (OrderInRow={orderInRow})");
        toolPanel.TopToolStripPanel.Join(toolStrip, new Point(rowState.CurrentX, targetY));
        toolPanel.TopToolStripPanel.PerformLayout();

        // The panel row can retain stale free space from a toolbar that was on this row
        // before Controls.Clear() (e.g. one just hidden via the layout dialog). Join()
        // does not always shift the joined toolbar left to close that gap, so pin its
        // X explicitly to the cumulative position we are tracking.
        if (toolStrip.Location.X != rowState.CurrentX)
        {
            toolStrip.Location = new Point(rowState.CurrentX, toolStrip.Location.Y);
            toolPanel.TopToolStripPanel.PerformLayout();
            LogToolbar($"[{logPrefix}] Corrected '{toolStrip.Name}' X from stale position to {rowState.CurrentX}");
        }

        bool isCustom = isCustomFn?.Invoke(toolStrip) ?? false;
        rowState.RowStrips.Add((toolStrip, isCustom));

        if (rowState.ActualRowY is null)
        {
            rowState.ActualRowY = toolStrip.Location.Y;
            LogToolbar($"[{logPrefix}] Row {row} actual Y locked to {rowState.ActualRowY}");
        }

        int actualWidth = toolStrip.Width > 0 ? toolStrip.Width : toolStrip.PreferredSize.Width;
        rowState.CurrentX = toolStrip.Location.X + actualWidth;
    }

    private void LogPanelDiagnostic(string logPrefix)
    {
        var panelRows = toolPanel.TopToolStripPanel.Rows;
        LogToolbar($"[{logPrefix}] DIAGNOSTIC: ToolStripPanel has {panelRows.Length} ToolStripPanelRow(s)");
        for (int i = 0; i < panelRows.Length; i++)
        {
            var pr = panelRows[i];
            var ctrls = pr.Controls.OfType<ToolStrip>()
                .Select(ts => $"{ts.Name}@X={ts.Location.X},Y={ts.Location.Y},W={ts.Width},Stretch={ts.Stretch}")
                .ToArray();
            LogToolbar($"[{logPrefix}] DIAGNOSTIC: Row[{i}] Bounds={pr.Bounds} Controls=[{string.Join(" | ", ctrls)}]");
        }

        LogToolbar($"[{logPrefix}] DIAGNOSTIC: Panel ClientSize={toolPanel.TopToolStripPanel.ClientSize}, Width={toolPanel.TopToolStripPanel.Width}");
    }

    internal void ReorganizeToolbarsCore()
    {
        ToolbarLayoutConfig? config = AppSettings.ToolbarLayout;

        HashSet<ToolStrip> customToolStrips = toolPanel.TopToolStripPanel.Controls
            .OfType<ToolStrip>()
            .Where(ts => ts.Name.StartsWith(CustomToolbarNamePrefix) && !ts.IsDisposed && !string.IsNullOrEmpty(ts.Text))
            .ToHashSet();

        // Nothing to reorganize for a default install: with no saved layout and no custom toolbars,
        // re-joining the built-in toolbars would discard the designer's single-row placement and can
        // push a toolbar onto a second row, shrinking the content area. Keep the original layout.
        // Note: AppSettings.ToolbarLayout never returns null (it yields an empty config), so test
        // for emptiness rather than null.
        if (!HasSavedToolbarLayout(config) && customToolStrips.Count == 0)
        {
            LogToolbar("[ReorganizeToolbars] No saved layout and no custom toolbars, keep designer layout");
            return;
        }

        List<(ToolStrip ToolStrip, int Row, int OrderInRow)> layoutInfo = BuildToolbarLayoutInfo(config, "ReorganizeToolbars");

        int panelWidth = toolPanel.TopToolStripPanel.ClientSize.Width;

        JoinToolbarsByRowOrder(
            layoutInfo,
            "ReorganizeToolbars",
            isCustomFn: customToolStrips.Contains,
            rowFinalizer: (row, rowStrips) => StretchCustomToolbarsIfAlone(panelWidth, rowStrips));

        ReAddInvisibleCustomToolbars(layoutInfo, customToolStrips);

        Dictionary<string, ToolStrip> dynamicToolbars = customToolStrips.ToDictionary(ts => ts.Name);

        _formBrowseMenus.RefreshToolbarsMenu(dynamicToolbars.Count > 0 ? dynamicToolbars : null);

        RestoreSeparatorAndLabelVisibility();
    }

    // Re-add invisible custom toolbars to the panel so they remain discoverable
    // (ToolbarsSettingsPage enumerates TopToolStripPanel.Controls to find them).
    private void ReAddInvisibleCustomToolbars(List<(ToolStrip ToolStrip, int Row, int OrderInRow)> layoutInfo, HashSet<ToolStrip> customToolStrips)
    {
        foreach (var li in layoutInfo.Where(li => !li.ToolStrip.Visible && customToolStrips.Contains(li.ToolStrip)))
        {
            toolPanel.TopToolStripPanel.Controls.Add(li.ToolStrip);
        }
    }

    // WinForms resets Visible to false on ToolStripSeparators and ToolStripLabels (spacers)
    // when they are inserted into a ToolStrip whose Visible is false at insertion time.
    // Buttons recover on their own; separators and spacers do not, so restore only those.
    private void RestoreSeparatorAndLabelVisibility()
    {
        foreach (Control control in toolPanel.TopToolStripPanel.Controls)
        {
            if (control is not ToolStrip toolStrip)
            {
                continue;
            }

            foreach (ToolStripItem stripItem in toolStrip.Items)
            {
                if (stripItem is ToolStripSeparator || stripItem is ToolStripLabel)
                {
                    stripItem.Visible = true;
                }
            }
        }
    }

    // Only stretch custom toolbars when they are alone on a row. When a custom toolbar shares
    // a row with built-in toolbars, forcing its width to fill remaining space can push the
    // total row width above the panel width and cause WinForms to wrap toolbars onto separate
    // rows. Letting AutoSize/PreferredSize govern keeps every toolbar on the requested row.
    private static void StretchCustomToolbarsIfAlone(int panelWidth, IReadOnlyList<(ToolStrip ToolStrip, bool IsCustom)> rowStrips)
    {
        if (panelWidth <= 0)
        {
            return;
        }

        bool hasBuiltIn = rowStrips.Any(s => !s.IsCustom);
        int customCount = rowStrips.Count(s => s.IsCustom);
        if (hasBuiltIn || customCount == 0)
        {
            return;
        }

        int customWidth = panelWidth / customCount;
        if (customWidth <= 0)
        {
            return;
        }

        foreach ((ToolStrip ts, bool isCustom) in rowStrips)
        {
            if (isCustom)
            {
                ts.Width = customWidth;
                LogToolbar($"[ReorganizeToolbars] Set custom width {customWidth} on {ts.Name}");
            }
        }
    }

    private void UpdateTooltipWithShortcut(ToolStripItem button, Command command)
        => UpdateTooltipWithShortcut(button, GetShortcutKeys(command));

    private static void UpdateTooltipWithShortcut(ToolStripItem button, Keys keys)
        => button.ToolTipText = button.ToolTipText!.UpdateSuffix(keys.ToShortcutKeyToolTipString());

    private void InsertFetchPullShortcuts()
    {
        int i = ToolStripMain.Items.IndexOf(toolStripButtonPull);
        ToolStripButton btn1 = CreateCorrespondingToolbarButton(fetchToolStripMenuItem, Command.QuickFetch);
        ToolStripButton btn2 = CreateCorrespondingToolbarButton(fetchAllToolStripMenuItem);
        ToolStripButton btn3 = CreateCorrespondingToolbarButton(fetchPruneAllToolStripMenuItem);
        ToolStripButton btn4 = CreateCorrespondingToolbarButton(mergeToolStripMenuItem, Command.QuickPull);
        ToolStripButton btn5 = CreateCorrespondingToolbarButton(rebaseToolStripMenuItem1);
        ToolStripButton btn6 = CreateCorrespondingToolbarButton(pullToolStripMenuItem1, Command.PullOrFetch);

        ToolStripMain.Items.Insert(i++, btn1);
        ToolStripMain.Items.Insert(i++, btn2);
        ToolStripMain.Items.Insert(i++, btn3);
        ToolStripMain.Items.Insert(i++, btn4);
        ToolStripMain.Items.Insert(i++, btn5);
        ToolStripMain.Items.Insert(i, btn6);

        // Store newly created items in the original items dictionary
        _originalToolbarItems[btn1.Name!] = btn1;
        _originalToolbarItems[btn2.Name!] = btn2;
        _originalToolbarItems[btn3.Name!] = btn3;
        _originalToolbarItems[btn4.Name!] = btn4;
        _originalToolbarItems[btn5.Name!] = btn5;
        _originalToolbarItems[btn6.Name!] = btn6;

        ToolStripButton CreateCorrespondingToolbarButton(ToolStripMenuItem toolStripMenuItem, Command? command = null)
        {
            string toolTipText = toolStripMenuItem.Text!.Replace("&", string.Empty);
            ToolStripButton clonedToolStripMenuItem = new()
            {
                Image = toolStripMenuItem.Image,
                Name = FetchPullToolbarShortcutsPrefix + toolStripMenuItem.Name,
                Size = toolStripMenuItem.Size,
                Text = toolTipText,
                ToolTipText = toolTipText.UpdateSuffix(command.HasValue ? GetShortcutKeyTooltipString(command.Value) : null!),
                DisplayStyle = ToolStripItemDisplayStyle.Image,
            };

            clonedToolStripMenuItem.Click += (_, _) => toolStripMenuItem.PerformClick();
            return clonedToolStripMenuItem;
        }
    }

    private void FillNextPullActionAsDefaultToolStripMenuItems()
    {
        ToolStripDropDownMenu setDefaultPullActionDropDown = (ToolStripDropDownMenu)setDefaultPullButtonActionToolStripMenuItem.DropDown;

        // Show both Check and Image margins in a menu
        setDefaultPullActionDropDown.ShowImageMargin = true;
        setDefaultPullActionDropDown.ShowCheckMargin = true;

        // Prevent submenu from closing while options are changed
        setDefaultPullActionDropDown.Closing += (sender, args) =>
        {
            if (args.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                args.Cancel = true;
            }
        };

        IEnumerable<ToolStripItem> setDefaultPullActionDropDownItems = toolStripButtonPull.DropDownItems
            .OfType<ToolStripMenuItem>()
            .Where(tsmi => tsmi.Tag is GitPullAction)
            .Select(tsmi =>
            {
                ToolStripItem tsi = new ToolStripMenuItem
                {
                    Name = $"{tsmi.Name}SetDefault",
                    Text = tsmi.Text,
                    CheckOnClick = true,
                    Image = tsmi.Image,
                    Tag = tsmi.Tag
                };

                tsi.Click += SetDefaultPullActionMenuItemClick;

                return tsi;
            });

        setDefaultPullActionDropDown.Items.AddRange([.. setDefaultPullActionDropDownItems]);

        void SetDefaultPullActionMenuItemClick(object? sender, EventArgs eventArgs)
        {
            ToolStripMenuItem clickedMenuItem = (ToolStripMenuItem)sender!;
            AppSettings.DefaultPullAction = (GitPullAction)clickedMenuItem.Tag!;
            RefreshDefaultPullAction();
        }
    }

    private void FillUserShells(string defaultShell)
    {
        userShell.DropDownItems.Clear();

        bool userShellAccessible = false;
        ToolStripMenuItem? selectedDefaultShell = null;
        foreach (IShellDescriptor shell in _shellProvider.GetShells())
        {
            if (!shell.HasExecutable)
            {
                continue;
            }

            ToolStripMenuItem toolStripMenuItem = new(shell.Name);
            userShell.DropDownItems.Add(toolStripMenuItem);
            toolStripMenuItem.Tag = shell;
            toolStripMenuItem.Image = shell.Icon;
            toolStripMenuItem.ToolTipText = shell.Name;
            toolStripMenuItem.Click += userShell_Click;

            if (selectedDefaultShell is null || string.Equals(shell.Name, defaultShell, StringComparison.InvariantCultureIgnoreCase))
            {
                userShellAccessible = true;
                selectedDefaultShell = toolStripMenuItem;
            }
        }

        if (selectedDefaultShell is not null)
        {
            userShell.Image = selectedDefaultShell.Image;
            userShell.ToolTipText = selectedDefaultShell.ToolTipText;
            userShell.Tag = selectedDefaultShell.Tag;
        }

        userShell.Visible = userShell.DropDownItems.Count > 0;

        // a user may have a specific shell configured in settings, but the shell is no longer available
        // set the first available shell as default
        if (userShell.Visible && !userShellAccessible)
        {
            IShellDescriptor shell = (IShellDescriptor)userShell.DropDownItems[0].Tag!;
            userShell.Image = shell.Icon;
            userShell.ToolTipText = shell.Name;
            userShell.Tag = shell;
        }
    }

    private void RefreshDefaultPullAction()
    {
        if (setDefaultPullButtonActionToolStripMenuItem is null)
        {
            // We may get called while instantiating the form
            return;
        }

        GitPullAction defaultPullAction = AppSettings.DefaultPullAction;

        foreach (ToolStripMenuItem menuItem in setDefaultPullButtonActionToolStripMenuItem.DropDown.Items)
        {
            menuItem.Checked = (GitPullAction)menuItem.Tag! == defaultPullAction;
        }

        switch (defaultPullAction)
        {
            case GitPullAction.Fetch:
                toolStripButtonPull.Image = fetchToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullFetch.Text;
                break;

            case GitPullAction.FetchAll:
                toolStripButtonPull.Image = fetchAllToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullFetchAll.Text;
                break;

            case GitPullAction.FetchPruneAll:
                toolStripButtonPull.Image = fetchPruneAllToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullFetchPruneAll.Text;
                break;

            case GitPullAction.Merge:
                toolStripButtonPull.Image = mergeToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullMerge.Text;
                break;

            case GitPullAction.Rebase:
                toolStripButtonPull.Image = rebaseToolStripMenuItem1.Image;
                toolStripButtonPull.ToolTipText = _pullRebase.Text;
                break;

            default:
                toolStripButtonPull.Image = pullToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullOpenDialog.Text;
                break;
        }

        // Assign Text after Image so that TextChanged fires with Image already updated.
        // Clones on custom toolbars subscribe to TextChanged to stay in sync (CloneItem).
        toolStripButtonPull.Text = toolStripButtonPull.ToolTipText;

        UpdateTooltipWithShortcut(toolStripButtonPull, Command.QuickPullOrFetch);
    }

    /// <summary>
    ///  Hides "Fetch all" item when there is only one remote,
    ///  since it is redundant with the single-remote "Fetch" command.
    /// </summary>
    private void UpdateFetchAllVisibility()
    {
        bool hasMultipleRemotes = Module.IsValidGitWorkingDir() && Module.GetRemoteNames().Count > 1;

        // Toolbar button drop down menu
        fetchAllToolStripMenuItem.Visible = hasMultipleRemotes;

        // Update the "set default pull action" submenu items
        if (setDefaultPullButtonActionToolStripMenuItem.DropDown is ToolStripDropDownMenu setDefaultMenu)
        {
            foreach (ToolStripItem item in setDefaultMenu.Items)
            {
                if (item.Tag is GitPullAction.FetchAll)
                {
                    item.Visible = hasMultipleRemotes;
                }
            }
        }
    }

    private Brush UpdateCommitButtonAndGetBrush(IReadOnlyList<GitItemStatus>? status, bool showCount)
    {
        try
        {
            ToolStripMain.SuspendLayout();
            RepoStateVisualiser repoStateVisualiser = new();
            (Image image, Brush brush) = repoStateVisualiser.Invoke(status);

            if (showCount)
            {
                toolStripButtonCommit.Image = image;

                if (status is not null)
                {
                    toolStripButtonCommit.Text = $"{_commitButtonText} ({status.Count})";
                    toolStripButtonCommit.AutoSize = true;
                }
                else
                {
                    int width = toolStripButtonCommit.Width;
                    toolStripButtonCommit.Text = _commitButtonText.Text;
                    if (width > toolStripButtonCommit.Width)
                    {
                        toolStripButtonCommit.AutoSize = false;
                        toolStripButtonCommit.Width = width;
                    }
                }
            }
            else
            {
                toolStripButtonCommit.Image = repoStateVisualiser.Invoke(new List<GitItemStatus>()).image;

                toolStripButtonCommit.Text = _commitButtonText.Text;
                toolStripButtonCommit.AutoSize = true;
            }

            return brush;
        }
        finally
        {
            ToolStripMain.ResumeLayout();
        }
    }
}
