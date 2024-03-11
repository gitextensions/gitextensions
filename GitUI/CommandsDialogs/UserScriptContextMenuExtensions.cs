using GitUI.LeftPanel.ContextMenu;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs
{
    public static class UserScriptContextMenuExtensions
    {
        private const string ScriptNameSuffix = "_ownScript";

        /// <summary>
        ///  Adds user scripts to the <paramref name="contextMenu"/>, or under <paramref name="hostMenuItem"/>,
        ///  if scripts are not marked as <see cref="ScriptInfo.AddToRevisionGridContextMenu"/>.
        /// </summary>
        /// <param name="contextMenu">The context menu to add user scripts to.</param>
        /// <param name="hostMenuItem">The menu item user scripts not marked as <see cref="ScriptInfo.AddToRevisionGridContextMenu"/> are added to.</param>
        /// <param name="scriptInvoker">The handler that handles user script invocation.</param>
        /// <param name="serviceProvider">The DI service provider.</param>
        public static void AddUserScripts(this ContextMenuStrip contextMenu, ToolStripMenuItem hostMenuItem, Func<int, bool> scriptInvoker, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(contextMenu);
            ArgumentNullException.ThrowIfNull(hostMenuItem);
            ArgumentNullException.ThrowIfNull(scriptInvoker);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            RemoveOwnScripts(contextMenu, hostMenuItem);
            int hostItemIndex = contextMenu.Items.IndexOf(hostMenuItem);
            int lastScriptItemIndex = hostItemIndex;

            IScriptsManager scriptsManager = serviceProvider.GetRequiredService<IScriptsManager>();
            IEnumerable<ScriptInfo> scripts = scriptsManager.GetScripts().Where(x => x.Enabled);

            IHotkeySettingsLoader hotkeySettingsLoader = serviceProvider.GetRequiredService<IHotkeySettingsLoader>();
            IReadOnlyList<HotkeyCommand> hotkeys = hotkeySettingsLoader.LoadHotkeys(FormSettings.HotkeySettingsName);

            foreach (ScriptInfo script in scripts)
            {
                ToolStripMenuItem item = new()
                {
                    Text = script.Name,
                    Name = $"{script.Name}{ScriptNameSuffix}",
                    Image = script.GetIcon(),
                    ShortcutKeyDisplayString = hotkeys.FirstOrDefault(h => h.Name == script.Name)?.KeyData.ToShortcutKeyDisplayString()
                };

                item.Click += (s, e) =>
                {
                    int scriptId = script.HotkeyCommandIdentifier;
                    scriptInvoker(scriptId);
                };

                if (script.AddToRevisionGridContextMenu)
                {
                    // insert items after hostMenuItem
                    contextMenu.Items.Insert(++lastScriptItemIndex, item);
                }
                else
                {
                    hostMenuItem.DropDown.Items.Add(item);
                    hostMenuItem.Enable(true);
                }
            }
        }

        /// <summary>
        ///  Removes user scripts from the <paramref name="contextMenu"/>, or from <paramref name="hostMenuItem"/>,
        ///  if scripts are not marked as <see cref="ScriptInfo.AddToRevisionGridContextMenu"/>.
        /// </summary>
        /// <param name="contextMenu">The context menu to remove user scripts from.</param>
        /// <param name="hostMenuItem">The menu item from which to remove user scripts not marked as <see cref="ScriptInfo.AddToRevisionGridContextMenu"/>.</param>
        public static void RemoveUserScripts(this ContextMenuStrip contextMenu, ToolStripMenuItem hostMenuItem)
        {
            ArgumentNullException.ThrowIfNull(contextMenu);
            ArgumentNullException.ThrowIfNull(hostMenuItem);

            RemoveOwnScripts(contextMenu, hostMenuItem);
        }

        private static void RemoveOwnScripts(ContextMenuStrip contextMenu, ToolStripMenuItem hostMenuItem)
        {
            hostMenuItem.DropDown.Items.Clear();
            hostMenuItem.Enable(false);

            List<ToolStripItem> list = contextMenu.Items.Cast<ToolStripItem>()
                .Where(x => x.Name.EndsWith(ScriptNameSuffix))
                .ToList();

            foreach (ToolStripItem item in list)
            {
                contextMenu.Items.Remove(item);
            }
        }
    }
}
