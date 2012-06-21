using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using GitPlugin.Commands;
using Microsoft.VisualStudio.CommandBars;
using Thread = System.Threading.Thread;

namespace GitPlugin
{
    /// <summary>
    ///   The object for implementing an Add-in.
    /// </summary>
    /// <seealso class = 'IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        private Plugin _gitPlugin;
        
        #region IDTCommandTarget Members

        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, 
            ref vsCommandStatus status, ref object commandText)
        {
            if (neededText != vsCommandStatusTextWanted.vsCommandStatusTextWantedNone ||
                !_gitPlugin.CanHandleCommand(commandName)) 
                return;

            if (_gitPlugin.IsCommandEnabled(commandName))
                status = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
            else
                status = vsCommandStatus.vsCommandStatusSupported;
        }

        public void Exec(string commandName, vsCommandExecOption executeOption, 
            ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption != vsCommandExecOption.vsCommandExecOptionDoDefault)
                return;

            handled = _gitPlugin.OnCommand(commandName);
        }

        #endregion

        #region IDTExtensibility2 Members

        /// <summary>
        /// Implements the OnConnection method of the IDTExtensibility2 interface.
        /// Receives notification that the Add-in is being loaded.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="connectMode">The connect mode.</param>
        /// <param name="addInInst">The add in inst.</param>
        /// <param name="custom">The custom.</param>
        /// <seealso class="IDTExtensibility2"/>
        public void OnConnection(object application, ext_ConnectMode connectMode, 
            object addInInst, ref Array custom)
        {
            if (_gitPlugin == null)
            {
                var cultureInfo = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = cultureInfo;

                _gitPlugin = 
                    new Plugin((DTE2) application, (AddIn) addInInst, "GitExtensions", "GitPlugin.Connect");
            }
            
            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
                this.GitPluginUISetup();
            if (connectMode == ext_ConnectMode.ext_cm_AfterStartup ||
                connectMode == ext_ConnectMode.ext_cm_Startup)
                this.GitPluginInit();

        }

        private void GitPluginInit()
        {
            this.GitPluginUISetup();
        }

        private void GitPluginUISetup()
        {
            if (_gitPlugin == null) return;

            try
            {
#if DEBUG
                this._gitPlugin.OutputPane.OutputString("Git Extensions plugin connected" + Environment.NewLine);
#endif

                this.RegiserGitPluginCommand();

                //Place the command on the tools menu.
                //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
                var menuBarCommandBar = _gitPlugin.GetMenuBar();

                CommandBarControl toolsControl;
                CommandBarPopup toolsPopup = null;
                try
                {
                    toolsControl = menuBarCommandBar.Controls["Git"];
                }
                catch
                {
                    toolsControl = null;
                }

                try
                {
                    if (toolsControl == null)
                    {

                        toolsControl = menuBarCommandBar.Controls.Add(MsoControlType.msoControlPopup, Type.Missing,
                                                                      Type.Missing, 4, false);
                        toolsControl.Caption = "&Git";
                    }

                    toolsPopup = (CommandBarPopup) toolsControl;
                    toolsPopup.Caption = "&Git";

                }
                catch (Exception ex)
                {
                    try
                    {
                        _gitPlugin.OutputPane.OutputString(
                            "Error creating git menu (trying to add commands to tools menu): " + ex);
                        if (toolsControl == null)
                        {
                            toolsControl = menuBarCommandBar.Controls[this.GetToolsMenuName()];
                            toolsPopup = (CommandBarPopup)toolsControl;
                        }
                    }
                    catch (Exception ex2)
                    {
                        _gitPlugin.OutputPane.OutputString("Error menu: " + ex2);
                    }
                }

                try
                {
                    // add the toolbar and menu commands
                    var commandBar = _gitPlugin.AddGitCommandBar(MsoBarPosition.msoBarTop);

                    _gitPlugin.AddToolbarCommandWithText(
                        commandBar, "GitExtensionsCommit", "Commit", "Commit changes", 7,1);

                    _gitPlugin.AddToolbarCommand(commandBar, 
                        "GitExtensionsBrowse", "Browse", "Browse repository", 12, 2);
                    
                    _gitPlugin.AddToolbarCommand(commandBar, "GitExtensionsPull", "Pull",
                                                "Pull changes from remote repository", 9, 3);

                    _gitPlugin.AddToolbarCommand(commandBar, "GitExtensionsPush", "Push",
                                                "Push changes to remote repository", 8, 4);
                    _gitPlugin.AddToolbarCommand(commandBar, 
                        "GitExtensionsStash", "Stash", "Stash changes", 3, 5);
                    _gitPlugin.AddToolbarCommand(commandBar,
                        "GitExtensionsSettings", "Settings", "Settings", 2, 6);
                }
                catch (Exception ex)
                {
                    _gitPlugin.OutputPane.OutputString("Error creating toolbar: " + ex);
                }
                try
                {
                    var n = 1;

                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsApplyPatch", "&Apply patch", "Apply patch", 0,
                                              n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsBrowse", "&Browse", "Browse repository", 12, n++);
                    
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsSwitchBranch", "Chec&kout branch",
                                              "Switch to branch", 10, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsCherryPick", "Cherry &pick", "Cherry pick commit",
                                              11, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsCommit", "&Commit", "Commit changes", 7, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsCreateBranch", "Create bra&nch",
                                              "Create new branch", 10, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsClone", "Clone &repository", "Clone existing Git",
                                              14, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsGitIgnore", "Edit &.gitignore",
                                              "Edit .gitignore file", 0, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsFormatPatch", "&Format patch", "Format patch", 0,
                                              n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsBash", "&Git bash", "Start git bash", 0, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsInitRepository", "Initialize new repositor&y",
                                              "Initialize new Git repository", 13, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsRemotes", "Manage rem&otes",
                                              "Manage remote repositories", 0, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsMerge", "&Merge", "merge", 0, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsPull", "P&ull",
                                              "Pull changes from remote repository", 9, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsPush", "Pu&sh",
                                              "Push changes to remote repository", 8, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsRebase", "R&ebase", "Rebase", 0, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsSearchFile", "Search fi&le", "Search a file in the repository", 0, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsSettings", "Se&ttings", "Settings", 2, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsSolveMergeConflicts", "Sol&ve mergeconflicts",
                                              "Solve mergeconflicts", 0, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsStash", "Stas&h", "Stash changes", 3, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsDiff", "V&iew changes",
                                              "View commit change history", 0, n++);
                    _gitPlugin.AddPopupCommand(toolsPopup, "GitExtensionsAbout", "About Git E&xtensions",
                                              "About Git Extensions", 0, n++);
                }
                catch (Exception ex)
                {
                    _gitPlugin.OutputPane.OutputString("Error creating contextmenu: " + ex);
                }

               

                AddContextMenuItemsToContextMenu("XML Editor");
                AddContextMenuItemsToContextMenu("Web Item");
                AddContextMenuItemsToContextMenu("Item");
                AddContextMenuItemsToContextMenu("Easy MDI Document Window");
                AddContextMenuItemsToContextMenu("Code Window");
                AddContextMenuItemsToContextMenu("Script Context");
                AddContextMenuItemsToContextMenu("ASPX Context");

                /*
                 * Uncomment the code block below to help find the name of commandbars in
                 * visual studio. All commandbars (and context menu's) will get a new entry
                 * with the name of that commandbar.
                foreach (var commandBar in ((CommandBars)_applicationObject.CommandBars))
                {
                    try
                    {
                        var name = Guid.NewGuid().ToString("N");
                        _gitPlugin.RegisterCommand(name, new ToolbarCommand<Remotes>());
                        _gitPlugin.AddMenuCommand(((CommandBar)commandBar).Name, name, ((CommandBar)commandBar).Name, ((CommandBar)commandBar).Name, 6, 4);
                    }
                    catch
                    {
                    }

                    _gitPlugin.OutputPane.OutputString(((CommandBar)commandBar).Name + Environment.NewLine);
                }
                */
            }
            catch (Exception ex)
            {
                this._gitPlugin.OutputPane.OutputString("Error loading plugin: " + ex);
            }
        }

        private void RegiserGitPluginCommand()
        {
            //GitPlugin.DeleteCommandBar("GitExtensions");
            try
            {
                this._gitPlugin.RegisterCommand("GitExtensionsFileHistory", new ToolbarCommand<FileHistory>());
                this._gitPlugin.RegisterCommand("GitExtensionsCommit", new ToolbarCommand<Commit>());
                this._gitPlugin.RegisterCommand("GitExtensionsBrowse", new ToolbarCommand<Browse>());
                this._gitPlugin.RegisterCommand("GitExtensionsClone", new ToolbarCommand<Clone>());
                this._gitPlugin.RegisterCommand("GitExtensionsCreateBranch", new ToolbarCommand<CreateBranch>());
                this._gitPlugin.RegisterCommand("GitExtensionsSwitchBranch", new ToolbarCommand<SwitchBranch>());
                this._gitPlugin.RegisterCommand("GitExtensionsDiff", new ToolbarCommand<ViewDiff>());
                this._gitPlugin.RegisterCommand("GitExtensionsInitRepository", new ToolbarCommand<Init>());
                this._gitPlugin.RegisterCommand("GitExtensionsFormatPatch", new ToolbarCommand<FormatPatch>());
                this._gitPlugin.RegisterCommand("GitExtensionsPull", new ToolbarCommand<Pull>());
                this._gitPlugin.RegisterCommand("GitExtensionsPush", new ToolbarCommand<Push>());
                this._gitPlugin.RegisterCommand("GitExtensionsRebase", new ToolbarCommand<Rebase>());
                this._gitPlugin.RegisterCommand("GitExtensionsRevert", new ToolbarCommand<Revert>());
                this._gitPlugin.RegisterCommand("GitExtensionsMerge", new ToolbarCommand<Merge>());
                this._gitPlugin.RegisterCommand("GitExtensionsCherryPick", new ToolbarCommand<Cherry>());
                this._gitPlugin.RegisterCommand("GitExtensionsStash", new ToolbarCommand<Stash>());
                this._gitPlugin.RegisterCommand("GitExtensionsSettings", new ToolbarCommand<Settings>());
                this._gitPlugin.RegisterCommand("GitExtensionsSolveMergeConflicts", new ToolbarCommand<SolveMergeConflicts>());
                this._gitPlugin.RegisterCommand("GitExtensionsApplyPatch", new ToolbarCommand<ApplyPatch>());
                this._gitPlugin.RegisterCommand("GitExtensionsAbout", new ToolbarCommand<About>());
                this._gitPlugin.RegisterCommand("GitExtensionsBash", new ToolbarCommand<Bash>());
                this._gitPlugin.RegisterCommand("GitExtensionsGitIgnore", new ToolbarCommand<GitIgnore>());
                this._gitPlugin.RegisterCommand("GitExtensionsRemotes", new ToolbarCommand<Remotes>());
                this._gitPlugin.RegisterCommand("GitExtensionsSearchFile", new ToolbarCommand<SearchFile>());
            }
            catch (Exception ex)
            {
                this._gitPlugin.OutputPane.OutputString("Error adding commands: " + ex);
            }
        }

        private void AddContextMenuItemsToContextMenu(string toolbarName)
        {
            try
            {
                _gitPlugin.AddMenuCommand(toolbarName, "GitExtensionsFileHistory", "File history", "Show file history", 6,
                                         4);
                _gitPlugin.AddMenuCommand(toolbarName, "GitExtensionsRevert", "Undo file changes",
                                         "Undo changes made to this file", 4, 5);
            }
            catch 
            {
                //ignore all exceptions....
                //When a commandbar is not found, an exception will be thrown -> todo avoid exceptions!
            }
        }

        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            //if (disconnectMode == ext_DisconnectMode.ext_dm_HostShutdown
            //    || disconnectMode == ext_DisconnectMode.ext_dm_UserClosed)
            //{
            //    _gitPlugin.DeleteCommands();
            //    _gitPlugin.DeleteCommandBar(GitToolBarName);
            //    //Place the command on the tools menu.
            //    //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            //    var menuBarCommandBar = ((CommandBars)_applicationObject.CommandBars)["MenuBar"];


            //    CommandBarControl toolsControl;
            //    try
            //    {
            //        toolsControl = menuBarCommandBar.Controls["Git"];
            //        if (toolsControl != null)
            //        {
            //            toolsControl.Delete();
            //        }
            //    }
            //    catch
            //    {
            //    }
            //}
        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnStartupComplete(ref Array custom)
        {
            this.GitPluginInit();
        }

        public void OnBeginShutdown(ref Array custom)
        {
        }

        #endregion


        private string GetToolsMenuName()
        {
            string toolsMenuName;

            try
            {
                //If you would like to move the command to a different menu, change the word "Tools" to the 
                //  English version of the menu. This code will take the culture, append on the name of the menu
                //  then add the command to that menu. You can find a list of all the top-level menus in the file
                //  CommandBar.resx.
                string resourceName;
                var resourceManager = new ResourceManager("GitPlugin.CommandBar", Assembly.GetExecutingAssembly());
                var cultureInfo = new CultureInfo(_gitPlugin.LocaleID);

                if (cultureInfo.TwoLetterISOLanguageName == "zh")
                {
                    var parentCultureInfo = cultureInfo.Parent;
                    resourceName = String.Concat(parentCultureInfo.Name, "Tools");
                }
                else
                {
                    resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
                }
                toolsMenuName = resourceManager.GetString(resourceName);
            }
            catch
            {
                //We tried to find a localized version of the word Tools, but one was not found.
                //  Default to the en-US word, which may work for the current culture.
                toolsMenuName = "Tools";
            }

            return toolsMenuName;
        }
    }
}