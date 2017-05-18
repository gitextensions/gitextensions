// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Collections.Generic;

namespace GitPluginShared
{
    public static class PluginHelpers
    {
        public const string GitCommandBarName = "GitExtensions";
        public static readonly string ConnectPath = "GitExt";

        public static bool ChangeCommandCaption(this DTE2 application, string commandBarName, string tooltipText, string caption)
        {
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

        public static CommandBars CommandBars(this DTE2 application)
        {
            return (CommandBars)application.CommandBars;
        }

        public static CommandBar AddGitExtCommandBar(this DTE2 application, MsoBarPosition position)
        {
            CommandBar bar =
                application.CommandBars().Cast<CommandBar>()
                    .FirstOrDefault(c => c.Name == GitCommandBarName);
            if (bar == null)
            {
                bar = (CommandBar)application.Commands.AddCommandBar(GitCommandBarName, vsCommandBarType.vsCommandBarTypeToolbar);
                bar.Position = position;
                bar.RowIndex = -1;
            }

            return bar;
        }

        public static CommandBar GetOrAddGitExtCommandBar(this DTE2 application)
        {
            CommandBar cb =
                application.CommandBars().Cast<CommandBar>()
                    .FirstOrDefault(c => c.Name == GitCommandBarName);
            if (cb == null)
            {
                cb = application.AddGitExtCommandBar(MsoBarPosition.msoBarTop);
            }

            return cb;
        }

        public static void AddToolbarCommand(this DTE2 application, CommandBar bar, string commandName, 
            string caption, string tooltip, int insertIndex, vsCommandStyle commandStyle, bool beginGroup)
        {
            // Do not try to add commands to a null bar
            if (bar == null)
                return;

            // Get commands collection
            var command = application.GetCommand(commandName);
            if (command == null)
                return;

            CommandBarButton button = GetCommandBarButton(bar, caption);
            if (button == null)
            {
                button = (CommandBarButton)command.AddControl(bar, insertIndex);
                button.Style = CommandStyleToButtonStyle(commandStyle);
                button.BeginGroup = beginGroup;
            }
            button.TooltipText = tooltip;
        }

        public static void AddToolbarCommand(this DTE2 application, CommandBar bar, string commandName, string caption,
                                      string tooltip, int iconIndex, int insertIndex, bool beginGroup = false)
        {
            application.AddToolbarCommand(bar, commandName, caption, tooltip, insertIndex,
                                    vsCommandStyle.vsCommandStylePict, beginGroup);
        }

        public static void AddToolbarCommandWithText(this DTE2 application, CommandBar bar, string commandName, string caption,
                                              string tooltip, int iconIndex, int insertIndex, bool beginGroup = false)
        {
            application.AddToolbarCommand(bar, commandName, caption, tooltip, insertIndex,
                                    vsCommandStyle.vsCommandStylePictAndText, beginGroup);
        }

        public static MsoButtonStyle CommandStyleToButtonStyle(vsCommandStyle commandStyle)
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

        private static CommandBarButton GetCommandBarButton(CommandBar commandBar, string caption)
        {
            caption = caption.Trim().Replace("&", "");
            return commandBar.Controls
                .Cast<CommandBarControl>()
                .Where(control => (control.Caption.Replace("&", "").Trim().Equals(caption.Replace("&", ""), StringComparison.CurrentCultureIgnoreCase) || (control.Caption.StartsWith("Commit") && caption.StartsWith("Commit"))))
                .Cast<CommandBarButton>()
                .FirstOrDefault();
        }

        public static Command GetCommand(this DTE2 application, string commandName)
        {
            var commands = (Commands2)application.Commands;
            string fullName = ConnectPath + "." + commandName;
            foreach (Command command in commands)
            {
                if (command.Name == fullName)
                    return command;
            }
            return null;
        }

        public static OutputWindowPane AquireOutputPane(DTE2 app, string name)
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