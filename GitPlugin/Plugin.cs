// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;

namespace GitPlugin.Commands
{
    // Wrapper class around registering other classes to handle the actual commands.
    // Interfaces with visual studio and handles the dispatch.
    public class Plugin
    {
        private const string GitCommandBarName = "GitExtensions";
        private const string OldGitMainMenuName = "&Git";
        private const string OldGitExtMainMenuName = "&GitExt";
        private const string GitMainMenuName = "G&itExt";

        private readonly AddIn _addIn;
        private readonly DTE2 _application;

        private readonly Dictionary<string, CommandBase> _commands = new Dictionary<string, CommandBase>();
        private readonly string _connectPath;
        private readonly OutputWindowPane _outputPane;
        private readonly Dictionary<string, Command> _visualStudioCommands = new Dictionary<string, Command>();

        // specify if captions of commands can be updated
        // On VS2013 (at least) update captions of command on hidden toolbar lead to create doubles of all commands on toolbar 2 commits, 4, 8, 16 ...
        public static bool AllowCaptionUpdate;

        public Plugin(DTE2 application, AddIn addIn, string panelName, string connectPath)
        {
            // TODO: This can be figured out from traversing the assembly and locating the Connect class...
            _connectPath = connectPath;

            _application = application;
            _addIn = addIn;
            _outputPane = AquireOutputPane(application, panelName);
        }

        public OutputWindowPane OutputPane
        {
            get { return _outputPane; }
        }

        public DTE2 Application
        {
            get
            {
                return _application;
            }
        }

        public int LocaleId
        {
            get
            {
                return _application.LocaleID;
            }
        }

        public void DeleteCommands()
        {
            foreach (Command command in _visualStudioCommands.Values)
            {
                command.Delete();
            }
        }

        public CommandBar GetMenuBar()
        {
            return CommandBars["MenuBar"];
        }

        public CommandBars CommandBars
        {
            get
            {
                return (CommandBars)_application.CommandBars;
            }
        }

        public void RegisterCommand(string commandName, CommandBase command)
        {
            if (commandName.IndexOf('.') >= 0)
                throw new ArgumentException("Command name cannot contain dot symbol.", "commandName");
            if (!_commands.ContainsKey(commandName))
                _commands.Add(commandName, command);
        }

        public bool CanHandleCommand(string commandName)
        {
            return TryGetCommand(commandName) != null;
        }

        public bool IsCommandEnabled(string commandName)
        {
            var command = TryGetCommand(commandName);
            return command != null && command.IsEnabled(_application);
        }

        private CommandBase TryGetCommand(string commandName)
        {
            var array = commandName.Split('.');
            if (array.Length != 3)
                return null;
            var commandKey = array[2];
            CommandBase result;
            return _commands.TryGetValue(commandKey, out result) ? result : null;
        }

        public bool OnCommand(string commandName)
        {
            var command = TryGetCommand(commandName);
            if (command == null)
                return false;
            command.OnCommand(_application, _outputPane);
            return true;
        }

        private Command GetCommand(string commandName)
        {
            var commands = (Commands2)_application.Commands;
            string fullName = _connectPath + "." + commandName;
            foreach (Command command in commands)
            {
                if (command.Name == fullName)
                    return command;
            }
            return null;
        }

        private static MsoButtonStyle CommandStyleToButtonStyle(vsCommandStyle commandStyle)
        {
            switch (commandStyle)
            {
                case vsCommandStyle.vsCommandStylePict:
                    return MsoButtonStyle.msoButtonIcon;
                case vsCommandStyle.vsCommandStyleText:
                    return MsoButtonStyle.msoButtonCaption;
                default:
                    return MsoButtonStyle.msoButtonIconAndCaption;
            }
        }

        private bool HasCommand(CommandBar commandBar, string caption)
        {
            caption = caption.Trim();
            return commandBar.Controls
                .Cast<CommandBarControl>()
                .Any(control => (control.Caption.Replace("&", "").Trim().Equals(caption.Replace("&", ""), StringComparison.CurrentCultureIgnoreCase) || (control.Caption.StartsWith("Commit") && caption.StartsWith("Commit"))));
        }

        public static bool ChangeCommandCaption(DTE2 application, string commandBarName, string tooltipText, string caption)
        {
            if (!AllowCaptionUpdate)
                return false;

            try
            {
                var cmdBars = (CommandBars)application.CommandBars;
                CommandBar commandBar = cmdBars[commandBarName];
                var cbcc = commandBar.Controls.Cast<CommandBarButton>().ToArray();
                foreach (var control in cbcc)
                {
                    if (control.TooltipText.Trim().Equals(tooltipText.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        control.Caption = caption;
                        control.Style = MsoButtonStyle.msoButtonIconAndCaption;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                //ignore!
                return false;
            }
        }

        public void DeleteGitExtCommandBar()
        {
            CommandBar cb =
                CommandBars.Cast<CommandBar>()
                    .FirstOrDefault(c => c.Name == GitCommandBarName);
            if (cb != null)
            {
                cb.Delete();
            }
        }

        public CommandBar AddGitExtCommandBar(MsoBarPosition position)
        {
            CommandBar bar =
                CommandBars.Cast<CommandBar>()
                    .FirstOrDefault(c => c.Name == GitCommandBarName);
            if (bar == null)
            {
                bar = (CommandBar)_application.Commands.AddCommandBar(GitCommandBarName, vsCommandBarType.vsCommandBarTypeToolbar);
                bar.Position = position;
                bar.RowIndex = -1;
            }

            return bar;
        }

        public void DeleteOldGitExtMainMenuBar()
        {
            try
            {
                CommandBarControl control =
                    GetMenuBar()
                        .Controls.Cast<CommandBarControl>()
                        .FirstOrDefault(c => c.Caption == OldGitMainMenuName);
                if (control != null)
                {
                    control.Delete(false);
                }
                control =
                    GetMenuBar()
                        .Controls.Cast<CommandBarControl>()
                        .FirstOrDefault(c => c.Caption == OldGitExtMainMenuName);
                if (control != null)
                {
                    control.Delete(false);
                }
                CommandBar cb =
                    CommandBars.Cast<CommandBar>()
                        .FirstOrDefault(c => c.Name == OldGitMainMenuName);
                if (cb != null && !cb.BuiltIn)
                {
                    cb.Delete();
                }
            }
            catch (Exception)
            {
            }
        }

        public void DeleteGitExtMainMenuBar()
        {
            try
            {
                CommandBarControl control =
                    GetMenuBar().Controls.Cast<CommandBarControl>()
                        .FirstOrDefault(c => c.Caption == GitMainMenuName);
                if (control != null)
                {
                    control.Delete(false);
                }
            }
            catch (Exception)
            {
            }
        }

        public bool IsReinstallMenuRequired()
        {
            try
            {
                CommandBarControl control =
                    GetMenuBar().Controls.Cast<CommandBarControl>()
                        .FirstOrDefault(c => c.Caption == GitMainMenuName);
                if (control == null)
                {
                    return true;
                }

                // menu from old versions
                control =
                    GetMenuBar()
                        .Controls.Cast<CommandBarControl>()
                        .FirstOrDefault(c => c.Caption == OldGitMainMenuName);
                if (control != null)
                {
                    return true;
                }
                control =
                    GetMenuBar()
                        .Controls.Cast<CommandBarControl>()
                        .FirstOrDefault(c => c.Caption == OldGitExtMainMenuName);
                if (control != null)
                {
                    return true;
                }
                CommandBar cb =
                    CommandBars.Cast<CommandBar>()
                        .FirstOrDefault(c => c.Name == OldGitMainMenuName);
                if (cb != null && !cb.BuiltIn)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public bool IsReinstallCommandBarRequired()
        {
            try
            {
                CommandBar cb =
                    CommandBars.Cast<CommandBar>()
                        .FirstOrDefault(c => c.Name == GitCommandBarName);
                if (cb == null)
                {
                    return true;
                }

                cb =
                    CommandBars.Cast<CommandBar>()
                        .FirstOrDefault(c => c.Name == OldGitMainMenuName);
                if (cb != null && !cb.BuiltIn)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public CommandBar AddGitExtMainMenuBar(string toolsMenuName)
        {
            CommandBar mainMenuBar = null;
            try
            {
                mainMenuBar = (CommandBar)_application.Commands
                    .AddCommandBar("GitExt", vsCommandBarType.vsCommandBarTypeMenu, GetMenuBar(), 4);

                ((CommandBarPopup)mainMenuBar.Parent).Caption = GitMainMenuName;
            }
            catch (Exception ex)
            {
                try
                {
                    OutputPane.OutputString("Error creating git menu (trying to add commands to tools menu): " + ex);
                    if (mainMenuBar == null)
                        mainMenuBar = (CommandBar)GetMenuBar().Controls[toolsMenuName];
                }
                catch (Exception ex2)
                {
                    OutputPane.OutputString("Error menu: " + ex2);
                }
            }

            return mainMenuBar;
        }

        public void AddToolbarCommand(CommandBar bar, string commandName, string caption,
                                      string tooltip, int iconIndex, int insertIndex, bool beginGroup = false)
        {
            AddToolbarOrMenuCommand(bar, commandName, caption, tooltip, iconIndex, insertIndex,
                                    vsCommandStyle.vsCommandStylePict, beginGroup);
        }

        public void AddToolbarCommandWithText(CommandBar bar, string commandName, string caption,
                                              string tooltip, int iconIndex, int insertIndex, bool beginGroup = false)
        {
            AddToolbarOrMenuCommand(bar, commandName, caption, tooltip, iconIndex, insertIndex,
                                    vsCommandStyle.vsCommandStylePictAndText, beginGroup);
        }

        public void AddMenuCommand(CommandBar bar, string commandName, string caption,
                                   string tooltip, int iconIndex, int insertIndex, bool beginGroup = false)
        {
            AddToolbarOrMenuCommand(bar, commandName, caption, tooltip, iconIndex, insertIndex,
                                    vsCommandStyle.vsCommandStylePictAndText, beginGroup);
        }

        public void AddMenuCommand(string toolbarName, string commandName, string caption,
                                   string tooltip, int iconIndex, int insertIndex, bool beginGroup = false)
        {
            CommandBar commandBar = CommandBars[toolbarName];
            if (commandBar != null)
                AddMenuCommand(commandBar, commandName, caption, tooltip, iconIndex, insertIndex, beginGroup);
        }

        public void AddPopupCommand(CommandBarPopup popup, string commandName, string caption,
                                    string tooltip, int iconIndex, int insertIndex, bool beginGroup = false)
        {
            // Do not try to add commands to a null menu
            if (popup == null)
                return;

            AddToolbarOrMenuCommand(popup.CommandBar, commandName, caption, tooltip, iconIndex, insertIndex,
                        vsCommandStyle.vsCommandStylePictAndText, beginGroup);
        }

        private void AddToolbarOrMenuCommand(CommandBar bar, string commandName, string caption,
                                             string tooltip, int iconIndex, int insertIndex, vsCommandStyle commandStyle, bool beginGroup)
        {
            // Do not try to add commands to a null bar
            if (bar == null)
                return;

            // Get commands collection
            var commands = (Commands2)_application.Commands;
            var command = GetCommand(commandName, caption, tooltip, iconIndex, commandStyle, commands);
            if (command == null)
                return;
            if (!HasCommand(bar, caption))
            {
#if DEBUG
                OutputPane.OutputString("Add toolbar command: " + caption + Environment.NewLine);
#endif
                var control = (CommandBarButton)command.AddControl(bar, insertIndex);
                control.Style = CommandStyleToButtonStyle(commandStyle);
                control.BeginGroup = beginGroup;
            }
        }

        public void UpdateCommandBarStyles()
        {
            CommandBar cb =
                CommandBars.Cast<CommandBar>()
                    .FirstOrDefault(c => c.Name == GitCommandBarName);
            if (cb != null)
            {
                foreach (CommandBarButton control in cb.Controls)
                {
                    if (control.Caption.StartsWith("Commit"))
                        control.Style = MsoButtonStyle.msoButtonIconAndCaption;
                    else
                        control.Style = MsoButtonStyle.msoButtonIcon;
                }
            }
        }

        private Command GetCommand(string commandName, string caption, string tooltip, int iconIndex,
            vsCommandStyle commandStyle, Commands2 commands)
        {
            var contextGUIDS = new object[] {};

            // Add command
            Command command = GetCommand(commandName);
            if (_visualStudioCommands.ContainsKey(commandName))
                return command;

            if (command == null && iconIndex > 0)
            {
                try
                {
                    command = commands.AddNamedCommand2(_addIn,
                        commandName, caption, tooltip, false, iconIndex,
                        ref contextGUIDS,
                        (int) vsCommandStatus.vsCommandStatusSupported |
                        (int) vsCommandStatus.vsCommandStatusEnabled,
                        (int) commandStyle);
                }
                catch (Exception)
                {
                }
            }

            if (command == null && commandStyle != vsCommandStyle.vsCommandStylePict)
            {
                command = commands.AddNamedCommand2(_addIn,
                    commandName, caption, tooltip, true, -1, ref contextGUIDS,
                    (int) vsCommandStatus.vsCommandStatusSupported |
                    (int) vsCommandStatus.vsCommandStatusEnabled,
                    (int) commandStyle);
            }

            if (command != null)
            {
                _visualStudioCommands[commandName] = command;
            }
            return command;
        }

        /*
            The toolbar name should be one of the following:
                    MenuBar
                    Standard
                    Build
                    XML Data
                    XML Schema
                    Context Menus
                    Dialog Editor
                    Image Editor
                    Text Editor
                    Source Control
                    Formatting
                    HTML Source Editing
                    Style Sheet
                    Device
                    Layout
                    Microsoft XML Editor
                    Class Designer Toolbar
                    Help
                    Debug Location
                    Debug
                    Recorder
                    Report Formatting
                    Report Borders
                    Data Design
                    Query Designer
                    View Designer
                    Database Diagram
                    Table Designer
                    Project Node
                    A&dd
                    Cab Project Node
                    A&dd
                    File nodes
                    Dep. file nodes
                    Assembly nodes
                    Dep. assembly nodes
                    MSM nodes
                    Dep. MSM nodes
                    Output nodes
                    Simple file nodes
                    Simple output nodes
                    Dependency node
                    Multiple selections
                    Dep. Multiple selections
                    View
                    Editor
                    Error List
                    Docked Window
                    Menu Designer
                    Properties Window
                    Toolbox
                    Task List
                    Results List
                    Stub Project
                    No Commands Available
                    Command Window
                    AutoHidden Windows
                    Expansion Manager
                    Find Regular Expression Builder
                    Replace Regular Expression Builder
                    Wild Card Expression Builder
                    Wild Card Expression Builder
                    External Tools Arguments
                    External Tools Directories
                    Easy MDI Tool Window
                    Easy MDI Document Window
                    Easy MDI Dragging
                    Open Drop Down
                    Object Browser Objects Pane
                    Object Browser Members Pane
                    Object Browser Description Pane
                    Find Symbol
                    Drag and Drop
                    Bookmark Window
                    Error Correction
                    EzMDI Files
                    Ca&ll Browser
                    Preview Changes
                    Smart Tag
                    Smart Tag
                    Editor Context Menus
                    Class View Context Menus
                    Debugger Context Menus
                    Project and Solution Context Menus
                    Other Context Menus
                    Sort By
                    Show Columns
                    Implement Interface
                    Resolve
                    Resolve
                    Refactor
                    Organize File
                    Class View Project
                    Class View Item
                    Class View Folder
                    Class View Grouping Folder
                    Class View Multi-select
                    Class View Multi-select members
                    Class View Member
                    Class View Grouping Members
                    Class View Project References Folder
                    Class View Project Reference
                    Project
                    Solution Folder
                    Cross Project Solution Project
                    Cross Project Solution Item
                    Cross Project Project Item
                    Cross Project Multi Project
                    Cross Project Multi Item
                    Cross Project Multi Solution Folder
                    Cross Project Multi Project/Folder
                    Item
                    Folder
                    Reference Root
                    Reference Item
                    Web Reference Folder
                    App Designer Folder
                    Web Project Folder
                    Web Folder
                    Web Item
                    Web SubWeb
                    Misc Files Project
                    Solution
                    Code Window
                    Registry
                    File System
                    File System
                    File Types
                    User Interface
                    Launch Conditions
                    Custom Actions
                    New
                    Add
                    Add Special Folder
                    View
                    Resource View
                    Resource Editors
                    Binary Editor
                    Propertysheet
                    Configuration
                    Project
                    Multi-Select
                    System Propertysheet
                    Checkin Dialog Context Menu
                    Pending Checkin Window Context Menu
                    Standard TreeGrid context menu
                    GetVersion Dialog Context Menu
                    Check Out Dialog Context Menu
                    Context
                    Basic Context
                    Context
                    Context
                    Context
                    Context
                    Context
                    Context
                    HTML Context
                    Script Context
                    Context
                    ASPX Context
                    ASPX Code Context
                    ASPX VB Code Context
                    ASMX Code Context
                    ASMX VB Code Context
                    ASMX Context
                    CSSDocOutline
                    CSSSource
                    Project Node
                    A&dd
                    Cab Project Node
                    A&dd
                    File nodes
                    Dep. file nodes
                    Assembly nodes
                    Dep. assembly nodes
                    MSM nodes
                    Dep. MSM nodes
                    Output nodes
                    Dependency node
                    Multiple selections
                    Dep. Multiple selections
                    View
                    Registry
                    File System
                    File System
                    New
                    Add
                    Add Special Folder
                    View
                    Selection
                    Container
                    TraySelection
                    Document Outline
                    Component Tray
                    Exe Project
                    Debug
                    OTBObjCtxtMenu
                    Class Designer Context Menu
                    Class Diagram Context Menu
                    TocContext
                    ResListContext
                    Editor
                    Script Outline
                    DefaultContext
                    ImageContext
                    SelectionContext
                    AnchorContext
                    Autos Window
                    Breakpoint
                    Breakpoints Window
                    Call Stack Window
                    Data Tip Window
                    Disassembly Window
                    Locals Window
                    Memory Window
                    Modules Window
                    Output Window
                    Processes Window
                    Registers Window
                    Threads Window
                    Watch Window
                    Server Explorer
                    PropertyBrowser
                    Macro
                    Module
                    Project
                    Root
                    Control
                    Report
                    Row/Column
                    Cell
                    Field Chooser
                    Row/Column
                    Chart
                    Database Project
                    DB Project Connection
                    DB Project Folder
                    Database References Folder
                    Folders
                    DB Project File
                    Query
                    Script
                    Database Reference Node
                    Files
                    Multi-select
                    Database Connection
                    Folder Multi-Selection
                    All Diagrams
                    All Tables
                    All Views
                    All Stored Procedures
                    All Package Specifications
                    All Package Bodies
                    All Synonyms
                    All Databases
                    All Users
                    All Roles
                    Node Multi-Selection
                    Diagram
                    Table
                    View
                    Stored Procedure
                    Function
                    Synonym
                    Package Spec
                    Package Body
                    Trigger
                    Column
                    SQL Editor
                    All Functions
                    Oracle Function
                    Oracle Procedure
                    Change &View
                    Single objet
                    Single static
                    Homogeneous objects
                    Mixed objects
                    Multiple static nodes
                    Mixed nodes
                    Add &New
                    Add &New
                    Surface
                    DataSourceContext
                    DbTableContext
                    DataTableContext
                    RelationContext
                    FunctionContext
                    ColumnContext
                    QueryContext
                    DataAccessorContext
                    Query Diagram Pane
                    Query Diagram Table
                    Query Diagram Table Column
                    Query Diagram Join Line
                    Query Diagram Multi-select
                    Query Grid Pane
                    Query SQL Pane
                    Query Results Pane
                    Database Designer
                    Database Designer Table
                    Database Designer Relationship
                    Text Annotation
                    Class Details Context Menu
                    TopicMenu
                    TopicMenu
                    Favorites Window Context Menu
                    Data Sources
                    Managed Resources Editor Context Menu
                    Settings Designer
                    System
         */

        private static OutputWindowPane AquireOutputPane(DTE2 app, string name)
        {
            try
            {
                if ("" == name)
                    return null;

                OutputWindowPane result = FindOutputPane(app, name);
                if (null != result)
                    return result;

                var outputWindow = (OutputWindow)app.Windows.Item(Constants.vsWindowKindOutput).Object;
                OutputWindowPanes panes = outputWindow.OutputWindowPanes;
                return panes.Add(name);
            }
            catch (Exception)
            {
                //ignore!!
                return null;
            }
        }

        public static OutputWindowPane FindOutputPane(DTE2 app, string name)
        {
            try
            {
                if ("" == name)
                    return null;

                var outputWindow = (OutputWindow)app.Windows.Item(Constants.vsWindowKindOutput).Object;
                OutputWindowPanes panes = outputWindow.OutputWindowPanes;

                foreach (OutputWindowPane pane in panes)
                {
                    if (name != pane.Name)
                        continue;

                    return pane;
                }

            }
            catch (Exception)
            {
                //ignore!!
            }
            return null;
        }
    }
}