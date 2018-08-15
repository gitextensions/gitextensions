// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.

using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.CommandBars;

namespace GitExtensionsVSIX
{
    public static class PluginHelpers
    {
        public const string GitCommandBarName = "GitExtensions";

        // specify if captions of commands can be updated
        // On VS2013 (at least) update captions of command on hidden toolbar lead to create doubles of all commands on toolbar 2 commits, 4, 8, 16 ...
        public static bool AllowCaptionUpdate;

        public static bool ChangeCommandCaption(_DTE application, string commandBarName, string tooltipText, string caption)
        {
            if (!AllowCaptionUpdate)
            {
                return false;
            }

            try
            {
                var cmdBars = (CommandBars)application.CommandBars;
                CommandBar commandBar = cmdBars[commandBarName];
                var btn = FindCommandBarButton(commandBar, tooltipText.Trim());
                if (btn != null)
                {
                    btn.Caption = caption;
                    btn.Style = MsoButtonStyle.msoButtonIconAndCaption;
                    return true;
                }

                return false;
            }
            catch
            {
                // ignore!
                return false;
            }
        }

        private static CommandBarButton FindCommandBarButton(CommandBar commandBar, string tooltipText)
        {
            return commandBar.Controls
                .Cast<CommandBarControl>()
                .Where(control => control.TooltipText.Trim() == tooltipText)
                .Cast<CommandBarButton>()
                .FirstOrDefault();
        }

        public static OutputWindowPane AcquireOutputPane(_DTE app, string name)
        {
            try
            {
                if (name == "")
                {
                    return null;
                }

                OutputWindowPane result = FindOutputPane(app, name);
                if (result != null)
                {
                    return result;
                }

                var outputWindow = (OutputWindow)app.Windows.Item(Constants.vsWindowKindOutput).Object;
                OutputWindowPanes panes = outputWindow.OutputWindowPanes;
                return panes.Add(name);
            }
            catch
            {
                // ignore!!
                return null;
            }
        }

        public static OutputWindowPane FindOutputPane(_DTE app, string name)
        {
            try
            {
                if (name == "")
                {
                    return null;
                }

                var outputWindow = (OutputWindow)app.Windows.Item(Constants.vsWindowKindOutput).Object;
                OutputWindowPanes panes = outputWindow.OutputWindowPanes;

                foreach (OutputWindowPane pane in panes)
                {
                    if (name != pane.Name)
                    {
                        continue;
                    }

                    return pane;
                }
            }
            catch
            {
                // ignore!!
            }

            return null;
        }
    }
}