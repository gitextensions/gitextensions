// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using GitPluginShared;
using GitPluginShared.Commands;

namespace GitPlugin
{
    /// <summary>
    /// Wrapper class around registering other classes to handle the actual commands.
    /// Interfaces with visual studio and handles the dispatch.
    /// </summary>
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
            _outputPane = PluginHelpers.AquireOutputPane(application, panelName);
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

            return GitPluginShared.PluginHelpers.ChangeCommandCaption(application, commandBarName, tooltipText, caption);
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
    }
}