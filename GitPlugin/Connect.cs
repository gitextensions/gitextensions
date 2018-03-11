using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using GitPluginShared;
using GitPluginShared.Commands;
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
            {
                return;
            }

            if (_gitPlugin.IsCommandEnabled(commandName))
            {
                status = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
            }
            else
            {
                status = vsCommandStatus.vsCommandStatusSupported;
            }
        }

        public void Exec(string commandName, vsCommandExecOption executeOption,
            ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption != vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                return;
            }

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
                    new Plugin((DTE2)application, (AddIn)addInInst, "GitExtensions", "GitPlugin.Connect");
            }

            switch (connectMode)
            {
                case ext_ConnectMode.ext_cm_UISetup:
                    // Install CommandBar permanently (only runs once per AddIn)
                    GitPluginUISetup();
                    break;

                case ext_ConnectMode.ext_cm_Startup:
                    // The add-in was marked to load on startup
                    // Do nothing at this point because the IDE may not be fully initialized
                    // Visual Studio will call OnStartupComplete when fully initialized
                    break;

                case ext_ConnectMode.ext_cm_AfterStartup:
                    // The add-in was loaded by hand after startup using the Add-In Manager
                    // Initialize it in the same way that when is loaded on startup
                    GitPluginUIUpdate();
                    break;
            }
        }

        private void GitPluginInit()
        {
            if (_gitPlugin == null)
            {
                return;
            }

            try
            {
#if DEBUG
                _gitPlugin.OutputPane.OutputString("Git Extensions plugin connected" + Environment.NewLine);
#endif
                RegisterGitPluginCommand();
            }
            catch (Exception ex)
            {
                _gitPlugin.OutputPane.OutputString("Error loading plugin: " + ex);
            }
        }

        private void GitPluginUISetupMainMenu()
        {
            // Try to delete the commandbar if it exists from a previous execution,
            // because the /resetaddin command-line switch of VS 2005 (or higher) add-in
            // projects only resets commands and buttons, not commandbars
            _gitPlugin.DeleteOldGitExtMainMenuBar();
            _gitPlugin.DeleteGitExtMainMenuBar();

            try
            {
                // Add a new commandbar popup
                CommandBar mainMenuBar = _gitPlugin.AddGitExtMainMenuBar(GetToolsMenuName());
                CommandBarPopup mainMenuPopup = (CommandBarPopup)mainMenuBar.Parent;

                var n = 1;

                // Add commands
                {
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Browse",              "&Browse", "Browse repository", 12, n++, true);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Clone",               "Clone repositor&y", "Clone existing Git", 14, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "CreateNewRepository", "Create new repositor&y", "Create new Git repository", 13, n++);
                }

                // Working with changes
                {
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Commit",              "&Commit", "Commit changes", 7, n++, true);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Pull",                "P&ull", "Pull changes from remote repository", 9, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Push",                "Pu&sh", "Push changes to remote repository", 8, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Stash",               "Stas&h", "Stash changes", 3, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "ResetFile",           "&Reset File Changes", "Reset file changes", 4, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Remotes",             "Manage rem&otes", "Manage remote repositories", 17, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "GitIgnore",           "Edit &.gitignore", "Edit .gitignore file", 22, n++);
                }

                // Patch
                {
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "ApplyPatch",          "&Apply patch", "Apply patch", 0, n++, true);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "FormatPatch",         "Format patch", "Format patch", 0, n++);
                }

                {
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "FileHistory",         "&File History", "View file history", 6, n++, true);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "ViewChanges",         "V&iew changes", "View commit change history", 24, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "FindFile",            "Find fi&le", "Search for a file in the repository", 23, n++);
                }

                // Branch manipulations
                {
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "SwitchBranch",        "Chec&kout branch", "Switch to branch", 16, n++, true);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "CreateBranch",        "Create bra&nch", "Create new branch", 10, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Merge",               "&Merge", "merge", 18, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Rebase",              "R&ebase", "Rebase", 19, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "SolveMergeConflicts", "Sol&ve merge conflicts", "Solve merge conflicts", 0, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "CherryPick",          "Cherry &pick", "Cherry pick commit", 15, n++);
                }

                {
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Bash",                "&Git bash", "Start git bash", 21, n++, true);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "Settings",            "Se&ttings", "Settings", 2, n++);
                    _gitPlugin.AddPopupCommand(mainMenuPopup, "About",               "About Git E&xtensions", "About Git Extensions", 20, n);
                }
            }
            catch (Exception ex)
            {
                _gitPlugin.OutputPane.OutputString("Error creating contextmenu: " + ex);
            }
        }

        private void GitPluginUISetupCommandBar()
        {
            // Try to delete the commandbar if it exists from a previous execution,
            // because the /resetaddin command-line switch of VS 2005 (or higher) add-in
            // projects only resets commands and buttons, not commandbars
            _gitPlugin.DeleteGitExtCommandBar();

            try
            {
                CommandBar commandBar = _gitPlugin.AddGitExtCommandBar(MsoBarPosition.msoBarTop);

                _gitPlugin.AddToolbarCommandWithText(commandBar, "Commit", "Commit", "Commit changes", 7, 1);
                _gitPlugin.AddToolbarCommand(commandBar, "Browse", "Browse", "Browse repository", 12, 2);
                _gitPlugin.AddToolbarCommand(commandBar, "Pull", "Pull", "Pull changes from remote repository", 9, 3);
                _gitPlugin.AddToolbarCommand(commandBar, "Push", "Push", "Push changes to remote repository", 8, 4);
                _gitPlugin.AddToolbarCommand(commandBar, "Stash", "Stash", "Stash changes", 3, 5);
                _gitPlugin.AddToolbarCommand(commandBar, "Settings", "Settings", "Settings", 2, 6);
            }
            catch (Exception ex)
            {
                _gitPlugin.OutputPane.OutputString("Error creating toolbar: " + ex);
            }
        }

        private void GitPluginUISetupContextMenu()
        {
            try
            {
                AddContextMenuItemsToContextMenu("Web Item", runForSelection: true);
                AddContextMenuItemsToContextMenu("Item", runForSelection: true);
                AddContextMenuItemsToContextMenu("Easy MDI Document Window");
                AddContextMenuItemsToContextMenu("Code Window");
                AddContextMenuItemsToContextMenu("Script Context");
                AddContextMenuItemsToContextMenu("ASPX Context");
            }
            catch (Exception ex)
            {
                _gitPlugin.OutputPane.OutputString("Error creating context menu: " + ex);
            }
        }

        private void GitPluginUISetup()
        {
            if (_gitPlugin == null)
            {
                return;
            }

            // TODO: After Setup call: devenv.exe /ResetAddin GitPlugin.Connect or
            //       Delete RegistryKey: HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\[version]\PreloadAddinStateManager\[GitPlugin.Connect-Key]

            try
            {
#if DEBUG
                _gitPlugin.OutputPane.OutputString("Git Extensions plugin UI setup" + Environment.NewLine);
#endif

                GitPluginInit();
                GitPluginUISetupMainMenu();
                GitPluginUISetupCommandBar();
                GitPluginUISetupContextMenu();

                // Uncomment the code block below to help find the name of commandbars in
                // visual studio. All commandbars (and context menu's) will get a new entry
                // with the name of that commandbar.
                ////foreach (var commandBar in _gitPlugin.CommandBars)
                ////{
                ////    _gitPlugin.OutputPane.OutputString(((CommandBar)commandBar).Name + Environment.NewLine);
                ////}
            }
            catch (Exception ex)
            {
                _gitPlugin.OutputPane.OutputString("Error loading plugin: " + ex);
            }
        }

        private void GitPluginUIUpdate()
        {
            if (_gitPlugin == null)
            {
                return;
            }

            GitPluginInit();
            GitPluginUIUpdateMenu();
            GitPluginUIUpdateCommandBar();

            // enable update captions after initialization
            PluginHelpers.AllowCaptionUpdate = true;
        }

        private void GitPluginUIUpdateMenu()
        {
            try
            {
                if (_gitPlugin.IsReinstallMenuRequired())
                {
                    GitPluginUISetupMainMenu();
                    GitPluginUISetupContextMenu();
                }
            }
            catch (Exception ex)
            {
                _gitPlugin.OutputPane.OutputString("Error installing GitExt menu: " + ex);
            }
        }

        private void GitPluginUIUpdateCommandBar()
        {
            try
            {
                if (_gitPlugin.IsReinstallCommandBarRequired())
                {
                    GitPluginUISetupCommandBar();
                }
                else
                {
                    _gitPlugin.UpdateCommandBarStyles();
                }
            }
            catch (Exception)
            {
            }
        }

        private void RegisterGitPluginCommand()
        {
            ////GitPlugin.DeleteCommandBar("GitExtensions");
            try
            {
                _gitPlugin.RegisterCommand("Difftool_Selection", new ToolbarCommand<OpenWithDiftool>(runForSelection: true));
                _gitPlugin.RegisterCommand("Difftool", new ToolbarCommand<OpenWithDiftool>());
                _gitPlugin.RegisterCommand("ShowFileHistory_Selection", new ToolbarCommand<FileHistory>(runForSelection: true));
                _gitPlugin.RegisterCommand("ShowFileHistory", new ToolbarCommand<FileHistory>());
                _gitPlugin.RegisterCommand("ResetChanges_Selection", new ToolbarCommand<Revert>(runForSelection: true));
                _gitPlugin.RegisterCommand("ResetChanges", new ToolbarCommand<Revert>());
                _gitPlugin.RegisterCommand("Commit", new ToolbarCommand<Commit>());
                _gitPlugin.RegisterCommand("Browse", new ToolbarCommand<Browse>());
                _gitPlugin.RegisterCommand("Clone", new ToolbarCommand<Clone>());
                _gitPlugin.RegisterCommand("CreateBranch", new ToolbarCommand<CreateBranch>());
                _gitPlugin.RegisterCommand("SwitchBranch", new ToolbarCommand<SwitchBranch>());
                _gitPlugin.RegisterCommand("ViewChanges", new ToolbarCommand<ViewChanges>());
                _gitPlugin.RegisterCommand("CreateNewRepository", new ToolbarCommand<Init>());
                _gitPlugin.RegisterCommand("FormatPatch", new ToolbarCommand<FormatPatch>());
                _gitPlugin.RegisterCommand("Pull", new ToolbarCommand<Pull>());
                _gitPlugin.RegisterCommand("Push", new ToolbarCommand<Push>());
                _gitPlugin.RegisterCommand("Rebase", new ToolbarCommand<Rebase>());
                _gitPlugin.RegisterCommand("Merge", new ToolbarCommand<Merge>());
                _gitPlugin.RegisterCommand("CherryPick", new ToolbarCommand<Cherry>());
                _gitPlugin.RegisterCommand("Stash", new ToolbarCommand<Stash>());
                _gitPlugin.RegisterCommand("Settings", new ToolbarCommand<Settings>());
                _gitPlugin.RegisterCommand("SolveMergeConflicts", new ToolbarCommand<SolveMergeConflicts>());
                _gitPlugin.RegisterCommand("ApplyPatch", new ToolbarCommand<ApplyPatch>());
                _gitPlugin.RegisterCommand("About", new ToolbarCommand<About>());
                _gitPlugin.RegisterCommand("Bash", new ToolbarCommand<Bash>());
                _gitPlugin.RegisterCommand("GitIgnore", new ToolbarCommand<GitIgnore>());
                _gitPlugin.RegisterCommand("Remotes", new ToolbarCommand<Remotes>());
                _gitPlugin.RegisterCommand("FindFile", new ToolbarCommand<FindFile>());
            }
            catch (Exception ex)
            {
                _gitPlugin.OutputPane.OutputString("Error adding commands: " + ex);
            }
        }

        private void AddContextMenuItemsToContextMenu(string toolbarName, bool runForSelection = false)
        {
            var suffix = runForSelection ? "_Selection" : "";
            try
            {
                _gitPlugin.AddMenuCommand(toolbarName, "Difftool" + suffix, "GitExt: Diff",
                                         "Open with difftool", 24, 4);
                _gitPlugin.AddMenuCommand(toolbarName, "ShowFileHistory" + suffix, "GitExt: File history",
                                         "Show file history", 6, 5);
                _gitPlugin.AddMenuCommand(toolbarName, "ResetChanges" + suffix, "GitExt: Reset file changes",
                                         "Undo changes made to this file", 4, 6);
            }
            catch (Exception)
            {
                // ignore all exceptions....
                // When a commandbar is not found, an exception will be thrown -> todo avoid exceptions!
            }
        }

        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            ////if (disconnectMode == ext_DisconnectMode.ext_dm_HostShutdown
            ////   || disconnectMode == ext_DisconnectMode.ext_dm_UserClosed)
            ////{
            ////   _gitPlugin.DeleteCommands();
            ////   _gitPlugin.DeleteCommandBar(GitToolBarName);
            ////   //Place the command on the tools menu.
            ////   //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            ////   var menuBarCommandBar = ((CommandBars)_applicationObject.CommandBars)["MenuBar"];
            ////
            ////CommandBarControl toolsControl;
            ////   try
            ////   {
            ////       toolsControl = menuBarCommandBar.Controls["Git"];
            ////       if (toolsControl != null)
            ////       {
            ////           toolsControl.Delete();
            ////       }
            ////   }
            ////   catch
            ////   {
            ////   }
            ////}
        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnStartupComplete(ref Array custom)
        {
            GitPluginUIUpdate();
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
                // If you would like to move the command to a different menu, change the word "Tools" to the
                //  English version of the menu. This code will take the culture, append on the name of the menu
                //  then add the command to that menu. You can find a list of all the top-level menus in the file
                //  CommandBar.resx.
                string resourceName;
                var resourceManager = new ResourceManager("GitPlugin.CommandBar", Assembly.GetExecutingAssembly());
                var cultureInfo = new CultureInfo(_gitPlugin.LocaleId);

                if (cultureInfo.TwoLetterISOLanguageName == "zh")
                {
                    var parentCultureInfo = cultureInfo.Parent;
                    resourceName = string.Concat(parentCultureInfo.Name, "Tools");
                }
                else
                {
                    resourceName = string.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
                }

                toolsMenuName = resourceManager.GetString(resourceName);
            }
            catch (Exception)
            {
                // We tried to find a localized version of the word Tools, but one was not found.
                //  Default to the en-US word, which may work for the current culture.
                toolsMenuName = "Tools";
            }

            return toolsMenuName;
        }
    }
}