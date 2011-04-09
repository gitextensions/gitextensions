using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using GitPlugin.Git;
using System.IO;
using System.Threading;

namespace GitPlugin.Commands
{
    public class Commit : ItemCommandBase
    {
        private static DateTime lastBranchCheck;
        private static string lastFile;
        private static bool? showCurrentBranch;

        public Commit()
            : base(true, true)
        {
            if (lastBranchCheck == null)
                lastBranchCheck = DateTime.MinValue;
            if (lastFile == null)
                lastFile = string.Empty;
            if (showCurrentBranch == null)
                showCurrentBranch = GitCommands.GetShowCurrentBranchSetting();
        }

        public override bool IsEnabled(EnvDTE80.DTE2 application)
        {
            bool enabled = base.IsEnabled(application);

            string fileName = GetSelectedFile(application);

            if (showCurrentBranch.Value && (fileName != lastFile || DateTime.Now - lastBranchCheck > new TimeSpan(0, 0, 0, 1, 0)))
            {
                if (enabled)
                {
                    Plugin.ChangeCommandCaption(application, "Commit changes", "Commit" + GitCommands.GetCurrentBranch(fileName));
                }
                else
                {
                    Plugin.ChangeCommandCaption(application, "Commit changes", "Commit");
                }

                lastBranchCheck = DateTime.Now;
                lastFile = fileName;
            }

            return enabled;
        }

        public override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("commit", fileName);
        }
        private static string GetSelectedFile(EnvDTE80.DTE2 application)
        {
            if (application.SelectedItems.Count == 0)
                return string.Empty;

            foreach (SelectedItem sel in application.SelectedItems)
            {
                if (sel.ProjectItem != null)
                {
                    //The try catch block belowe fixed issue 57:
                    //http://github.com/spdr870/gitextensions/issues/#issue/57
                    try
                    {
                        return sel.ProjectItem.get_FileNames(0);
                    }
                    catch (ArgumentException)
                    {
                        if (sel.ProjectItem.FileCount > 0)
                        {
                            return sel.ProjectItem.get_FileNames(1);
                        }
                        else
                        {
                            //ignore!
                        }
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
