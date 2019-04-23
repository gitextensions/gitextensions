using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitUI.Script;

namespace GitUI.Browsing
{
    public interface IUserScriptMenuBuilder
    {
        /// <summary>
        /// Build and insert 'Run script' tool
        /// </summary>
        /// <param name="tool">Top tool</param>
        void Build(ToolStrip tool);

        /// <summary>
        /// Build and insert 'Run script' menu
        /// </summary>
        /// <param name="contextMenu">Context menu</param>
        void Build(ContextMenuStrip contextMenu);
    }

    /// <summary>
    /// Build and insert 'Run script' menu
    /// </summary>
    internal sealed class UserScriptMenuBuilder : IUserScriptMenuBuilder
    {
        private const string RunScriptMenuItemName = "runScriptToolStripMenuItem";
        private const string OwnScriptsSeparatorName = "ownScriptsSeparator";
        private const string OwnScriptPostfix = "_ownScript";
        private const string UserScript = "userscript";

        private static bool _settingsLoaded;

        private readonly IScriptManager _scriptManager;
        private readonly IScriptRunner _scriptRunner;
        private readonly ICanRefreshRevisions _canRefreshRevisions;
        private readonly ICanLoadSettings _canLoadSettings;

        public UserScriptMenuBuilder(
            IScriptManager scriptManager,
            IScriptRunner scriptRunner,
            ICanRefreshRevisions canRefreshRevisions,
            ICanLoadSettings canLoadSettings)
        {
            _scriptManager = scriptManager ?? throw new ArgumentNullException(nameof(scriptManager));
            _scriptRunner = scriptRunner ?? throw new ArgumentNullException(nameof(scriptRunner));
            _canRefreshRevisions = canRefreshRevisions ?? throw new ArgumentNullException(nameof(canRefreshRevisions));
            _canLoadSettings = canLoadSettings ?? throw new ArgumentNullException(nameof(canLoadSettings));
        }

        public void Build(ToolStrip tool)
        {
            var items = tool.Items
                .OfType<ToolStripItem>()
                .Where(x => x.Name == UserScript)
                .ToList();

            foreach (var item in items)
            {
                tool.Items.RemoveByKey(item.Name);
            }

            var scripts = _scriptManager.GetScripts()
                .Where(x => x.Enabled)
                .Where(x => x.OnEvent == ScriptEvent.ShowInUserMenuBar)
                .ToList();

            if (scripts.Count == 0)
            {
                return;
            }

            tool.Items.Add(new ToolStripSeparator { Name = UserScript });

            foreach (var script in scripts)
            {
                var button = new ToolStripButton
                {
                    Text = script.Name,
                    Name = UserScript,
                    Enabled = true,
                    Visible = true,
                    Image = script.GetIcon(),
                    DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
                };

                button.Click += RunScript;

                // add to toolstrip
                tool.Items.Add(button);
            }
        }

        public void Build(ContextMenuStrip contextMenu)
        {
            var runScriptToolStripMenuItem = contextMenu.Items
                .OfType<ToolStripMenuItem>()
                .First(x => x.Name == RunScriptMenuItemName);

            RemoveOwnScripts(contextMenu, runScriptToolStripMenuItem);
            AddOwnScripts(contextMenu, runScriptToolStripMenuItem);
        }

        private static void RemoveOwnScripts(ContextMenuStrip contextMenu, ToolStripMenuItem runScriptToolStripMenuItem)
        {
            runScriptToolStripMenuItem.DropDown.Items.Clear();

            var items = contextMenu.Items
                .OfType<ToolStripItem>()
                .Where(x => x.Name.EndsWith(OwnScriptPostfix) || x.Name == OwnScriptsSeparatorName)
                .ToList();

            foreach (var item in items)
            {
                contextMenu.Items.RemoveByKey(item.Name);
            }

            runScriptToolStripMenuItem.Visible = false;
        }

        private void AddOwnScripts(ContextMenuStrip contextMenu, ToolStripMenuItem runScriptToolStripMenuItem)
        {
            var lastIndex = contextMenu.Items.Count;
            var scripts = _scriptManager.GetScripts();
            var toRunScriptMenu = scripts.Where(x => x.Enabled)
                .Where(x => !x.AddToRevisionGridContextMenu)
                .Select(x => CreateToolStripMenuItem(x.Name, x.GetIcon()))
                .Cast<ToolStripItem>()
                .ToArray();

            runScriptToolStripMenuItem.DropDown.Items.AddRange(toRunScriptMenu);
            runScriptToolStripMenuItem.Visible = toRunScriptMenu.Any();

            var toMainContextMenu = scripts.Where(x => x.Enabled)
                .Where(x => x.AddToRevisionGridContextMenu)
                .Select(x => CreateToolStripMenuItem(x.Name, x.GetIcon()))
                .Cast<ToolStripItem>()
                .ToArray();

            contextMenu.Items.AddRange(toMainContextMenu);

            if (toMainContextMenu.Any())
            {
                contextMenu.Items.Insert(lastIndex, new ToolStripSeparator { Name = OwnScriptsSeparatorName });
            }
        }

        private ToolStripMenuItem CreateToolStripMenuItem(string name, Image image)
        {
            return new ToolStripMenuItem(name, image, RunScript)
            {
                Name = $"{name}{OwnScriptPostfix}"
            };
        }

        private void RunScript(object sender, EventArgs e)
        {
            if (_settingsLoaded == false)
            {
                _canLoadSettings.LoadSettings();

                _settingsLoaded = true;
            }

            if (_scriptRunner.RunScript(sender.ToString()).NeedsGridRefresh)
            {
                _canRefreshRevisions.RefreshRevisions();
            }
        }
    }
}
