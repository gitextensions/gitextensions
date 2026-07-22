using Avalonia.Controls;
using GitUI.Compat;
using GitUI.ScriptsEngine;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs;

public static class UserScriptContextMenuExtensions
{
    private const string ScriptNameSuffix = "_ownScript";

    public static bool AddUserScripts(
        this ContextMenu contextMenu,
        MenuItem hostMenuItem,
        Func<int, bool> scriptInvoker,
        Func<ScriptInfo, bool> scriptFilterAddDirect,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(contextMenu);
        ArgumentNullException.ThrowIfNull(hostMenuItem);
        ArgumentNullException.ThrowIfNull(scriptInvoker);
        ArgumentNullException.ThrowIfNull(scriptFilterAddDirect);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        RemoveOwnScripts(contextMenu, hostMenuItem);
        int hostItemIndex = contextMenu.Items.IndexOf(hostMenuItem);
        int lastScriptItemIndex = hostItemIndex;

        if (serviceProvider.GetService(typeof(IScriptsManager)) is not IScriptsManager scriptsManager
            || serviceProvider.GetService(typeof(IHotkeySettingsLoader)) is not IHotkeySettingsLoader hotkeySettingsLoader)
        {
            return false;
        }

        IEnumerable<ScriptInfo> scripts = scriptsManager.GetScripts().Where(script => script.Enabled);
        IReadOnlyList<HotkeyCommand> hotkeys = hotkeySettingsLoader.LoadHotkeys(FormSettings.HotkeySettingsName);
        bool itemsAdded = false;

        foreach (ScriptInfo script in scripts)
        {
            MenuItem item = new()
            {
                Header = script.Name,
                Tag = ScriptNameSuffix,
                InputGesture = KeysMapper.ToKeyGesture(hotkeys.FirstOrDefault(hotkey => hotkey.Name == script.GetDisplayName())?.KeyData),
            };

            if (script.GetIcon() is { } icon)
            {
                item.Icon = new Image { Width = 16, Height = 16, Source = icon };
            }

            item.Click += (_, _) => scriptInvoker(script.HotkeyCommandIdentifier);

            if (scriptFilterAddDirect(script) && hostItemIndex >= 0)
            {
                contextMenu.Items.Insert(++lastScriptItemIndex, item);
            }
            else
            {
                hostMenuItem.Items.Add(item);
                hostMenuItem.IsEnabled = true;
            }

            itemsAdded = true;
        }

        return itemsAdded;
    }

    public static void RemoveUserScripts(this ContextMenu contextMenu, MenuItem hostMenuItem)
    {
        ArgumentNullException.ThrowIfNull(contextMenu);
        ArgumentNullException.ThrowIfNull(hostMenuItem);

        RemoveOwnScripts(contextMenu, hostMenuItem);
    }

    private static void RemoveOwnScripts(ContextMenu contextMenu, MenuItem hostMenuItem)
    {
        hostMenuItem.Items.Clear();
        hostMenuItem.IsEnabled = false;

        List<MenuItem> ownItems = [.. contextMenu.Items
            .OfType<MenuItem>()
            .Where(item => item.Tag as string == ScriptNameSuffix)];
        foreach (MenuItem item in ownItems)
        {
            contextMenu.Items.Remove(item);
        }
    }
}
