﻿using System;
using EnvDTE;
using GitPluginShared.Git;

namespace GitPluginShared.Commands
{
    public sealed class Commit : ItemCommandBase
    {
        private static DateTime lastBranchCheck;
        private static string lastFile;
        private static string _lastUpdatedCaption;

        public Commit()
        {
        }

        public override bool IsEnabled(EnvDTE80.DTE2 application)
        {
            bool enabled = base.IsEnabled(application);

            string fileName = GetSelectedFile(application);

            if ((fileName != lastFile || DateTime.Now - lastBranchCheck > new TimeSpan(0, 0, 0, 2, 0)))
            {
                string newCaption = "&Commit";
                if (enabled)
                {
                    bool showCurrentBranch = GitCommands.GetShowCurrentBranchSetting();
                    if (showCurrentBranch && !string.IsNullOrEmpty(fileName))
                    {
                        string head = GitCommands.GetCurrentBranch(fileName);
                        if (!string.IsNullOrEmpty(head))
                        {
                            string headShort;
                            if (head.Length > 27)
                                headShort = "..." + head.Substring(head.Length - 23);
                            else
                                headShort = head;

                            newCaption = "&Commit (" + headShort + ")";
                        }
                    }

                    lastBranchCheck = DateTime.Now;
                    lastFile = fileName;
                }

                // This guard required not only for perfromance, but also for prevent StackOverflowException.
                // IDE.QueryStatus -> Commit.IsEnabled -> Plugin.UpdateCaption -> IDE.QueryStatus ...
                if (_lastUpdatedCaption != newCaption)
                {
                    _lastUpdatedCaption = newCaption;

                    // try apply new caption (operation can fail)
                    if (!PluginHelpers.ChangeCommandCaption(application, PluginHelpers.GitCommandBarName, "Commit changes", newCaption))
                        _lastUpdatedCaption = null;
                }
            }

            return enabled;
        }

        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            if (item != null)
            {
                const string saveAllCommandName = "File.SaveAll";
                item.DTE.ExecuteCommand(saveAllCommandName, string.Empty);
            }

            RunGitEx("commit", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;

        private static string GetSelectedFile(EnvDTE80.DTE2 application)
        {
            foreach (SelectedItem sel in application.SelectedItems)
            {
                if (sel.ProjectItem != null)
                {
                    if (sel.ProjectItem.FileCount > 0)
                    {
                        //Unfortunaly FileNames[1] is not supported by .net 3.5
                        return sel.ProjectItem.get_FileNames(1);
                    }
                }
                else
                    if (sel.Project != null)
                        return sel.Project.FullName;
            }
            if (application.Solution.IsOpen)
                return application.Solution.FullName;
            return string.Empty;
        }
    }

}
