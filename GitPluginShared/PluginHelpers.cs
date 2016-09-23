// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;

namespace GitPluginShared
{
    public static class PluginHelpers
    {
        public static bool ChangeCommandCaption(DTE2 application, string commandBarName, string tooltipText, string caption)
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