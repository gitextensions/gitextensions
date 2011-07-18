﻿using System;
using EnvDTE;
using GitPlugin.Git;

namespace GitPlugin.Commands
{
    public sealed class Commit : ItemCommandBase
    {
        private static DateTime lastBranchCheck;
        private static string lastFile;
        private static bool showCurrentBranch;

        public Commit()
        {
            if (lastFile == null)
                lastFile = string.Empty;
            showCurrentBranch = GitCommands.GetShowCurrentBranchSetting();
        }

        public override bool IsEnabled(EnvDTE80.DTE2 application)
        {
            bool enabled = base.IsEnabled(application);

            string fileName = GetSelectedFile(application);

            if (showCurrentBranch && (fileName != lastFile || DateTime.Now - lastBranchCheck > new TimeSpan(0, 0, 0, 1, 0)))
            {
                if (enabled)
                {
                    string head = GitCommands.GetCurrentBranch(fileName);
                    if (!string.IsNullOrEmpty(head))
                    {
                        string headShort;
                        if (head.Length > 27)
                            headShort = "..." + head.Substring(head.Length - 23);
                        else
                            headShort = head;
                        Plugin.ChangeCommandCaption(application, "GitExtensions", "Commit changes", "Commit (" + headShort + ")");
                    }
                    else
                    {
                        Plugin.ChangeCommandCaption(application, "GitExtensions", "Commit changes", "Commit");
                    }
                }
                else
                {
                    Plugin.ChangeCommandCaption(application, "GitExtensions", "Commit changes", "Commit");
                }

                lastBranchCheck = DateTime.Now;
                lastFile = fileName;
            }

            return enabled;
        }

        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            const string saveAllCommandName = "File.SaveAll";

            item.DTE.ExecuteCommand(saveAllCommandName, string.Empty);
            RunGitEx("commit", fileName);
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.SolutionExplorerFileItem; }
        }

        private static string GetSelectedFile(EnvDTE80.DTE2 application)
        {
            if (application.SelectedItems.Count == 0)
                return string.Empty;

            foreach (SelectedItem sel in application.SelectedItems)
            {
                if (sel.ProjectItem != null)
                {
                    if (sel.ProjectItem.FileCount > 0)
                    {
                        return sel.ProjectItem.FileNames[1];
                    }
                }
                else
                    if (sel.Project != null)
                        return sel.Project.FullName;
            }
            return string.Empty;
        }
    }

}
