using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitUI;

/// <summary>Loads configured diff or merge tools and applies them to Avalonia menus.</summary>
public sealed class CustomDiffMergeToolProvider
{
    /// <summary>
    /// Time to wait before loading custom diff tools in FormBrowse.
    /// Avoid loading while git-log and git-diff run.
    /// </summary>
    private const int FormBrowseToolDelay = 8000;

    /// <summary>Clear the existing caches.</summary>
    /// <param name="isDiff">True if diff, false if merge.</param>
    public async Task ClearAsync(bool isDiff)
    {
        if (isDiff)
        {
            await CustomDiffMergeToolCache.DiffToolCache.ClearAsync();
        }
        else
        {
            await CustomDiffMergeToolCache.MergeToolCache.ClearAsync();
        }
    }

    /// <summary>Load the available diff/merge tools and apply them to the menus.</summary>
    public void LoadCustomDiffMergeTools(
        IGitModule module,
        IList<CustomDiffMergeTool> menus,
        bool isDiff,
        int delay = FormBrowseToolDelay,
        CancellationToken cancellationToken = default)
    {
        InitMenus(menus);

        if (isDiff && !AppSettings.ShowAvailableDiffTools)
        {
            return;
        }

        ThreadHelper.FileAndForget(() => LoadCustomDiffMergeToolsAsync(module, menus, isDiff, delay, cancellationToken));
    }

    private static async Task LoadCustomDiffMergeToolsAsync(
        IGitModule module,
        IList<CustomDiffMergeTool> menus,
        bool isDiff,
        int delay,
        CancellationToken cancellationToken)
    {
        List<string> tools = [.. await (isDiff ? CustomDiffMergeToolCache.DiffToolCache : CustomDiffMergeToolCache.MergeToolCache)
            .GetToolsAsync(module, delay, cancellationToken)];

        if (tools.Count <= 1)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => PopulateMenus(menus, tools, isDiff));
    }

    private static void PopulateMenus(IList<CustomDiffMergeTool> menus, IReadOnlyList<string> tools, bool isDiff)
    {
        foreach (CustomDiffMergeTool menu in menus)
        {
            foreach (string tool in tools)
            {
                MenuItem item = new()
                {
                    Header = tool.Replace("_", "__", StringComparison.Ordinal),
                    Tag = tool,
                };

                if (menu.MenuItem.Items.Count == 0)
                {
                    item.FontWeight = FontWeight.Bold;
                    item.InputGesture = menu.MenuItem.InputGesture;
                }

                item.Click += (_, e) =>
                {
                    e.Handled = true;
                    menu.Click(item, EventArgs.Empty);
                };
                menu.MenuItem.Items.Add(item);
            }

            if (isDiff)
            {
                menu.MenuItem.Items.Add(new Separator());
                MenuItem disableItem = new()
                {
                    Header = ResourceManager.TranslatedStrings.DisableMenuItem,
                };

                disableItem.Click += (_, e) =>
                {
                    e.Handled = true;
                    AppSettings.ShowAvailableDiffTools = false;
                    InitMenus(menus);
                };
                menu.MenuItem.Items.Add(disableItem);
            }
        }
    }

    private static void InitMenus(IList<CustomDiffMergeTool> menus)
    {
        foreach (CustomDiffMergeTool menu in menus)
        {
            menu.MenuItem.Items.Clear();
        }
    }

    internal TestAccessor GetTestAccessor() => new();

    internal readonly struct TestAccessor
    {
        internal Task LoadCustomDiffMergeToolsAsync(
            IGitModule module,
            IList<CustomDiffMergeTool> menus,
            bool isDiff,
            int delay = 0,
            CancellationToken cancellationToken = default)
            => CustomDiffMergeToolProvider.LoadCustomDiffMergeToolsAsync(module, menus, isDiff, delay, cancellationToken);
    }
}

/// <summary>Associates one tool-selection menu with its consumer click handler.</summary>
public sealed class CustomDiffMergeTool
{
    public CustomDiffMergeTool(MenuItem menuItem, EventHandler click)
    {
        MenuItem = menuItem;
        Click = click;
    }

    public MenuItem MenuItem { get; }
    public EventHandler Click { get; }
}
