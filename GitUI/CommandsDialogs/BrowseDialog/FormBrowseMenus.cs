
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    /// <summary>
    /// Add MenuCommands as menus to the FormBrowse main menu.
    /// This class is intended to have NO dependency to FormBrowse
    ///   (if needed this kind of code should be done in FormBrowseMenuCommands).
    /// </summary>
    class FormBrowseMenus : ITranslate, IDisposable
    {
        MenuStrip _menuStrip;

        IList<MenuCommand> _navigateMenuCommands;
        IList<MenuCommand> _viewMenuCommands;

        ToolStripMenuItem _navigateToolStripMenuItem;
        ToolStripMenuItem _viewToolStripMenuItem;

        // we have to remember which items we registered with the menucommands because other
        // location (RevisionGrid) can register items too!
        IList<ToolStripMenuItem> _itemsRegisteredWithMenuCommand = new List<ToolStripMenuItem>();

        public FormBrowseMenus(MenuStrip menuStrip)
        {
            _menuStrip = menuStrip;

            CreateAdditionalMainMenuItems();
            Translate();
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

        public void ResetMenuCommandSets()
        {
            _navigateMenuCommands = null;
            _viewMenuCommands = null;
        }

        /// <summary>
        /// each new command set will be automatically separated by a separator
        /// </summary>
        /// <param name="mainMenuItem"></param>
        /// <param name="menuCommands"></param>
        public void AddMenuCommandSet(MainMenuItem mainMenuItem, IEnumerable<MenuCommand> menuCommands)
        {
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
        /// inserts
        /// - Navigate (after Repository)
        /// - View (after Navigate)
        /// </summary>
        public void InsertAdditionalMainMenuItems(ToolStripMenuItem insertAfterMenuItem)
        {
            RemoveAdditionalMainMenuItems();

            SetDropDownItems(_navigateToolStripMenuItem, _navigateMenuCommands);
            SetDropDownItems(_viewToolStripMenuItem, _viewMenuCommands);

            _menuStrip.Items.Insert(_menuStrip.Items.IndexOf(insertAfterMenuItem) + 1, _navigateToolStripMenuItem);
            _menuStrip.Items.Insert(_menuStrip.Items.IndexOf(_navigateToolStripMenuItem) + 1, _viewToolStripMenuItem);

            // maybe set check marks on menu items
            OnMenuCommandsPropertyChanged();
        }

        /// <summary>
        /// call in ctor before translation
        /// </summary>
        private void CreateAdditionalMainMenuItems()
        {
            if (_navigateToolStripMenuItem == null)
            {
                _navigateToolStripMenuItem = new ToolStripMenuItem();
                _navigateToolStripMenuItem.Name = "navigateToolStripMenuItem";
                _navigateToolStripMenuItem.Text = "Navigate";
            }

            if (_viewToolStripMenuItem == null)
            {
                _viewToolStripMenuItem = new ToolStripMenuItem();
                _viewToolStripMenuItem.Name = "viewToolStripMenuItem";
                _viewToolStripMenuItem.Text = "View";
            }
        }

        private IEnumerable<Tuple<string, object>> GetAdditionalMainMenuItemsForTranslation()
        {
            var list = new List<ToolStripMenuItem> { _navigateToolStripMenuItem, _viewToolStripMenuItem };
            return list.Select(menuItem => new Tuple<string, object>(menuItem.Name, menuItem));
        }

        private void SetDropDownItems(ToolStripMenuItem toolStripMenuItemTarget, IEnumerable<MenuCommand> menuCommands)
        {
            toolStripMenuItemTarget.DropDownItems.Clear();

            var toolStripItems = new List<ToolStripItem>();
            foreach (var menuCommand in menuCommands)
            {
                var toolStripItem = MenuCommand.CreateToolStripItem(menuCommand);
                toolStripItems.Add(toolStripItem);

                var toolStripMenuItem = toolStripItem as ToolStripMenuItem;
                if (toolStripMenuItem != null)
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
        public void RemoveAdditionalMainMenuItems()
        {
            _menuStrip.Items.Remove(_navigateToolStripMenuItem);
            _menuStrip.Items.Remove(_viewToolStripMenuItem);

            // don't forget to clear old associated menu items
            if (_itemsRegisteredWithMenuCommand != null)
            {
                if (_navigateMenuCommands != null)
                {
                    _navigateMenuCommands.ForEach(mc => mc.UnregisterMenuItems(_itemsRegisteredWithMenuCommand));
                }

                if (_viewMenuCommands != null)
                {
                    _viewMenuCommands.ForEach(mc => mc.UnregisterMenuItems(_itemsRegisteredWithMenuCommand));
                }

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
                return new List<MenuCommand>();
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
            }
        }
    }

    enum MainMenuItem
    {
        NavigateMenu,
        ViewMenu
    }
}
