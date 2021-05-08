using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public class CustomDiffMergeToolProvider
    {
        /// <summary>
        /// Time to wait before loading custom diff tools in FormBrowse
        /// Avoid loading while git-log and git-diff run.
        /// </summary>
        private const int FormBrowseToolDelay = 15000;

        /// <summary>
        /// Clear the existing caches.
        /// </summary>
        public void Clear()
        {
            CustomDiffMergeToolCache.DiffToolCache.Clear();
            CustomDiffMergeToolCache.MergeToolCache.Clear();
        }

        /// <summary>
        /// Load the available  DiffMerge tools and apply to the menus.
        /// </summary>
        /// <param name="module">The Git module.</param>
        /// <param name="menus">The menus to update.</param>
        /// <param name="components">The calling Form components, to dispose correctly.</param>
        /// <param name="isDiff">True if diff, false if merge.</param>
        /// <param name="delay">The delay before starting the operation.</param>
        public void LoadCustomDiffMergeTools(GitModule module, IList<CustomDiffMergeTool> menus, IContainer components, bool isDiff, int delay = FormBrowseToolDelay)
        {
            InitMenus(menus);

            if (isDiff && !AppSettings.ShowAvailableDiffTools)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                // get the tools, possibly with a delay as requesting requires considerable time
                // cache is shared
                List<string> tools = (await (isDiff ? CustomDiffMergeToolCache.DiffToolCache : CustomDiffMergeToolCache.MergeToolCache)
                    .GetToolsAsync(module, delay))
                    .ToList();

                if (tools.Count() <= 1)
                {
                    // No need to show the menu
                    return;
                }

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                foreach (var menu in menus)
                {
                    menu.MenuItem.DropDown = new ContextMenuStrip(components);
                    foreach (var tool in tools)
                    {
                        var item = new ToolStripMenuItem(tool) { Tag = tool };

                        item.Click += menu.Click;
                        menu.MenuItem.DropDown.Items.Add(item);
                        if (menu.MenuItem.DropDownItems.Count == 1)
                        {
                            item.Font = new Font(item.Font, FontStyle.Bold);
                            if (menu.MenuItem.ShortcutKeys != Keys.None)
                            {
                                item.ShortcutKeys = menu.MenuItem.ShortcutKeys;
                            }
                            else
                            {
                                item.ShortcutKeyDisplayString = menu.MenuItem.ShortcutKeyDisplayString;
                            }
                        }
                    }

                    if (isDiff)
                    {
                        // Allow disabling for difftools
                        menu.MenuItem.DropDown.Items.Add(new ToolStripSeparator());
                        var disableItem = new ToolStripMenuItem
                        {
                            Text = ResourceManager.TranslatedStrings.DisableMenuItem
                        };

                        disableItem.Click += (o, s) =>
                        {
                            // Disables the dropdown in the current form only
                            // RevDiff will not be affected from other forms, requires a restart
                            // To overcome this limitation other forms would require a reference to RevDiff
                            AppSettings.ShowAvailableDiffTools = false;
                            InitMenus(menus);
                        };
                        menu.MenuItem.DropDown.Items.Add(disableItem);
                    }
                }
            }).FileAndForget();
            return;

            static void InitMenus(IList<CustomDiffMergeTool> menus)
            {
                foreach (var menu in menus)
                {
                    menu.MenuItem.DropDown = null;
                }
            }
        }
    }
}
