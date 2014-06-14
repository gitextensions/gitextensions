﻿using System.Threading;
using EnvDTE;
using EnvDTE80;
using GitPlugin.Git;

namespace GitPlugin.Commands
{
    public sealed class FindFile : ItemCommandBase
    {
        public override void OnCommand(DTE2 application, OutputWindowPane pane)
        {
            ThreadPool.QueueUserWorkItem(
                        o =>
                        {
                            string file = GitCommands.RunGitExWait("searchfile", application.Solution.FullName);
                            if (file == null || string.IsNullOrEmpty(file.Trim()))
                                return;
                            application.ExecuteCommand("File.OpenFile", file);
                        });
        }


        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.SolutionExplorerFileItem; }
        }

        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            throw new System.NotImplementedException();
        }
    }
}
