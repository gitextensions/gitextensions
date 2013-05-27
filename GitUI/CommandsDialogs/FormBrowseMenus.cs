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

        public void AddMenuCommandSet(MainMenuItem mainMenuItem, IEnumerable<MenuCommand> menuCommands)
        {
            IList<MenuCommand> selectedMenuCommands = null;

            switch (mainMenuItem)
            {
                case MainMenuItem.NavigateMenu:
                    if (_navigateMenuCommands == null)
                    {
                        _navigateMenuCommands = new List<MenuCommand>();
                    }
                    else
                    {
                        _navigateMenuCommands.Add(null); // add separator
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
                        _viewMenuCommands.Add(null); // add separator
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
        }

        private void SetDropDownItems(ToolStripMenuItem toolStripMenuItemTarget, IEnumerable<MenuCommand> menuCommands)
        {
            var toolStripItems = new List<ToolStripItem>();
            foreach (var menuCommand in menuCommands)
            {
                if (menuCommand == null)
                {
                    toolStripItems.Add(new ToolStripSeparator());
                    continue;
                }

                var toolStripMenuItem = new ToolStripMenuItem();
                toolStripMenuItem.Name = menuCommand.Name;
                toolStripMenuItem.Text = menuCommand.Text;
                toolStripMenuItem.Image = menuCommand.Image;
                toolStripMenuItem.ShortcutKeys = menuCommand.ShortcutKeys;
                toolStripMenuItem.ShortcutKeyDisplayString = menuCommand.ShortcutKeyDisplayString;
                toolStripMenuItem.Click += (obj, sender) => menuCommand.ExecuteAction();

                toolStripItems.Add(toolStripMenuItem);
            }

            toolStripMenuItemTarget.DropDownItems.AddRange(toolStripItems.ToArray());
        }

        public void RemoveAdditionalMainMenuItems()
        {
            _menuStrip.Items.Remove(_navigateToolStripMenuItem);
            _menuStrip.Items.Remove(_viewToolStripMenuItem);
        }

        public void OnMenuCommandsPropertyChanged()
        {
            // TODO
        }
    }

    enum MainMenuItem
    {
        NavigateMenu,
        ViewMenu
    }

    class MenuCommand
    {
        IList<ToolStripMenuItem> _registeredMenuItems = new List<ToolStripMenuItem>();

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
        /// called if the menu item wants to know if the Checked property
        /// should be true or false. If null then false.
        /// </summary>
        public Func<bool> IsCheckedFunc;

        /////// <summary>
        /////// To make the MenuCommand interact with all its associated menu items
        /////// this method is used to register the all menu items that where generated by this MenuCommand
        /////// </summary>
        /////// <param name="menuItem"></param>
        ////public void RegisterMenuItem(ToolStripMenuItem menuItem)
        ////{
        ////    _registeredMenuItems.Add(menuItem);
        ////}

        /////// <summary>
        /////// for all registered menu items: set the check or not
        /////// </summary>
        /////// <param name="isChecked"></param>
        /////// <returns></returns>
        ////public void SetChecked(bool isChecked)
        ////{
        ////    foreach (var item in _registeredMenuItems)
        ////    {
        ////        item.Checked = isChecked;
        ////    }
        ////}
    }
}
