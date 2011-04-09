// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.Win32;
using GitPlugin.Git;

namespace GitPlugin.Commands
{
    public abstract class CommandBase
    {
        abstract public void OnCommand(DTE2 application, OutputWindowPane pane);

        abstract public bool IsEnabled(DTE2 application);

        public void RunGitEx(string command, string filename)
        {
            GitCommands.RunGitEx(command, filename);
        }

        public static void Run(string cmd, string arguments)
        {
            GitCommands.Run(cmd, arguments);
        }

    }
}
