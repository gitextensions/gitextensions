using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    /// <summary>
    /// Add MenuCommands as menus to the FormBrowse main menu.
    /// This class is intended to have NO dependency to FormBrowse
    ///   (if needed this kind of code should be done in FormBrowseMenuCommands).
    /// </summary>
    internal class FormBrowseMenus : ITranslate, IDisposable
    {
        /// <summary>
        /// The menu to which we will be adding RevisionGrid command menus.
        /// </summary>
        private readonly ToolStrip _mainMenuStrip;

        /// <summary>
        /// The context menu that be shown to allow toggle visibilty of toolbars in <see cref="FormBrowse"/>.
        /// </summary>
        private readonly ContextMenuStrip _toolStripContextMenu = new ContextMenuStrip();

        private List<MenuCommand> _navigateMenuCommands;
        private List<MenuCommand> _viewMenuCommands;

        private ToolStripMenuItem _navigateToolStripMenuItem;
        private ToolStripMenuItem _viewToolStripMenuItem;
        private ToolStripMenuItem _toolbarsMenuItem;

        // we have to remember which items we registered with the menucommands because other
        // location (RevisionGrid) can register items too!
        private readonly List<ToolStripMenuItem> _itemsRegisteredWithMenuCommand = new List<ToolStripMenuItem>();

        public FormBrowseMenus(ToolStrip menuStrip)
        {
            _mainMenuStrip = menuStrip;

            CreateMenuItems();
            Translate();

            _toolStripContextMenu.Opening += (s, e) => RefreshToolbarsMenuItemCheckedState(_toolStripContextMenu.Items);
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
                _navigateToolStripMenuItem.Dispose();
                _viewToolStripMenuItem.Dispose();
                _toolbarsMenuItem.Dispose();
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
            Debug.Assert(_toolbarsMenuItem != null, "Toolbars menu item must be already created.");

            foreach (ToolStrip toolStrip in toolStrips)
            {
                Debug.Assert(!string.IsNullOrEmpty(toolStrip.Text), "Toolstrip must specify its name via Text property.");

                _toolStripContextMenu.Items.Add(CreateItem(toolStrip));
                _toolbarsMenuItem.DropDownItems.Add(CreateItem(toolStrip));
            }

            static ToolStripItem CreateItem(ToolStrip senderToolStrip)
            {
                var toolStripItem = new ToolStripMenuItem(senderToolStrip.Text)
                {
                    Checked = senderToolStrip.Visible,
                    CheckOnClick = true,
                    Tag = senderToolStrip
                };
                toolStripItem.Click += (s, e) =>
                {
                    senderToolStrip.Visible = !senderToolStrip.Visible;
                };

                return toolStripItem;
            }
        }

        public void ShowToolStripContextMenu(Point point) => _toolStripContextMenu.Show(point);

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

            IList<MenuCommand> selectedMenuCommands = null; // make that more clear

            switch (mainMenuItem)
            {
                case MainMenuItem.NavigateMenu:
                    if (_navigateMenuCommands == null)
                    {
                        _navigateMenuCommands = new List<MenuCommand>();
                    }
                    else
                    {
                        _navigateMenuCommands.Add(MenuCommand.CreateSeparator());
                    }

                    selectedMenuCommands = _navigateMenuCommands;
                    break;

                case MainMenuItem.ViewMenu:
                    if (_viewMenuCommands == null)
                    {
                        _viewMenuCommands = new List<MenuCommand>();
                    }
                    else
                    {
                        _viewMenuCommands.Add(MenuCommand.CreateSeparator());
                    }

                    selectedMenuCommands = _viewMenuCommands;
                    break;
            }

            selectedMenuCommands.AddAll(menuCommands);
        }

        /// <summary>
        /// Inserts "Navigate" and "View" menus after the <paramref name="insertAfterMenuItem"/>.
        /// </summary>
        public void InsertRevisionGridMainMenuItems(ToolStripItem insertAfterMenuItem)
        {
            RemoveRevisionGridMainMenuItems();

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
            if (_navigateToolStripMenuItem is null)
            {
                _navigateToolStripMenuItem = new ToolStripMenuItem
                {
                    Name = "navigateToolStripMenuItem",
                    Text = "Navigate"
                };
            }

            if (_viewToolStripMenuItem is null)
            {
                _viewToolStripMenuItem = new ToolStripMenuItem
                {
                    Name = "viewToolStripMenuItem",
                    Text = "View"
                };
            }

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
            foreach (ToolStripMenuItem menuItem in new[] { _navigateToolStripMenuItem, _viewToolStripMenuItem, _toolbarsMenuItem })
            {
                yield return (menuItem.Name, menuItem);
            }
        }

        private void SetDropDownItems(ToolStripMenuItem toolStripMenuItemTarget, IEnumerable<MenuCommand> menuCommands)
        {
            toolStripMenuItemTarget.DropDownItems.Clear();

            var toolStripItems = new List<ToolStripItem>();
            foreach (var menuCommand in menuCommands)
            {
                var toolStripItem = MenuCommand.CreateToolStripItem(menuCommand);
                toolStripItems.Add(toolStripItem);

                if (toolStripItem is ToolStripMenuItem toolStripMenuItem)
                {
                    menuCommand.RegisterMenuItem(toolStripMenuItem);
                    _itemsRegisteredWithMenuCommand.Add(toolStripMenuItem);
                }
            }

            toolStripMenuItemTarget.DropDownItems.AddRange(toolStripItems.ToArray());
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
            if (_itemsRegisteredWithMenuCommand != null)
            {
                _navigateMenuCommands?.ForEach(mc => mc.UnregisterMenuItems(_itemsRegisteredWithMenuCommand));

                _viewMenuCommands?.ForEach(mc => mc.UnregisterMenuItems(_itemsRegisteredWithMenuCommand));

                _itemsRegisteredWithMenuCommand.Clear();
            }
        }

        public void OnMenuCommandsPropertyChanged()
        {
            var menuCommands = GetNavigateAndViewMenuCommands();

            foreach (var menuCommand in menuCommands)
            {
                menuCommand.SetCheckForRegisteredMenuItems();
                menuCommand.UpdateMenuItemsShortcutKeyDisplayString();
            }
        }

        private IEnumerable<MenuCommand> GetNavigateAndViewMenuCommands()
        {
            if (_navigateMenuCommands == null && _viewMenuCommands == null)
            {
                return Enumerable.Empty<MenuCommand>();
            }
            else if (_navigateMenuCommands != null && _viewMenuCommands != null)
            {
                return _navigateMenuCommands.Concat(_viewMenuCommands);
            }
            else
            {
                throw new ApplicationException("this case is not allowed");
            }
        }

        private void RefreshToolbarsMenuItemCheckedState(ToolStripItemCollection toolStripItems)
        {
            foreach (ToolStripMenuItem item in toolStripItems)
            {
                Debug.Assert(item.Tag is ToolStrip, "Toolbars context menu items must reference Toolstrips via Tag property.");
                item.Checked = ((ToolStrip)item.Tag).Visible;
            }
        }
    }

    internal enum MainMenuItem
    {
        NavigateMenu,
        ViewMenu
    }
}
