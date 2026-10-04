namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Before toolbars could be customized, View > Toolbars let the user hide single buttons of the
// main window's toolbars, and whole groups of the Filters toolbar, one boolean setting each. A
// setting was only written when it differed from the default: false for an item hidden by the
// user, true for one of the fetch and pull shortcuts, which were hidden unless asked for.
//
// Those settings are no longer read, so on upgrade they are translated once into the toolbar
// layout - where an item is shown by being on its toolbar - and then removed.
internal static class LegacyToolbarVisibility
{
    public const string SettingPrefix = "formbrowse_toolbar_visibility_";

    private const string GroupPrefix = "ToolBar_group:";
    private const string PullShortcutPrefix = "pull_shortcut_";
    private const string PullButtonName = "toolStripButtonPull";

    // The fetch and pull shortcuts, in the order they used to sit just before the Pull button.
    // Each stood for an entry of the Pull button's drop-down, which a toolbar can now hold itself.
    private static readonly string[] _pullShortcutMenuItemNames =
    [
        "fetchToolStripMenuItem",
        "fetchAllToolStripMenuItem",
        "fetchPruneAllToolStripMenuItem",
        "mergeToolStripMenuItem",
        "rebaseToolStripMenuItem1",
        "pullToolStripMenuItem1",
    ];

    // Every setting the old code could write. "Text search" belonged to the commit dialog's file
    // list rather than to a toolbar of the main window, so there is nothing to translate it into.
    public static readonly IReadOnlyList<string> SettingNames =
    [
        SettingPrefix + "toolStripButtonLevelUp",
        SettingPrefix + "toolStripWorktrees",
        SettingPrefix + "branchSelect",
        SettingPrefix + "toolStripSplitStash",
        SettingPrefix + "toolStripButtonCommit",
        SettingPrefix + PullButtonName,
        SettingPrefix + "toolStripButtonPush",
        SettingPrefix + "toolStripFileExplorer",
        SettingPrefix + "userShell",
        .. _pullShortcutMenuItemNames.Select(name => SettingPrefix + PullShortcutPrefix + name),
        SettingPrefix + GroupPrefix + "Branch filter",
        SettingPrefix + GroupPrefix + "Text filter",
        SettingPrefix + GroupPrefix + "Text search",
    ];

    /// <summary>
    /// Tells whether any of the old settings is present, that is, whether the user changed
    /// anything the old way.
    /// </summary>
    /// <param name="readSetting">Reads a boolean setting, <see langword="null"/> when absent.</param>
    public static bool HasAny(Func<string, bool?> readSetting)
        => SettingNames.Any(name => readSetting(name) is not null);

    /// <summary>
    /// Works out what a toolbar holds once the old settings are applied to it.
    /// </summary>
    /// <remarks>
    /// An item hidden by the old settings is left out, along with every item of a hidden group.
    /// A fetch or pull shortcut that was shown becomes its drop-down entry, placed before the Pull
    /// button as the shortcut was. Separators left with nothing to separate are dropped, as the old
    /// code hid them.
    /// </remarks>
    /// <param name="items">The toolbar's items, in order.</param>
    /// <param name="readSetting">Reads a boolean setting, <see langword="null"/> when absent.</param>
    /// <param name="changed">
    /// Whether the old settings hide or add anything on this toolbar. A setting can be present
    /// without doing either - "Text search" names no toolbar of the main window, and a button can
    /// be stored as shown - and then there is nothing to carry over.
    /// </param>
    /// <returns>The toolbar's content, in order.</returns>
    public static List<LegacyToolbarSlot> Apply(IReadOnlyList<LegacyToolbarItem> items, Func<string, bool?> readSetting, out bool changed)
    {
        List<LegacyToolbarSlot> slots = [];
        changed = false;

        for (int index = 0; index < items.Count; index++)
        {
            LegacyToolbarItem item = items[index];

            if (item.Name == PullButtonName)
            {
                int countBefore = slots.Count;
                slots.AddRange(_pullShortcutMenuItemNames
                    .Where(name => readSetting(SettingPrefix + PullShortcutPrefix + name) is true)
                    .Select(name => new LegacyToolbarSlot(SourceIndex: null, InsertedItemName: name)));
                changed |= slots.Count > countBefore;
            }

            if (!item.IsSeparator && IsHidden(item, readSetting))
            {
                changed = true;
                continue;
            }

            slots.Add(new LegacyToolbarSlot(index, InsertedItemName: null));
        }

        return DropStraySeparators(slots, items);
    }

    /// <summary>
    /// Removes every old setting, so that they are translated only once.
    /// </summary>
    /// <param name="removeSetting">Removes a setting.</param>
    public static void Purge(Action<string> removeSetting)
    {
        foreach (string name in SettingNames)
        {
            removeSetting(name);
        }
    }

    // A grouped item was stored under its group, any other under its own name.
    private static bool IsHidden(LegacyToolbarItem item, Func<string, bool?> readSetting)
    {
        string? key = item.Group is not null && item.Group.StartsWith(GroupPrefix, StringComparison.Ordinal)
            ? item.Group
            : item.Name;

        return !string.IsNullOrEmpty(key) && readSetting(SettingPrefix + key) is false;
    }

    // Drops the separators at either end and all but the first of a run.
    private static List<LegacyToolbarSlot> DropStraySeparators(List<LegacyToolbarSlot> slots, IReadOnlyList<LegacyToolbarItem> items)
    {
        bool IsSeparator(LegacyToolbarSlot slot) => slot.SourceIndex is int index && items[index].IsSeparator;

        List<LegacyToolbarSlot> kept = [];
        foreach (LegacyToolbarSlot slot in slots)
        {
            if (IsSeparator(slot) && (kept.Count == 0 || IsSeparator(kept[^1])))
            {
                continue;
            }

            kept.Add(slot);
        }

        while (kept.Count > 0 && IsSeparator(kept[^1]))
        {
            kept.RemoveAt(kept.Count - 1);
        }

        return kept;
    }
}
