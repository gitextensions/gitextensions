using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using GitPluginShared.Commands;
using Microsoft.VisualStudio.Shell;
using GitPluginShared;

namespace GitExtensionsVSIX.Commands
{
    public class VsixCommit : VsixCommandBase
    {
        private Commit CommitCommand => (Commit)BaseCommand;

        public VsixCommit() : base(new Commit())
        {
        }

        public override void BeforeQueryStatus(DTE2 application, OleMenuCommand menuCommand)
        {
            base.BeforeQueryStatus(application, menuCommand);

            string newCaption = CommitCommand.ComputeCaption(application);
            if (menuCommand.Text != newCaption)
            {
                menuCommand.Text = newCaption;
                if (!application.ChangeCommandCaption(PluginHelpers.GitCommandBarName, "Commit changes", newCaption))
                {
                    menuCommand.Text = "";
                }
            }
        }
    }
}
