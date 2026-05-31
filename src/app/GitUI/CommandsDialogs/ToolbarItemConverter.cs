using System.Reflection;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs.Menus;

namespace GitUI.CommandsDialogs;

/// <summary>
/// Converts a <see cref="ToolStripMenuItem"/> from a menu into a <see cref="ToolStripButton"/>
/// or <see cref="ToolStripSplitButton"/> suitable for placement on a custom toolbar.
/// </summary>
/// <remarks>
/// Shared by <see cref="FormBrowse"/> (startup load) and
/// <see cref="SettingsDialog.Pages.ToolbarsSettingsPage"/> (reload after Settings - Toolbars
/// changes) so both code paths produce identical buttons. Previously each site had its own
/// copy with subtle behavioural drift.
/// </remarks>
internal static class ToolbarItemConverter
{
    // The main-menu "Navigate" and "View" menus share their Name with icon-bearing duplicates in the
    // RevisionGrid context menu (GotoCommit / AdvancedSettings). When a custom toolbar resolves them
    // by name at startup the menu-bar copies are not inserted yet, so resolution falls through to the
    // context-menu duplicates and picks up their category icon, which is not meaningful as a toolbar
    // button. Force the neutral default icon for these so they match other menu buttons (e.g. Start).
    private static readonly HashSet<string> _menuNamesUsingDefaultIcon =
        new(StringComparer.Ordinal) { "navigateToolStripMenuItem", "viewToolStripMenuItem" };

    private static Image ResolveButtonImage(ToolStripMenuItem menuItem)
        => menuItem.Image is { } image && !_menuNamesUsingDefaultIcon.Contains(menuItem.Name ?? string.Empty)
            ? image
            : Properties.Images.ApplicationBlue;

    // Raises DropDownOpening on the target item to force population of dynamic sub-menus
    // (plugins, recent repos, scripts) without visually showing the dropdown window.
    // ToolStripDropDownItem.OnDropDownOpening is a protected virtual stable since .NET 2.0.
    // Falls back to ShowDropDown/HideDropDown if reflection is unavailable.
    private static readonly MethodInfo? _onDropDownOpening =
        typeof(ToolStripDropDownItem).GetMethod(
            "OnDropDownOpening",
            BindingFlags.NonPublic | BindingFlags.Instance);

    private static void TriggerDropDownPopulation(ToolStripDropDownItem item, Action<string>? log)
    {
        if (_onDropDownOpening is not null)
        {
            try
            {
                _onDropDownOpening.Invoke(item, [EventArgs.Empty]);
                return;
            }
            catch (Exception ex)
            {
                log?.Invoke($"[ToolbarItemConverter] OnDropDownOpening reflection failed for {item.Name}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        try
        {
            item.ShowDropDown();
            item.HideDropDown();
        }
        catch (Exception ex)
        {
            log?.Invoke($"[ToolbarItemConverter] ShowDropDown/HideDropDown threw for {item.Name}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    // ToolStripItem stores its MouseUp subscribers in the EventHandlerList keyed by a private
    // static field. We invoke that delegate directly to "replay" a right-click on the original
    // item. Calling the protected OnMouseUp (or internal HandleMouseUp) does NOT raise the MouseUp
    // event for a right-click (verified), whereas invoking the registered delegate does, with no
    // side effect on ButtonClick. Field name is s_mouseUpEvent on modern .NET, EventMouseUp on
    // legacy; if neither is found (future runtime), forwarding is silently skipped.
    private static readonly FieldInfo? _mouseUpEventKey =
        typeof(ToolStripItem).GetField("s_mouseUpEvent", BindingFlags.NonPublic | BindingFlags.Static)
        ?? typeof(ToolStripItem).GetField("EventMouseUp", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly PropertyInfo? _componentEvents =
        typeof(System.ComponentModel.Component).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);

    // Replays a right-click on the original toolbar item so MouseUp handlers wired outside this
    // converter (e.g. branchSelect_MouseUp -> Checkout branch, WorkingDirectory -> Open repository)
    // also fire on clones placed on custom toolbars. Restricted to the right button: the left
    // button is already forwarded through ButtonClick / Click, and the known handlers only act on
    // a right-click. Generic by design — any future right-click action benefits automatically.
    private static void ForwardRightClickToOriginal(ToolStripItem original, MouseEventArgs e, Action<string>? log)
    {
        if (e.Button != MouseButtons.Right || _mouseUpEventKey is null || _componentEvents is null)
        {
            return;
        }

        try
        {
            if (_componentEvents.GetValue(original) is System.ComponentModel.EventHandlerList events
                && _mouseUpEventKey.GetValue(null) is object key
                && events[key] is MouseEventHandler handler)
            {
                handler(original, e);
            }
        }
        catch (Exception ex)
        {
            log?.Invoke($"[ToolbarItemConverter] ForwardRightClickToOriginal failed for {original.Name}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    // Creates a dropdown menu pre-configured with the Professional renderer BEFORE it is attached
    // to a split button. Once attached to a ToolStripEx parent, the auto-created DropDown silently
    // mirrors the parent's system renderer and rejects later Renderer assignments — so checkable
    // items (e.g. the View category) would lose their blue check highlight.
    // Pre-configuring our own dropdown makes the Professional renderer survive parent attachment.
    private static ToolStripDropDownMenu CreateStyledDropDown()
        => new()
        {
            ShowCheckMargin = false,
            ShowImageMargin = true,
            Renderer = new ToolStripProfessionalRenderer(),
            Font = GitCommands.AppSettings.MenuFont
        };

    public static ToolStripItem Convert(
        ToolStripMenuItem menuItem,
        ToolStripItemDisplayStyle displayStyle,
        Dictionary<string, ToolStripItem>? storeIn = null,
        Action<string>? log = null)
    {
        if (menuItem.HasDropDownItems)
        {
            return BuildSplitButton(menuItem, displayStyle, storeIn, log);
        }

        return BuildButton(menuItem, displayStyle, storeIn, log);
    }

    private static ToolStripSplitButton BuildSplitButton(
        ToolStripMenuItem menuItem,
        ToolStripItemDisplayStyle displayStyle,
        Dictionary<string, ToolStripItem>? storeIn,
        Action<string>? log)
    {
        ToolStripSplitButton splitButton = new()
        {
            Name = $"btn_{menuItem.Name}",
            Text = menuItem.Text,
            Image = ResolveButtonImage(menuItem),
            ToolTipText = string.IsNullOrEmpty(menuItem.ToolTipText) ? menuItem.Text?.Replace("&", "") ?? string.Empty : menuItem.ToolTipText,
            DisplayStyle = displayStyle,
            ImageTransparentColor = menuItem.ImageTransparentColor,
            Tag = menuItem,
            Enabled = menuItem.Enabled,
            Visible = menuItem.Visible
        };

        ToolStripDropDownMenu splitDropDown = CreateStyledDropDown();
        splitButton.DropDown = splitDropDown;

        CloneDropDownItems(splitButton, menuItem, log);

        splitButton.DropDownOpening += (s, e) =>
        {
            splitDropDown.Font = GitCommands.AppSettings.MenuFont;
            log?.Invoke($"[ToolbarItemConverter] DropDownOpening for {splitButton.Name}: Renderer={splitDropDown.Renderer.GetType().Name}, source={menuItem.Name}, sourceItems={menuItem.DropDownItems.Count}");

            // Trigger DropDownOpening on the source menu item to force population of dynamic
            // items (plugins, scripts) without making the dropdown window visible.
            TriggerDropDownPopulation(menuItem, log);

            splitButton.DropDownItems.Clear();
            CloneDropDownItems(splitButton, menuItem, log);
        };

        splitButton.ButtonClick += (s, e) => splitButton.ShowDropDown();

        Store(splitButton, storeIn, log);
        return splitButton;
    }

    private static ToolStripButton BuildButton(
        ToolStripMenuItem menuItem,
        ToolStripItemDisplayStyle displayStyle,
        Dictionary<string, ToolStripItem>? storeIn,
        Action<string>? log)
    {
        ToolStripButton button = new()
        {
            Name = $"btn_{menuItem.Name}",
            Text = menuItem.Text,
            Image = ResolveButtonImage(menuItem),
            ToolTipText = string.IsNullOrEmpty(menuItem.ToolTipText) ? menuItem.Text?.Replace("&", "") ?? string.Empty : menuItem.ToolTipText,
            DisplayStyle = displayStyle,
            ImageTransparentColor = menuItem.ImageTransparentColor,
            Tag = menuItem,
            Enabled = menuItem.Enabled,
            Visible = menuItem.Visible,
            Checked = menuItem.Checked
        };

        button.Click += (s, e) =>
        {
            log?.Invoke($"[ToolbarItemConverter] Button {button.Name} clicked, triggering {menuItem.Name}");
            try
            {
                menuItem.PerformClick();
                button.Checked = menuItem.Checked;
            }
            catch (Exception ex)
            {
                log?.Invoke($"[ToolbarItemConverter] ERROR triggering {menuItem.Name}: {ex.Message}");
            }
        };

        // Mirror Checked state changes triggered by any source (menu bar, keyboard shortcut,
        // or another toolbar button targeting the same action). MenuCommand.SetCheckForRegisteredMenuItems
        // updates menuItem.Checked whenever TriggerMenuChanged fires, which covers all toggle paths.
        void SyncChecked(object? s, EventArgs e) => button.Checked = menuItem.Checked;
        menuItem.CheckedChanged += SyncChecked;
        button.Disposed += (s, e) => menuItem.CheckedChanged -= SyncChecked;

        Store(button, storeIn, log);
        return button;
    }

    private static void CloneDropDownItems(ToolStripSplitButton splitButton, ToolStripMenuItem menuItem, Action<string>? log)
        => PopulateDropDownItems(splitButton.DropDownItems, menuItem, log);

    private static void PopulateDropDownItems(ToolStripItemCollection target, ToolStripMenuItem menuItem, Action<string>? log)
    {
        foreach (ToolStripItem dropDownItem in menuItem.DropDownItems)
        {
            if (dropDownItem is ToolStripMenuItem subMenuItem)
            {
                target.Add(CloneMenuItemRecursive(subMenuItem, log));
            }
            else if (dropDownItem is ToolStripSeparator)
            {
                target.Add(new ToolStripSeparator());
            }
        }
    }

    private static void PopulateDropDownItemsFromCollection(ToolStripItemCollection target, ToolStripItemCollection source, ToolStripDropDownItem owner, Action<string>? log)
    {
        foreach (ToolStripItem item in source)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                target.Add(CloneMenuItemRecursive(menuItem, log));
            }
            else if (item is ToolStripSeparator)
            {
                target.Add(new ToolStripSeparator());
            }
            else if (item is ToolStripTextBox textBox)
            {
                // The source dropdown can carry a search box (e.g. the "Change working directory"
                // split button). It cannot be re-parented (single Owner constraint), so recreate a
                // fresh, fully-wired one bound to the clone's own dropdown.
                target.Add(WorkingDirectoryToolStripSplitButton.CreateFilterTextBox(
                    owner, textBox.TextBox?.PlaceholderText ?? string.Empty));
            }
        }
    }

    // Sizes the clone's search box to match the original's behaviour (avoid the dropdown growing
    // wider than its items). Mirrors the sizing done in WorkingDirectoryToolStripSplitButton.FillDropDown.
    private static void ResizeFilterTextBox(ToolStripDropDownItem owner)
    {
        ToolStripTextBox? filter = owner.DropDownItems.OfType<ToolStripTextBox>().FirstOrDefault();
        if (filter is null)
        {
            return;
        }

        int maxWidth = owner.DropDownItems.Cast<ToolStripItem>().Max(item => item == filter ? 0 : item.Width);
        const int paddingToAvoidGrowth = 60;
        filter.Size = new Size(maxWidth - DpiUtil.Scale(paddingToAvoidGrowth), filter.Height);
    }

    // Clones a ToolStripMenuItem into a new instance suitable for display inside a toolbar
    // dropdown. If the source has its own DropDownItems (e.g. PuTTY, Recent repositories),
    // the clone is populated with a nested sub-menu that mirrors the original hierarchy, so
    // the expand arrow and child items are preserved.
    private static ToolStripMenuItem CloneMenuItemRecursive(ToolStripMenuItem source, Action<string>? log)
    {
        ToolStripMenuItem clone = new()
        {
            Name = source.Name,
            Text = source.Text,
            Image = source.Image,

            // Preserve the "exclude from filter" marker so fixed commands (favourites, Open,
            // Close, Configure) stay visible while the search box filters recent repositories.
            // Other items keep a reference to their source (used by callers as informational Tag).
            Tag = ReferenceEquals(source.Tag, WorkingDirectoryToolStripSplitButton.ExcludeFromFilterMarker)
                ? source.Tag
                : source,
            Checked = source.Checked,
            Enabled = source.Enabled,

            // Force Visible=true: items sourced from context menus or sub-menus that
            // have never been displayed report Visible=false even when functionally
            // available. Cloning their flag would yield an empty dropdown.
            Visible = true
        };

        if (source.HasDropDownItems)
        {
            // Fill the nested sub-menu now, at parent-open / build time. The whole clone tree is
            // rebuilt every time the toolbar dropdown opens (see BuildSplitButton), so triggering
            // the source's DropDownOpening here lets dynamic sub-menus (Recent repositories, etc.)
            // expose their current items before we clone them — no per-hover refresh needed.
            //
            // We deliberately do NOT rebuild from the clone's own DropDownOpening. Calling Clear()
            // on a nested sub-menu while its parent dropdown is on screen tears down the live
            // dropdown chain and closes the parent menu (observed on custom toolbars when hovering
            // "Recent repositories" or "Git maintenance"). The in-place "Change working directory"
            // split button fills its sub-menus the same way and leaves them untouched on hover.
            TriggerDropDownPopulation(source, log);
            PopulateSubMenuClone(clone, source, log);
        }
        else
        {
            clone.Click += (s, e) =>
            {
                log?.Invoke($"[ToolbarItemConverter] Dropdown item {clone.Name} clicked, triggering {source.Name}");
                try
                {
                    source.PerformClick();
                }
                catch (Exception ex)
                {
                    log?.Invoke($"[ToolbarItemConverter] ERROR triggering dropdown item {source.Name}: {ex.Message}");
                }
            };
        }

        return clone;
    }

    private static void PopulateSubMenuClone(ToolStripMenuItem cloneParent, ToolStripMenuItem sourceParent, Action<string>? log)
    {
        foreach (ToolStripItem child in sourceParent.DropDownItems)
        {
            if (child is ToolStripMenuItem childMenuItem)
            {
                cloneParent.DropDownItems.Add(CloneMenuItemRecursive(childMenuItem, log));
            }
            else if (child is ToolStripSeparator)
            {
                cloneParent.DropDownItems.Add(new ToolStripSeparator());
            }
        }
    }

    /// <summary>
    /// Clones a toolbar item produced by <see cref="Convert"/> so that the same logical action
    /// can live on more than one toolbar simultaneously. A <see cref="ToolStripItem"/> can only
    /// have a single <see cref="ToolStripItem.Owner"/>, so placing it on a second toolbar
    /// requires a delegate clone that forwards all interactions to the original.
    /// </summary>
    /// <param name="wantsText">
    /// Whether this clone should display its text label. When <see langword="false"/>, the clone
    /// stays in <see cref="ToolStripItemDisplayStyle.Image"/> mode regardless of what the original
    /// does (e.g. the Push button switching to ImageAndText while showing ahead/behind data).
    /// When <see langword="true"/>, the clone mirrors the original's DisplayStyle exactly.
    /// </param>
    public static ToolStripItem CloneItem(ToolStripItem original, bool wantsText = false)
    {
        string cloneName = $"clone_{original.Name}";

        return original switch
        {
            ToolStripSplitButton splitOriginal => CloneSplitButton(splitOriginal, cloneName, wantsText),
            ToolStripDropDownButton dropOriginal => CloneDropDownButton(dropOriginal, cloneName, wantsText),

            // ToolStripPushButton gets its own clone class so each toolbar instance carries
            // independent ShowLabel / LabelText state (Option 1). This avoids the global-state
            // problem where ApplyItemDisplayStyle on a plain clone would redirect to the original.
            ToolStripPushButton pushOriginal => new ToolStripPushButtonClone(pushOriginal, showLabel: wantsText),
            _ => CloneButton((ToolStripButton)original, cloneName, wantsText)
        };
    }

    private static ToolStripItem CloneSplitButton(ToolStripSplitButton splitOriginal, string cloneName, bool wantsText)
    {
        ToolStripSplitButton clone = new()
        {
            Name = cloneName,
            Text = splitOriginal.Text,
            Image = splitOriginal.Image,
            ToolTipText = splitOriginal.ToolTipText,
            DisplayStyle = wantsText ? splitOriginal.DisplayStyle : ToolStripItemDisplayStyle.Image,
            ImageTransparentColor = splitOriginal.ImageTransparentColor,
            Enabled = splitOriginal.Enabled,
            Visible = true,
            Tag = splitOriginal
        };

        // Use the same pre-configured Professional-renderer dropdown as BuildSplitButton so a
        // cloned split button (e.g. a View action placed on a second toolbar) keeps the blue
        // check highlight for checkable items.
        clone.DropDown = CreateStyledDropDown();

        void SyncSplitFromOriginal(object? s, EventArgs e)
        {
            clone.Text = splitOriginal.Text;
            clone.Image = splitOriginal.Image;
            clone.ToolTipText = splitOriginal.ToolTipText;
            clone.DisplayStyle = wantsText ? splitOriginal.DisplayStyle : ToolStripItemDisplayStyle.Image;
            clone.Enabled = splitOriginal.Enabled;
            clone.ImageAlign = splitOriginal.ImageAlign;
        }

        splitOriginal.TextChanged += SyncSplitFromOriginal;
        splitOriginal.DisplayStyleChanged += SyncSplitFromOriginal;
        splitOriginal.EnabledChanged += SyncSplitFromOriginal;
        clone.Disposed += (s, e) =>
        {
            splitOriginal.TextChanged -= SyncSplitFromOriginal;
            splitOriginal.DisplayStyleChanged -= SyncSplitFromOriginal;
            splitOriginal.EnabledChanged -= SyncSplitFromOriginal;
        };

        clone.ButtonClick += (s, e) =>
        {
            // Split buttons converted from a menu (Tag is the source ToolStripMenuItem) have no
            // real button action other than opening their menu. Replaying that on the original
            // from here does not surface its dropdown (the original is anchored to its own toolbar
            // and the menu never becomes visible), so open the clone's own dropdown directly.
            if (splitOriginal.Tag is ToolStripMenuItem)
            {
                clone.ShowDropDown();
                return;
            }

            splitOriginal.PerformButtonClick();

            // If PerformButtonClick caused the original's dropdown to open (e.g. no
            // superproject available so the handler falls back to ShowDropDown), redirect
            // the opening to the clone so the menu appears beneath the clicked button.
            if (splitOriginal.DropDown.Visible)
            {
                splitOriginal.HideDropDown();
                clone.ShowDropDown();
            }
        };
        clone.DropDownOpening += (s, e) =>
        {
            clone.DropDown.Font = GitCommands.AppSettings.MenuFont;

            // Rebuild from the source ToolStripMenuItem rather than moving items from
            // splitOriginal. AddRange re-parents items (WinForms single-owner constraint),
            // which would empty splitOriginal.DropDownItems after each clone open.
            if (splitOriginal.Tag is ToolStripMenuItem sourceMenuItem)
            {
                TriggerDropDownPopulation(sourceMenuItem, null);
                clone.DropDownItems.Clear();
                CloneDropDownItems(clone, sourceMenuItem, null);
            }
            else
            {
                // Fallback for SplitButtons whose DropDownItems are populated directly
                // (not via a ToolStripMenuItem Tag), e.g. the "Change working directory" split
                // button or toolStripButtonLevelUp (Submodules). Trigger the original's own
                // DropDownOpening first so its (possibly dynamic) items are current before we copy.
                TriggerDropDownPopulation(splitOriginal, null);

                if (splitOriginal.DropDownItems.Count > 0)
                {
                    clone.DropDownItems.Clear();
                    PopulateDropDownItemsFromCollection(clone.DropDownItems, splitOriginal.DropDownItems, clone, null);
                    ResizeFilterTextBox(clone);
                }
            }
        };

        // Forward right-clicks to the original so handlers wired outside this converter
        // (e.g. branchSelect_MouseUp -> Checkout branch, WorkingDirectory -> Open repository)
        // also work on the clone. Left-click is already handled via ButtonClick above.
        clone.MouseUp += (s, e) => ForwardRightClickToOriginal(splitOriginal, e, null);

        return clone;
    }

    private static ToolStripItem CloneDropDownButton(ToolStripDropDownButton dropOriginal, string cloneName, bool wantsText)
    {
        ToolStripDropDownButton clone = new()
        {
            Name = cloneName,
            Text = dropOriginal.Text,
            Image = dropOriginal.Image,
            ToolTipText = dropOriginal.ToolTipText,
            DisplayStyle = wantsText ? dropOriginal.DisplayStyle : ToolStripItemDisplayStyle.Image,
            ImageTransparentColor = dropOriginal.ImageTransparentColor,
            Enabled = dropOriginal.Enabled,
            Visible = true,
            Tag = dropOriginal
        };

        void SyncDropFromOriginal(object? s, EventArgs e)
        {
            clone.Text = dropOriginal.Text;
            clone.Image = dropOriginal.Image;
            clone.ToolTipText = dropOriginal.ToolTipText;
            clone.DisplayStyle = wantsText ? dropOriginal.DisplayStyle : ToolStripItemDisplayStyle.Image;
            clone.Enabled = dropOriginal.Enabled;
            clone.ImageAlign = dropOriginal.ImageAlign;
        }

        dropOriginal.TextChanged += SyncDropFromOriginal;
        dropOriginal.DisplayStyleChanged += SyncDropFromOriginal;
        dropOriginal.EnabledChanged += SyncDropFromOriginal;
        clone.Disposed += (s, e) =>
        {
            dropOriginal.TextChanged -= SyncDropFromOriginal;
            dropOriginal.DisplayStyleChanged -= SyncDropFromOriginal;
            dropOriginal.EnabledChanged -= SyncDropFromOriginal;
        };

        // Convert never produces ToolStripDropDownButton (only SplitButton), so this branch
        // is defensive code for external callers. Tag layout varies: direct convert has
        // Tag = ToolStripMenuItem; a clone-of-clone has Tag = ToolStripDropDownButton whose
        // own Tag is the ToolStripMenuItem. Support both.
        ToolStripMenuItem? resolveSourceMenuItem()
            => dropOriginal.Tag is ToolStripMenuItem m ? m
             : (dropOriginal.Tag as ToolStripDropDownButton)?.Tag as ToolStripMenuItem;

        clone.DropDownOpening += (s, e) =>
        {
            clone.DropDown.Font = GitCommands.AppSettings.MenuFont;

            // Rebuild from the source ToolStripMenuItem rather than moving items from
            // dropOriginal. AddRange re-parents items (WinForms single-owner constraint),
            // which would empty dropOriginal.DropDownItems after each clone open.
            ToolStripMenuItem? sourceMenuItem = resolveSourceMenuItem();
            if (sourceMenuItem is not null)
            {
                TriggerDropDownPopulation(sourceMenuItem, null);
                clone.DropDownItems.Clear();
                PopulateDropDownItems(clone.DropDownItems, sourceMenuItem, null);
            }
        };
        return clone;
    }

    private static ToolStripItem CloneButton(ToolStripButton btnOriginal, string cloneName, bool wantsText)
    {
        // Default: ToolStripButton
        ToolStripButton btnClone = new()
        {
            Name = cloneName,
            Text = btnOriginal.Text,
            Image = btnOriginal.Image,
            ToolTipText = btnOriginal.ToolTipText,
            DisplayStyle = wantsText ? btnOriginal.DisplayStyle : ToolStripItemDisplayStyle.Image,
            ImageTransparentColor = btnOriginal.ImageTransparentColor,
            AutoSize = btnOriginal.AutoSize,
            ImageAlign = btnOriginal.ImageAlign,
            Enabled = btnOriginal.Enabled,
            Visible = true,
            Checked = btnOriginal.Checked,
            Tag = btnOriginal
        };
        btnClone.Click += (s, e) => btnOriginal.PerformClick();

        // Keep visual properties in sync when the original updates (e.g. toggle state changed
        // via menu bar / keyboard shortcut). wantsText controls whether this clone shows text.
        void SyncFromOriginal(object? s, EventArgs e)
        {
            btnClone.Text = btnOriginal.Text;
            btnClone.Image = btnOriginal.Image;
            btnClone.ToolTipText = btnOriginal.ToolTipText;
            btnClone.DisplayStyle = wantsText ? btnOriginal.DisplayStyle : ToolStripItemDisplayStyle.Image;
            btnClone.AutoSize = btnOriginal.AutoSize;
            btnClone.ImageAlign = btnOriginal.ImageAlign;
        }

        void SyncCheckedFromOriginal(object? s, EventArgs e) => btnClone.Checked = btnOriginal.Checked;

        btnOriginal.TextChanged += SyncFromOriginal;
        btnOriginal.DisplayStyleChanged += SyncFromOriginal;
        btnOriginal.CheckedChanged += SyncCheckedFromOriginal;
        btnClone.Disposed += (s, e) =>
        {
            btnOriginal.TextChanged -= SyncFromOriginal;
            btnOriginal.DisplayStyleChanged -= SyncFromOriginal;
            btnOriginal.CheckedChanged -= SyncCheckedFromOriginal;
        };

        // Forward right-clicks to the original (see split-button branch above). No plain
        // ToolStripButton carries a MouseUp handler today, but this keeps clones at parity with
        // the original for any future right-click action.
        btnClone.MouseUp += (s, e) => ForwardRightClickToOriginal(btnOriginal, e, null);

        return btnClone;
    }

    private static void Store(ToolStripItem item, Dictionary<string, ToolStripItem>? storeIn, Action<string>? log)
    {
        if (storeIn is null || string.IsNullOrWhiteSpace(item.Name) || storeIn.ContainsKey(item.Name))
        {
            return;
        }

        storeIn[item.Name] = item;
        log?.Invoke($"[ToolbarItemConverter] Stored converted {item.GetType().Name}: {item.Name}");
    }
}
