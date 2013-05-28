using GitCommands;
using ResourceManager.Translation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    class FormBrowseMenus : ITranslate
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
            Translate();

            _menuStrip = menuStrip;
        }

        public void Translate()
        {
            Translator.Translate(this, Settings.CurrentTranslation);
        }

        public virtual void AddTranslationItems(Translation translation)
        {
            TranslationUtl.AddTranslationItemsFromFields("FormBrowse", this, translation);
        }

        public virtual void TranslateItems(Translation translation)
        {
            TranslationUtl.TranslateItemsFromFields("FormBrowse", this, translation);
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

            _navigateToolStripMenuItem = new ToolStripMenuItem();
            _navigateToolStripMenuItem.Name = "navigateToolStripMenuItem";
            _navigateToolStripMenuItem.Text = "Navigate";
            SetDropDownItems(_navigateToolStripMenuItem, _navigateMenuCommands);
            _menuStrip.Items.Insert(_menuStrip.Items.IndexOf(insertAfterMenuItem) + 1, _navigateToolStripMenuItem);

            _viewToolStripMenuItem = new ToolStripMenuItem();
            _viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            _viewToolStripMenuItem.Text = "View";
            SetDropDownItems(_viewToolStripMenuItem, _viewMenuCommands);
            _menuStrip.Items.Insert(_menuStrip.Items.IndexOf(_navigateToolStripMenuItem) + 1, _viewToolStripMenuItem);

            // maybe set check marks on menu items
            OnMenuCommandsPropertyChanged();
        }

        private void SetDropDownItems(ToolStripMenuItem toolStripMenuItemTarget, IEnumerable<MenuCommand> menuCommands)
        {
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
    }

    enum MainMenuItem
    {
        NavigateMenu,
        ViewMenu
    }

    /// <summary>
    /// TODO: move to separate file
    /// </summary>
    class MenuCommand
    {
        public static MenuCommand CreateSeparator()
        {
            return new MenuCommand { IsSeparator = true };
        }

        public static ToolStripItem CreateToolStripItem(MenuCommand menuCommand)
        {
            if (menuCommand.IsSeparator)
            {
                return new ToolStripSeparator();
            }
            else
            {
                var toolStripMenuItem = new ToolStripMenuItem();
                toolStripMenuItem.Name = menuCommand.Name;
                toolStripMenuItem.Text = menuCommand.Text;
                toolStripMenuItem.Image = menuCommand.Image;
                toolStripMenuItem.ShortcutKeys = menuCommand.ShortcutKeys;
                toolStripMenuItem.ShortcutKeyDisplayString = menuCommand.ShortcutKeyDisplayString;
                toolStripMenuItem.Click += (obj, sender) => menuCommand.ExecuteAction();

                return toolStripMenuItem;
            }
        }

        IList<ToolStripMenuItem> _registeredMenuItems = new List<ToolStripMenuItem>();

        /// <summary>
        /// if true all other properties have no meaning
        /// </summary>
        public bool IsSeparator { get; set; }

        /// <summary>
        /// used only for toolstripitem
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// text of the menu item
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// image of the menu item
        /// </summary>
        public Image Image { get; set; }

        public Keys ShortcutKeys { get; set; }
        public string ShortcutKeyDisplayString { get; set; }

        /// <summary>
        /// execute on menu click
        /// </summary>
        public Action ExecuteAction;

        /// <summary>
        /// called if the menu item want to know if the Checked property
        /// should be true or false. If null then false.
        /// </summary>
        public Func<bool> IsCheckedFunc;

        /// <summary>
        /// To make the MenuCommand interact with all its associated menu items
        /// this method is used to register the all menu items that where generated by this MenuCommand
        /// </summary>
        /// <param name="menuItem"></param>
        public void RegisterMenuItem(ToolStripMenuItem menuItem)
        {
            _registeredMenuItems.Add(menuItem);
        }

        public void UnregisterMenuItems(IEnumerable<ToolStripMenuItem> items)
        {
            foreach (var item in items)
            {
                _registeredMenuItems.Remove(item);
            }
        }

        public void SetCheckForRegisteredMenuItems()
        {
            if (IsCheckedFunc != null)
            {
                bool isChecked = IsCheckedFunc();
                _registeredMenuItems.ForEach(mi => mi.Checked = isChecked);
            }
        }

        public void UpdateMenuItemsShortcutKeyDisplayString()
        {
            _registeredMenuItems.ForEach(mi => mi.ShortcutKeyDisplayString = ShortcutKeyDisplayString);
        }
    }
}
